using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using LackOfNameStuff.Items.Accessories;

namespace LackOfNameStuff.Worlds
{
    public class ChronosWorld : ModSystem
    {
        // Global bullet time state - syncs automatically in multiplayer
        public static bool GlobalBulletTimeActive { get; private set; } = false;
        public static int GlobalBulletTimeRemaining { get; private set; } = 0;
        public static int GlobalBulletTimeOwnerWhoAmI { get; private set; } = -1;
        public static Vector2 GlobalBulletTimeOrigin { get; private set; } = Vector2.Zero;
        public static string GlobalBulletTimeOwnerName { get; private set; } = "";

        // Helper property to get the actual owner player
        public static Player GlobalBulletTimeOwner
        {
            get
            {
                if (GlobalBulletTimeOwnerWhoAmI >= 0 && GlobalBulletTimeOwnerWhoAmI < Main.maxPlayers && Main.player[GlobalBulletTimeOwnerWhoAmI].active)
                {
                    return Main.player[GlobalBulletTimeOwnerWhoAmI];
                }
                return null;
            }
        }

        public override void PostUpdateWorld()
        {
            // Update bullet time state every frame
            UpdateGlobalBulletTime();
        }

        private void UpdateGlobalBulletTime()
        {
            bool wasActive = GlobalBulletTimeActive;

            if (GlobalBulletTimeActive && GlobalBulletTimeRemaining > 0)
            {
                // Countdown the remaining time
                GlobalBulletTimeRemaining--;
                
                if (GlobalBulletTimeRemaining <= 0)
                {
                    // Bullet time expired
                    DeactivateBulletTime();
                }
            }
            else if (GlobalBulletTimeActive)
            {
                // Safety check - deactivate if somehow remaining time is 0 but still active
                DeactivateBulletTime();
            }

            // Debug output for all game modes
            if (GlobalBulletTimeActive != wasActive)
            {
                string modeText = Main.netMode == NetmodeID.SinglePlayer ? "[SP]" : 
                                 Main.netMode == NetmodeID.MultiplayerClient ? "[MP-Client]" : "[MP-Server]";
                Main.NewText($"{modeText} Bullet Time {(GlobalBulletTimeActive ? "Activated" : "Deactivated")} by {GlobalBulletTimeOwnerName} - Remaining: {GlobalBulletTimeRemaining}", 
                    GlobalBulletTimeActive ? Color.Cyan : Color.Orange);
            }
        }

        // Force world sync in multiplayer
        private static void SyncWorldData()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                // On server, sync to all clients
                NetMessage.SendData(MessageID.WorldData);
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // On client, request sync from server
                NetMessage.SendData(MessageID.RequestWorldData);
            }
        }

        // Called by players to activate bullet time
        public static void ActivateBulletTime(Player player)
        {
            if (GlobalBulletTimeActive) return; // Already active

            GlobalBulletTimeActive = true;
            GlobalBulletTimeRemaining = ChronosWatch.BulletTimeDuration;
            GlobalBulletTimeOwnerWhoAmI = player.whoAmI;
            GlobalBulletTimeOrigin = player.Center;
            GlobalBulletTimeOwnerName = player.name;

            // Apply buff to all players
            ApplyBulletTimeBuffToAllPlayers();

            // Sync in multiplayer
            SyncWorldData();
        }

        // Called when bullet time naturally expires or is manually deactivated
        public static void DeactivateBulletTime()
        {
            if (!GlobalBulletTimeActive) return; // Already inactive

            GlobalBulletTimeActive = false;
            GlobalBulletTimeRemaining = 0;
            GlobalBulletTimeOwnerWhoAmI = -1;
            GlobalBulletTimeOwnerName = "";
            // Keep origin for visual effects

            // Remove buff from all players
            RemoveBulletTimeBuffFromAllPlayers();

            // Sync in multiplayer
            SyncWorldData();
        }

        private static void ApplyBulletTimeBuffToAllPlayers()
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active)
                {
                    // Add the bullet time buff - we'll create this buff next
                    Main.player[i].AddBuff(ModContent.BuffType<Buffs.BulletTimeBuff>(), ChronosWatch.BulletTimeDuration);
                }
            }
        }

        private static void RemoveBulletTimeBuffFromAllPlayers()
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active)
                {
                    // Remove the bullet time buff
                    Main.player[i].DelBuff(Main.player[i].FindBuffIndex(ModContent.BuffType<Buffs.BulletTimeBuff>()));
                }
            }
        }

        public override void PostSetupContent()
        {
            // Reset state when mod loads
            GlobalBulletTimeActive = false;
            GlobalBulletTimeRemaining = 0;
            GlobalBulletTimeOwnerWhoAmI = -1;
            GlobalBulletTimeOrigin = Vector2.Zero;
            GlobalBulletTimeOwnerName = "";
        }
    }
}