// TemporalMissile.cs - Homing temporal missile that splits into sub-missiles near target
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.Audio;
using Terraria.GameContent;
using System;

namespace LackOfNameStuff.Projectiles
{
    public class TemporalMissile : ModProjectile
    {
        // Cached state derived from ai[]
        private int targetNPC = -1; // ai[0]
        private int armorTier = 1;  // ai[1]

        // Tunables
        private const float BaseSpeed = 12f;
        private const float SpeedPerTier = 3f;
        private const float BaseHoming = 0.08f;
        private const float HomingPerTier = 0.02f;
        private const float SplitDistance = 80f;
        private const float SearchRadius = 800f;

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
            // Use standard position caching to avoid jittery trail artifacts
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void AI()
        {
            // Cache AI parameters with sane clamps
            targetNPC = (int)Projectile.ai[0];
            armorTier = (int)MathHelper.Clamp(Projectile.ai[1], 1, 4);

            // Always perform homing behavior
            PerformHomingBehavior();

            // Rotation follows velocity
            if (Projectile.velocity.LengthSquared() > 0.001f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Enhanced trail particles - more frequent and varied
            CreateEnhancedTrailDust();

            // Check for nearby enemies to split (within SplitDistance)
            if (CheckForSplitCondition())
            {
                SplitIntoSubMissiles();
            }
        }

        private void PerformHomingBehavior()
        {
            NPC target = AcquireTarget();
            if (target == null)
                return;

            Vector2 desiredVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * (BaseSpeed + armorTier * SpeedPerTier);
            float homingStrength = BaseHoming + armorTier * HomingPerTier; // Stronger homing for higher tiers

            // Smoothly steer toward target while preserving speed feel
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, MathHelper.Clamp(homingStrength, 0f, 0.5f));
        }

        private bool CheckForSplitCondition()
        {
            NPC target = AcquireTarget();
            if (target == null)
                return false;

            return Vector2.Distance(Projectile.Center, target.Center) < SplitDistance;
        }

        private void SplitIntoSubMissiles()
        {
            // Multiplayer safety: only the owner spawns children
            if (Main.myPlayer == Projectile.owner)
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
            }

            // Explosion effect
            CreateSplitExplosion();

            // Kill the main missile
            Projectile.Kill();
        }

        private NPC AcquireTarget()
        {
            NPC target = (targetNPC >= 0 && targetNPC < Main.maxNPCs) ? Main.npc[targetNPC] : null;

            bool invalid = target == null || !target.active || target.friendly || target.lifeMax <= 5 || target.type == NPCID.TargetDummy;
            if (!invalid)
                return target;

            // Find nearest enemy within radius
            NPC closest = null;
            float closestDistance = SearchRadius;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy(this) && npc.type != NPCID.TargetDummy)
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < closestDistance)
                    {
                        closest = npc;
                        closestDistance = distance;
                    }
                }
            }

            if (closest != null)
            {
                // Update ai[0] for network sync
                Projectile.ai[0] = closest.whoAmI;
                targetNPC = closest.whoAmI;
                Projectile.netUpdate = true;
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
            // We fully handle drawing below to control glow; prevent default drawing to avoid double-render
            return false;
        }

        private void DrawEnhancedTemporalTrail()
        {
            // Restore the luscious Perlin-textured ribbon with smoothing and clamping
            var lineTexture = Main.Assets.Request<Texture2D>("Images/Misc/Perlin").Value;
            var lineOrigin = new Vector2(0, lineTexture.Height * 0.5f);

            Color silkyPurple = new Color(180, 60, 220);
            Color glowPurple = new Color(220, 120, 255);

            const float stepSize = 8f;        // draw small segments to avoid large stretches
            const float maxPairDistance = 180f; // be more tolerant; we already segment

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i - 1] == Vector2.Zero)
                    continue;

                Vector2 start = Projectile.oldPos[i - 1] + Projectile.Size * 0.5f - Main.screenPosition;
                Vector2 end = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                Vector2 diff = end - start;
                float segLen = diff.Length();
                if (segLen <= 0.1f)
                    continue;

                // Skip pathological long segments to prevent screen-spanning artifacts
                if (segLen > maxPairDistance)
                    continue;

                Vector2 dir = diff / segLen;
                float rotation = dir.ToRotation();

                // Fade and thickness along the trail history
                float baseProgress = 1f - (float)i / Projectile.oldPos.Length;
                float baseThickness = Math.Max(1.25f, 5f * baseProgress);

                int steps = Math.Max(1, (int)Math.Ceiling(segLen / stepSize));
                float actualStep = segLen / steps;

                for (int s = 0; s < steps; s++)
                {
                    // Localized progress along this pair and global trail progress combined
                    float pairT = steps <= 1 ? 1f : (float)s / (steps - 1);
                    float progress = MathHelper.Lerp(baseProgress * 0.9f, baseProgress, pairT);
                    float thickness = MathHelper.Lerp(baseThickness * 0.9f, baseThickness, pairT);

                    Color color = Color.Lerp(glowPurple, silkyPurple, 0.5f + 0.5f * progress) * (0.55f + 0.65f * progress);
                    color.A = (byte)MathHelper.Clamp(180f * progress, 0f, 255f);

                    Vector2 pos = start + dir * (s * actualStep);
                    // Source rect to tile/stretch the Perlin horizontally for consistent brightness
                    int srcW = Math.Max(1, (int)actualStep);
                    if (srcW > lineTexture.Width) srcW = lineTexture.Width;
                    Rectangle? src = new Rectangle(0, 0, srcW, lineTexture.Height);
                    Vector2 scale = new Vector2(1f, Math.Max(0.001f, thickness / lineTexture.Height));

                    Main.EntitySpriteDraw(
                        lineTexture,
                        pos,
                        src,
                        color,
                        rotation,
                        new Vector2(0, lineOrigin.Y),
                        scale,
                        SpriteEffects.None,
                        0
                    );
                }
            }

            // Draw core missile with slight glow (unchanged)
            var missileTexture = TextureAssets.Projectile[Projectile.type].Value;
            var missileOrigin = missileTexture.Size() * 0.5f;
            Vector2 coreDrawPos = Projectile.Center - Main.screenPosition;
            Color coreColor = GetTierColor(armorTier);

            // Outer glow
            Main.EntitySpriteDraw(
                missileTexture,
                coreDrawPos,
                null,
                Color.White * 0.3f,
                Projectile.rotation,
                missileOrigin,
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
                missileOrigin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );
        }

        // Prevent hitting or interacting with target dummies to avoid odd re-fire behavior
        public override bool? CanHitNPC(NPC target)
        {
            if (target.type == NPCID.TargetDummy)
                return false;
            return null;
        }
    }
}