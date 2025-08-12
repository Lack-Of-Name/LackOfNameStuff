using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace MyMod.Items.Ammo
{
    public class ExplosiveArrow : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Explosive Arrow");
            // Tooltip.SetDefault("Explodes on impact, destroying terrain but not walls");
            
            // Creative mode research requirement
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
        }

        public override void SetDefaults()
        {
            // Item properties
            Item.damage = 12;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 14;
            Item.height = 34;
            Item.maxStack = 999;
            Item.consumable = true;
            Item.knockBack = 3f;
            Item.value = Item.buyPrice(copper: 50); // 50 copper per arrow
            Item.rare = ItemRarityID.Orange; // Orange rarity
            Item.shoot = ModContent.ProjectileType<Projectiles.ExplosiveArrow>();
            Item.shootSpeed = 3f;
            Item.ammo = AmmoID.Arrow; // This is arrow ammo
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(50); // Makes 50 arrows per craft
            recipe.AddIngredient(ItemID.WoodenArrow, 50);
            recipe.AddIngredient(ItemID.Dynamite, 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();

            // Alternative recipe with bombs
            Recipe recipe2 = CreateRecipe(25); // Makes 25 arrows per craft (bombs are cheaper)
            recipe2.AddIngredient(ItemID.WoodenArrow, 25);
            recipe2.AddIngredient(ItemID.Bomb, 1);
            recipe2.AddTile(TileID.WorkBenches);
            recipe2.Register();

            // Higher tier recipe with grenades (post-hardmode)
            Recipe recipe3 = CreateRecipe(100); // Makes 100 arrows per craft
            recipe3.AddIngredient(ItemID.JestersArrow, 100);
            recipe3.AddIngredient(ItemID.Grenade, 1);
            recipe3.AddTile(TileID.MythrilAnvil);
            recipe3.Register();
        }
    }
}