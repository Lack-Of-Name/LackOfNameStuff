using Terraria.ModLoader;

namespace LackOfNameStuff.Players
{
    public class BlackShardPlayer : ModPlayer
    {
        private bool flipNext;

        public bool ConsumeFlipToggle()
        {
            flipNext = !flipNext;
            return flipNext;
        }
    }
}
