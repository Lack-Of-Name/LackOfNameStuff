using System;
using LackOfNameStuff;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace LackOfNameStuff.Commands
{
    public class TemporalPickaxeCommand : ModCommand
    {
        public override string Command => "tpic";
        public override CommandType Type => CommandType.Chat;
        public override string Description => "Temporal pickaxe debug tools. Usage: /tpic show | /tpic set blocks <value> | /tpic reset";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (caller.Player == null)
            {
                caller.Reply("No player context available.", Color.OrangeRed);
                return;
            }

            var pickaxePlayer = caller.Player.GetModPlayer<TemporalPickaxePlayer>();
            if (pickaxePlayer == null)
            {
                caller.Reply("Temporal pickaxe data could not be retrieved.", Color.OrangeRed);
                return;
            }

            if (args.Length == 0)
            {
                ReplyUsage(caller);
                return;
            }

            string subcommand = args[0].ToLowerInvariant();
            switch (subcommand)
            {
                case "show":
                    ShowStatus(caller, pickaxePlayer);
                    return;

                case "reset":
                    pickaxePlayer.SetBlocksMined(0);
                    caller.Reply("Temporal pickaxe progress reset to 0 blocks.", Color.Cyan);
                    return;

                case "set":
                    HandleSetCommand(caller, pickaxePlayer, args);
                    return;

                default:
                    ReplyUsage(caller);
                    return;
            }
        }

        private static void HandleSetCommand(CommandCaller caller, TemporalPickaxePlayer pickaxePlayer, string[] args)
        {
            if (args.Length < 3 || !args[1].Equals("blocks", StringComparison.OrdinalIgnoreCase))
            {
                ReplyUsage(caller);
                return;
            }

            if (!int.TryParse(args[2], out int value) || value < 0)
            {
                caller.Reply("Please provide a non-negative integer for blocks.", Color.OrangeRed);
                return;
            }

            pickaxePlayer.SetBlocksMined(value);
            float bonus = pickaxePlayer.GetSpeedBonus();
            int blocksToNext = pickaxePlayer.GetBlocksUntilNextMilestone();

            caller.Reply($"Temporal pickaxe block counter set to {value}.", Color.LightGreen);
            caller.Reply($"Current speed bonus: {bonus:F1}% | Blocks to next milestone: {blocksToNext}", Color.LightGreen);
        }

        private static void ShowStatus(CommandCaller caller, TemporalPickaxePlayer pickaxePlayer)
        {
            int mined = pickaxePlayer.BlocksMinedWithTemporalPickaxe;
            float bonus = pickaxePlayer.GetSpeedBonus();
            int milestones = pickaxePlayer.GetUnlockedMilestoneCount();
            int remaining = pickaxePlayer.GetBlocksUntilNextMilestone();
            bool gated = pickaxePlayer.IsBossGateHoldingProgress();

            caller.Reply($"Blocks mined: {mined}", Color.LightGreen);
            caller.Reply($"Speed bonus: {bonus:F1}% | Boss milestones unlocked: {milestones}", Color.LightGreen);
            caller.Reply($"Blocks to next milestone: {remaining} | Boss gate active: {(gated ? "YES" : "NO")}", Color.LightGreen);
        }

        private static void ReplyUsage(CommandCaller caller)
        {
            caller.Reply("Usage: /tpic show | /tpic reset | /tpic set blocks <value>", Color.Yellow);
        }
    }
}
