using Terraria;
using Terraria.ModLoader;

namespace LackOfNameStuff.Players
{
    public class SwarmPotionPlayer : ModPlayer
    {
        internal const int SpawnRateMultiplier = 5;
        internal const int MaxSpawnOverride = short.MaxValue;

        public bool SwarmPotionActive { get; set; }

        public override void ResetEffects()
        {
            SwarmPotionActive = false;
        }
    }
}
