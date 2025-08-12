using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System.Collections.Generic;
using LackOfNameStuff.Players;
using LackOfNameStuff.Systems;
using LackOfNameStuff.Globals;

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
            // Method 1: Direct field manipulation using reflection on backing fields
            var chronosSystemType = typeof(ChronosSystem);
            
            // Get all static fields and properties, including private backing fields
            var fields = chronosSystemType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            
            foreach (var field in fields)
            {
                // Reset any field that sounds like bullet time state
                if (field.Name.Contains("BulletTime") || field.Name.Contains("bulletTime") || 
                    field.Name.Contains("ScreenEffect") || field.Name.Contains("screenEffect") ||
                    field.Name.Contains("frozenEnemy") || field.Name.Contains("framesSince"))
                {
                    try
                    {
                        if (field.FieldType == typeof(bool))
                            field.SetValue(null, false);
                        else if (field.FieldType == typeof(int))
                            field.SetValue(null, field.Name.Contains("framesSince") ? 999 : 0); // Set framesSince to high value
                        else if (field.FieldType == typeof(float))
                            field.SetValue(null, 0f);
                        else if (field.FieldType == typeof(Player))
                            field.SetValue(null, null);
                        else if (field.FieldType == typeof(Vector2))
                            field.SetValue(null, Vector2.Zero);
                    }
                    catch { /* Ignore read-only fields */ }
                }
            }

            // Method 2: Force reset all players' bullet time state completely
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active)
                {
                    var chronosPlayer = Main.player[i].GetModPlayer<ChronosPlayer>();
                    chronosPlayer.bulletTimeActive = false;
                    chronosPlayer.bulletTimeRemaining = 0;
                    chronosPlayer.bulletTimeCooldown = 0;
                    chronosPlayer.activeRipples.Clear();
                    chronosPlayer.wasGlobalBulletTimeActive = false;
                    
                    // Also reset any private fields in the player
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

            // Method 3: AGGRESSIVELY unfreeze ALL entities and clear their global state
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active)
                {
                    // Force unfreeze the NPC by giving it some velocity if it's stuck
                    if (Main.npc[i].velocity == Vector2.Zero && Main.npc[i].position == Main.npc[i].oldPosition)
                    {
                        // Give it a tiny random velocity to break the frozen state
                        Main.npc[i].velocity = new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), Main.rand.NextFloat(-0.1f, 0.1f));
                        Main.npc[i].position += new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.5f, 0.5f));
                    }

                    // Reset any stored velocity in global NPC
                    try
                    {
                        var globalNPC = Main.npc[i].GetGlobalNPC<ChronosGlobalNPC>();
                        var globalFields = typeof(ChronosGlobalNPC).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                        
                        foreach (var field in globalFields)
                        {
                            if (field.Name.Contains("stored") || field.Name.Contains("Stored") || field.Name.Contains("has"))
                            {
                                if (field.FieldType == typeof(Vector2))
                                    field.SetValue(globalNPC, Vector2.Zero);
                                else if (field.FieldType == typeof(bool))
                                    field.SetValue(globalNPC, false);
                            }
                        }
                    }
                    catch { /* Ignore if global doesn't exist */ }
                }
            }

            // Method 4: Force restore projectile velocities and unfreeze them
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active)
                {
                    // If projectile is frozen (zero velocity), give it some movement
                    if (Main.projectile[i].velocity == Vector2.Zero)
                    {
                        Main.projectile[i].velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
                    }

                    try
                    {
                        var globalProj = Main.projectile[i].GetGlobalProjectile<ChronosGlobalProjectile>();
                        var globalFields = typeof(ChronosGlobalProjectile).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                        
                        foreach (var field in globalFields)
                        {
                            if (field.Name.Contains("stored") || field.Name.Contains("Stored"))
                            {
                                if (field.FieldType == typeof(Vector2))
                                {
                                    var storedVel = (Vector2)field.GetValue(globalProj);
                                    if (storedVel != Vector2.Zero)
                                    {
                                        Main.projectile[i].velocity = storedVel;
                                    }
                                    field.SetValue(globalProj, Vector2.Zero);
                                }
                            }
                            else if (field.Name.Contains("has") && field.FieldType == typeof(bool))
                            {
                                field.SetValue(globalProj, false);
                            }
                        }
                    }
                    catch { /* Ignore if global doesn't exist */ }
                }
            }

            // Method 5: Nuclear option - force multiple system updates to break the detection loop
            try
            {
                var systemInstance = ModContent.GetInstance<ChronosSystem>();
                if (systemInstance != null)
                {
                    // Force multiple updates to break the detection cycle
                    for (int cycle = 0; cycle < 10; cycle++)
                    {
                        // Reset detection fields each cycle
                        var frozenField = chronosSystemType.GetField("frozenEnemyCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                        var framesField = chronosSystemType.GetField("framesSinceBulletTimeDetected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                        var lastFrameField = chronosSystemType.GetField("lastFrameBulletTimeActive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                        
                        frozenField?.SetValue(null, 0);
                        framesField?.SetValue(null, 999);
                        lastFrameField?.SetValue(null, false);
                        
                        // Force system update
                        systemInstance.PostUpdateEverything();
                    }
                }
            }
            catch { /* Ignore if this fails */ }

            // Method 6: Add a temporary "disable detection" flag if possible
            try
            {
                // Try to set a flag that might disable the detection temporarily
                var disableField = chronosSystemType.GetField("disableDetection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                disableField?.SetValue(null, true);
            }
            catch { /* Field doesn't exist, that's fine */ }

            // Method 7: Send a chat message so we know it actually ran
            Main.NewText("AGGRESSIVE emergency stop executed - unfroze all entities", Color.Red);
            Main.NewText("If effects return, the detection system is re-triggering", Color.Yellow);
        }

        public override void SetStaticDefaults()
        {
            // Modern tModLoader 1.4+ syntax
        }
    }
}