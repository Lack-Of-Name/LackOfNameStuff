using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System.Collections.Generic;
using LackOfNameStuff.Players;
using LackOfNameStuff.Systems;
using LackOfNameStuff.Worlds;

namespace LackOfNameStuff.Items.Debug
{
    public class ChronosStopper : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.value = 0;
            Item.rare = ItemRarityID.Red;
            Item.consumable = false;
            Item.autoReuse = false;
            Item.useTurn = false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // Add item name as first line
            TooltipLine titleLine = new TooltipLine(Mod, "ItemName", "Chronos Debug Stopper");
            titleLine.OverrideColor = Color.Red;
            tooltips.Insert(0, titleLine);

            TooltipLine debugLine = new TooltipLine(Mod, "DebugWarning", 
                "DEBUG ITEM: Emergency stops all bullet time effects");
            debugLine.OverrideColor = Color.Red;
            tooltips.Add(debugLine);

            TooltipLine statusLine = new TooltipLine(Mod, "CurrentStatus", 
                $"Current bullet time active: {ChronosSystem.GlobalBulletTimeActive}");
            statusLine.OverrideColor = ChronosSystem.GlobalBulletTimeActive ? Color.Cyan : Color.Gray;
            tooltips.Add(statusLine);

            if (ChronosSystem.GlobalBulletTimeActive)
            {
                TooltipLine remainingLine = new TooltipLine(Mod, "TimeRemaining", 
                    $"Time remaining: {ChronosSystem.GlobalBulletTimeRemaining / 60f:F1}s");
                remainingLine.OverrideColor = Color.Yellow;
                tooltips.Add(remainingLine);
            }

            TooltipLine intensityLine = new TooltipLine(Mod, "ScreenIntensity", 
                $"Screen effect intensity: {ChronosSystem.GlobalScreenEffectIntensity * 100f:F0}%");
            intensityLine.OverrideColor = Color.Orange;
            tooltips.Add(intensityLine);
        }

        public override bool? UseItem(Player player)
        {
            // Force stop all bullet time effects immediately
            EmergencyStopBulletTime();
            
            // Visual feedback
            for (int i = 0; i < 20; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(8f, 8f);
                Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, 
                    DustID.Electric, velocity.X, velocity.Y, 100, Color.Red, 1.5f);
                dust.noGravity = true;
            }

            // Chat message for feedback
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText("Bullet time effects forcibly stopped!", Color.Red);
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                Main.NewText("Bullet time effects forcibly stopped on client!", Color.Red);
            }

            return true;
        }

        private void EmergencyStopBulletTime()
        {
            // Method 1: Force deactivate through the world system
            ChronosWorld.DeactivateBulletTime();

            // Method 2: Reset ChronosSystem screen effects
            var chronosSystemType = typeof(ChronosSystem);
            var fields = chronosSystemType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            
            foreach (var field in fields)
            {
                // Reset screen effect fields
                if (field.Name.Contains("ScreenEffect") || field.Name.Contains("screenEffect"))
                {
                    try
                    {
                        if (field.FieldType == typeof(float))
                            field.SetValue(null, 0f);
                    }
                    catch { /* Ignore read-only fields */ }
                }
            }

            // Method 3: Reset all players' ChronosPlayer state
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active)
                {
                    var chronosPlayer = Main.player[i].GetModPlayer<ChronosPlayer>();
                    
                    // Reset the fields we can access (not the read-only properties)
                    chronosPlayer.bulletTimeCooldown = 0;
                    chronosPlayer.activeRipples.Clear();
                    chronosPlayer.wasGlobalBulletTimeActive = false;
                    
                    // Reset any private fields in the player using reflection
                    var playerFields = typeof(ChronosPlayer).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    foreach (var field in playerFields)
                    {
                        try
                        {
                            if (field.Name.Contains("bulletTime") || field.Name.Contains("BulletTime") || 
                                field.Name.Contains("screenEffect") || field.Name.Contains("ScreenEffect"))
                            {
                                if (field.FieldType == typeof(bool))
                                    field.SetValue(chronosPlayer, false);
                                else if (field.FieldType == typeof(int))
                                    field.SetValue(chronosPlayer, 0);
                                else if (field.FieldType == typeof(float))
                                    field.SetValue(chronosPlayer, 0f);
                            }
                        }
                        catch { /* Ignore issues */ }
                    }
                }
            }

            // Method 4: Force reset ChronosWorld state using reflection
            var chronosWorldType = typeof(ChronosWorld);
            var worldFields = chronosWorldType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            foreach (var field in worldFields)
            {
                try
                {
                    if (field.Name.Contains("GlobalBulletTime"))
                    {
                        if (field.FieldType == typeof(bool))
                            field.SetValue(null, false);
                        else if (field.FieldType == typeof(int))
                            field.SetValue(null, -1); // Reset owner to -1, remaining to 0
                        else if (field.FieldType == typeof(Vector2))
                            field.SetValue(null, Vector2.Zero);
                        else if (field.FieldType == typeof(string))
                            field.SetValue(null, "");
                    }
                }
                catch { /* Ignore read-only fields */ }
            }

            // Method 5: Unfreeze any entities that might still be frozen (simplified version)
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].velocity == Vector2.Zero)
                {
                    // Give frozen NPCs a tiny random velocity to break any frozen state
                    Main.npc[i].velocity = new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), Main.rand.NextFloat(-0.1f, 0.1f));
                }
            }

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].velocity == Vector2.Zero && Main.projectile[i].hostile)
                {
                    // Give frozen hostile projectiles some velocity
                    Main.projectile[i].velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
                }
            }

            // Method 6: Force multiple system updates to break any detection cycles
            try
            {
                var systemInstance = ModContent.GetInstance<ChronosSystem>();
                if (systemInstance != null)
                {
                    // Force multiple updates to break any detection cycles
                    for (int cycle = 0; cycle < 5; cycle++)
                    {
                        systemInstance.PostUpdateEverything();
                    }
                }
            }
            catch { /* Ignore if this fails */ }

            // Send feedback messages
            Main.NewText("Emergency stop executed - all bullet time effects cleared", Color.Red);
            Main.NewText("World state reset, player states cleared, entities unfrozen", Color.Yellow);
        }

        public override void SetStaticDefaults()
        {
            // Modern tModLoader 1.4+ syntax
        }
    }
}