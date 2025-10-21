using LackOfNameStuff.Common;
using Terraria.ModLoader;

namespace LackOfNameStuff.Players
{
    public class LittleSpongePlayer : ModPlayer
    {
        private int fallbackStealthCharge;
        private const int FallbackStealthChargeRequired = 5;

        public bool TryConsumeFallbackStealthStrike()
        {
            if (CalamityIntegration.RogueStealthIntegrationAvailable)
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
