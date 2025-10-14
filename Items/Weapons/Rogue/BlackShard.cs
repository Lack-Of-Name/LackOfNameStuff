using LackOfNameStuff.Common;
using LackOfNameStuff.Players;
using LackOfNameStuff.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace LackOfNameStuff.Items.Weapons.Rogue
{
    public class BlackShard : ModItem
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
            Item.damage = 198;
            Item.DamageType = ResolveDamageClass();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 14;
            Item.useTime = 14;
            Item.knockBack = 6.5f;
            Item.UseSound = SoundID.Item60;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<BlackShardProjectile>();
            Item.shootSpeed = 18f;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.buyPrice(gold: 50);
            Item.crit = 10;
            Item.noUseGraphic = false;
            Item.noMelee = false;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 normalizedVelocity = velocity.SafeNormalize(Vector2.UnitX) * Item.shootSpeed;
            BlackShardPlayer shardPlayer = player.GetModPlayer<BlackShardPlayer>();
            bool flip = shardPlayer.ConsumeFlipToggle();
            bool stealthStrikeTriggered = CalamityIntegration.TryConsumeRogueStealthStrike(player);

            if (stealthStrikeTriggered)
            {
                shardPlayer.ResetFallbackStealthProgress();
            }
            else
            {
                stealthStrikeTriggered = shardPlayer.TryConsumeFallbackStealthStrike();
            }

            if (stealthStrikeTriggered)
            {
                const int projectileCount = 3;
                const float spreadDegrees = 12f;

                for (int i = 0; i < projectileCount; i++)
                {
                    float offset = projectileCount == 1 ? 0f : MathHelper.Lerp(-spreadDegrees, spreadDegrees, i / (float)(projectileCount - 1));
                    Vector2 perturbedVelocity = normalizedVelocity.RotatedBy(MathHelper.ToRadians(offset));
                    int specialIndex = Projectile.NewProjectile(source, position, perturbedVelocity, ModContent.ProjectileType<BlackShardProjectile>(), (int)(damage * 1.15f), knockback + 1f, player.whoAmI, flip ? 1f : 0f, 1f);

                    if (specialIndex >= 0 && specialIndex < Main.maxProjectiles)
                    {
                        Main.projectile[specialIndex].DamageType = ResolveDamageClass();
                    }

                    flip = !flip;
                }

                return false;
            }

            int projectileIndex = Projectile.NewProjectile(source, position, normalizedVelocity, ModContent.ProjectileType<BlackShardProjectile>(), damage, knockback, player.whoAmI, flip ? 1f : 0f, 0f);

            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
            {
                Main.projectile[projectileIndex].DamageType = ResolveDamageClass();
            }

            return false;
        }

        public override void AddRecipes()
        {
            Recipe baseRecipe = CreateRecipe();
            baseRecipe.AddIngredient(ItemID.LunarBar, 16);
            baseRecipe.AddIngredient(ItemID.FragmentSolar, 12);
            baseRecipe.AddIngredient(ItemID.FragmentVortex, 12);
            baseRecipe.AddIngredient(ModContent.ItemType<Items.Materials.EternalGem>(), 2);
            baseRecipe.AddTile(TileID.LunarCraftingStation);
            baseRecipe.Register();

            if (CalamityIntegration.TryGetCalamityItem("CosmiliteBar", out int cosmiliteBar))
            {
                Recipe calamityRecipe = CreateRecipe();
                calamityRecipe.AddIngredient(cosmiliteBar, 12);

                if (CalamityIntegration.TryGetCalamityItem("AscendantSpiritEssence", out int ascendantSpiritEssence))
                {
                    calamityRecipe.AddIngredient(ascendantSpiritEssence, 3);
                }

                calamityRecipe.AddIngredient(ModContent.ItemType<Items.Materials.EternalGem>(), 2);
                calamityRecipe.AddTile(TileID.LunarCraftingStation);
                calamityRecipe.Register();
            }
        }
    }
}
