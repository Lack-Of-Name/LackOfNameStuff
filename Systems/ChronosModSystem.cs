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
        // Server-side global bullet time state tracking
        private static bool serverGlobalBulletTimeActive = false;
        private static int serverGlobalBulletTimeRemaining = 0;
        private static int lastActivatingPlayer = -1;
        
        // Server-side cleanup system to prevent stale bullet time states
        private int cleanupTimer = 0;
        
        public override void PostUpdateEverything()
        {
            // Only run on server
            if (Main.netMode != NetmodeID.Server) return;
            
            // Update server-side global state tracking
            UpdateServerGlobalState();
            
            cleanupTimer++;
            
            // Every 5 seconds, do a comprehensive cleanup check
            if (cleanupTimer >= 300) // 300 frames = 5 seconds
            {
                cleanupTimer = 0;
                PerformMaintenanceCleanup();
            }
        }
        
        private void UpdateServerGlobalState()
        {
            // Count how many players actually have the buff
            int playersWithBuff = 0;
            int maxBuffTime = 0;
            
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active)
                {
                    bool hasBuff = Main.player[i].HasBuff(ModContent.BuffType<BulletTimeBuff>());
                    if (hasBuff)
                    {
                        playersWithBuff++;
                        int buffIndex = Main.player[i].FindBuffIndex(ModContent.BuffType<BulletTimeBuff>());
                        if (buffIndex != -1)
                        {
                            int buffTime = Main.player[i].buffTime[buffIndex];
                            if (buffTime > maxBuffTime)
                                maxBuffTime = buffTime;
                        }
                    }
                }
            }
            
            // Update global state
            bool shouldBeActive = playersWithBuff > 0 && maxBuffTime > 0;
            
            if (serverGlobalBulletTimeActive != shouldBeActive)
            {
                Mod.Logger.Info($"Server global bullet time state changed: {serverGlobalBulletTimeActive} -> {shouldBeActive} (PlayersWithBuff: {playersWithBuff}, MaxBuffTime: {maxBuffTime})");
                serverGlobalBulletTimeActive = shouldBeActive;
            }
            
            serverGlobalBulletTimeRemaining = maxBuffTime;
        }
        
        private void PerformMaintenanceCleanup()
        {
            Mod.Logger.Info("Performing maintenance cleanup...");
            
            // Force synchronization of buff states
            bool foundInconsistencies = false;
            
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active)
                {
                    bool hasBuff = Main.player[i].HasBuff(ModContent.BuffType<BulletTimeBuff>());
                    int buffIndex = Main.player[i].FindBuffIndex(ModContent.BuffType<BulletTimeBuff>());
                    int buffTime = buffIndex != -1 ? Main.player[i].buffTime[buffIndex] : 0;
                    
                    // Clear buffs with very low time or inconsistent states
                    if (hasBuff && (buffTime <= 30 || buffIndex == -1))
                    {
                        Main.player[i].ClearBuff(ModContent.BuffType<BulletTimeBuff>());
                        Mod.Logger.Info($"Maintenance: Cleared inconsistent bullet time buff from player {i} (buffTime: {buffTime}, buffIndex: {buffIndex})");
                        foundInconsistencies = true;
                    }
                }
            }
            
            if (foundInconsistencies)
            {
                // Send cleanup sync to all clients
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)4); // Cleanup sync
                packet.Send();
                Mod.Logger.Info("Sent maintenance cleanup sync to all clients");
                
                // Reset global state
                serverGlobalBulletTimeActive = false;
                serverGlobalBulletTimeRemaining = 0;
            }
        }
        
        // Static method to check server global state (for use in packet handler)
        public static bool IsServerGlobalBulletTimeActive()
        {
            return serverGlobalBulletTimeActive;
        }
        
        public static int GetServerGlobalBulletTimeRemaining()
        {
            return serverGlobalBulletTimeRemaining;
        }
    }
    
    // Add this to main mod class file
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
                    
                    // COMPREHENSIVE STATE CHECK with detailed logging
                    bool anyPlayerInBulletTime = false;
                    string globalStateDebug = "Global bullet time check: ";
                    int totalPlayersWithBuff = 0;
                    
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        if (Main.player[i].active)
                        {
                            bool hasBuffCheck = Main.player[i].HasBuff(ModContent.BuffType<BulletTimeBuff>());
                            int buffIndex = Main.player[i].FindBuffIndex(ModContent.BuffType<BulletTimeBuff>());
                            int buffTime = buffIndex != -1 ? Main.player[i].buffTime[buffIndex] : 0;
                            
                            globalStateDebug += $"P{i}(active:true, hasBuff:{hasBuffCheck}, buffIdx:{buffIndex}, buffTime:{buffTime}) ";
                            
                            if (hasBuffCheck && buffTime > 0)
                            {
                                anyPlayerInBulletTime = true;
                                totalPlayersWithBuff++;
                                mod.Logger.Info($"Player {i} has active bullet time (buffTime: {buffTime}) - blocking new activation");
                            }
                            else if (hasBuffCheck && buffTime <= 0)
                            {
                                // Clean up stale buff immediately
                                Main.player[i].ClearBuff(ModContent.BuffType<BulletTimeBuff>());
                                mod.Logger.Info($"Cleaned up stale bullet time buff from player {i} (buffTime was {buffTime})");
                            }
                        }
                        else
                        {
                            globalStateDebug += $"P{i}(inactive) ";
                        }
                    }
                    
                    mod.Logger.Info(globalStateDebug);
                    mod.Logger.Info($"Server global state - Active: {ChronosModSystem.IsServerGlobalBulletTimeActive()}, Remaining: {ChronosModSystem.GetServerGlobalBulletTimeRemaining()}");
                    
                    // Use BOTH individual checks AND server global state
                    bool serverGlobalActive = ChronosModSystem.IsServerGlobalBulletTimeActive();
                    
                    // Detailed condition checking with specific denial reasons
                    string denialReason = "";
                    bool canActivate = true;
                    
                    if (!requestingModPlayer.hasChronosWatch)
                    {
                        denialReason = "Player doesn't have ChronosWatch";
                        canActivate = false;
                    }
                    else if (requestingModPlayer.bulletTimeCooldown > 0)
                    {
                        denialReason = $"Player on cooldown ({requestingModPlayer.bulletTimeCooldown} frames remaining)";
                        canActivate = false;
                    }
                    else if (requestingModPlayer.IsInBulletTime)
                    {
                        denialReason = "Requesting player already in bullet time";
                        canActivate = false;
                    }
                    else if (anyPlayerInBulletTime)
                    {
                        denialReason = $"Another player has bullet time active (found {totalPlayersWithBuff} players with buff)";
                        canActivate = false;
                    }
                    else if (serverGlobalActive)
                    {
                        denialReason = $"Server global state indicates bullet time is active (remaining: {ChronosModSystem.GetServerGlobalBulletTimeRemaining()})";
                        canActivate = false;
                    }
                    
                    if (canActivate)
                    {
                        mod.Logger.Info($"✓ Activating bullet time for player {requestingPlayer} - all conditions met");
                        
                        // Set their cooldown FIRST before applying buffs
                        requestingModPlayer.bulletTimeCooldown = ChronosWatch.CooldownDuration;
                        mod.Logger.Info($"Set cooldown for player {requestingPlayer} to {ChronosWatch.CooldownDuration} frames");
                        
                        // Apply to all players
                        ApplyBulletTimeToAllPlayers(mod, requestingPlayer);
                        
                        // Send visual effects to all clients
                        SendVisualEffects(mod, requestingPlayer, Main.player[requestingPlayer].Center);
                        
                        // Send confirmation to the requesting player
                        SendActivationConfirmation(mod, requestingPlayer, true);
                    }
                    else
                    {
                        mod.Logger.Info($"✗ Bullet time activation DENIED for player {requestingPlayer} - Reason: {denialReason}");
                        
                        // Send denial to the requesting player
                        SendActivationConfirmation(mod, requestingPlayer, false);
                    }
                }
            }
            else if (messageType == 1) // Sync bullet time activation (clients)
            {
                int activatingPlayer = reader.ReadInt32();
                int duration = reader.ReadInt32();
                
                mod.Logger.Info($"Applying bullet time buff to all players for {duration} frames");
                
                // COMPLETE buff state reset before applying new buffs
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active)
                    {
                        // First, completely clear ALL buffs of this type
                        bool hadBuff = Main.player[i].HasBuff(ModContent.BuffType<BulletTimeBuff>());
                        if (hadBuff)
                        {
                            // Use DelBuff instead of ClearBuff for more thorough removal
                            int buffIndex = Main.player[i].FindBuffIndex(ModContent.BuffType<BulletTimeBuff>());
                            while (buffIndex != -1)
                            {
                                Main.player[i].DelBuff(buffIndex);
                                buffIndex = Main.player[i].FindBuffIndex(ModContent.BuffType<BulletTimeBuff>());
                            }
                        }
                        
                        // Small delay to ensure complete removal
                        // Apply new buff
                        Main.player[i].AddBuff(ModContent.BuffType<BulletTimeBuff>(), duration);
                        
                        // Verify the buff was applied
                        bool hasBuffAfter = Main.player[i].HasBuff(ModContent.BuffType<BulletTimeBuff>());
                        int buffTimeAfter = hasBuffAfter ? Main.player[i].buffTime[Main.player[i].FindBuffIndex(ModContent.BuffType<BulletTimeBuff>())] : 0;
                        
                        mod.Logger.Info($"Applied bullet time buff to player {i} - HadBefore: {hadBuff}, HasAfter: {hasBuffAfter}, BuffTime: {buffTimeAfter}");
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
            else if (messageType == 3) // Activation confirmation/denial
            {
                int targetPlayer = reader.ReadInt32();
                bool wasAccepted = reader.ReadBoolean();
                
                mod.Logger.Info($"Received activation response for player {targetPlayer}: {(wasAccepted ? "ACCEPTED" : "DENIED")}");
                
                // Only process if this is for the local player
                if (targetPlayer == Main.LocalPlayer.whoAmI)
                {
                    var localPlayer = Main.LocalPlayer.GetModPlayer<ChronosPlayer>();
                    if (wasAccepted)
                    {
                        // Server accepted our request - set cooldown
                        localPlayer.bulletTimeCooldown = ChronosWatch.CooldownDuration;
                        mod.Logger.Info($"Client {targetPlayer} setting cooldown after server acceptance");
                    }
                    else
                    {
                        // Server denied our request - don't set cooldown, show message
                        mod.Logger.Info($"Client {targetPlayer} request was denied by server");
                        
                        // Optional: Show a message to the player
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            Main.NewText("Bullet time activation failed - already active or on cooldown", Microsoft.Xna.Framework.Color.Red);
                        }
                    }
                }
            }
            else if (messageType == 4) // Cleanup sync from server
            {
                mod.Logger.Info("Received cleanup sync from server - clearing any stale bullet time buffs");
                
                // Force clear ALL bullet time buffs from ALL players
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active)
                    {
                        bool hadBuff = Main.player[i].HasBuff(ModContent.BuffType<BulletTimeBuff>());
                        if (hadBuff)
                        {
                            // Use DelBuff for thorough removal
                            int buffIndex = Main.player[i].FindBuffIndex(ModContent.BuffType<BulletTimeBuff>());
                            while (buffIndex != -1)
                            {
                                Main.player[i].DelBuff(buffIndex);
                                mod.Logger.Info($"Client cleanup: Removed bullet time buff from player {i}");
                                buffIndex = Main.player[i].FindBuffIndex(ModContent.BuffType<BulletTimeBuff>());
                            }
                        }
                        
                        // Also reset their ChronosPlayer state
                        var chronosPlayer = Main.player[i].GetModPlayer<ChronosPlayer>();
                        if (i == Main.LocalPlayer.whoAmI)
                        {
                            // Only reset cooldown for local player if they're not the ones who should have it
                            chronosPlayer.bulletTimeCooldown = 0;
                        }
                    }
                }
            }
        }
        
        private static void ApplyBulletTimeToAllPlayers(Mod mod, int activatingPlayer)
        {
            mod.Logger.Info($"Server applying bullet time to all players, activated by {activatingPlayer}");
            
            // COMPREHENSIVE buff state reset and application
            int successfulApplications = 0;
            
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active)
                {
                    // Method 1: Complete buff removal using DelBuff
                    bool hadBuff = Main.player[i].HasBuff(ModContent.BuffType<BulletTimeBuff>());
                    if (hadBuff)
                    {
                        int buffIndex = Main.player[i].FindBuffIndex(ModContent.BuffType<BulletTimeBuff>());
                        while (buffIndex != -1)
                        {
                            Main.player[i].DelBuff(buffIndex);
                            buffIndex = Main.player[i].FindBuffIndex(ModContent.BuffType<BulletTimeBuff>());
                        }
                        mod.Logger.Info($"Completely cleared existing bullet time buff from player {i}");
                    }
                    
                    // Method 2: Apply new buff
                    Main.player[i].AddBuff(ModContent.BuffType<BulletTimeBuff>(), ChronosWatch.BulletTimeDuration);
                    
                    // Method 3: Verify application
                    bool hasBuffAfter = Main.player[i].HasBuff(ModContent.BuffType<BulletTimeBuff>());
                    int buffTimeAfter = 0;
                    if (hasBuffAfter)
                    {
                        int buffIndex = Main.player[i].FindBuffIndex(ModContent.BuffType<BulletTimeBuff>());
                        if (buffIndex != -1)
                        {
                            buffTimeAfter = Main.player[i].buffTime[buffIndex];
                            successfulApplications++;
                        }
                    }
                    
                    mod.Logger.Info($"Applied bullet time buff to player {i} - Success: {hasBuffAfter}, BuffTime: {buffTimeAfter}");
                }
            }
            
            mod.Logger.Info($"Server successfully applied bullet time to {successfulApplications} players");
            
            // Sync this to all clients
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)1); // Message type: Sync bullet time activation
            packet.Write(activatingPlayer);
            packet.Write(ChronosWatch.BulletTimeDuration);
            packet.Send();
            
            mod.Logger.Info("Server sent bullet time sync packet to all clients");
        }
        
        private static void SendActivationConfirmation(Mod mod, int targetPlayer, bool wasAccepted)
        {
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)3); // Message type: Activation confirmation
            packet.Write(targetPlayer);
            packet.Write(wasAccepted);
            packet.Send(targetPlayer); // Send only to the specific player
            
            mod.Logger.Info($"Server sent activation {(wasAccepted ? "confirmation" : "denial")} to player {targetPlayer}");
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