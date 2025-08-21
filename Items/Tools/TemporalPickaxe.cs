using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using LackOfNameStuff.Items.Materials;

namespace LackOfNameStuff.Items.Tools
{
    public class TemporalPickaxe : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 80;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6;
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Cyan;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            
            Item.pick = 225; // Lunar pickaxe power
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            // Apply speed bonus to mining
            var modPlayer = player.GetModPlayer<TemporalPickaxePlayer>();
            float speedBonus = modPlayer.GetSpeedBonus();
            
            // Convert speed bonus to useTime reduction
            if (speedBonus > 0)
            {
                Item.useTime = Math.Max(1, (int)(15 / (1 + speedBonus / 100f)));
                Item.useAnimation = Item.useTime;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<TemporalPickaxePlayer>();
            int blocksMined = modPlayer.BlocksMinedWithTemporalPickaxe;
            float speedBonus = modPlayer.GetSpeedBonus();
            
            TooltipLine blocksLine = new TooltipLine(Mod, "BlocksMined", $"Blocks mined: {blocksMined:N0}")
            {
                OverrideColor = Color.LightBlue
            };
            tooltips.Add(blocksLine);
            
            TooltipLine speedLine = new TooltipLine(Mod, "SpeedBonus", $"Mining speed: +{speedBonus:F1}%")
            {
                OverrideColor = speedBonus >= 1000 ? Color.Gold : Color.LightGreen
            };
            tooltips.Add(speedLine);
            
            if (speedBonus < 1000)
            {
                int blocksToMax = 5000 - blocksMined;
                TooltipLine progressLine = new TooltipLine(Mod, "Progress", $"{blocksToMax:N0} blocks to maximum speed")
                {
                    OverrideColor = Color.Gray
                };
                tooltips.Add(progressLine);
            }
            else
            {
                TooltipLine maxLine = new TooltipLine(Mod, "MaxSpeed", "MAXIMUM SPEED ACHIEVED")
                {
                    OverrideColor = Color.Gold
                };
                tooltips.Add(maxLine);
            }
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Temporal Pickaxe");
            // Tooltip.SetDefault("Mining speed increases permanently with each block mined\n'Time remembers every swing'");
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 12);
            recipe.AddIngredient(ItemID.FragmentNebula, 8);
            recipe.AddIngredient(ItemID.FragmentSolar, 8);
            recipe.AddIngredient(ModContent.ItemType<TimeShard>(), 1);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}