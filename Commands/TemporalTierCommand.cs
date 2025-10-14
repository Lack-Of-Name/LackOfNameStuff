// Optional: Add a ModCommand for admin/debug purposes
using Terraria.ModLoader;
using LackOfNameStuff.Players;

namespace LackOfNameStuff.Commands
{
    public class TemporalTierCommand : ModCommand
    {
        public override string Command => "temporaltier";
        public override CommandType Type => CommandType.Chat;
    public override string Description => "Set your temporal tier (admin/debug). Usage: /temporaltier [1-4] [lock]";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length == 0 || (args.Length == 1 && args[0].Equals("show", System.StringComparison.OrdinalIgnoreCase)))
            {
                var temporalPlayer = caller.Player.GetModPlayer<TemporalPlayer>();
                caller.Reply($"Current temporal tier: {temporalPlayer.currentTier}");
                caller.Reply($"Unlocked: Shard={temporalPlayer.hasUnlockedTimeShard}, Eternal Shard={temporalPlayer.hasUnlockedEternalShard}, Gem={temporalPlayer.hasUnlockedTimeGem}, Eternal Gem={temporalPlayer.hasUnlockedEternalGem}");
                caller.Reply($"Debug lock: {(temporalPlayer.debugLockTier ? "ON" : "OFF")}");
                return;
            }

            // Lock toggle command: /temporaltier lock [on|off]
            if (args.Length >= 1 && args[0].Equals("lock", System.StringComparison.OrdinalIgnoreCase))
            {
                var temporalPlayer = caller.Player.GetModPlayer<TemporalPlayer>();
                if (args.Length >= 2)
                {
                    temporalPlayer.debugLockTier = args[1].Equals("on", System.StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    // Toggle if no explicit state provided
                    temporalPlayer.debugLockTier = !temporalPlayer.debugLockTier;
                }
                caller.Reply($"Temporal tier auto-unlock lock is now {(temporalPlayer.debugLockTier ? "ON" : "OFF")}");
                return;
            }

            if (int.TryParse(args[0], out int tier) && tier >= 1 && tier <= 4)
            {
                var temporalPlayer = caller.Player.GetModPlayer<TemporalPlayer>();
                int oldTier = temporalPlayer.currentTier;
                // Use SetTier to ensure flags are synced and downgrades revert properly
                temporalPlayer.SetTier(tier, showEffects: false);
                // Optional second arg: "lock" to keep the tier from auto-upgrading via inventory/pickups
                if (args.Length >= 2 && args[1].Equals("lock", System.StringComparison.OrdinalIgnoreCase))
                {
                    temporalPlayer.debugLockTier = true;
                }
                else
                {
                    temporalPlayer.debugLockTier = false;
                }
                caller.Reply($"Temporal tier set to {tier}! (was {oldTier}) | debug lock: {(temporalPlayer.debugLockTier ? "ON" : "OFF")}");
            }
            else
            {
                caller.Reply("Usage: /temporaltier [1-4] [lock] | /temporaltier show | /temporaltier lock [on|off]");
            }
        }
    }
}