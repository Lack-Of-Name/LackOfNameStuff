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
    private const int TrailCacheLength = 18;
    private const float BaseScale = 1.15f;
    private const float ScalePulseAmplitude = 0.35f;
    private const float ScalePulseFrequency = 5.4f;
    private const int OrbitalDustRate = 3;
    private const int CometDustRate = 4;
    private const int NonStealthShardInterval = 18;
    private const int StealthShardInterval = 8;
    private const float NonStealthShardDamageFactor = 0.4f;
    private const float NonStealthShardKnockbackFactor = 0.35f;
    private const float StealthShardDamageFactor = 0.55f;
    private const float StealthShardKnockbackFactor = 0.5f;

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
            Projectile.penetrate = 5;
            Projectile.DamageType = ResolveDamageClass();
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.extraUpdates = 1;
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

            Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.2f, 0.6f));

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
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Ichor, 180);

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

            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemEmerald);
                dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.1f, 1.5f);
            }

            SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
        }

        private void SpawnShards(Vector2 origin)
        {
            if (Projectile.owner != Main.myPlayer)
            {
                return;
            }

            int shardCount = IsStealthStrike ? 12 : 9;
            float shardSpeed = IsStealthStrike ? 13f : 11f;
            float shardDamageMultiplier = IsStealthStrike ? 0.75f : 0.6f;
            float shardKnockbackMultiplier = IsStealthStrike ? 0.65f : 0.5f;

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
            Vector2 spawnVelocity = direction * Projectile.velocity.Length() * 0.75f;
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

            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float progress = i / (float)Projectile.oldPos.Length;
                float scale = Projectile.scale * MathHelper.Lerp(0.55f, 0.9f, 1f - progress);
                Color trailColor = baseColor * MathHelper.Lerp(0.05f, 0.35f, 1f - progress);
                trailColor.A = 0;

                Main.spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            }

            Vector2 center = Projectile.Center - Main.screenPosition;
            Main.spriteBatch.Draw(texture, center, null, highlightColor * 0.6f, Projectile.rotation, origin, Projectile.scale * 1.35f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, center, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}
