using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LackOfNameStuff.Items.Consumables
{
    public class RewindPotion : ModItem
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
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(gold: 2);
            Item.buffType = BuffID.PotionSickness;
            Item.buffTime = 3600; // 60 seconds (3600 ticks)
        }

        public override bool CanUseItem(Player player)
        {
            // Can't use if player has potion sickness
            return !player.HasBuff(BuffID.PotionSickness);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                var modPlayer = player.GetModPlayer<RewindPlayer>();
                modPlayer.TriggerRewind();
            }
            return true;
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Rewind Potion");
            // Tooltip.SetDefault("Teleports you to where you were 10 seconds ago\nCannot be used while under Potion Sickness");
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.RecallPotion);
            recipe.AddIngredient(ItemID.FallenStar, 3);
            recipe.AddIngredient(ItemID.BottledWater);
            recipe.AddTile(TileID.AlchemyTable);
            recipe.Register();
        }
    }
}