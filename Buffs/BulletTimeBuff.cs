using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace LackOfNameStuff.Buffs
{
    public class BulletTimeBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false; // This is not a debuff
            Main.pvpBuff[Type] = false; // Not a PvP buff
            Main.buffNoSave[Type] = true; // Don't save this buff (it shouldn't persist)
            BuffID.Sets.LongerExpertDebuff[Type] = false; // Not affected by expert mode
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // This buff indicates the player should experience bullet time effects
            
            // Optional: Add some visual dust or effects to show the player is in bullet time
            if (Main.rand.NextBool(30)) // Every ~0.5 seconds
            {
                Dust dust = Dust.NewDustDirect(
                    player.position, 
                    player.width, 
                    player.height,
                    DustID.Electric,
                    0f, 0f, 100, 
                    Color.Cyan, 
                    0.8f
                );
                dust.velocity *= 0.3f;
                dust.noGravity = true;
            }
        }

        public override bool RightClick(int buffIndex)
        {
            // Don't allow manual removal of this buff
            return false;
        }
    }
}