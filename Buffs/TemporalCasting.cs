
// TemporalCasting.cs
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace LackOfNameStuff.Buffs
{
    public class TemporalCasting : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true; // Don't save this buff when exiting world
            Main.buffNoTimeDisplay[Type] = false; // Show time remaining
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = false; // Nurse can remove if needed
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Reduce mana costs by 25%
            player.manaCost *= 0.75f;
            
            // Optional: Add some visual flair
            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(
                    player.position, 
                    player.width, 
                    player.height, 
                    DustID.MagicMirror
                );
                dust.velocity = Main.rand.NextVector2Circular(2f, 2f);
                dust.scale = 0.6f;
                dust.noGravity = true;
                dust.color = Microsoft.Xna.Framework.Color.Magenta;
                dust.alpha = 150;
            }
        }
    }
}