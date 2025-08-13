using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Items.Accessories;
using LackOfNameStuff.Worlds;

namespace LackOfNameStuff.Globals
{
    // Players' items have slower use time during bullet time for balance
    public class ChronosGlobalItem : GlobalItem
    {
        public override float UseTimeMultiplier(Item item, Player player)
        {
            // Check world-based bullet time state
            bool anyBulletTimeActive = ChronosWorld.GlobalBulletTimeActive;
            
            // Debug output to verify this is being called
            if (Main.netMode == NetmodeID.SinglePlayer && anyBulletTimeActive && item.useTime > 0)
            {
                // Uncomment for debugging: Main.NewText($"Slowing item use: {item.Name} from {item.useTime} to {item.useTime / ChronosWatch.TimeSlowFactor}", Color.Yellow);
            }
            
            if (anyBulletTimeActive)
            {
                // Make items take longer to use (inverse of time slow factor) for balance
                return 1f / ChronosWatch.TimeSlowFactor; // This should be 4.0f if TimeSlowFactor is 0.25f
            }
            return 1f;
        }

        public override float UseAnimationMultiplier(Item item, Player player)
        {
            if (ChronosWorld.GlobalBulletTimeActive)
            {
                // Make use animations slower for balance
                return 1f / ChronosWatch.TimeSlowFactor; // This should be 4.0f if TimeSlowFactor is 0.25f
            }
            return 1f;
        }

        // Handle item movement during bullet time
        public override void PostUpdate(Item item)
        {
            if (ChronosWorld.GlobalBulletTimeActive)
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

        public override bool InstancePerEntity => true;

        public override void PostAI(Projectile projectile)
        {
            if (ChronosWorld.GlobalBulletTimeActive)
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
                    projectile.velocity = storedVelocity;
                    hasStoredVelocity = false; // Reset the flag
                }
            }
        }
    }

    // Enemies get completely frozen during bullet time
    public class ChronosGlobalNPC : GlobalNPC
    {
        public Vector2 storedVelocity = Vector2.Zero;
        public Vector2 storedPosition = Vector2.Zero;
        public bool hasStoredState = false;

        public override bool InstancePerEntity => true;

        public override void PostAI(NPC npc)
        {
            if (ChronosWorld.GlobalBulletTimeActive)
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
                // Bullet time is not active - restore state if we had stored it
                if (hasStoredState)
                {
                    // Don't restore velocity as it might have changed naturally
                    // Just reset the stored state flag
                    hasStoredState = false;
                }
            }
        }
    }
}