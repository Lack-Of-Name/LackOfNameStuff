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

        // Called by players to activate bullet time
        public static void ActivateBulletTime(Player player)
        {
            if (GlobalBulletTimeActive) return; // Already active

            GlobalBulletTimeActive = true;
            GlobalBulletTimeRemaining = ChronosWatch.BulletTimeDuration;
            GlobalBulletTimeOwnerWhoAmI = player.whoAmI;
            GlobalBulletTimeOrigin = player.Center;
            GlobalBulletTimeOwnerName = player.name;

            // Don't apply buffs for now to avoid issues
            // ApplyBulletTimeBuffToAllPlayers();

            // Don't sync world data - let the mod system handle state naturally
            // SyncWorldData();
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

            // Don't remove buffs for now
            // RemoveBulletTimeBuffFromAllPlayers();

            // Don't force sync
            // SyncWorldData();
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