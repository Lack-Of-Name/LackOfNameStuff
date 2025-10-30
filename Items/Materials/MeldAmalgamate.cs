using LackOfNameStuff.Common;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LackOfNameStuff.Items.Materials
{
	public class MeldAmalgamate : ModItem
	{
		private static readonly Color[] NameCycleColors =
		{
			new Color(254, 105, 47),
			new Color(0, 118, 157),
			new Color(0, 106, 185),
			new Color(190, 30, 209),
			new Color(25, 35, 47)
		};

		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 25;
			Rarity.RegisterItemCycle("Mods.LackOfNameStuff.Items.Materials.MeldAmalgamate.DisplayName", 1.6, NameCycleColors);
		}

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.maxStack = 999;
			Item.value = Item.buyPrice(gold: 2);
			Item.rare = ItemRarityID.Yellow;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			if (!Rarity.TryBuildCyclingName(DisplayName.Key, out string animated))
			{
				return;
			}

			for (int i = 0; i < tooltips.Count; i++)
			{
				TooltipLine line = tooltips[i];
				if (line.Mod == "Terraria" && line.Name == "ItemName")
				{
					line.Text = animated;
					break;
				}
			}
		}

		public override void AddRecipes()
		{
			if (!CalamityIntegration.TryGetCalamityItem("MeldBlob", out int meldBlob))
			{
				return;
			}

			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.FragmentSolar);
			recipe.AddIngredient(ItemID.FragmentVortex);
			recipe.AddIngredient(ItemID.FragmentNebula);
			recipe.AddIngredient(ItemID.FragmentStardust);
			recipe.AddIngredient(meldBlob);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.Register();
		}
	}
}
