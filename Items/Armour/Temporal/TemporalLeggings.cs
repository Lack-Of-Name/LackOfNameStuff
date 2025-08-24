// TemporalLeggings.cs
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Items.Materials;

namespace LackOfNameStuff.Items.Armour.Temporal
{
    [AutoloadEquip(EquipType.Legs)]
    public class TemporalLeggings : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(gold: 6);
            Item.rare = ItemRarityID.Red;
            Item.defense = 32;
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Temporal Leggings");
            // Tooltip.SetDefault("Allows for swift temporal manipulation");
        }

        public override void UpdateEquip(Player player)
        {
            // 15% movement speed and 10% attack speed
            player.moveSpeed += 0.15f;
            player.GetAttackSpeed(DamageClass.Generic) += 0.10f;
            
            // Enhanced jump
            player.jumpBoost = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 10);
            recipe.AddIngredient(ItemID.FragmentSolar, 3);
            recipe.AddIngredient(ItemID.FragmentVortex, 3);
            recipe.AddIngredient(ItemID.FragmentNebula, 3);
            recipe.AddIngredient(ItemID.FragmentStardust, 3);
            recipe.AddIngredient(ModContent.ItemType<TimeShard>(), 6);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}