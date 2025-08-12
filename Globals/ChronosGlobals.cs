using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using MyMod.Items.Accessories;
using MyMod.Systems;

namespace MyMod.Globals
{
    // Players' items have slower use time during bullet time for balance
    public class ChronosGlobalItem : GlobalItem
    {
        public override float UseTimeMultiplier(Item item, Player player)
        {
            if (ChronosSystem.GlobalBulletTimeActive)
            {
                // Make items take longer to use (inverse of time slow factor) for balance
                return 1f / ChronosWatch.TimeSlowFactor;
            }
            return 1f;
        }

        public override float UseAnimationMultiplier(Item item, Player player)
        {
            if (ChronosSystem.GlobalBulletTimeActive)
            {
                // Make use animations slower for balance
                return 1f / ChronosWatch.TimeSlowFactor;
            }
            return 1f;
        }

        // Handle item movement during bullet time
        public override void PostUpdate(Item item)
        {
            if (ChronosSystem.GlobalBulletTimeActive)
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
            if (ChronosSystem.GlobalBulletTimeActive)
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
        public override void PostAI(NPC npc)
        {
            if (ChronosSystem.GlobalBulletTimeActive)
            {
                // Freeze enemies completely - no velocity multiplication issues
                npc.velocity = Vector2.Zero;
                
                // Also freeze their position to prevent any drift
                npc.position = npc.oldPosition;
            }
        }
    }
}