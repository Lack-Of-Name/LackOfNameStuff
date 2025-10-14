using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Common;
using LackOfNameStuff.Projectiles;

namespace LackOfNameStuff.Items.Weapons.Rogue
{
    public class LittleSponge : ModItem
    {
        private DamageClass ResolveDamageClass()
        {
            if (CalamityIntegration.CalamityLoaded &&
                CalamityIntegration.CalamityMod.TryFind("RogueDamageClass", out DamageClass rogueDamage))
            {
                return rogueDamage;
            }

            return DamageClass.Ranged;
        }

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.damage = 253;
            Item.DamageType = ResolveDamageClass();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.knockBack = 5f;
            Item.UseSound = SoundID.Item1;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<LittleSpongeProjectile>();
            Item.shootSpeed = 16f;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.buyPrice(gold: 15);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 normalizedVelocity = velocity.SafeNormalize(Vector2.UnitX) * Item.shootSpeed;
            bool stealthStrike = CalamityIntegration.TryConsumeRogueStealthStrike(player);

            int projectileIndex = Projectile.NewProjectile(
                source,
                position,
                normalizedVelocity,
                ModContent.ProjectileType<LittleSpongeProjectile>(),
                damage,
                knockback,
                player.whoAmI,
                stealthStrike ? 1f : 0f);

            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
            {
                Main.projectile[projectileIndex].DamageType = ResolveDamageClass();
            }

            if (stealthStrike && player.whoAmI == Main.myPlayer)
            {
                const int shardCount = 6;
                const float shardSpeed = 12f;

                for (int i = 0; i < shardCount; i++)
                {
                    float angle = MathHelper.TwoPi / shardCount * i;
                    Vector2 shardVelocity = angle.ToRotationVector2() * shardSpeed;

                    int shardIndex = Projectile.NewProjectile(
                        source,
                        position,
                        shardVelocity,
                        ModContent.ProjectileType<LittleSpongeShardProjectile>(),
                        (int)(damage * 0.65f),
                        knockback * 0.6f,
                        player.whoAmI);

                    if (shardIndex >= 0 && shardIndex < Main.maxProjectiles)
                    {
                        Main.projectile[shardIndex].DamageType = ResolveDamageClass();
                    }
                }
            }

            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 10);
            recipe.AddIngredient(ItemID.FragmentStardust, 8);
            recipe.AddIngredient(ModContent.ItemType<Items.Materials.TimeGem>(), 5);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();

            if (CalamityIntegration.TryGetCalamityItem("CosmiliteBar", out int cosmiliteBar))
            {
                Recipe calamityRecipe = CreateRecipe();
                calamityRecipe.AddIngredient(cosmiliteBar, 8);

                if (CalamityIntegration.TryGetCalamityItem("RuinousSoul", out int ruinousSoul))
                {
                    calamityRecipe.AddIngredient(ruinousSoul, 5);
                }

                calamityRecipe.AddIngredient(ModContent.ItemType<Items.Materials.TimeGem>(), 6);
                calamityRecipe.AddTile(TileID.LunarCraftingStation);
                calamityRecipe.Register();
            }
        }
    }
}
