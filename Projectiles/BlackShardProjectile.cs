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
    private const int TrailLength = 22;
    private const float BaseScale = 1.35f;
    private const float PulseAmplitude = 0.3f;
    private const float PulseFrequency = 5.1f;
    private const float AuraDustInterval = 6f;
    private const float RiftDustInterval = 9f;
    private const float RiftRadius = 24f;
    private const float HomingTrailLerp = 0.4f;
    private const float NonHomingTrailLerp = 0.22f;
    private const float HitboxRadius = 48f;

        private ref float AuraCounter => ref Projectile.localAI[0];
        private ref float RiftCounter => ref Projectile.localAI[1];
        private ref float SpawnedShards => ref Projectile.localAI[2];

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
            Projectile.width = 72;
            Projectile.height = 72;
            Projectile.penetrate = 5;
            Projectile.extraUpdates = 1;
            Projectile.DamageType = ResolveDamageClass();
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.aiStyle = 0;
            AIType = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, new Vector3(0.35f, 0.05f, 0.15f));

            if (SpawnedShards == 0f)
            {
                SpawnedShards = 1f;
                SoundEngine.PlaySound(SoundID.Item60 with { Volume = 0.8f, PitchVariance = 0.2f }, Projectile.Center);
            }

            float pulseTime = (float)(Main.GlobalTimeWrappedHourly * PulseFrequency + Projectile.identity * 0.17f);
            float pulseFactor = (float)(Math.Sin(pulseTime) * 0.5 + 0.5);
            Projectile.scale = BaseScale + PulseAmplitude * pulseFactor;

            if (Projectile.ai[1] >= 0.5f)
            {
                ApplyHomingBehavior();
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
                dust.velocity = Projectile.velocity * 0.18f;
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.05f, 1.4f);
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

            Color inner = new Color(70, 10, 115);
            Color middle = new Color(150, 40, 210);
            Color outer = new Color(255, 110, 220);

            float lerpFactor = Projectile.ai[1] >= 0.5f ? HomingTrailLerp : NonHomingTrailLerp;

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
                float scale = Projectile.scale * MathHelper.Lerp(0.45f, 0.9f, 1f - weightedProgress);
                Color trailColor = Color.Lerp(inner, outer, weightedProgress) * MathHelper.Lerp(0.1f, 0.65f, 1f - weightedProgress);
                trailColor.A = 0;

                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.rotation, origin, scale, effects, 0);
            }

            Vector2 center = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(texture, center, null, outer * 0.7f, Projectile.rotation, origin, Projectile.scale * 1.4f, effects, 0);
            Main.EntitySpriteDraw(texture, center, null, middle, Projectile.rotation, origin, Projectile.scale * 1.1f, effects, 0);
            Main.EntitySpriteDraw(texture, center, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

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
            SpawnShadowRift(target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            Vector2 orth = forward.RotatedBy(MathHelper.PiOver2);

            SpawnShadowRift(Projectile.Center + forward * 22f);
            SpawnShadowRift(Projectile.Center - forward * 22f);
            SpawnShadowRift(Projectile.Center + orth * 18f);
        }

        private void SpawnShadowRift(Vector2 position)
        {
            if (Projectile.owner != Main.myPlayer)
            {
                return;
            }

            int projType = ModContent.ProjectileType<BlackShardRift>();
            int damage = (int)(Projectile.damage * 0.55f);
            float knockback = Projectile.knockBack * 0.6f;

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero, projType, damage, knockback, Projectile.owner, Projectile.ai[1]);
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
