// Optional: Add a ModCommand for admin/debug purposes
using Terraria.ModLoader;

namespace LackOfNameStuff.Commands
{
    public class TemporalTierCommand : ModCommand
    {
        public override string Command => "temporaltier";
        public override CommandType Type => CommandType.Chat;
        public override string Description => "Set your temporal tier (admin/debug)";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length == 0)
            {
                var temporalPlayer = caller.Player.GetModPlayer<Players.TemporalPlayer>();
                caller.Reply($"Current temporal tier: {temporalPlayer.currentTier}");
                caller.Reply($"Unlocked: Shard={temporalPlayer.hasUnlockedTimeShard}, Eternal Shard={temporalPlayer.hasUnlockedEternalShard}, Gem={temporalPlayer.hasUnlockedTimeGem}, Eternal Gem={temporalPlayer.hasUnlockedEternalGem}");
                return;
            }

            if (int.TryParse(args[0], out int tier) && tier >= 1 && tier <= 4)
            {
                var temporalPlayer = caller.Player.GetModPlayer<Players.TemporalPlayer>();
                temporalPlayer.ForceUnlockTier(tier);
                caller.Reply($"Temporal tier set to {tier}!");
            }
            else
            {
                caller.Reply("Usage: /temporaltier [1-4] or /temporaltier (to check current)");
            }
        }
    }
}