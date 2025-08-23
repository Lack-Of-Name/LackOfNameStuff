// TemporalLauncher.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Items.Materials;
using Terraria.DataStructures;
using System.Security.Cryptography.X509Certificates;
using LackOfNameStuff.Globals;

namespace LackOfNameStuff.Items.Weapons.Ranged
{
    public class TemporalLauncher : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Set temporal weapon properties here
            var temporalData = Item.GetGlobalItem<TemporalWeaponData>();
            temporalData.TemporalWeapon = true;
            temporalData.TemporalBuffDamage = 1.4f;    // 40% more damage
            temporalData.TemporalBuffSpeed = 1.8f;     // 80% faster
            temporalData.TemporalBuffCrit = 20f;       // +20% crit chance
            temporalData.TemporalBuffKnockback = 2.4f; // 140% more knockback
            // Just other setstaticdefault stuff
            // DisplayName.SetDefault("Temporal Launcher");
            // Tooltip.SetDefault("Fires heavy temporal orbs that manipulate time on impact\n'The weight of eternity in every shot'");

        }

        public override void SetDefaults()
        {
            // Basic weapon stats
            Item.damage = 185;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 64;
            Item.height = 28;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 12);
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item61; // Rocket launcher sound
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<TemporalShot>();
            Item.shootSpeed = 20f;
            Item.useAmmo = ModContent.ItemType<TimeShard>();
            Item.scale = 0.8f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire the temporal orb
            Vector2 shootPos = position + Vector2.Normalize(velocity) * 40f;
            Projectile.NewProjectile(source, shootPos, velocity, ModContent.ProjectileType<TemporalShot>(), damage, knockback, player.whoAmI);

            // Visual effect at barrel
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(shootPos, 10, 10, DustID.Electric);
                dust.velocity = velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.3f, 0.8f);
                dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
                dust.noGravity = true;
                dust.color = Color.Lerp(Color.Orange, Color.Yellow, Main.rand.NextFloat());
            }

            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 12);
            recipe.AddIngredient(ItemID.FragmentSolar, 8);
            recipe.AddIngredient(ItemID.RocketLauncher, 1);
            recipe.AddIngredient(ModContent.ItemType<TimeShard>(), 8);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }

    public class TemporalShot : ModProjectile
    {
        private bool hasHitGround = false;
        private int rippleTimer = 0;
        private Vector2 lastPosition;

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.aiStyle = 0; // Custom AI
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.scale = 1.2f;
            Projectile.tileCollide = true;
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Temporal Orb");
        }

        public override void AI()
        {
            lastPosition = Projectile.position;

            // Rotation based on velocity
            Projectile.rotation += 0.1f;

            // Trail particles
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric);
                dust.velocity = -Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(2f, 2f);
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                dust.noGravity = true;
                dust.color = Color.Lerp(Color.Orange, Color.Yellow, Main.rand.NextFloat());
                dust.alpha = 100;
            }

            // Gravity affects the projectile over time
            Projectile.velocity.Y += 0.4f;

            // Slow down horizontally slightly due to "temporal drag"
            Projectile.velocity.X *= 0.995f;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            CreateTemporalExplosion();
            return true; // Destroy projectile
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            CreateTemporalExplosion();

            // Apply temporal slow debuff to the hit enemy
            target.AddBuff(ModContent.BuffType<Buffs.TemporalSlow>(), 300); // 5 seconds
        }

        private void CreateTemporalExplosion()
        {
            // Sound effect
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            // Create expanding temporal ripple
            for (int ring = 0; ring < 3; ring++)
            {
                int numDust = 20 + (ring * 10);
                float ringRadius = 30f + (ring * 40f);

                for (int i = 0; i < numDust; i++)
                {
                    float angle = (float)i / numDust * MathHelper.TwoPi;
                    Vector2 dustPos = Projectile.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * ringRadius;

                    Dust dust = Dust.NewDustDirect(dustPos, 0, 0, DustID.Electric);
                    dust.velocity = Vector2.Zero;
                    dust.scale = 1.5f - (ring * 0.3f);
                    dust.noGravity = true;
                    dust.color = Color.Lerp(Color.Orange, Color.Yellow, Main.rand.NextFloat());
                    dust.fadeIn = 1f;
                    dust.alpha = 50;
                }
            }

            // Slow + AOE damage
            float explosionRadius = 120f;
            int aoeDamage = (int)(Projectile.damage * 0.75f);
            float knockback = 4f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && !npc.immortal &&
                    Vector2.Distance(npc.Center, Projectile.Center) < explosionRadius)
                {
                    // Apply debuff
                    npc.AddBuff(ModContent.BuffType<Buffs.TemporalSlow>(), 240);

                    // Deal damage (handles MP sync automatically)
                    int hitDir = npc.Center.X > Projectile.Center.X ? 1 : -1;

                    // Call this on the server/host to avoid double hits from clients
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        npc.SimpleStrikeNPC(aoeDamage, hitDir, crit: false, knockBack: knockback, damageType: Projectile.DamageType);

                    // Visual feedback
                    for (int j = 0; j < 8; j++)
                    {
                        Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Electric);
                        dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
                        dust.scale = 0.8f;
                        dust.noGravity = true;
                        dust.color = Color.Cyan;
                    }
                }
            }
        }
    }
}
