using LackOfNameStuff.Common;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace LackOfNameStuff.Players
{
    public class MeldPlayer : ModPlayer
    {
        private const int SatelliteCount = 3;
        private const float SatelliteRadius = 96f;

        internal bool hasMeldCoif;
        internal bool hasMeldCarapace;
        internal bool hasMeldGreaves;
        internal bool meldSetActive;

        private int cosmicDustTimer;
        private int cosmicBurstCooldown;

        private bool stealthOverridesApplied;
        private float? cachedStealthRegen;
        private float? targetStealthRegen;

        public override void ResetEffects()
        {
            hasMeldCoif = false;
            hasMeldCarapace = false;
            hasMeldGreaves = false;
            meldSetActive = false;
        }

        public override void PostUpdateEquips()
        {
            if (meldSetActive)
            {
                EnsureStealthOverrides();
                ApplySetBonusEffects();
            }
            else
            {
                RestoreStealthOverrides();
            }
        }

        public override void PostUpdate()
        {
            if (cosmicBurstCooldown > 0)
            {
                cosmicBurstCooldown--;
            }
        }

        public override void OnRespawn()
        {
            RestoreStealthOverrides();
        }

        public override void PlayerDisconnect()
        {
            RestoreStealthOverrides();
        }

        private void ApplySetBonusEffects()
        {
            DamageClass rogueClass = ResolveRogueDamageClass();

            Player.GetDamage(rogueClass) += 0.12f;
            Player.GetCritChance(rogueClass) += 10f;
            Player.endurance += 0.08f;
            Player.moveSpeed += 0.12f;
            Player.lifeRegen += 3;

            if (hasMeldCoif)
            {
                Player.nightVision = true;
            }

            if (hasMeldCarapace)
            {
                Player.buffImmune[BuffID.OnFire] = true;
                Player.buffImmune[BuffID.OnFire3] = true;
            }

            if (hasMeldGreaves)
            {
                Player.jumpSpeedBoost += 0.4f;
                Player.runAcceleration *= 1.12f;
            }

            if (Player.statLife <= Player.statLifeMax2 / 2)
            {
                Player.endurance += 0.06f;
                Player.moveSpeed += 0.15f;
                Player.GetCritChance(rogueClass) += 6f;
                Player.AddBuff(BuffID.Shine, 2);
            }

            if (Player.whoAmI == Main.myPlayer)
            {
                MaintainSatellites(rogueClass);
            }

            EmitCosmicDust();
        }

        private void EnsureStealthOverrides()
        {
            if (!CalamityIntegration.CalamityLoaded)
            {
                return;
            }

            if (CalamityIntegration.TryAccessRogueStealthRegen(Player, out float currentRegen, out var setter))
            {
                float desired = currentRegen * 1.6f + 0.5f;

                if (!stealthOverridesApplied)
                {
                    cachedStealthRegen = currentRegen;
                    targetStealthRegen = desired;
                    setter(desired);
                    stealthOverridesApplied = true;
                }
                else if (targetStealthRegen.HasValue && Math.Abs(currentRegen - targetStealthRegen.Value) > 0.01f)
                {
                    setter(targetStealthRegen.Value);
                }
            }
        }

        private void RestoreStealthOverrides()
        {
            if (!stealthOverridesApplied)
            {
                return;
            }

            if (cachedStealthRegen.HasValue)
            {
                CalamityIntegration.TrySetRogueStealthRegen(Player, cachedStealthRegen.Value);
            }

            cachedStealthRegen = null;
            targetStealthRegen = null;
            stealthOverridesApplied = false;
        }

        private void MaintainSatellites(DamageClass rogueClass)
        {
            int projectileType = ModContent.ProjectileType<Projectiles.AstralMeldSatellite>();
            int owned = Player.ownedProjectileCounts[projectileType];

            if (owned >= SatelliteCount)
            {
                return;
            }

            for (int i = owned; i < SatelliteCount; i++)
            {
                float offset = MathHelper.TwoPi * i / SatelliteCount;
                int damage = (int)Math.Round(Player.GetTotalDamage(rogueClass).ApplyTo(120f));
                float knockback = Player.GetKnockback(rogueClass).ApplyTo(2.5f);

                int projIndex = Projectile.NewProjectile(
                    Player.GetSource_Misc("MeldSet"),
                    Player.Center,
                    Vector2.Zero,
                    projectileType,
                    damage,
                    knockback,
                    Player.whoAmI,
                    offset,
                    SatelliteRadius);

                if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                {
                    Projectile proj = Main.projectile[projIndex];
                    Vector2 spawnOffset = offset.ToRotationVector2() * SatelliteRadius;
                    proj.Center = Player.Center + spawnOffset;
                    proj.netUpdate = true;
                }
            }
        }

        private void EmitCosmicDust()
        {
            cosmicDustTimer++;
            if (cosmicDustTimer < 12)
            {
                return;
            }

            cosmicDustTimer = 0;
            Vector2 offset = Main.rand.NextVector2Circular(40f, 18f);
            int dustIndex = Dust.NewDust(Player.Center + offset, 0, 0, DustID.PortalBoltTrail, 0f, 0f, 150, default, 1.2f);
            if (dustIndex >= 0 && dustIndex < Main.maxDust)
            {
                Dust dust = Main.dust[dustIndex];
                dust.velocity *= 0.35f;
                dust.noGravity = true;
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TryTriggerCosmicBurst(target, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TryTriggerCosmicBurst(target, damageDone);
        }

        private void TryTriggerCosmicBurst(NPC target, int damageDone)
        {
            if (!meldSetActive || target == null || !target.active || target.friendly || target.type == NPCID.TargetDummy)
            {
                return;
            }

            if (damageDone <= 0 || Main.myPlayer != Player.whoAmI || cosmicBurstCooldown > 0)
            {
                return;
            }

            cosmicBurstCooldown = 45;

            Vector2 spawn = target.Center + new Vector2(Main.rand.NextFloat(-160f, 160f), -420f);
            Vector2 velocity = (target.Center - spawn).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(11f, 15f);

            DamageClass rogueClass = ResolveRogueDamageClass();
            int damage = (int)Math.Round(Player.GetTotalDamage(rogueClass).ApplyTo(160f));
            float knockback = Player.GetKnockback(rogueClass).ApplyTo(3f);

            Projectile.NewProjectile(
                Player.GetSource_OnHit(target, "MeldSet"),
                spawn,
                velocity,
                ModContent.ProjectileType<Projectiles.AstralMeldComet>(),
                damage,
                knockback,
                Player.whoAmI,
                0f,
                target.whoAmI);

            SoundEngine.PlaySound(SoundID.Item92.WithVolumeScale(0.6f), spawn);
        }

        private static DamageClass ResolveRogueDamageClass()
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
