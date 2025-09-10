// TemporalMinions.cs
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace LackOfNameStuff.Buffs
{
    public class TemporalMinions : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true; // Don't save this buff when exiting world
            Main.buffNoTimeDisplay[Type] = false; // Show time remaining
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = false; // Nurse can remove if needed
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Increase minion damage by 35%
            player.GetDamage(DamageClass.Summon) += 0.35f;
            
            // Optional: Increase minion attack speed slightly
            // Note: This affects all summon weapons the player uses while buff is active
            player.GetAttackSpeed(DamageClass.Summon) += 0.15f;
            
            // Optional: Add visual effect around player
            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(
                    player.position - new Microsoft.Xna.Framework.Vector2(10, 10), 
                    player.width + 20, 
                    player.height + 20, 
                    DustID.PurpleTorch
                );
                dust.velocity = Main.rand.NextVector2Circular(1.5f, 1.5f);
                dust.scale = Main.rand.NextFloat(0.7f, 1.0f);
                dust.noGravity = true;
                dust.color = Microsoft.Xna.Framework.Color.Purple;
                dust.alpha = 100;
            }
        }
    }
}