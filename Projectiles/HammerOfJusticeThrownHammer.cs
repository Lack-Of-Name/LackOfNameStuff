using LackOfNameStuff.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace LackOfNameStuff.Projectiles
{
    public class HammerOfJusticeThrownHammer : ModProjectile
    {
        private const int TrailCacheLength = 12;
        private const float SpinSpeed = 0.58f;
        private const float GlowScale = 1.25f;

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
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 44;
            Projectile.friendly = true;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 75;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = ResolveDamageClass();
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.extraUpdates = 2;
            Projectile.ArmorPenetration = 24;
        }

        public override void AI()
        {
            Projectile.direction = Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;
            Projectile.rotation += SpinSpeed * Projectile.direction;
            Projectile.velocity *= 0.985f;
            Projectile.velocity.Y += 0.12f;

            Lighting.AddLight(Projectile.Center, new Vector3(1.05f, 0.85f, 0.35f));

            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame);
                dust.velocity = Projectile.velocity * 0.24f + Main.rand.NextVector2Circular(1.6f, 1.6f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.2f, 1.55f);
                dust.fadeIn = 1.05f;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.penetrate--;
            if (Projectile.penetrate <= 0)
            {
                Projectile.Kill();
                return false;
            }

            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.7f;
            }

            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y * 0.7f;
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Daybreak, 180);
            target.AddBuff(BuffID.OnFire3, 180);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item89 with { Volume = 0.7f }, Projectile.Center);

            for (int i = 0; i < 16; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 10, 10, DustID.GoldFlame);
                dust.velocity = Main.rand.NextVector2Circular(6.5f, 6.5f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.25f, 1.7f);
                dust.fadeIn = 1.1f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("LackOfNameStuff/Projectiles/HammerOfJusticeThrownHammer").Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color color = new Color(255, 230, 140);

            Texture2D bloom = TextureAssets.Extra[91].Value;
            Color bloomColor = new Color(255, 215, 120) * 0.35f;
            Main.EntitySpriteDraw(bloom, drawPosition, null, bloomColor, 0f, bloom.Size() * 0.5f, Projectile.scale * 1.55f, SpriteEffects.None, 0f);

            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float progress = i / (float)Projectile.oldPos.Length;
                Color trailColor = color * MathHelper.Lerp(0.05f, 0.45f, 1f - progress);
                trailColor.A = 0;
                float scale = MathHelper.Lerp(0.85f, 0.45f, progress);
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            }

            Main.EntitySpriteDraw(texture, drawPosition, null, Color.White, Projectile.rotation, origin, 0.6f * GlowScale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
