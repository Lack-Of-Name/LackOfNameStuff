using LackOfNameStuff.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace LackOfNameStuff.Projectiles
{
    public class BlackShardProjectile : ModProjectile
    {
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
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.TerraBeam);
            Projectile.width = 56;
            Projectile.height = 56;
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
            Lighting.AddLight(Projectile.Center, new Vector3(0.25f, 0.05f, 0.05f));

            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                SoundEngine.PlaySound(SoundID.Item60 with { Volume = 0.8f, PitchVariance = 0.2f }, Projectile.Center);
            }

            if (Projectile.ai[1] >= 0.5f)
            {
                ApplyHomingBehavior();
            }

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CrimsonTorch, 0f, 0f, 40, Color.Red, 1.15f);
                dust.velocity = Projectile.velocity * 0.2f;
                dust.noGravity = true;
            }
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

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                effects,
                0);

            return false;
        }
    }
}
