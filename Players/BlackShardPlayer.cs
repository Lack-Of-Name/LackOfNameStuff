using LackOfNameStuff.Common;
using Terraria.ModLoader;

namespace LackOfNameStuff.Players
{
    public class BlackShardPlayer : ModPlayer
    {
        private bool flipNext;
        private int fallbackStealthCharge;
        private const int FallbackStealthChargeRequired = 4;

        public bool ConsumeFlipToggle()
        {
            flipNext = !flipNext;
            return flipNext;
        }

        public bool TryConsumeFallbackStealthStrike()
        {
            if (CalamityIntegration.CalamityLoaded)
            {
                fallbackStealthCharge = 0;
                return false;
            }

            fallbackStealthCharge++;

            if (fallbackStealthCharge < FallbackStealthChargeRequired)
            {
                return false;
            }

            fallbackStealthCharge = 0;
            return true;
        }

        public void ResetFallbackStealthProgress()
        {
            fallbackStealthCharge = 0;
        }
    }
}
