using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Items.Accessories;
using LackOfNameStuff.Worlds;

namespace LackOfNameStuff.Globals
{
    // Only affect player items, no entity freezing
    public class ChronosGlobalItem : GlobalItem
    {
        public override float UseTimeMultiplier(Item item, Player player)
        {
            // Check world-based bullet time state
            bool anyBulletTimeActive = ChronosWorld.GlobalBulletTimeActive;
            
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
    }

    // Remove all projectile and NPC freezing - no more entity detection issues
}