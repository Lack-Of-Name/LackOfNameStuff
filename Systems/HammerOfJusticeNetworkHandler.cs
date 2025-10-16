using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Players;

namespace LackOfNameStuff.Systems
{
    public static class HammerOfJusticeNetworkHandler
    {
        private const byte DashRequest = 100;
        private const byte DashSync = 101;
        private const byte ParryRequest = 102;
        private const byte ParryStartSync = 103;
        private const byte ParrySuccessSync = 104;

        public static void SendDashRequest(Mod mod, int playerId, Vector2 direction)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                return;
            }

            ModPacket packet = mod.GetPacket();
            packet.Write(DashRequest);
            packet.Write(playerId);
            packet.Write(direction.X);
            packet.Write(direction.Y);
            packet.Send();
        }

        public static void SendDashSync(Mod mod, int playerId, Vector2 direction, int dashDuration, int cooldown, bool isUltimate)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                return;
            }

            ModPacket packet = mod.GetPacket();
            packet.Write(DashSync);
            packet.Write(playerId);
            packet.Write(direction.X);
            packet.Write(direction.Y);
            packet.Write(dashDuration);
            packet.Write(cooldown);
            packet.Write(isUltimate);
            packet.Send();
        }

        public static void SendParryRequest(Mod mod, int playerId)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                return;
            }

            ModPacket packet = mod.GetPacket();
            packet.Write(ParryRequest);
            packet.Write(playerId);
            packet.Send();
        }

        public static void SendParryStart(Mod mod, int playerId, int parryDuration, int cooldown)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                return;
            }

            ModPacket packet = mod.GetPacket();
            packet.Write(ParryStartSync);
            packet.Write(playerId);
            packet.Write(parryDuration);
            packet.Write(cooldown);
            packet.Send();
        }

        public static void SendParrySuccess(Mod mod, int playerId, int newCooldown, int chainWindow)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                return;
            }

            ModPacket packet = mod.GetPacket();
            packet.Write(ParrySuccessSync);
            packet.Write(playerId);
            packet.Write(newCooldown);
            packet.Write(chainWindow);
            packet.Send();
        }

        public static bool TryHandlePacket(Mod mod, byte messageType, BinaryReader reader, int whoAmI)
        {
            switch (messageType)
            {
                case DashRequest:
                    if (Main.netMode == NetmodeID.Server)
                    {
                        int playerIndex = reader.ReadInt32();
                        float dirX = reader.ReadSingle();
                        float dirY = reader.ReadSingle();

                        if (IsValidPlayer(playerIndex))
                        {
                            Player player = Main.player[playerIndex];
                            var hammerPlayer = player.GetModPlayer<HammerOfJusticePlayer>();
                            hammerPlayer.ExecuteDash(new Vector2(dirX, dirY), syncToClients: true);
                        }
                    }
                    return true;

                case DashSync:
                {
                    int playerIndex = reader.ReadInt32();
                    float dirX = reader.ReadSingle();
                    float dirY = reader.ReadSingle();
                    int dashDuration = reader.ReadInt32();
                    int cooldown = reader.ReadInt32();
                    bool isUltimate = reader.ReadBoolean();

                    if (IsValidPlayer(playerIndex))
                    {
                        Player player = Main.player[playerIndex];
                        var hammerPlayer = player.GetModPlayer<HammerOfJusticePlayer>();
                        hammerPlayer.ApplyDashFromNetwork(new Vector2(dirX, dirY), dashDuration, cooldown, isUltimate);
                    }

                    return true;
                }

                case ParryRequest:
                    if (Main.netMode == NetmodeID.Server)
                    {
                        int playerIndex = reader.ReadInt32();

                        if (IsValidPlayer(playerIndex))
                        {
                            Player player = Main.player[playerIndex];
                            var hammerPlayer = player.GetModPlayer<HammerOfJusticePlayer>();
                            hammerPlayer.ExecuteParryStart(syncToClients: true);
                        }
                    }
                    return true;

                case ParryStartSync:
                {
                    int playerIndex = reader.ReadInt32();
                    int parryDuration = reader.ReadInt32();
                    int cooldown = reader.ReadInt32();

                    if (IsValidPlayer(playerIndex))
                    {
                        Player player = Main.player[playerIndex];
                        var hammerPlayer = player.GetModPlayer<HammerOfJusticePlayer>();
                        hammerPlayer.ApplyParryStartFromNetwork(parryDuration, cooldown);
                    }

                    return true;
                }

                case ParrySuccessSync:
                {
                    int playerIndex = reader.ReadInt32();
                    int newCooldown = reader.ReadInt32();
                    int chainWindow = reader.ReadInt32();

                    if (IsValidPlayer(playerIndex))
                    {
                        Player player = Main.player[playerIndex];
                        var hammerPlayer = player.GetModPlayer<HammerOfJusticePlayer>();
                        hammerPlayer.ApplyParrySuccessFromNetwork(newCooldown, chainWindow);
                    }

                    return true;
                }
            }

            return false;
        }

        private static bool IsValidPlayer(int index)
        {
            return index >= 0 && index < Main.maxPlayers && Main.player[index].active;
        }
    }
}
