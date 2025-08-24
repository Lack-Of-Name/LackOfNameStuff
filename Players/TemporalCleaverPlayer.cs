using Terraria;
using Terraria.ModLoader;
using LackOfNameStuff.Players;

namespace LackOfNameStuff.Players
{
    public class TemporalCleaverPlayer : ModPlayer
    {
        public int storedEnergy = 0;
        
        public override void ResetEffects()
        {
            // Don't reset energy here - let it persist between swings
        }

        public override void PostUpdateMiscEffects()
        {
            // Cap stored energy and add decay if not in bullet time
            if (storedEnergy > 100) 
                storedEnergy = 100;
            
            var chronosPlayer = Player.GetModPlayer<ChronosPlayer>();
            if (!chronosPlayer.bulletTimeActive && storedEnergy > 0)
            {
                // Slowly decay energy when not in bullet time
                if (Main.rand.NextBool(60)) // Every second roughly
                    storedEnergy--;
            }
        }
    }
}