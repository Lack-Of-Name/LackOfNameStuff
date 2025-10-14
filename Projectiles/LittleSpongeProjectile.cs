using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Common;

namespace LackOfNameStuff.Projectiles
{
    public class LittleSpongeProjectile : ModProjectile
    {
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
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 3;
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

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver4;

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

            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch);
                dust.velocity = Projectile.velocity * 0.2f;
                dust.noGravity = true;
                dust.scale = 1.1f;
            }

            if (IsStealthStrike && Projectile.owner == Main.myPlayer)
            {
                int timer = (int)(Projectile.ai[1] += 1f);

                if (timer <= 36 && timer % 6 == 0)
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
    }
}
