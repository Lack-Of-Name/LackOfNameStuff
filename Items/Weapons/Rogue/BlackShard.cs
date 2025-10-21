using LackOfNameStuff.Common;
using LackOfNameStuff.Players;
using LackOfNameStuff.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            Item.damage = 360;
            Item.DamageType = ResolveDamageClass();
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 8;
            Item.useTime = 8;
            Item.knockBack = 7.75f;
            Item.UseSound = SoundID.Item60;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<BlackShardProjectile>();
            Item.shootSpeed = 22f;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(gold: 95);
            Item.crit = 16;
            Item.ArmorPenetration = 18;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.scale = 1f;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            scale = 0.2f;
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            scale = 1f;
            return true;
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
                const int projectileCount = 4;
                const float spreadDegrees = 14f;

                for (int i = 0; i < projectileCount; i++)
                {
                    float offset = projectileCount == 1 ? 0f : MathHelper.Lerp(-spreadDegrees, spreadDegrees, i / (float)(projectileCount - 1));
                    Vector2 perturbedVelocity = normalizedVelocity.RotatedBy(MathHelper.ToRadians(offset));
                    int specialIndex = Projectile.NewProjectile(source, position, perturbedVelocity, ModContent.ProjectileType<BlackShardProjectile>(), (int)(damage * 1.25f), knockback + 1.5f, player.whoAmI, flip ? 1f : 0f, 1f);

                    if (specialIndex >= 0 && specialIndex < Main.maxProjectiles)
                    {
                        Main.projectile[specialIndex].DamageType = ResolveDamageClass();
                        Main.projectile[specialIndex].ArmorPenetration += 12;
                    }

                    flip = !flip;
                }

                Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<BlackShardRift>(), (int)(damage * 0.85f), knockback, player.whoAmI, 1f);
                return false;
            }

            int projectileIndex = Projectile.NewProjectile(source, position, normalizedVelocity, ModContent.ProjectileType<BlackShardProjectile>(), damage, knockback, player.whoAmI, flip ? 1f : 0f, 0f);

            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
            {
                Main.projectile[projectileIndex].DamageType = ResolveDamageClass();
                Main.projectile[projectileIndex].ArmorPenetration += 8;
            }

            if (player.whoAmI == Main.myPlayer && Main.rand.NextBool(3))
            {
                Vector2 offset = normalizedVelocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-18f, 18f))) * 0.35f;
                Projectile.NewProjectile(source, position, offset, ModContent.ProjectileType<BlackShardRift>(), (int)(damage * 0.6f), knockback * 0.75f, player.whoAmI, 0f);
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
