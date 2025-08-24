using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace LackOfNameStuff.Items.Ammo
{
    public class ExplosiveBullet : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Explosive Bullet");
            // Tooltip.SetDefault("Explodes on impact, destroying terrain but not walls\nSmaller explosion than explosive arrows");
            
            // Creative mode research requirement
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
        }

        public override void SetDefaults()
        {
            // Item properties
            Item.damage = 8;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 8;
            Item.height = 8;
            Item.maxStack = 3996; // Changed to match normal ammo stack size
            Item.consumable = true;
            Item.knockBack = 2f;
            Item.value = Item.buyPrice(copper: 25); // 25 copper per bullet (cheaper than arrows)
            Item.rare = ItemRarityID.Orange; // Orange rarity
            Item.shoot = ModContent.ProjectileType<Projectiles.ExplosiveBullet>();
            Item.shootSpeed = 7f;
            Item.ammo = AmmoID.Bullet; // This is bullet ammo
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(100); // Makes 100 bullets per craft
            recipe.AddIngredient(ItemID.MusketBall, 100);
            recipe.AddIngredient(ItemID.Dynamite, 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();

            // Alternative recipe with bombs
            Recipe recipe2 = CreateRecipe(50); // Makes 50 bullets per craft
            recipe2.AddIngredient(ItemID.MusketBall, 50);
            recipe2.AddIngredient(ItemID.Bomb, 1);
            recipe2.AddTile(TileID.WorkBenches);
            recipe2.Register();

            // Higher tier recipe with grenades (post-hardmode)
            Recipe recipe3 = CreateRecipe(200); // Makes 200 bullets per craft
            recipe3.AddIngredient(ItemID.SilverBullet, 200);
            recipe3.AddIngredient(ItemID.Bomb, 1);
            recipe3.AddTile(TileID.MythrilAnvil);
            recipe3.Register();

            // Meteor shot variant
            Recipe recipe4 = CreateRecipe(150); // Makes 150 bullets per craft
            recipe4.AddIngredient(ItemID.MeteorShot, 150);
            recipe4.AddIngredient(ItemID.Bomb, 1);
            recipe4.AddTile(TileID.Anvils);
            recipe4.Register();
        }
    }
}