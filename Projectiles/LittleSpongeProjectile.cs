using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Common;

namespace LackOfNameStuff.Projectiles
{
    public class LittleSpongeProjectile : ModProjectile
    {
        private const int TrailCacheLength = 26;
        private const float BaseScale = 1.28f;
        private const float ScalePulseAmplitude = 0.42f;
        private const float ScalePulseFrequency = 5.4f;
        private const int OrbitalDustRate = 3;
        private const int CometDustRate = 4;
        private const int RadiantDustRate = 2;
        private const int RadiantBurstInterval = 24;
        private const float RadiantBurstSpeed = 11.5f;
        private const float RadiantBurstScale = 32f;
        private const int NonStealthShardInterval = 16;
        private const int StealthShardInterval = 7;
        private const float NonStealthShardDamageFactor = 0.45f;
        private const float NonStealthShardKnockbackFactor = 0.4f;
        private const float StealthShardDamageFactor = 0.6f;
        private const float StealthShardKnockbackFactor = 0.55f;

        private bool IsStealthStrike => Projectile.ai[0] >= 0.5f;

        private DamageClass ResolveDamageClass()
        {
            if (CalamityIntegration.CalamityLoaded &&
                CalamityIntegration.CalamityMod.TryFind("RogueDamageClass", out DamageClass rogueDamage))
            {
                return rogueDamage;
            }

            return DamageClass.Ranged;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailCacheLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 62;
            Projectile.height = 62;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 7;
            Projectile.DamageType = ResolveDamageClass();
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 210;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.extraUpdates = 1;
            Projectile.ArmorPenetration = 24;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.6f }, Projectile.Center);
            }

            Projectile.ai[1] += 1f;
            float lifetime = Projectile.ai[1];

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver4;

            float pulseTime = (float)(Main.GlobalTimeWrappedHourly * ScalePulseFrequency + Projectile.identity * 0.23f);
            float pulse = (float)(Math.Sin(pulseTime) * 0.5 + 0.5);
            Projectile.scale = BaseScale + ScalePulseAmplitude * pulse;

            if (Projectile.localAI[1] < 20f)
            {
                Projectile.localAI[1]++;
                Projectile.velocity *= 1.02f;
            }
            else
            {
                Projectile.velocity *= 0.995f;
            }

            Lighting.AddLight(Projectile.Center, new Vector3(0.4f, 0.3f, 0.85f));

            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.rand.NextBool(OrbitalDustRate))
                {
                    SpawnOrbitalDust(pulse);
                }

                if (Main.rand.NextBool(CometDustRate))
                {
                    SpawnCometDust(pulse);
                }

                if (Main.rand.NextBool(RadiantDustRate))
                {
                    SpawnRadiantDust(pulse);
                }
            }

            if (Projectile.owner == Main.myPlayer)
            {
                if (IsStealthStrike)
                {
                    if (lifetime <= 48f && lifetime % StealthShardInterval == 0f)
                    {
                        SpawnTrailingShard(StealthShardDamageFactor, StealthShardKnockbackFactor);
                        SpawnTrailingShard(StealthShardDamageFactor, StealthShardKnockbackFactor, MathHelper.Pi / 12f);
                    }

                    if (lifetime <= 48f && lifetime % 6f == 0f)
                    {
                        Vector2 shardVelocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.35f, 0.35f)) * 0.85f;

                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            shardVelocity,
                            ModContent.ProjectileType<LittleSpongeShardProjectile>(),
                            (int)(Projectile.damage * 0.5f),
                            Projectile.knockBack * 0.4f,
                            Projectile.owner);
                    }
                }
                else if (lifetime % NonStealthShardInterval == 0f)
                {
                    SpawnTrailingShard(NonStealthShardDamageFactor, NonStealthShardKnockbackFactor);
                }
            }

            if (Main.netMode != NetmodeID.Server && Projectile.numUpdates == 0)
            {
                int lifetimeFrames = (int)Projectile.ai[1];
                if (lifetimeFrames > 12 && lifetimeFrames % RadiantBurstInterval == 0)
                {
                    SpawnRadiantBurst(pulse);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Ichor, 240);
            target.AddBuff(BuffID.Frostburn2, 240);

            if (Projectile.localAI[2] == 0f)
            {
                Projectile.localAI[2] = 1f;
                SpawnShards(target.Center);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.localAI[2] == 0f)
            {
                SpawnShards(Projectile.Center);
            }

            for (int i = 0; i < 18; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemEmerald);
                dust.velocity = Main.rand.NextVector2Circular(7f, 7f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.15f, 1.6f);
                dust.fadeIn = 1.05f;
            }

            if (Main.netMode != NetmodeID.Server)
            {
                SpawnRadiantBurst(1f);
            }

            SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
        }

        private void SpawnShards(Vector2 origin)
        {
            if (Projectile.owner != Main.myPlayer)
            {
                return;
            }

            int shardCount = IsStealthStrike ? 14 : 10;
            float shardSpeed = IsStealthStrike ? 14.5f : 12f;
            float shardDamageMultiplier = IsStealthStrike ? 0.8f : 0.65f;
            float shardKnockbackMultiplier = IsStealthStrike ? 0.7f : 0.55f;

            for (int i = 0; i < shardCount; i++)
            {
                float angle = MathHelper.TwoPi / shardCount * i + MathHelper.PiOver4;
                Vector2 velocity = angle.ToRotationVector2() * shardSpeed;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    origin,
                    velocity,
                    ModContent.ProjectileType<LittleSpongeShardProjectile>(),
                    (int)(Projectile.damage * shardDamageMultiplier),
                    Projectile.knockBack * shardKnockbackMultiplier,
                    Projectile.owner);
            }
        }

        private void SpawnTrailingShard(float damageFactor, float knockbackFactor, float angleOffset = 0f)
        {
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(angleOffset);
            Vector2 spawnVelocity = direction * Projectile.velocity.Length() * 0.85f;
            int damage = (int)(Projectile.damage * damageFactor);
            float knockback = Projectile.knockBack * knockbackFactor;

            int shardIndex = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                spawnVelocity,
                ModContent.ProjectileType<LittleSpongeShardProjectile>(),
                damage,
                knockback,
                Projectile.owner);

            if (shardIndex >= 0 && shardIndex < Main.maxProjectiles)
            {
                Main.projectile[shardIndex].DamageType = ResolveDamageClass();
            }
        }

        private void SpawnOrbitalDust(float pulse)
        {
            float orbitRadius = MathHelper.Lerp(14f, 32f, pulse);
            float spin = (float)(Main.GlobalTimeWrappedHourly * 6.2f + Projectile.identity * 0.41f);
            Vector2 offset = new Vector2(0f, orbitRadius).RotatedBy(spin);
            Vector2 spawnPosition = Projectile.Center + offset;
            Vector2 velocity = offset.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(2f, 4.5f);

            Dust dust = Dust.NewDustPerfect(spawnPosition, DustID.IceTorch, velocity);
            dust.noGravity = true;
            dust.scale = MathHelper.Lerp(0.9f, 1.25f, pulse);
            dust.fadeIn = 1.05f;
        }

        private void SpawnCometDust(float pulse)
        {
            Vector2 trailDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 lateral = trailDirection.RotatedBy(MathHelper.PiOver2);
            Vector2 spawnPosition = Projectile.Center - trailDirection * Main.rand.NextFloat(16f, 34f) + lateral * Main.rand.NextFloat(-18f, 18f);
            Vector2 velocity = -trailDirection * Main.rand.NextFloat(2f, 5f);

            int dustType = Main.rand.NextBool() ? DustID.BlueFairy : DustID.RainbowMk2;
            Dust comet = Dust.NewDustPerfect(spawnPosition, dustType, velocity);
            comet.noGravity = true;
            comet.scale = MathHelper.Lerp(1.05f, 1.45f, pulse);
            comet.fadeIn = 1.2f;
            comet.alpha = 60;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Color baseColor = new Color(179, 235, 255);
            Color highlightColor = new Color(100, 180, 255);
            Vector2 center = Projectile.Center - Main.screenPosition;

            Texture2D bloom = TextureAssets.Extra[91].Value;
            float bloomRotation = Main.GlobalTimeWrappedHourly * 2.3f + Projectile.identity * 0.1f;
            Color bloomColor = new Color(120, 210, 255) * 0.35f;
            Color bloomOuterColor = new Color(80, 140, 255) * 0.25f;

            Main.EntitySpriteDraw(bloom, center, null, bloomColor, bloomRotation, bloom.Size() * 0.5f, Projectile.scale * 1.9f, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(bloom, center, null, bloomOuterColor, -bloomRotation * 1.4f, bloom.Size() * 0.5f, Projectile.scale * 1.4f, SpriteEffects.None, 0f);

            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float progress = i / (float)Projectile.oldPos.Length;
                float scale = Projectile.scale * MathHelper.Lerp(0.55f, 0.9f, 1f - progress);
                Color trailColor = baseColor * MathHelper.Lerp(0.05f, 0.35f, 1f - progress);
                trailColor.A = 0;

                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            }

            Color pulseHighlight = new Color(160, 225, 255) * 0.8f;
            Main.EntitySpriteDraw(texture, center, null, highlightColor * 0.75f, Projectile.rotation, origin, Projectile.scale * 1.48f, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(texture, center, null, pulseHighlight, Projectile.rotation, origin, Projectile.scale * 1.18f, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(texture, center, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        private void SpawnRadiantDust(float pulse)
        {
            float radius = MathHelper.Lerp(16f, 42f, pulse);
            Vector2 offset = Main.rand.NextVector2CircularEdge(radius, radius);
            Vector2 spawnPosition = Projectile.Center + offset;
            Vector2 velocity = -offset.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(2.4f, 4.6f);

            int dustType = Main.rand.NextBool(3) ? DustID.Enchanted_Pink : DustID.BlueCrystalShard;
            Dust dust = Dust.NewDustPerfect(spawnPosition, dustType, velocity);
            dust.noGravity = true;
            dust.scale = Main.rand.NextFloat(1.05f, 1.45f);
            dust.fadeIn = 1.15f;
            dust.alpha = 40;
        }

        private void SpawnRadiantBurst(float pulse)
        {
            const int burstCount = 6;
            float baseRotation = Main.rand.NextFloat(MathHelper.TwoPi);

            for (int i = 0; i < burstCount; i++)
            {
                float angle = baseRotation + MathHelper.TwoPi * i / burstCount;
                Vector2 velocity = angle.ToRotationVector2() * RadiantBurstSpeed;
                Vector2 spawn = Projectile.Center + velocity.SafeNormalize(Vector2.UnitY) * RadiantBurstScale;

                Dust dust = Dust.NewDustPerfect(spawn, DustID.WhiteTorch, velocity * 0.45f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.35f, 1.75f) * MathHelper.Lerp(0.9f, 1.15f, pulse);
                dust.fadeIn = 1.25f;
                dust.alpha = 30;
            }
        }
    }
}
