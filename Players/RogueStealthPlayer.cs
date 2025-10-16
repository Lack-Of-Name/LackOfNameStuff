using System;
using LackOfNameStuff.Common;
using Terraria;
using Terraria.ModLoader;

namespace LackOfNameStuff.Players
{
    public class RogueStealthPlayer : ModPlayer
    {
        private float? cachedStealthRegen;
        private bool regenBoostApplied;
    private float? targetStealthRegen;

        public override void PostUpdate()
        {
            if (!CalamityIntegration.CalamityLoaded)
            {
                RestoreStealthRegen();
                return;
            }

            float multiplier = GetHeldStealthMultiplier();
            if (multiplier <= 0f)
            {
                RestoreStealthRegen();
                return;
            }

            if (regenBoostApplied)
            {
                if (targetStealthRegen.HasValue && CalamityIntegration.TryAccessRogueStealthRegen(Player, out float currentRegen, out var currentSetter))
                {
                    if (Math.Abs(currentRegen - targetStealthRegen.Value) > 0.01f)
                    {
                        currentSetter(targetStealthRegen.Value);
                    }
                }

                return;
            }

            if (CalamityIntegration.TryAccessRogueStealthRegen(Player, out float current, out var setter))
            {
                cachedStealthRegen = current;
                targetStealthRegen = current * multiplier;
                setter(targetStealthRegen.Value);
                regenBoostApplied = true;
            }
        }

        public override void ResetEffects()
        {
            if (!CalamityIntegration.CalamityLoaded)
            {
                RestoreStealthRegen();
            }
        }

        public override void OnRespawn()
        {
            RestoreStealthRegen();
        }

        public override void PlayerDisconnect()
        {
            RestoreStealthRegen();
        }

        private float GetHeldStealthMultiplier()
        {
            Item heldItem = Player.HeldItem;
            if (heldItem == null || heldItem.IsAir)
            {
                return 0f;
            }

            int type = heldItem.type;

            if (type == ModContent.ItemType<Items.Weapons.Rogue.BlackShard>())
            {
                return 2.4f;
            }

            if (type == ModContent.ItemType<Items.Weapons.Rogue.LittleSponge>())
            {
                return 1.8f;
            }

            if (type == ModContent.ItemType<Items.Weapons.Rogue.HammerOfJustice>())
            {
                return 2.1f;
            }

            return 0f;
        }

        private void RestoreStealthRegen()
        {
            if (!regenBoostApplied || !cachedStealthRegen.HasValue)
            {
                regenBoostApplied = false;
                cachedStealthRegen = null;
                targetStealthRegen = null;
                return;
            }

            CalamityIntegration.TrySetRogueStealthRegen(Player, cachedStealthRegen.Value);
            regenBoostApplied = false;
            cachedStealthRegen = null;
            targetStealthRegen = null;
        }
    }
}
