using LackOfNameStuff.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace LackOfNameStuff.Projectiles
{
    public class AstralMeldComet : ModProjectile
    {
        private const float HomingRange = 720f;
        private const float HomingStrength = 0.18f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = ResolveRogueDamage();
            Projectile.timeLeft = 240;
            Projectile.extraUpdates = 1;
            Projectile.scale = 0.75f;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.localAI[0] = 1f;
            }

            NPC target = FindNearestTarget();
            if (target != null)
            {
                Vector2 desiredVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 16f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, HomingStrength);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            if (++Projectile.localAI[1] % 6f == 0f)
            {
                CreateDustTrail(0.9f);
            }

            if (Projectile.timeLeft < 45)
            {
                Projectile.alpha = (int)MathHelper.Clamp(255 * (1f - Projectile.timeLeft / 45f), 0f, 255f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.CursedInferno, 120);
            SpawnImpactDust();
        }

        public override void OnKill(int timeLeft)
        {
            SpawnImpactDust();
            SoundEngine.PlaySound(SoundID.Item89.WithVolumeScale(0.5f), Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawCenter = Projectile.Center - Main.screenPosition;
            float rotation = Projectile.rotation;

            Color headGlow = new Color(170, 240, 255) * 0.7f;
            Color tailStart = new Color(40, 215, 255);
            Color tailEnd = new Color(255, 96, 188);

            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float progress = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
                float scale = Projectile.scale * MathHelper.Lerp(0.95f, 0.35f, progress);
                Color trailColor = Color.Lerp(tailStart, tailEnd, progress) * (1f - progress) * 0.9f;
                trailColor.A = 0;

                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, rotation, origin, scale, SpriteEffects.None, 0f);
            }

            Texture2D bloom = TextureAssets.Extra[91].Value;
            float bloomRotation = Main.GlobalTimeWrappedHourly * 4f;
            Color bloomColor = new Color(90, 180, 255) * 0.45f;
            Color bloomColorOuter = new Color(255, 120, 210) * 0.25f;
            Main.EntitySpriteDraw(bloom, drawCenter, null, bloomColor, bloomRotation, bloom.Size() * 0.5f, Projectile.scale * 0.95f, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(bloom, drawCenter, null, bloomColorOuter, -bloomRotation * 0.6f, bloom.Size() * 0.5f, Projectile.scale * 1.35f, SpriteEffects.None, 0f);

            Main.EntitySpriteDraw(texture, drawCenter, null, headGlow, rotation, origin, Projectile.scale * 1.08f, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(texture, drawCenter, null, Color.White, rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        private NPC FindNearestTarget()
        {
            NPC closest = null;
            float closestDistance = HomingRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                {
                    continue;
                }

                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (distance < closestDistance && (npc.CanBeChasedBy(this) || npc.whoAmI == (int)Projectile.ai[1]))
                {
                    closest = npc;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        private void CreateDustTrail(float scale)
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Vortex, Projectile.velocity * -0.2f, 100, default, scale);
            dust.noGravity = true;
        }

        private void SpawnImpactDust()
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 velocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(2.5f, 5.2f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, velocity, 150, default, 1.2f);
                dust.noGravity = true;
            }
        }

        private static DamageClass ResolveRogueDamage()
        {
            if (CalamityIntegration.CalamityLoaded &&
                CalamityIntegration.CalamityMod.TryFind("RogueDamageClass", out DamageClass rogueDamage))
            {
                return rogueDamage;
            }

            return DamageClass.Throwing;
        }
    }
}
