using LackOfNameStuff.Common;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LackOfNameStuff.Projectiles
{
    public class HammerOfJusticeUltimateShockwave : ModProjectile
    {
        private const int Lifetime = 24;
        private const float StartRadius = 48f;
        private const float EndRadius = 420f;
    private const float TrailRadiusScale = 0.65f;

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
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = ResolveDamageClass();
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 255;
            Projectile.hide = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            bool isTrail = Projectile.ai[0] == 1f;
            float radiusScale = isTrail ? TrailRadiusScale : 1f;
            float progress = 1f - Projectile.timeLeft / (float)Lifetime;
            float radius = MathHelper.Lerp(StartRadius * radiusScale, EndRadius * radiusScale, progress);
            Projectile.localAI[0] = radius;

            Lighting.AddLight(Projectile.Center, new Vector3(1.25f, 1.05f, 0.35f) * 0.6f);

            if (Main.netMode != NetmodeID.Server)
            {
                SpawnGoldenRing(radius, progress, isTrail);
            }
        }

        private void SpawnGoldenRing(float radius, float progress, bool isTrail)
        {
            int ringDust = isTrail ? 12 : 18;
            float wobble = MathHelper.Lerp(isTrail ? 0.15f : 0.2f, 0.05f, progress);
            float speedBase = MathHelper.Lerp(isTrail ? 10f : 12f, isTrail ? 5f : 6f, progress);

            for (int i = 0; i < ringDust; i++)
            {
                float angle = MathHelper.TwoPi * (i / (float)ringDust) + Main.rand.NextFloat(-wobble, wobble);
                Vector2 offset = angle.ToRotationVector2() * radius;
                Vector2 position = Projectile.Center + offset;
                Vector2 velocity = offset.SafeNormalize(Vector2.Zero) * speedBase;

                Dust dust = Dust.NewDustPerfect(position, DustID.GoldFlame, velocity);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.1f, 1.6f);
                dust.fadeIn = 1.1f;
                dust.alpha = 60;
            }
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            float radius = Projectile.localAI[0];
            hitbox = Utils.CenteredRectangle(Projectile.Center, new Vector2(radius * 2f));
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float radius = Projectile.localAI[0];

            float nearestX = MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right);
            float nearestY = MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom);
            Vector2 nearestPoint = new Vector2(nearestX, nearestY);

            return Vector2.DistanceSquared(Projectile.Center, nearestPoint) <= radius * radius;
        }

        public override bool PreDraw(ref Color lightColor) => false;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Ichor, 210);
        }
    }
}
