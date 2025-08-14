using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Items.Accessories;
using LackOfNameStuff.Buffs;

namespace LackOfNameStuff.Globals
{
    // Players' items have slower use time during bullet time for balance
    public class ChronosGlobalItem : GlobalItem
    {
        // Check if ANY player has the bullet time buff (local check only)
        private bool IsAnyPlayerInBulletTime()
        {
            // Only check local player for now to avoid multiplayer complications
            return Main.LocalPlayer.HasBuff<BulletTimeBuff>();
        }

        public override float UseTimeMultiplier(Item item, Player player)
        {
            if (IsAnyPlayerInBulletTime())
            {
                // Make items take longer to use (inverse of time slow factor) for balance
                return 1f / ChronosWatch.TimeSlowFactor; // This should be 4.0f if TimeSlowFactor is 0.25f
            }
            return 1f;
        }

        public override float UseAnimationMultiplier(Item item, Player player)
        {
            if (IsAnyPlayerInBulletTime())
            {
                // Make use animations slower for balance
                return 1f / ChronosWatch.TimeSlowFactor; // This should be 4.0f if TimeSlowFactor is 0.25f
            }
            return 1f;
        }

        // Handle item movement during bullet time
        public override void PostUpdate(Item item)
        {
            if (IsAnyPlayerInBulletTime())
            {
                // Slow down item velocity (both X and Y)
                item.velocity *= ChronosWatch.TimeSlowFactor;
            }
        }
    }

    // Player projectiles remain at normal speed, enemy projectiles get frozen
    public class ChronosGlobalProjectile : GlobalProjectile
    {
        public Vector2 storedVelocity = Vector2.Zero;
        public bool hasStoredVelocity = false;
        
        // Add tracking for clean reset
        private bool wasInBulletTimePreviously = false;

        public override bool InstancePerEntity => true;

        // Check if local player has bullet time buff
        private bool IsLocalPlayerInBulletTime()
        {
            return Main.LocalPlayer.HasBuff<BulletTimeBuff>();
        }

        public override void PostAI(Projectile projectile)
        {
            bool currentlyInBulletTime = IsLocalPlayerInBulletTime();
            
            if (currentlyInBulletTime)
            {
                // Only affect enemy projectiles - freeze them completely
                if (projectile.hostile && !projectile.friendly)
                {
                    // Store the original velocity before freezing (only once)
                    if (!hasStoredVelocity)
                    {
                        storedVelocity = projectile.velocity;
                        hasStoredVelocity = true;
                    }
                    
                    // Freeze enemy projectiles completely
                    projectile.velocity = Vector2.Zero;
                    
                    // Extend their lifetime so they don't despawn while frozen
                    if (projectile.timeLeft < 600) // If less than 10 seconds left
                    {
                        projectile.timeLeft = 600; // Give them 10 seconds
                    }
                }
                // Player projectiles (friendly && !hostile) are completely unaffected - remain at normal speed
            }
            else
            {
                // Bullet time is not active - restore velocity if we had stored it
                if (hasStoredVelocity && projectile.hostile && !projectile.friendly)
                {
                    // Only restore if we actually have stored velocity and it's not zero
                    if (storedVelocity != Vector2.Zero)
                    {
                        projectile.velocity = storedVelocity;
                    }
                }
                
                // CRITICAL FIX: Always reset the stored state when bullet time ends
                if (wasInBulletTimePreviously && !currentlyInBulletTime)
                {
                    // Complete state reset
                    hasStoredVelocity = false;
                    storedVelocity = Vector2.Zero;
                }
            }
            
            // Track previous state for clean transitions
            wasInBulletTimePreviously = currentlyInBulletTime;
        }
        
        // Add a method to force reset (for use by stopper tool)
        public void ForceReset()
        {
            hasStoredVelocity = false;
            storedVelocity = Vector2.Zero;
            wasInBulletTimePreviously = false;
        }
    }

    // Enemies get completely frozen during bullet time
    public class ChronosGlobalNPC : GlobalNPC
    {
        public Vector2 storedVelocity = Vector2.Zero;
        public Vector2 storedPosition = Vector2.Zero;
        public bool hasStoredState = false;
        
        // Add tracking for clean reset
        private bool wasInBulletTimePreviously = false;

        public override bool InstancePerEntity => true;

        // Check if local player has bullet time buff
        private bool IsLocalPlayerInBulletTime()
        {
            return Main.LocalPlayer.HasBuff<BulletTimeBuff>();
        }

        public override void PostAI(NPC npc)
        {
            bool currentlyInBulletTime = IsLocalPlayerInBulletTime();
            
            if (currentlyInBulletTime)
            {
                // Store the state before freezing (only once)
                if (!hasStoredState)
                {
                    storedVelocity = npc.velocity;
                    storedPosition = npc.position;
                    hasStoredState = true;
                }

                // Freeze enemies completely - no velocity multiplication issues
                npc.velocity = Vector2.Zero;
                
                // Also freeze their position to prevent any drift
                npc.position = storedPosition;
            }
            else
            {
                // CRITICAL FIX: Always reset stored state when bullet time ends
                if (wasInBulletTimePreviously && !currentlyInBulletTime)
                {
                    // Complete state reset - don't try to restore velocity as it may have changed
                    hasStoredState = false;
                    storedVelocity = Vector2.Zero;
                    storedPosition = Vector2.Zero;
                    
                    // Give the NPC a tiny nudge to ensure it starts moving again if needed
                    if (npc.velocity == Vector2.Zero && npc.aiStyle > 0)
                    {
                        npc.velocity = new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), Main.rand.NextFloat(-0.1f, 0.1f));
                    }
                }
            }
            
            // Track previous state for clean transitions
            wasInBulletTimePreviously = currentlyInBulletTime;
        }
        
        // Add a method to force reset (for use by stopper tool)
        public void ForceReset()
        {
            hasStoredState = false;
            storedVelocity = Vector2.Zero;
            storedPosition = Vector2.Zero;
            wasInBulletTimePreviously = false;
        }
    }
}