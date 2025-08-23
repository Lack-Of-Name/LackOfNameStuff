// TemporalSlow.cs
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace LackOfNameStuff.Buffs
{
    public class TemporalSlow : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Temporal Slow");
            // Description.SetDefault("Movement and attack speed reduced by temporal distortion");
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // Slow down the NPC
            npc.velocity *= 0.6f; // 40% movement speed reduction
            
            // Visual effect
            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Electric);
                dust.velocity = Main.rand.NextVector2Circular(1f, 1f);
                dust.scale = 0.6f;
                dust.noGravity = true;
                dust.color = Color.Cyan;
                dust.alpha = 150;
            }
        }
    }
}