using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using LackOfNameStuff.Players;
using LackOfNameStuff.Systems;
using LackOfNameStuff.Buffs;

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
                "DEBUG ITEM: Emergency removes bullet time buff from all players");
            debugLine.OverrideColor = Color.Red;
            tooltips.Add(debugLine);

            // Check local player's bullet time status
            var localPlayer = Main.LocalPlayer.GetModPlayer<ChronosPlayer>();
            bool hasBulletTimeBuff = Main.LocalPlayer.HasBuff<BulletTimeBuff>();
            
            TooltipLine statusLine = new TooltipLine(Mod, "CurrentStatus", 
                $"Local player has bullet time buff: {hasBulletTimeBuff}");
            statusLine.OverrideColor = hasBulletTimeBuff ? Color.Cyan : Color.Gray;
            tooltips.Add(statusLine);

            if (hasBulletTimeBuff)
            {
                int buffIndex = Main.LocalPlayer.FindBuffIndex(ModContent.BuffType<BulletTimeBuff>());
                if (buffIndex >= 0)
                {
                    int remainingTime = Main.LocalPlayer.buffTime[buffIndex];
                    TooltipLine remainingLine = new TooltipLine(Mod, "TimeRemaining", 
                        $"Time remaining: {remainingTime / 60f:F1}s");
                    remainingLine.OverrideColor = Color.Yellow;
                    tooltips.Add(remainingLine);
                }
            }

            TooltipLine intensityLine = new TooltipLine(Mod, "ScreenIntensity", 
                $"Screen effect intensity: {localPlayer.screenEffectIntensity * 100f:F0}%");
            intensityLine.OverrideColor = Color.Orange;
            tooltips.Add(intensityLine);

            TooltipLine cooldownLine = new TooltipLine(Mod, "Cooldown", 
                $"Bullet time cooldown: {localPlayer.bulletTimeCooldown / 60f:F1}s");
            cooldownLine.OverrideColor = localPlayer.bulletTimeCooldown > 0 ? Color.Red : Color.Green;
            tooltips.Add(cooldownLine);
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
            Main.NewText("Bullet time buffs forcibly removed from all players!", Color.Red);

            return true;
        }

        private void EmergencyStopBulletTime()
        {
            // Method 1: Remove bullet time buff from all players
            int bulletTimeBuffType = ModContent.BuffType<BulletTimeBuff>();
            
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active)
                {
                    // Remove the bullet time buff
                    int buffIndex = Main.player[i].FindBuffIndex(bulletTimeBuffType);
                    if (buffIndex >= 0)
                    {
                        Main.player[i].DelBuff(buffIndex);
                        Main.NewText($"Removed bullet time buff from {Main.player[i].name}", Color.Orange);
                    }
                }
            }

            // Method 2: Reset all players' ChronosPlayer state
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active)
                {
                    var chronosPlayer = Main.player[i].GetModPlayer<ChronosPlayer>();
                    
                    // Reset the fields we can access
                    chronosPlayer.bulletTimeCooldown = 0;
                    chronosPlayer.activeRipples.Clear();
                    chronosPlayer.screenEffectIntensity = 0f;
                    
                    // Reset any private fields in the player using reflection
                    var playerFields = typeof(ChronosPlayer).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    foreach (var field in playerFields)
                    {
                        try
                        {
                            if (field.Name.Contains("screenEffect") || field.Name.Contains("ScreenEffect"))
                            {
                                if (field.FieldType == typeof(float))
                                    field.SetValue(chronosPlayer, 0f);
                            }
                        }
                        catch { /* Ignore issues */ }
                    }
                }
            }

            // Method 3: Force unfreeze entities by resetting their global states
            // Reset NPCs
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active)
                {
                    try
                    {
                        var globalNPC = Main.npc[i].GetGlobalNPC<Globals.ChronosGlobalNPC>();
                        globalNPC.hasStoredState = false;
                        globalNPC.storedVelocity = Vector2.Zero;
                        globalNPC.storedPosition = Vector2.Zero;
                        
                        // Give frozen NPCs a tiny velocity to unstick them
                        if (Main.npc[i].velocity == Vector2.Zero)
                        {
                            Main.npc[i].velocity = new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), Main.rand.NextFloat(-0.1f, 0.1f));
                        }
                    }
                    catch { /* Ignore if global doesn't exist */ }
                }
            }

            // Reset Projectiles
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active)
                {
                    try
                    {
                        var globalProj = Main.projectile[i].GetGlobalProjectile<Globals.ChronosGlobalProjectile>();
                        
                        // Restore stored velocity if it exists
                        if (globalProj.hasStoredVelocity && globalProj.storedVelocity != Vector2.Zero)
                        {
                            Main.projectile[i].velocity = globalProj.storedVelocity;
                        }
                        else if (Main.projectile[i].velocity == Vector2.Zero && Main.projectile[i].hostile)
                        {
                            // Give frozen hostile projectiles some velocity
                            Main.projectile[i].velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
                        }
                        
                        // Reset the global state
                        globalProj.hasStoredVelocity = false;
                        globalProj.storedVelocity = Vector2.Zero;
                    }
                    catch { /* Ignore if global doesn't exist */ }
                }
            }

            // Send feedback messages
            Main.NewText("Emergency stop executed - all bullet time buffs removed", Color.Red);
            Main.NewText("Player states reset, entities unfrozen", Color.Yellow);
        }

        public override void SetStaticDefaults()
        {
            // Modern tModLoader 1.4+ syntax
        }
    }
}