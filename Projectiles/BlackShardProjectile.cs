using System;
using LackOfNameStuff.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Projectiles;

namespace LackOfNameStuff.Projectiles
{
    public class BlackShardProjectile : ModProjectile
    {
        private const int TrailLength = 28;
        private const float BaseScale = 1.45f;
        private const float PulseAmplitude = 0.34f;
        private const float PulseFrequency = 5.4f;
        private const float AuraDustInterval = 5f;
        private const float RiftDustInterval = 7f;
        private const float RiftRadius = 26f;
        private const float HomingTrailLerp = 0.46f;
        private const float NonHomingTrailLerp = 0.26f;
        private const float HitboxRadius = 56f;
        private const int RiftSpawnInterval = 18;
        private const float RiftDamageFactor = 0.68f;
        private const float RiftKnockbackFactor = 0.65f;
        private const int ImpactRiftCount = 2;
        private const float ImpactRiftOffset = 32f;
        private const int ImpactDustCount = 20;

        private ref float AuraCounter => ref Projectile.localAI[0];
        private ref float RiftCounter => ref Projectile.localAI[1];
        private ref float InternalTimer => ref Projectile.localAI[2];

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
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.TerraBeam);
            Projectile.width = 76;
            Projectile.height = 76;
            Projectile.penetrate = 6;
            Projectile.extraUpdates = 2;
            Projectile.DamageType = ResolveDamageClass();
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.aiStyle = 0;
            AIType = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ArmorPenetration = 28;
            Projectile.CritChance = 12;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, new Vector3(0.45f, 0.1f, 0.38f));

            if (InternalTimer == 0f)
            {
                InternalTimer = 1f;
                SoundEngine.PlaySound(SoundID.Item60 with { Volume = 0.9f, PitchVariance = 0.18f }, Projectile.Center);
            }
            else if (Projectile.numUpdates == 0)
            {
                InternalTimer++;
            }

            float pulseTime = (float)(Main.GlobalTimeWrappedHourly * PulseFrequency + Projectile.identity * 0.17f);
            float pulseFactor = (float)(Math.Sin(pulseTime) * 0.5 + 0.5);
            Projectile.scale = BaseScale + PulseAmplitude * pulseFactor;

            if (Projectile.ai[1] >= 0.5f)
            {
                ApplyHomingBehavior();
            }

            if (Projectile.owner == Main.myPlayer && Projectile.numUpdates == 0)
            {
                int timerFrames = (int)InternalTimer;
                if (timerFrames > RiftSpawnInterval && timerFrames % RiftSpawnInterval == 0)
                {
                    Vector2 retreat = Projectile.velocity.SafeNormalize(Vector2.UnitY) * -28f;
                    SpawnShadowRift(Projectile.Center + retreat, RiftDamageFactor * 0.7f, RiftKnockbackFactor * 0.8f);
                }
            }

            AuraCounter++;
            RiftCounter++;

            if (Main.netMode != NetmodeID.Server)
            {
                if (AuraCounter >= AuraDustInterval)
                {
                    AuraCounter = 0f;
                    SpawnAuraDust(pulseFactor);
                }

                if (RiftCounter >= RiftDustInterval)
                {
                    RiftCounter = 0f;
                    SpawnRiftDust();
                }
            }

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);
                dust.velocity = Projectile.velocity * 0.22f + Main.rand.NextVector2Circular(1.8f, 1.8f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.1f, 1.55f);
                dust.fadeIn = 1.05f;
            }
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox = Utils.CenteredRectangle(Projectile.Center, new Vector2(HitboxRadius * 2f));
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 closest = targetHitbox.ClosestPointInRect(Projectile.Center);
            return Vector2.DistanceSquared(closest, Projectile.Center) <= HitboxRadius * HitboxRadius;
        }

        private void ApplyHomingBehavior()
        {
            const float maxSeekDistance = 800f;
            const float homingStrength = 0.12f;
            const float desiredSpeed = 18f;

            NPC target = FindHomingTarget(maxSeekDistance);
            if (target == null)
            {
                return;
            }

            Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * desiredSpeed;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, homingStrength);
        }

        private NPC FindHomingTarget(float maxDistance)
        {
            NPC bestTarget = null;
            float squaredMaxDistance = maxDistance * maxDistance;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(this))
                {
                    continue;
                }

                float squaredDistance = Vector2.DistanceSquared(npc.Center, Projectile.Center);
                if (squaredDistance > squaredMaxDistance)
                {
                    continue;
                }

                if (!Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1))
                {
                    continue;
                }

                if (bestTarget == null || squaredDistance < Vector2.DistanceSquared(bestTarget.Center, Projectile.Center))
                {
                    bestTarget = npc;
                }
            }

            return bestTarget;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            SpriteEffects effects = Projectile.ai[0] >= 0.5f ? SpriteEffects.FlipVertically : SpriteEffects.None;

            Color inner = new Color(60, 0, 105);
            Color middle = new Color(150, 35, 215);
            Color outer = new Color(255, 130, 250);

            float lerpFactor = Projectile.ai[1] >= 0.5f ? HomingTrailLerp : NonHomingTrailLerp;

            Texture2D bloom = TextureAssets.Extra[91].Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            float bloomRotation = Main.GlobalTimeWrappedHourly * 3.1f + Projectile.identity * 0.13f;
            Color bloomColor = new Color(175, 60, 255) * 0.3f;
            Color bloomSecondary = new Color(90, 25, 180) * 0.4f;

            Main.EntitySpriteDraw(bloom, center, null, bloomSecondary, bloomRotation, bloom.Size() * 0.5f, Projectile.scale * 1.7f, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(bloom, center, null, bloomColor, -bloomRotation * 0.6f, bloom.Size() * 0.5f, Projectile.scale * 1.25f, SpriteEffects.None, 0f);

            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                Vector2 oldPos = Projectile.oldPos[i];
                if (oldPos == Vector2.Zero)
                {
                    continue;
                }

                float progress = i / (float)Projectile.oldPos.Length;
                float weightedProgress = MathHelper.Lerp(progress, 1f, lerpFactor);
                Vector2 drawPos = oldPos + Projectile.Size * 0.5f - Main.screenPosition;
                float scale = Projectile.scale * MathHelper.Lerp(0.48f, 0.92f, 1f - weightedProgress);
                Color trailColor = Color.Lerp(inner, outer, weightedProgress) * MathHelper.Lerp(0.12f, 0.7f, 1f - weightedProgress);
                trailColor.A = 0;

                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.rotation, origin, scale, effects, 0f);
            }

            Main.EntitySpriteDraw(texture, center, null, outer * 0.75f, Projectile.rotation, origin, Projectile.scale * 1.45f, effects, 0f);
            Main.EntitySpriteDraw(texture, center, null, middle, Projectile.rotation, origin, Projectile.scale * 1.12f, effects, 0f);
            Main.EntitySpriteDraw(texture, center, null, Color.White, Projectile.rotation, origin, Projectile.scale, effects, 0f);

            return false;
        }

        private void SpawnAuraDust(float pulse)
        {
            Vector2 normal = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            normal = normal.RotatedBy(MathHelper.PiOver2);
            float lateral = MathHelper.Lerp(-RiftRadius, RiftRadius, Main.rand.NextFloat());
            Vector2 offset = normal * lateral;
            Vector2 spawn = Projectile.Center + offset;
            Vector2 velocity = -offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1.2f, 3.6f);

            int dustType = Main.rand.NextBool() ? DustID.Shadowflame : DustID.DemonTorch;
            Dust dust = Dust.NewDustPerfect(spawn, dustType, velocity);
            dust.noGravity = true;
            dust.scale = MathHelper.Lerp(1f, 1.45f, pulse);
            dust.fadeIn = 1.05f;
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnShadowRift(target.Center, RiftDamageFactor * 1.05f, RiftKnockbackFactor);

            Vector2 travelDirection = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            for (int i = 0; i < ImpactRiftCount; i++)
            {
                float rotation = (i == 0 ? 1f : -1f) * MathHelper.PiOver2;
                Vector2 offset = travelDirection.RotatedBy(rotation) * ImpactRiftOffset;
                SpawnShadowRift(target.Center + offset, RiftDamageFactor * 0.85f, RiftKnockbackFactor);
            }

            SpawnImpactDust(target.Center);
            target.AddBuff(BuffID.ShadowFlame, 240);
            target.AddBuff(BuffID.CursedInferno, 180);
        }

        public override void OnKill(int timeLeft)
        {
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            Vector2 orth = forward.RotatedBy(MathHelper.PiOver2);

            SpawnShadowRift(Projectile.Center + forward * 28f, RiftDamageFactor, RiftKnockbackFactor);
            SpawnShadowRift(Projectile.Center - forward * 28f, RiftDamageFactor, RiftKnockbackFactor);
            SpawnShadowRift(Projectile.Center + orth * 24f, RiftDamageFactor * 0.9f, RiftKnockbackFactor);
            SpawnShadowRift(Projectile.Center - orth * 24f, RiftDamageFactor * 0.9f, RiftKnockbackFactor);

            SpawnImpactDust(Projectile.Center);
        }

        private void SpawnShadowRift(Vector2 position, float damageMultiplier = RiftDamageFactor, float knockbackMultiplier = RiftKnockbackFactor)
        {
            if (Projectile.owner != Main.myPlayer)
            {
                return;
            }

            int projType = ModContent.ProjectileType<BlackShardRift>();
            int damage = (int)(Projectile.damage * damageMultiplier);
            float knockback = Projectile.knockBack * knockbackMultiplier;

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero, projType, damage, knockback, Projectile.owner, Projectile.ai[1]);
        }

        private void SpawnImpactDust(Vector2 position)
        {
            for (int i = 0; i < ImpactDustCount; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(6.5f, 6.5f);
                Dust dust = Dust.NewDustPerfect(position, DustID.ShadowbeamStaff, velocity);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.1f, 1.7f);
                dust.fadeIn = 1.1f;
                dust.alpha = 80;
            }
        }
        private void SpawnRiftDust()
        {
            float angle = Projectile.rotation + MathHelper.PiOver2;
            Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(12f, 28f);
            Vector2 spawn = Projectile.Center + offset;
            Vector2 velocity = -offset.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(3.8f, 7.5f);

            Dust dust = Dust.NewDustPerfect(spawn, DustID.PurpleTorch, velocity);
            dust.noGravity = true;
            dust.scale = Main.rand.NextFloat(1.1f, 1.6f);
            dust.alpha = 80;
        }
    }
}
