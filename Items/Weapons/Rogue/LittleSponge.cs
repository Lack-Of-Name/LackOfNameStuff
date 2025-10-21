using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Common;
using LackOfNameStuff.Players;
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
            Item.damage = 360;
            Item.DamageType = ResolveDamageClass();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 18;
            Item.useTime = 18;
            Item.knockBack = 6.5f;
            Item.UseSound = SoundID.Item1;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<LittleSpongeProjectile>();
            Item.shootSpeed = 21f;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.buyPrice(gold: 22);
            Item.crit = 10;
            Item.ArmorPenetration = 12;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 normalizedVelocity = velocity.SafeNormalize(Vector2.UnitX) * Item.shootSpeed;
            LittleSpongePlayer spongePlayer = player.GetModPlayer<LittleSpongePlayer>();
            bool stealthStrike = CalamityIntegration.TryConsumeRogueStealthStrike(player);

            if (stealthStrike)
            {
                spongePlayer.ResetFallbackStealthProgress();
            }
            else
            {
                stealthStrike = spongePlayer.TryConsumeFallbackStealthStrike();
            }

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

            if (!stealthStrike && player.whoAmI == Main.myPlayer)
            {
                Vector2 lateral = velocity.RotatedBy(MathHelper.ToRadians(12f)) * 0.6f;
                int echoIndex = Projectile.NewProjectile(
                    source,
                    position,
                    lateral,
                    ModContent.ProjectileType<LittleSpongeShardProjectile>(),
                    (int)(damage * 0.45f),
                    knockback * 0.4f,
                    player.whoAmI);

                if (echoIndex >= 0 && echoIndex < Main.maxProjectiles)
                {
                    Main.projectile[echoIndex].DamageType = ResolveDamageClass();
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
