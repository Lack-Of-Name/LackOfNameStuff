using Terraria;
using Terraria.ModLoader;

namespace LackOfNameStuff.Buffs
{
    public class ButterscotchCinnamonBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.3f;
            player.GetCritChance(DamageClass.Generic) += 12f;
            player.statDefense += 20;
            player.moveSpeed += 0.25f;
            player.jumpSpeedBoost += 1.5f;
            player.lifeRegen += 6;
            player.statLifeMax2 += 40;
            player.endurance += 0.1f;
        }
    }
}
