using LackOfNameStuff.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace LackOfNameStuff.Projectiles
{
    public class BlackShardRift : ModProjectile
    {
        private const int Lifetime = 42;
        private const float StartRadius = 26f;
        private const float EndRadius = 92f;
        private const int FrameCount = 4;
        private const int FrameTime = 6;

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
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            Main.projFrames[Projectile.type] = FrameCount;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 42;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.hide = true;
            Projectile.DamageType = ResolveDamageClass();
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.3f, PitchVariance = 0.25f }, Projectile.Center);
        }

        public override void AI()
        {
            float progress = 1f - Projectile.timeLeft / (float)Lifetime;
            float radius = MathHelper.Lerp(StartRadius, EndRadius, progress);
            Projectile.localAI[0] = radius;

            Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.05f, 0.25f));

            if (Main.rand.NextBool(2))
            {
                SpawnDustRing(radius, progress);
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= FrameTime)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % FrameCount;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float radius = Projectile.localAI[0];
            Vector2 closest = targetHitbox.ClosestPointInRect(Projectile.Center);
            return Vector2.DistanceSquared(closest, Projectile.Center) <= radius * radius;
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            float radius = Projectile.localAI[0];
            hitbox = Utils.CenteredRectangle(Projectile.Center, new Vector2(radius * 2f));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("LackOfNameStuff/Projectiles/BlackShardRift").Value;
            Rectangle frame = texture.Frame(1, FrameCount, 0, Projectile.frame);
            float radius = Projectile.localAI[0];
            float scale = radius / 21f;
            Color color = new Color(150, 40, 210) * 0.35f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(texture, drawPosition, frame, color, 0f, frame.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            return false;
        }

        private void SpawnDustRing(float radius, float progress)
        {
            const int petals = 10;
            for (int i = 0; i < petals; i++)
            {
                float angle = MathHelper.TwoPi * i / petals + Main.rand.NextFloat(-0.12f, 0.12f);
                Vector2 offset = angle.ToRotationVector2() * radius;
                Vector2 spawn = Projectile.Center + offset;
                Vector2 velocity = -offset.SafeNormalize(Vector2.UnitY) * MathHelper.Lerp(6f, 2f, progress);

                int dustType = Main.rand.NextBool() ? DustID.Shadowflame : DustID.PurpleTorch;
                Dust dust = Dust.NewDustPerfect(spawn, dustType, velocity);
                dust.noGravity = true;
                dust.scale = MathHelper.Lerp(1.5f, 0.95f, progress);
                dust.fadeIn = 1.1f;
                dust.alpha = 70;
            }
        }
    }
}
