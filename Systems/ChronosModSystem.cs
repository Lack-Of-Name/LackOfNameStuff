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
        // This will be called by the mod's packet handler
        // We'll implement this in the Mod class instead
    }
    
    // Add this to your main mod class file
    public class ChronosNetworkHandler
    {
        public static void HandlePacket(Mod mod, BinaryReader reader, int whoAmI)
        {
            byte messageType = reader.ReadByte();
            
            // Add some debug logging
            if (Main.netMode == NetmodeID.Server)
            {
                mod.Logger.Info($"Server received packet type {messageType} from player {whoAmI}");
            }
            else
            {
                mod.Logger.Info($"Client received packet type {messageType}");
            }
            
            if (messageType == 0 && Main.netMode == NetmodeID.Server) // Request bullet time activation (server only)
            {
                int requestingPlayer = reader.ReadInt32();
                
                mod.Logger.Info($"Player {requestingPlayer} requesting bullet time activation");
                
                if (requestingPlayer >= 0 && requestingPlayer < Main.maxPlayers && Main.player[requestingPlayer].active)
                {
                    // Verify the requesting player has the watch and isn't on cooldown
                    var requestingModPlayer = Main.player[requestingPlayer].GetModPlayer<ChronosPlayer>();
                    
                    mod.Logger.Info($"Player {requestingPlayer} - HasWatch: {requestingModPlayer.hasChronosWatch}, Cooldown: {requestingModPlayer.bulletTimeCooldown}, InBulletTime: {requestingModPlayer.IsInBulletTime}");
                    
                    if (requestingModPlayer.hasChronosWatch && requestingModPlayer.bulletTimeCooldown <= 0 && !requestingModPlayer.IsInBulletTime)
                    {
                        mod.Logger.Info($"Activating bullet time for player {requestingPlayer}");
                        
                        // Set their cooldown
                        requestingModPlayer.bulletTimeCooldown = ChronosWatch.CooldownDuration;
                        
                        // Apply to all players
                        ApplyBulletTimeToAllPlayers(mod, requestingPlayer);
                        
                        // Send visual effects to all clients
                        SendVisualEffects(mod, requestingPlayer, Main.player[requestingPlayer].Center);
                    }
                    else
                    {
                        mod.Logger.Info($"Bullet time activation denied for player {requestingPlayer}");
                    }
                }
            }
            else if (messageType == 1) // Sync bullet time activation (clients)
            {
                int activatingPlayer = reader.ReadInt32();
                int duration = reader.ReadInt32();
                
                mod.Logger.Info($"Applying bullet time buff to all players for {duration} frames");
                
                // Apply buff to all players
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active)
                    {
                        // Remove existing buff first to prevent stacking issues
                        Main.player[i].ClearBuff(ModContent.BuffType<BulletTimeBuff>());
                        Main.player[i].AddBuff(ModContent.BuffType<BulletTimeBuff>(), duration);
                        mod.Logger.Info($"Applied bullet time buff to player {i}");
                    }
                }
            }
            else if (messageType == 2) // Visual effects
            {
                int activatingPlayer = reader.ReadInt32();
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                Vector2 position = new Vector2(x, y);
                
                mod.Logger.Info($"Creating visual effects for activation by player {activatingPlayer}");
                
                // Create visual effects for the local player
                var localPlayer = Main.LocalPlayer.GetModPlayer<ChronosPlayer>();
                localPlayer.activeRipples.Add(new BulletTimeRipple(position, ChronosWatch.RippleLifetime, true));
                
                if (activatingPlayer != Main.LocalPlayer.whoAmI)
                {
                    SoundEngine.PlaySound(SoundID.Item29.WithVolumeScale(0.4f).WithPitchOffset(-0.3f), position);
                }
            }
        }
        
        private static void ApplyBulletTimeToAllPlayers(Mod mod, int activatingPlayer)
        {
            mod.Logger.Info($"Server applying bullet time to all players, activated by {activatingPlayer}");
            
            // Apply buff to all active players
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active)
                {
                    // Clear any existing bullet time buff first
                    Main.player[i].ClearBuff(ModContent.BuffType<BulletTimeBuff>());
                    Main.player[i].AddBuff(ModContent.BuffType<BulletTimeBuff>(), ChronosWatch.BulletTimeDuration);
                    mod.Logger.Info($"Server applied bullet time buff to player {i}");
                }
            }
            
            // Sync this to all clients
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)1); // Message type: Sync bullet time activation
            packet.Write(activatingPlayer);
            packet.Write(ChronosWatch.BulletTimeDuration);
            packet.Send();
            
            mod.Logger.Info("Server sent bullet time sync packet to all clients");
        }
        
        private static void SendVisualEffects(Mod mod, int activatingPlayer, Vector2 position)
        {
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)2); // Message type: Visual effects
            packet.Write(activatingPlayer);
            packet.Write(position.X);
            packet.Write(position.Y);
            packet.Send();
        }
    }
}