using LackOfNameStuff.Buffs;
using LackOfNameStuff.Common;
using LackOfNameStuff.Projectiles;
using LackOfNameStuff.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace LackOfNameStuff.Players
{
    public class HammerOfJusticePlayer : ModPlayer
    {
        private const int DashCooldownFrames = 180;
        private const int DashDurationFrames = 14;
        private const float DashSpeed = 24f;
        private const float DashDamageMultiplier = 1.35f;
        private const float DashKnockbackBonus = 6.5f;
        private const int DashDamageLingeringFrames = 6;

        private const int UltimateDashDurationFrames = 32;
        private const int UltimateDashCooldownFrames = 240;
        private const float UltimateDashSpeedMultiplier = 1.7f;
        private const float UltimateDashDamageMultiplier = 3.1f;
        private const float UltimateDashKnockbackBonus = 14f;
        private const int UltimateImmunePadding = 36;
        private const int UltimateAftershockDust = 48;
        private const int UltimateShockwaveIntervalFrames = 20;

        private const int DashComboTimeoutFrames = 240;

        private const int ParryCooldownFrames = 150;
        private const int ParryChainCooldownFrames = 45;
        private const int ParryActiveFrames = 18;
        private const int ParryChainWindowFrames = 30;
        private const float ParryRadius = 160f;

        public bool HasHammerEquipped { get; set; }
        public int DashCooldownTimer { get; private set; }
        public int ParryCooldownTimer { get; private set; }
        public int DashActiveTimer { get; private set; }
        public int ParryActiveTimer { get; private set; }
        public int ParryChainWindowTimer { get; private set; }

        private Vector2 cachedDashDirection = Vector2.UnitX;
        private bool parrySuccessRegistered;
        private bool dashIsUltimate;
        private bool ultimateCrashTriggered;
        private float currentDashSpeed = DashSpeed;
        private float currentDashDamageMultiplier = DashDamageMultiplier;
        private float currentDashKnockbackBonus = DashKnockbackBonus;
        private int currentDashImmuneFrames = DashDurationFrames + 10;
        private int lastDashDuration;
        private int lastDashCooldown;
        private int dashChainCounter;
        private int dashComboTimer;
        private int currentDashLingerFrames;
        private int ultimateShockwaveTimer = -1;
        private bool ultimateHitRegistered;

        public override void ResetEffects()
        {
            HasHammerEquipped = false;
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (!HasHammerEquipped || Player.dead || Player.CCed)
            {
                return;
            }

            ChronosKeybindSystem keybindSystem = ModContent.GetInstance<ChronosKeybindSystem>();

            if (Player.whoAmI == Main.myPlayer && keybindSystem.HammerDashKey?.JustPressed == true)
            {
                Vector2 direction = Main.MouseWorld - Player.Center;
                AttemptDash(direction);
            }

            if (Player.whoAmI == Main.myPlayer && keybindSystem.HammerParryKey?.JustPressed == true)
            {
                AttemptParry();
            }
        }

        public override void PostUpdate()
        {
            if (!HasHammerEquipped)
            {
                dashChainCounter = 0;
                dashComboTimer = 0;
                ultimateShockwaveTimer = -1;
            }

            if (DashCooldownTimer > 0)
            {
                DashCooldownTimer--;
            }

            if (ParryCooldownTimer > 0)
            {
                ParryCooldownTimer--;
            }

            if (DashActiveTimer > 0)
            {
                MaintainDashMotion();
                DashActiveTimer--;
            }
            else if (dashIsUltimate && !ultimateCrashTriggered && lastDashDuration > 0)
            {
                ultimateCrashTriggered = true;
                CreateUltimateImpact();
                ultimateShockwaveTimer = -1;
            }

            if (ParryActiveTimer > 0)
            {
                MaintainParryState();
                ParryActiveTimer--;
            }

            if (ParryChainWindowTimer > 0)
            {
                ParryChainWindowTimer--;
            }

            if (dashComboTimer > 0 && DashActiveTimer <= 0)
            {
                dashComboTimer--;
                if (dashComboTimer <= 0)
                {
                    dashChainCounter = 0;
                }
            }
        }

        private void AttemptDash(Vector2 direction)
        {
            if (DashCooldownTimer > 0)
            {
                return;
            }

            if (direction.LengthSquared() <= 0.001f)
            {
                direction = Player.direction == 1 ? Vector2.UnitX : -Vector2.UnitX;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                HammerOfJusticeNetworkHandler.SendDashRequest(Mod, Player.whoAmI, direction);
            }
            else
            {
                ExecuteDash(direction, Main.netMode == NetmodeID.Server);
            }
        }

        public void ExecuteDash(Vector2 direction, bool syncToClients)
        {
            if (DashCooldownTimer > 0)
            {
                return;
            }

            bool isUltimate = TryConsumeUltimateDash();
            int dashDuration = isUltimate ? UltimateDashDurationFrames : DashDurationFrames;
            int cooldown = isUltimate ? UltimateDashCooldownFrames : DashCooldownFrames;

            PerformDash(direction, dashDuration, cooldown, spawnProjectile: Main.netMode != NetmodeID.MultiplayerClient, isUltimate);

            if (syncToClients && Main.netMode == NetmodeID.Server)
            {
                HammerOfJusticeNetworkHandler.SendDashSync(Mod, Player.whoAmI, cachedDashDirection, dashDuration, cooldown, isUltimate);
            }
        }

        public void ApplyDashFromNetwork(Vector2 direction, int dashDuration, int cooldown, bool isUltimate)
        {
            PerformDash(direction, dashDuration, cooldown, spawnProjectile: false, isUltimate);
        }

        private void PerformDash(Vector2 direction, int dashDuration, int cooldown, bool spawnProjectile, bool isUltimate)
        {
            if (direction.LengthSquared() <= 0.001f)
            {
                direction = Vector2.UnitX * Player.direction;
            }

            direction.Normalize();
            cachedDashDirection = direction;

            dashIsUltimate = isUltimate;
            ultimateCrashTriggered = false;
            ultimateHitRegistered = false;
            currentDashSpeed = DashSpeed * (dashIsUltimate ? UltimateDashSpeedMultiplier : 1f);
            currentDashDamageMultiplier = dashIsUltimate ? DashDamageMultiplier * UltimateDashDamageMultiplier : DashDamageMultiplier;
            currentDashKnockbackBonus = dashIsUltimate ? DashKnockbackBonus + UltimateDashKnockbackBonus : DashKnockbackBonus;
            currentDashImmuneFrames = dashIsUltimate ? dashDuration + UltimateImmunePadding : dashDuration + 10;
            lastDashDuration = dashDuration;
            lastDashCooldown = cooldown;
            dashComboTimer = DashComboTimeoutFrames;

            int lingerFrames = dashIsUltimate ? DashDamageLingeringFrames * 2 : DashDamageLingeringFrames;
            currentDashLingerFrames = lingerFrames;
            ultimateShockwaveTimer = dashIsUltimate ? 0 : -1;
            DashActiveTimer = dashDuration + lingerFrames;
            DashCooldownTimer = cooldown;
            ApplyCooldownBuff(ModContent.BuffType<HammerOfJusticeDashCooldown>(), DashCooldownTimer);

            Player.velocity = direction * currentDashSpeed;
            Player.immuneTime = System.Math.Max(Player.immuneTime, currentDashImmuneFrames);
            Player.immune = true;

            PlayDashEffects(dashIsUltimate);

            if (spawnProjectile)
            {
                int damage = GetDashDamage();
                float knockback = GetDashKnockback();
                SpawnDashProjectile(damage, knockback, dashIsUltimate);
            }
        }

        private void MaintainDashMotion()
        {
            Player.velocity = cachedDashDirection * currentDashSpeed;

            Player.immune = true;
            Player.immuneTime = System.Math.Max(Player.immuneTime, dashIsUltimate ? 12 : 6);

            if (dashIsUltimate && Main.netMode != NetmodeID.Server)
            {
                Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.GoldFlame);
                dust.velocity = cachedDashDirection.RotatedByRandom(0.5f) * Main.rand.NextFloat(10f, 16f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.4f, 1.9f);
            }

            if (dashIsUltimate && DashActiveTimer > currentDashLingerFrames && Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (ultimateShockwaveTimer <= 0)
                {
                    SpawnTrailingShockwave();
                    ultimateShockwaveTimer = UltimateShockwaveIntervalFrames;
                }
                else if (ultimateShockwaveTimer > 0)
                {
                    ultimateShockwaveTimer--;
                }
            }
        }

        private int GetDashDamage()
        {
            Item held = Player.HeldItem;
            if (held == null || held.IsAir)
            {
                return 0;
            }

            int baseDamage = Player.GetWeaponDamage(held);
            return (int)(baseDamage * currentDashDamageMultiplier);
        }

        private float GetDashKnockback()
        {
            Item held = Player.HeldItem;
            if (held == null || held.IsAir)
            {
                return currentDashKnockbackBonus;
            }

            float baseKnockback = Player.GetWeaponKnockback(held);
            return baseKnockback + currentDashKnockbackBonus;
        }

        private void SpawnDashProjectile(int damage, float knockback, bool isUltimate)
        {
            if (damage <= 0)
            {
                return;
            }

            IEntitySource source = Player.GetSource_Misc("HammerOfJusticeDash");
            int projType = ModContent.ProjectileType<HammerOfJusticeDashProjectile>();
            int projectileIndex = Projectile.NewProjectile(source, Player.Center, cachedDashDirection * currentDashSpeed, projType, damage, knockback, Player.whoAmI, isUltimate ? 1f : 0f);

            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
            {
                Projectile projectile = Main.projectile[projectileIndex];
                projectile.netUpdate = true;
            }
        }

        private void PlayDashEffects(bool isUltimate)
        {
            SoundEngine.PlaySound((isUltimate ? SoundID.Item117 : SoundID.Item74) with { Pitch = isUltimate ? -0.4f : -0.2f }, Player.Center);

            if (Main.netMode == NetmodeID.Server)
            {
                return;
            }

            int dustCount = isUltimate ? 32 : 14;
            for (int i = 0; i < dustCount; i++)
            {
                Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.GoldFlame);
                float speed = isUltimate ? Main.rand.NextFloat(10f, 18f) : Main.rand.NextFloat(8f, 12f);
                dust.velocity = cachedDashDirection.RotatedByRandom(isUltimate ? 0.55f : 0.4f) * speed;
                dust.noGravity = true;
                dust.scale = isUltimate ? Main.rand.NextFloat(1.5f, 2.0f) : Main.rand.NextFloat(1.1f, 1.4f);
            }

            Vector3 lightColor = isUltimate ? new Vector3(1.2f, 0.95f, 0.35f) : new Vector3(0.8f, 0.6f, 0.2f);
            Lighting.AddLight(Player.Center, lightColor);
        }

        private bool TryConsumeUltimateDash()
        {
            int interval = GetUltimateInterval();
            dashChainCounter = (dashChainCounter % interval) + 1;

            if (dashChainCounter >= interval)
            {
                dashChainCounter = 0;
                return true;
            }

            return false;
        }

        public void OnDashImpact(NPC target, bool fromUltimate)
        {
            if (!fromUltimate || ultimateHitRegistered)
            {
                return;
            }

            ultimateHitRegistered = true;

            if (Player.whoAmI == Main.myPlayer && Main.netMode != NetmodeID.Server)
            {
                CombatText.NewText(target.getRect(), new Color(255, 215, 120), "Hammerfall!", dramatic: true);
            }
        }

        private void SpawnTrailingShockwave()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }

            Item held = Player.HeldItem;
            if (held == null || held.IsAir)
            {
                return;
            }

            IEntitySource source = Player.GetSource_Misc("HammerOfJusticeTrail");
            int damage = (int)(Player.GetWeaponDamage(held) * currentDashDamageMultiplier * 0.6f);
            float knockback = GetDashKnockback() * 0.75f;
            int projType = ModContent.ProjectileType<HammerOfJusticeUltimateShockwave>();

            int projectileIndex = Projectile.NewProjectile(source, Player.Center, Vector2.Zero, projType, damage, knockback, Player.whoAmI, 1f);

            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
            {
                Main.projectile[projectileIndex].netUpdate = true;
            }
        }

        private void CreateUltimateImpact()
        {
            Item held = Player.HeldItem;
            if (held == null || held.IsAir)
            {
                return;
            }

            IEntitySource source = Player.GetSource_Misc("HammerOfJusticeUltimate");
            int damage = (int)(Player.GetWeaponDamage(held) * currentDashDamageMultiplier * 1.25f);
            float knockback = GetDashKnockback();

            int projType = ModContent.ProjectileType<HammerOfJusticeUltimateShockwave>();
            Projectile.NewProjectile(source, Player.Center, Vector2.Zero, projType, damage, knockback, Player.whoAmI);

            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < UltimateAftershockDust; i++)
                {
                    Dust dust = Dust.NewDustDirect(Player.Center - new Vector2(32f), 64, 64, DustID.GoldFlame);
                    Vector2 direction = Vector2.Normalize(dust.position - Player.Center);
                    dust.velocity = direction * Main.rand.NextFloat(8f, 16f);
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(1.5f, 2.1f);
                }

                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.9f, Pitch = -0.45f }, Player.Center);
            }
        }

        private void AttemptParry()
        {
            bool canParry = ParryCooldownTimer <= 0 || ParryChainWindowTimer > 0;
            if (!canParry)
            {
                return;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                HammerOfJusticeNetworkHandler.SendParryRequest(Mod, Player.whoAmI);
            }
            else
            {
                ExecuteParryStart(Main.netMode == NetmodeID.Server);
            }
        }

        public void ExecuteParryStart(bool syncToClients)
        {
            int initialCooldown = ParryChainWindowTimer > 0 ? ParryChainCooldownFrames : ParryCooldownFrames;
            StartParryWindow(ParryActiveFrames, initialCooldown);

            if (syncToClients && Main.netMode == NetmodeID.Server)
            {
                HammerOfJusticeNetworkHandler.SendParryStart(Mod, Player.whoAmI, ParryActiveFrames, initialCooldown);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (TryReflectProjectiles(ParryRadius))
                {
                    HandleParrySuccess(Main.netMode == NetmodeID.Server);
                }
            }
        }

        public void ApplyParryStartFromNetwork(int parryDuration, int baseCooldown)
        {
            StartParryWindow(parryDuration, baseCooldown);
        }

        public void ApplyParrySuccessFromNetwork(int newCooldown, int chainWindow)
        {
            parrySuccessRegistered = true;
            ParryCooldownTimer = newCooldown;
            ParryChainWindowTimer = chainWindow;
            ApplyCooldownBuff(ModContent.BuffType<HammerOfJusticeParryCooldown>(), ParryCooldownTimer);
            PlayParrySuccessEffects();
        }

        private void StartParryWindow(int parryDuration, int cooldown)
        {
            ParryActiveTimer = parryDuration;
            ParryCooldownTimer = cooldown;
            ParryChainWindowTimer = 0;
            parrySuccessRegistered = false;

            ApplyCooldownBuff(ModContent.BuffType<HammerOfJusticeParryCooldown>(), ParryCooldownTimer);
            PlayParryStartEffects();
        }

        private int GetUltimateInterval()
        {
            int configured = ModContent.GetInstance<BalanceConfig>().HammerOfJusticeUltimateDashInterval;
            return Utils.Clamp(configured, 2, 8);
        }

        private void MaintainParryState()
        {
            Player.immune = true;
            Player.immuneTime = System.Math.Max(Player.immuneTime, 10);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                bool reflected = TryReflectProjectiles(ParryRadius);
                if (reflected && !parrySuccessRegistered)
                {
                    HandleParrySuccess(Main.netMode == NetmodeID.Server);
                }
            }
        }

        private bool TryReflectProjectiles(float radius)
        {
            bool reflected = false;
            float radiusSquared = radius * radius;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (!projectile.active || !projectile.hostile || projectile.friendly || projectile.damage <= 0)
                {
                    continue;
                }

                float distanceSquared = Vector2.DistanceSquared(projectile.Center, Player.Center);
                if (distanceSquared > radiusSquared)
                {
                    continue;
                }

                Vector2 newVelocity = projectile.velocity.LengthSquared() <= 0.01f ? -cachedDashDirection * 12f : -projectile.velocity;
                projectile.velocity = newVelocity;
                projectile.hostile = false;
                projectile.friendly = true;
                projectile.owner = Player.whoAmI;
                projectile.DamageType = Player.HeldItem?.DamageType ?? DamageClass.Generic;
                projectile.damage = System.Math.Max(projectile.damage, GetDashDamage());
                if (projectile.penetrate == 0)
                {
                    projectile.penetrate = 1;
                }

                projectile.netUpdate = true;
                reflected = true;
            }

            if (reflected && Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.GoldCoin);
                    dust.noGravity = true;
                    dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
                    dust.scale = Main.rand.NextFloat(1.2f, 1.6f);
                }
            }

            return reflected;
        }

        private void HandleParrySuccess(bool sendNetwork)
        {
            if (parrySuccessRegistered)
            {
                return;
            }

            parrySuccessRegistered = true;
            ParryCooldownTimer = System.Math.Min(ParryCooldownTimer, ParryChainCooldownFrames);
            ParryChainWindowTimer = ParryChainWindowFrames;
            ApplyCooldownBuff(ModContent.BuffType<HammerOfJusticeParryCooldown>(), ParryCooldownTimer);
            PlayParrySuccessEffects();

            if (sendNetwork)
            {
                HammerOfJusticeNetworkHandler.SendParrySuccess(Mod, Player.whoAmI, ParryCooldownTimer, ParryChainWindowTimer);
            }
        }

        private void PlayParryStartEffects()
        {
            SoundEngine.PlaySound(SoundID.Item37 with { Pitch = 0.3f }, Player.Center);

            if (Main.netMode == NetmodeID.Server)
            {
                return;
            }

            for (int i = 0; i < 18; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
                Dust dust = Dust.NewDustDirect(Player.Center - new Vector2(16f), 32, 32, DustID.Enchanted_Pink);
                dust.velocity = velocity;
                dust.noGravity = true;
            }
        }

        private void PlayParrySuccessEffects()
        {
            SoundEngine.PlaySound(SoundID.Item95 with { Pitch = -0.15f }, Player.Center);

            if (Main.netMode == NetmodeID.Server)
            {
                return;
            }

            for (int i = 0; i < 26; i++)
            {
                Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.GoldFlame);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(7f, 7f);
                dust.scale = Main.rand.NextFloat(1.3f, 1.7f);
            }

            if (Main.netMode != NetmodeID.Server)
            {
                CombatText.NewText(Player.getRect(), new Color(245, 230, 120), "Parried!", dramatic: true);
            }
        }

        private void ApplyCooldownBuff(int buffType, int duration)
        {
            if (duration <= 0)
            {
                int index = Player.FindBuffIndex(buffType);
                if (index >= 0)
                {
                    Player.DelBuff(index);
                }
                return;
            }

            int existing = Player.FindBuffIndex(buffType);
            if (existing == -1)
            {
                Player.AddBuff(buffType, duration);
            }
            else
            {
                Player.buffTime[existing] = duration;
            }
        }
    }
}
