using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Buffs;

namespace LackOfNameStuff.Items.Consumables
{
    public class ButterscotchCinnamonPie : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 24;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.UseSound = SoundID.Item2;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.buyPrice(gold: 5);
            Item.buffType = ModContent.BuffType<ButterscotchCinnamonBuff>();
            Item.buffTime = 60 * 90; // 90 seconds
        }

        public override bool CanUseItem(Player player)
        {
            return !player.HasBuff(Item.buffType);
        }
    }
}
