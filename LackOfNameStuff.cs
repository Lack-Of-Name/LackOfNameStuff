using System.IO;
using Terraria.ModLoader;
using LackOfNameStuff.Systems;
using LackOfNameStuff.Common;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using LackOfNameStuff.Items.Tools;
using LackOfNameStuff.Globals;
using LackOfNameStuff.Players;

namespace LackOfNameStuff
{
    public class LackOfNameStuff : Mod
    {
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            byte messageType = reader.ReadByte();

            if (HammerOfJusticeNetworkHandler.TryHandlePacket(this, messageType, reader, whoAmI))
            {
                return;
            }

            if (ChronosNetworkHandler.TryHandlePacket(this, messageType, reader, whoAmI))
            {
                return;
            }

            Logger.Warn($"Unknown network message type {messageType} from {whoAmI}");
        }
    }

    public class RewindPlayer : ModPlayer
    {
        // Store last 600 ticks (10 seconds) of positions
        private Queue<Vector2> positionHistory = new Queue<Vector2>();
        private const int MaxHistoryLength = 600; // 10 seconds at 60 FPS

        public override void PostUpdate()
        {
            // Record current position every tick
            positionHistory.Enqueue(Player.position);

            // Keep only the last 10 seconds
            while (positionHistory.Count > MaxHistoryLength)
            {
                positionHistory.Dequeue();
            }
        }

        public void TriggerRewind()
        {
            if (positionHistory.Count > 0)
            {
                // Get the oldest position (10 seconds ago)
                Vector2 rewindPosition = positionHistory.Peek();

                // Teleport player
                Player.Teleport(rewindPosition);

                // Add some visual/audio feedback
                for (int i = 0; i < 30; i++)
                {
                    Vector2 dustPos = Player.position + new Vector2(Main.rand.Next(Player.width), Main.rand.Next(Player.height));
                    Dust dust = Dust.NewDustDirect(dustPos, 0, 0, DustID.MagicMirror);
                    dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
                    dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                }

                // Play sound
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item6, Player.position);
            }
        }

        // Reset position history when player dies or world changes
        public override void OnRespawn()
        {
            positionHistory.Clear();
        }

        public override void OnEnterWorld()
        {
            positionHistory.Clear();
        }
    }

    public class TemporalPickaxePlayer : ModPlayer
    {
        private const float MaxSpeedBonusPercent = 1000f;
        private const float ProgressCurveBase = 6f;
        internal const int BlocksPerMilestone = 60000;

        private static readonly Func<bool>[] BossMilestoneChecks =
        {
            () => TemporalProgressionSystem.HasDefeatedProvidence(),
            () => TemporalProgressionSystem.HasDefeatedPolterghast(),
            () => TemporalProgressionSystem.HasDefeatedDoG(),
            () => CalamityIntegration.DownedYharon,
            () => CalamityIntegration.DownedSCal
        };

        private float _lastDisplayedBonus;

        public int BlocksMinedWithTemporalPickaxe { get; private set; }

        internal static int TotalMilestones => BossMilestoneChecks.Length;

        public float GetSpeedBonus()
        {
            float normalizedProgress = GetNormalizedProgress();
            if (normalizedProgress <= 0f)
            {
                return 0f;
            }

            double curve = (Math.Pow(ProgressCurveBase, normalizedProgress) - 1d) / (ProgressCurveBase - 1d);
            float scaledBonus = (float)(curve * MaxSpeedBonusPercent);
            return Math.Min(MaxSpeedBonusPercent, scaledBonus);
        }

        public int GetUnlockedMilestoneCount() => CountDefeatedMilestoneBosses();

        public int GetBlocksUntilNextMilestone()
        {
            int unlocked = GetUnlockedMilestoneCount();
            if (unlocked >= TotalMilestones)
            {
                return GetBlocksRemainingToMaxSpeed();
            }

            int nextThreshold = BlocksPerMilestone * (unlocked + 1);
            return Math.Max(0, nextThreshold - BlocksMinedWithTemporalPickaxe);
        }

        public int GetBlocksRemainingToMaxSpeed()
        {
            int totalRequirement = BlocksPerMilestone * TotalMilestones;
            return Math.Max(0, totalRequirement - BlocksMinedWithTemporalPickaxe);
        }

        public bool IsBossGateHoldingProgress()
        {
            int unlocked = GetUnlockedMilestoneCount();
            if (unlocked >= TotalMilestones)
            {
                return false;
            }

            int nextThreshold = BlocksPerMilestone * (unlocked + 1);
            return BlocksMinedWithTemporalPickaxe >= nextThreshold;
        }

        public void OnBlockMinedByPickaxe()
        {
            if (Player.HeldItem.ModItem is TemporalPickaxe)
            {
                RegisterBlockMined();
            }
        }

        public void OnBlockMined()
        {
            if (Player.HeldItem.ModItem is TemporalPickaxe)
            {
                RegisterBlockMined();
            }
        }

        public override void OnEnterWorld()
        {
            _lastDisplayedBonus = GetSpeedBonus();
        }

        public override void SaveData(TagCompound tag)
        {
            tag["BlocksMinedWithTemporalPickaxe"] = BlocksMinedWithTemporalPickaxe;
        }

        public override void LoadData(TagCompound tag)
        {
            BlocksMinedWithTemporalPickaxe = tag.GetInt("BlocksMinedWithTemporalPickaxe");
            _lastDisplayedBonus = GetSpeedBonus();
        }

        private void RegisterBlockMined()
        {
            BlocksMinedWithTemporalPickaxe++;

            float currentBonus = GetSpeedBonus();
            bool increased = currentBonus - _lastDisplayedBonus > 0.01f;

            if (BlocksMinedWithTemporalPickaxe % 100 == 0 && increased)
            {
                CombatText.NewText(Player.getRect(), Color.Cyan, $"+{currentBonus:F1}% speed!", true);
            }

            if (increased)
            {
                _lastDisplayedBonus = currentBonus;
            }
        }

        private float GetNormalizedProgress()
        {
            if (TotalMilestones == 0)
            {
                return 0f;
            }

            float totalRequirement = BlocksPerMilestone * TotalMilestones;
            if (totalRequirement <= 0f)
            {
                return 0f;
            }

            float rawProgress = BlocksMinedWithTemporalPickaxe / totalRequirement;
            float bossCap = GetBossMilestoneCap();
            float normalized = Math.Min(rawProgress, bossCap);
            return Math.Max(0f, Math.Min(1f, normalized));
        }

        private float GetBossMilestoneCap()
        {
            int unlocked = CountDefeatedMilestoneBosses();
            return unlocked / (float)TotalMilestones;
        }

        private int CountDefeatedMilestoneBosses()
        {
            int count = 0;
            foreach (Func<bool> check in BossMilestoneChecks)
            {
                if (check())
                {
                    count++;
                }
            }

            return count;
        }
    }
}