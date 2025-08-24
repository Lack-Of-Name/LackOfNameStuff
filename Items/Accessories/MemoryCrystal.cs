using LackOfNameStuff.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LackOfNameStuff.Items.Accessories
{
    public class MemoryCrystal : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Memory Crystal");
            // Tooltip.SetDefault("Stores damage dealt during bullet time\nReplays all stored damage when time resumes\n'Remember every strike'");
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.value = Item.buyPrice(gold: 15);
            Item.rare = ItemRarityID.Red;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<MemoryCrystalPlayer>().hasMemoryCrystal = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 10);
            recipe.AddIngredient(ItemID.FragmentVortex, 12);
            recipe.AddIngredient(ModContent.ItemType<Items.Materials.TimeShard>(), 8);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}