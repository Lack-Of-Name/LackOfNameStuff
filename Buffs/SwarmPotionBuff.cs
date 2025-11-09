using Terraria;
using Terraria.ModLoader;
using LackOfNameStuff.Players;

namespace LackOfNameStuff.Buffs
{
    public class SwarmPotionBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<SwarmPotionPlayer>().SwarmPotionActive = true;
        }
    }
}
