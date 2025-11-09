using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Buffs;
using LackOfNameStuff.Items.Materials;

namespace LackOfNameStuff.Items.Consumables
{
	public class SwarmPotion : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 26;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useAnimation = 17;
			Item.useTime = 17;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item3;
			Item.maxStack = 30;
			Item.consumable = true;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.buyPrice(gold: 1, silver: 50);
			Item.buffType = ModContent.BuffType<SwarmPotionBuff>();
			Item.buffTime = 60 * 60 * 10; // 10 minutes
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddIngredient(ModContent.ItemType<MeldAmalgamate>());
			recipe.AddTile(TileID.AlchemyTable);
			recipe.Register();
		}
	}
}
