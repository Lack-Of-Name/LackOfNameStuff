// TemporalSubMissile.cs - Enhanced sub-missiles with improved trail effects
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
    public class TemporalSubMissile : ModProjectile
    {
        private int armorTier = 1; // from ai[1]

        // Tunables
        private const float BaseSpeed = 8f;
        private const float HomingBase = 0.04f;
        private const float HomingPerTier = 0.01f;
        private const float SearchRadius = 240f;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void SetStaticDefaults()
        {
            // Shorter trail for sub-missiles but still visible
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            // Use standard position caching to avoid jitter
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void AI()
        {
            armorTier = (int)MathHelper.Clamp(Projectile.ai[1], 1, 4);

            // Slight homing behavior toward closest valid target
            NPC target = AcquireTarget();
            if (target != null)
            {
                Vector2 desiredVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * BaseSpeed;
                float homingStrength = HomingBase + armorTier * HomingPerTier;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, MathHelper.Clamp(homingStrength, 0f, 0.35f));
            }

            if (Projectile.velocity.LengthSquared() > 0.001f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Enhanced trail dust for sub-missiles
            CreateSubMissileTrailDust();
        }

        private void CreateSubMissileTrailDust()
        {
            if (Main.rand.NextBool(3))
            {
                // Primary trail dust - smaller than main missiles
                Dust dust = Dust.NewDustDirect(
                    Projectile.position, 
                    Projectile.width, 
                    Projectile.height, 
                    DustID.PurpleMoss
                );
                dust.velocity = -Projectile.velocity * Main.rand.NextFloat(0.1f, 0.3f);
                dust.scale = Main.rand.NextFloat(0.4f, 0.7f);
                dust.noGravity = true;
                dust.color = GetTierColor(armorTier);
                dust.alpha = 80;

                // Occasional sparkles
                if (Main.rand.NextBool(4))
                {
                    Dust sparkle = Dust.NewDustDirect(Projectile.Center, 2, 2, DustID.Electric);
                    sparkle.velocity = Main.rand.NextVector2Circular(1f, 1f);
                    sparkle.scale = 0.5f;
                    sparkle.noGravity = true;
                    sparkle.color = Color.White;
                    sparkle.alpha = 120;
                }

                // Tier 4 rainbow effects
                if (armorTier >= 4 && Main.rand.NextBool(4))
                {
                    Dust rainbow = Dust.NewDustDirect(Projectile.Center, 3, 3, DustID.RainbowMk2);
                    rainbow.velocity = -Projectile.velocity * 0.2f;
                    rainbow.scale = 0.6f;
                    rainbow.noGravity = true;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Enhanced explosion on hit
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position, 
                    Projectile.width, 
                    Projectile.height, 
                    DustID.PurpleMoss
                );
                dust.velocity = Main.rand.NextVector2Circular(8f, 8f);
                dust.scale = Main.rand.NextFloat(0.6f, 1.0f);
                dust.noGravity = true;
                dust.color = GetTierColor(armorTier);
            }

            // Electric burst
            for (int i = 0; i < 4; i++)
            {
                Dust electric = Dust.NewDustDirect(Projectile.Center, 2, 2, DustID.Electric);
                electric.velocity = Main.rand.NextVector2Circular(6f, 6f);
                electric.scale = 0.8f;
                electric.noGravity = true;
                electric.color = Color.White;
            }
            
            SoundEngine.PlaySound(SoundID.Item10.WithVolumeScale(0.4f), Projectile.Center);
        }

        private NPC AcquireTarget()
        {
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
            return closest;
        }

        private Color GetTierColor(int tier)
        {
            return tier switch
            {
                1 => Color.Orange,
                2 => Color.Purple,
                3 => Color.Cyan,
                4 => Main.hslToRgb((Main.GlobalTimeWrappedHourly * 0.5f) % 1f, 1f, 0.8f),
                _ => Color.Yellow
            };
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw sub-missile trail
            DrawSubMissileTrail();
            // Prevent default draw to avoid double-rendering with glow layers
            return false;
        }

        private void DrawSubMissileTrail()
        {
            var texture = TextureAssets.Projectile[Projectile.type].Value;
            var origin = texture.Size() * 0.5f;

            // Draw trail with glow effect
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                float progress = 1f - (float)i / Projectile.oldPos.Length;
                Color trailColor = GetTierColor(armorTier) * (progress * 0.6f);
                
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                
                // Outer glow
                Main.EntitySpriteDraw(
                    texture,
                    drawPos,
                    null,
                    Color.White * (progress * 0.2f),
                    Projectile.oldRot[i],
                    origin,
                    Projectile.scale * progress * 1.2f,
                    SpriteEffects.None,
                    0
                );

                // Inner trail
                Main.EntitySpriteDraw(
                    texture,
                    drawPos,
                    null,
                    trailColor,
                    Projectile.oldRot[i],
                    origin,
                    Projectile.scale * progress,
                    SpriteEffects.None,
                    0
                );
            }

            // Draw core sub-missile with slight glow
            Vector2 coreDrawPos = Projectile.Center - Main.screenPosition;
            Color coreColor = GetTierColor(armorTier);
            
            // Outer glow
            Main.EntitySpriteDraw(
                texture,
                coreDrawPos,
                null,
                Color.White * 0.2f,
                Projectile.rotation,
                origin,
                Projectile.scale * 1.2f,
                SpriteEffects.None,
                0
            );
            
            // Inner core
            Main.EntitySpriteDraw(
                texture,
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

        public override bool? CanHitNPC(NPC target)
        {
            if (target.type == NPCID.TargetDummy)
                return false;
            return null;
        }
    }
}