// TemporalMissile.cs
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.Audio;
using System;

namespace LackOfNameStuff.Projectiles
{
    public class TemporalMissile : ModProjectile
    {
        private int targetNPC = -1;
        private int armorTier = 1;
        private int homingTimer = 0;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic; // Scales with all damage types
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600; // 10 seconds max lifetime
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // Phases through walls
        }

        public override void SetStaticDefaults()
        {
            // Increase trail length for smoother effect
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void AI()
        {
            targetNPC = (int)Projectile.ai[0];
            armorTier = (int)Projectile.ai[1];

            homingTimer++;

            // Initial phase: Straight flight for first 1.5 seconds
            if (homingTimer < 90)
            {
                return;
            }
            // Phase 2: Home in on target
            else
            {
                PerformHomingBehavior();
            }

            // Rotation follows velocity
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Enhanced trail particles - more frequent and varied
            CreateEnhancedTrailDust();

            // Check for nearby enemies to split (within 80 pixels)
            if (CheckForSplitCondition())
            {
                SplitIntoSubMissiles();
            }
        }

        private void PerformHomingBehavior()
        {
            NPC target = targetNPC >= 0 && targetNPC < Main.maxNPCs ? Main.npc[targetNPC] : null;
            
            // If original target is dead/invalid, find nearest enemy
            if (target == null || !target.active || target.friendly)
            {
                target = FindNearestEnemy();
            }

            if (target != null)
            {
                Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                float homingStrength = 0.08f + (armorTier * 0.02f); // Stronger homing for higher tiers
                float speed = 12f + (armorTier * 3f);
                
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * speed, homingStrength);
            }
        }

        private bool CheckForSplitCondition()
        {
            NPC target = targetNPC >= 0 && targetNPC < Main.maxNPCs ? Main.npc[targetNPC] : FindNearestEnemy();
            
            if (target != null && Vector2.Distance(Projectile.Center, target.Center) < 80f)
            {
                return true;
            }
            return false;
        }

        private void SplitIntoSubMissiles()
        {
            int subMissileCount = 3 + armorTier; // 4-7 sub-missiles based on tier
            int subMissileDamage = (int)(Projectile.damage * (0.4f + armorTier * 0.1f)); // 50%-70% damage per sub-missile

            for (int i = 0; i < subMissileCount; i++)
            {
                float angle = (float)i / subMissileCount * MathHelper.TwoPi;
                Vector2 velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (6f + armorTier);
                
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    velocity,
                    ModContent.ProjectileType<TemporalSubMissile>(),
                    subMissileDamage,
                    Projectile.knockBack * 0.6f,
                    Projectile.owner,
                    ai1: armorTier
                );
            }

            // Explosion effect
            CreateSplitExplosion();
            
            // Kill the main missile
            Projectile.Kill();
        }

        private NPC FindNearestEnemy()
        {
            NPC closest = null;
            float closestDistance = 800f; // Search radius
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5)
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < closestDistance)
                    {
                        closest = npc;
                        closestDistance = distance;
                    }
                }
            }
            
            return closest;
        }

        private void CreateEnhancedTrailDust()
        {
            // More frequent and varied dust particles
            if (Main.rand.NextBool(2)) // More frequent spawning
            {
                // Primary trail dust
                Dust dust = Dust.NewDustDirect(
                    Projectile.position + new Vector2(-4, -4), 
                    Projectile.width + 8, 
                    Projectile.height + 8, 
                    DustID.PurpleMoss // Purple dust for temporal theme
                );
                dust.velocity = -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.4f);
                dust.scale = Main.rand.NextFloat(0.8f, 1.3f);
                dust.noGravity = true;
                dust.color = GetTierColor(armorTier);
                dust.alpha = 50;

                // Secondary sparkle effects
                if (Main.rand.NextBool(3))
                {
                    Dust sparkle = Dust.NewDustDirect(Projectile.Center, 4, 4, DustID.Electric);
                    sparkle.velocity = Main.rand.NextVector2Circular(2f, 2f);
                    sparkle.scale = Main.rand.NextFloat(0.5f, 0.8f);
                    sparkle.noGravity = true;
                    sparkle.color = Color.White;
                    sparkle.alpha = 100;
                }

                // Tier 4 special effects (rainbow sparkles)
                if (armorTier >= 4 && Main.rand.NextBool(2))
                {
                    Dust rainbow = Dust.NewDustDirect(Projectile.Center, 6, 6, DustID.RainbowMk2);
                    rainbow.velocity = -Projectile.velocity * 0.3f;
                    rainbow.scale = 1.0f;
                    rainbow.noGravity = true;
                }
            }
        }

        private void CreateSplitExplosion()
        {
            // Enhanced visual explosion when splitting
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position, 
                    Projectile.width, 
                    Projectile.height, 
                    DustID.PurpleMoss
                );
                dust.velocity = Main.rand.NextVector2Circular(10f, 10f);
                dust.scale = Main.rand.NextFloat(1f, 1.8f);
                dust.noGravity = true;
                dust.color = GetTierColor(armorTier);
            }

            // Electric burst effect
            for (int i = 0; i < 8; i++)
            {
                Dust electric = Dust.NewDustDirect(Projectile.Center, 4, 4, DustID.Electric);
                electric.velocity = Main.rand.NextVector2Circular(8f, 8f);
                electric.scale = 1.2f;
                electric.noGravity = true;
                electric.color = Color.White;
            }
            
            SoundEngine.PlaySound(SoundID.Item14.WithVolumeScale(0.5f), Projectile.Center);
        }

        private Color GetTierColor(int tier)
        {
            return tier switch
            {
                1 => Color.Orange,
                2 => Color.Purple,
                3 => Color.Cyan,
                4 => Main.hslToRgb((Main.GlobalTimeWrappedHourly * 0.5f) % 1f, 1f, 0.8f), // Rainbow for Eternal Gem
                _ => Color.Yellow
            };
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw enhanced trail effect
            DrawEnhancedTemporalTrail();
            return true;
        }

        private void DrawEnhancedTemporalTrail()
        {
            // Use a custom trail texture for the ribbon effect
            // Place a PNG at Projectiles/TemporalMissileTrail.png (e.g., 64x8, horizontal gradient)
            var trailTexture = ModContent.Request<Texture2D>("LackOfNameStuff/Projectiles/TemporalMissileTrail").Value;
            var trailOrigin = new Vector2(0, trailTexture.Height / 2);

            // Silky, luscious purple color for the trail
            Color silkyPurple = new Color(180, 60, 220); // Rich purple
            Color glowPurple = new Color(220, 120, 255); // Lighter glow

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Vector2 start = Projectile.oldPos[i - 1] + Projectile.Size * 0.5f - Main.screenPosition;
                Vector2 end = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i - 1] == Vector2.Zero) continue;

                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                float progress = 1f - (float)i / Projectile.oldPos.Length;

                int tileWidth = trailTexture.Width;
                int tileCount = Math.Max(1, (int)(length / tileWidth));
                Vector2 step = direction / tileCount;

                for (int t = 0; t < tileCount; t++)
                {
                    Vector2 pos = start + step * t;
                    float fade = progress * (1f - (float)t / tileCount);
                    // Silky purple with soft fade and glow
                    Color color = Color.Lerp(glowPurple, silkyPurple, 0.5f + 0.5f * fade) * (0.5f + 0.5f * fade);
                    color.A = (byte)(180 * fade); // Soft fade out
                    Main.EntitySpriteDraw(
                        trailTexture,
                        pos,
                        null,
                        color,
                        rotation,
                        trailOrigin,
                        1f,
                        SpriteEffects.None,
                        0
                    );
                }
            }

            // Draw core missile with slight glow (unchanged)
            var missileTexture = ModContent.Request<Texture2D>("Terraria/Images/Projectile_" + Projectile.type).Value;
            var origin = missileTexture.Size() * 0.5f;
            Vector2 coreDrawPos = Projectile.Center - Main.screenPosition;
            Color coreColor = GetTierColor(armorTier);

            // Outer glow
            Main.EntitySpriteDraw(
                missileTexture,
                coreDrawPos,
                null,
                Color.White * 0.3f,
                Projectile.rotation,
                origin,
                Projectile.scale * 1.4f,
                SpriteEffects.None,
                0
            );

            // Inner core
            Main.EntitySpriteDraw(
                missileTexture,
                coreDrawPos,
                null,
                coreColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );
        }
    }
}