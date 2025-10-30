using LackOfNameStuff.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace LackOfNameStuff.Projectiles
{
    public class AstralMeldSatellite : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = ResolveRogueDamage();
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.netImportant = true;
            Projectile.scale = 0.9f;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            var meldPlayer = owner.GetModPlayer<Players.MeldPlayer>();
            if (!meldPlayer.meldSetActive)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            float offsetAngle = Projectile.ai[0];
            float orbitRadius = Projectile.ai[1] > 0f ? Projectile.ai[1] : 72f;
            float rotationSpeed = 0.03f;
            float angle = offsetAngle + Main.GameUpdateCount * rotationSpeed;

            Vector2 desiredPosition = owner.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * orbitRadius;
            Projectile.Center = Vector2.Lerp(Projectile.Center, desiredPosition, 0.35f);
            Projectile.velocity = Vector2.Zero;

            Projectile.rotation += 0.25f;
            Lighting.AddLight(Projectile.Center, 0.2f, 0.35f, 0.6f);
            AttemptRangedStrike(owner, meldPlayer);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawCenter = Projectile.Center - Main.screenPosition;

            Texture2D bloom = TextureAssets.Extra[91].Value;
            float bloomRotation = Main.GlobalTimeWrappedHourly * 3.2f + Projectile.identity * 0.11f;
            Color bloomInner = new Color(70, 180, 255) * 0.4f;
            Color bloomOuter = new Color(255, 110, 220) * 0.25f;

            Main.EntitySpriteDraw(bloom, drawCenter, null, bloomInner, bloomRotation, bloom.Size() * 0.5f, Projectile.scale * 0.8f, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(bloom, drawCenter, null, bloomOuter, -bloomRotation * 0.7f, bloom.Size() * 0.5f, Projectile.scale * 1.15f, SpriteEffects.None, 0f);

            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float progress = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
                float scale = Projectile.scale * MathHelper.Lerp(0.9f, 0.55f, progress);
                Color trailColor = Color.Lerp(new Color(50, 220, 255), new Color(255, 120, 210), progress) * (1f - progress) * 0.6f;
                trailColor.A = 0;

                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            }

            Color mainColor = new Color(190, 245, 255) * 0.85f;
            Main.EntitySpriteDraw(texture, drawCenter, null, mainColor, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(texture, drawCenter, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        private void AttemptRangedStrike(Player owner, Players.MeldPlayer meldPlayer)
        {
            if (Main.myPlayer != owner.whoAmI)
            {
                return;
            }

            Projectile.ai[2] += 1f;
            if (Projectile.ai[2] < 60f)
            {
                return;
            }

            Projectile.ai[2] = 0f;
            NPC target = AcquireTarget();
            if (target == null)
            {
                return;
            }

            DamageClass rogueClass = ResolveRogueDamage();
            int damage = (int)Math.Round(owner.GetTotalDamage(rogueClass).ApplyTo(90f));
            float knockback = owner.GetKnockback(rogueClass).ApplyTo(2f);
            Vector2 velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 12f;

            Projectile.NewProjectile(
                owner.GetSource_Misc("MeldSatellite"),
                Projectile.Center,
                velocity,
                ModContent.ProjectileType<AstralMeldComet>(),
                damage,
                knockback,
                owner.whoAmI,
                0f,
                target.whoAmI);
        }

        private NPC AcquireTarget()
        {
            NPC best = null;
            float bestDistance = 600f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                {
                    continue;
                }

                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (distance < bestDistance && npc.CanBeChasedBy(this))
                {
                    best = npc;
                    bestDistance = distance;
                }
            }

            return best;
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
