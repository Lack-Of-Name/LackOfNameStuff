using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Players;
using LackOfNameStuff.Items.Accessories;
using LackOfNameStuff.Effects;
using LackOfNameStuff.Buffs;

namespace LackOfNameStuff.Systems
{
    public class ChronosModSystem : ModSystem
    {
        // Simple cleanup timer
        private int cleanupTimer = 0;
        
        public override void PostUpdateEverything()
        {
            // Only run cleanup on server every 5 seconds
            if (Main.netMode != NetmodeID.Server) return;
            
            // Update server-side global state tracking
            // UpdateServerGlobalState(); // Removed: method does not exist
            // No need to call UpdateServerGlobalState(); it was likely a placeholder for server-side state sync.
            // All necessary server-side state updates are handled below.            
                // Decrement cooldown for all active players
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active)
                    {
                        var modPlayer = Main.player[i].GetModPlayer<ChronosPlayer>();
                        if (modPlayer.bulletTimeCooldown > 0)
                            modPlayer.bulletTimeCooldown--;
                    }
                }

            cleanupTimer++;
            if (cleanupTimer >= 300) // 5 seconds
            {
                cleanupTimer = 0;
                CleanupStaleBuffs();
            }
        }
        
        private void CleanupStaleBuffs()
        {
            // Remove any buffs with very low time remaining to prevent desync
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active)
                {
                    int buffIndex = Main.player[i].FindBuffIndex(ModContent.BuffType<BulletTimeBuff>());
                    if (buffIndex != -1 && Main.player[i].buffTime[buffIndex] <= 30)
                    {
                        Main.player[i].DelBuff(buffIndex);
                    }
                }
            }
        }
    }
    
    // Simplified network handler
    public class ChronosNetworkHandler
    {
        public static bool TryHandlePacket(Mod mod, byte messageType, BinaryReader reader, int whoAmI)
        {
            switch (messageType)
            {
                case 0 when Main.netMode == NetmodeID.Server:
                {
                    int requestingPlayer = reader.ReadInt32();

                    if (requestingPlayer >= 0 && requestingPlayer < Main.maxPlayers && Main.player[requestingPlayer].active)
                    {
                        var requestingModPlayer = Main.player[requestingPlayer].GetModPlayer<ChronosPlayer>();

                        bool canActivate = requestingModPlayer.hasChronosWatch &&
                                           requestingModPlayer.bulletTimeCooldown <= 0 &&
                                           !requestingModPlayer.IsInBulletTime &&
                                           !IsAnyPlayerInBulletTime();

                        if (canActivate)
                        {
                            requestingModPlayer.ApplyBulletTimeToAllPlayers();
                            SendVisualEffects(mod, requestingPlayer, Main.player[requestingPlayer].Center);
                        }
                    }

                    return true;
                }

                case 1:
                {
                    int activatingPlayer = reader.ReadInt32();
                    int duration = reader.ReadInt32();

                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        if (Main.player[i].active)
                        {
                            Main.player[i].AddBuff(ModContent.BuffType<BulletTimeBuff>(), duration);

                            if (i == activatingPlayer)
                            {
                                Main.player[i].GetModPlayer<ChronosPlayer>().bulletTimeCooldown = ChronosWatch.CooldownDuration;
                            }
                        }
                    }

                    return true;
                }

                case 2:
                {
                    int activatingPlayer = reader.ReadInt32();
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    Vector2 position = new Vector2(x, y);

                    var localPlayer = Main.LocalPlayer.GetModPlayer<ChronosPlayer>();
                    localPlayer.CreateNetworkRipple(position, activatingPlayer);
                    return true;
                }
            }

            return false;
        }
        
        private static bool IsAnyPlayerInBulletTime()
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active && Main.player[i].HasBuff(ModContent.BuffType<BulletTimeBuff>()))
                {
                    return true;
                }
            }
            return false;
        }
        
        private static void SendVisualEffects(Mod mod, int activatingPlayer, Vector2 position)
        {
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)2); // Visual effects
            packet.Write(activatingPlayer);
            packet.Write(position.X);
            packet.Write(position.Y);
            packet.Send();
        }
    }
}