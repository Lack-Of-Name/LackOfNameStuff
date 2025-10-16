using LackOfNameStuff.Common;
using LackOfNameStuff.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace LackOfNameStuff.Projectiles
{
    public class HammerOfJusticeDashProjectile : ModProjectile
    {
        private bool IsUltimate => Projectile.ai[0] >= 0.5f;

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
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 22;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.DamageType = ResolveDamageClass();
            Projectile.hide = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = player.Center;
            Projectile.velocity = player.velocity;
            Projectile.DamageType = ResolveDamageClass();

            HammerOfJusticePlayer hammerPlayer = player.GetModPlayer<HammerOfJusticePlayer>();
            if (hammerPlayer.DashActiveTimer <= 0)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = System.Math.Max(Projectile.timeLeft, hammerPlayer.DashActiveTimer + 6);

            if (IsUltimate)
            {
                Projectile.scale = 1.3f;
                Projectile.localNPCHitCooldown = 6;
            }

            int dustChance = IsUltimate ? 1 : 4;
            if (Main.rand.NextBool(dustChance))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, IsUltimate ? DustID.GoldFlame : DustID.Enchanted_Gold);
                dust.velocity = dust.velocity * 0.3f + player.velocity * 0.8f;
                dust.noGravity = true;
                dust.scale = IsUltimate ? 1.6f : 1.2f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Confused, 120);
            if (IsUltimate)
            {
                target.AddBuff(BuffID.Ichor, 240);
            }

            Player player = Main.player[Projectile.owner];
            if (player.active)
            {
                player.GetModPlayer<HammerOfJusticePlayer>().OnDashImpact(target, IsUltimate);
            }
        }
    }
}
