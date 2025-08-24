// TemporalChestplate.cs
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Items.Materials;

namespace LackOfNameStuff.Items.Armour.Temporal
{
    [AutoloadEquip(EquipType.Body)]
    public class TemporalChestplate : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(gold: 8);
            Item.rare = ItemRarityID.Red;
            Item.defense = 37;
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Temporal Chestplate");
            // Tooltip.SetDefault("Channels temporal energy through the wearer");
        }

        public override void UpdateEquip(Player player)
        {
            // Increase damage and crit for all class damage
            player.GetDamage(DamageClass.Generic) += 0.15f;
            player.GetCritChance(DamageClass.Generic) += 10;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 12);
            recipe.AddIngredient(ItemID.FragmentSolar, 3);
            recipe.AddIngredient(ItemID.FragmentVortex, 3);
            recipe.AddIngredient(ItemID.FragmentNebula, 3);
            recipe.AddIngredient(ItemID.FragmentStardust, 3);
            recipe.AddIngredient(ModContent.ItemType<TimeShard>(), 8);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}