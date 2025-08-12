using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using LackOfNameStuff.Players;
using LackOfNameStuff.Systems;

namespace LackOfNameStuff.Items.Accessories
{
    public class ChronosWatch : ModItem
    {
        // === CONFIGURATION ===
        public static readonly int CooldownDuration = 1200; // 60 = 1 second at 60fps
        public static readonly int BulletTimeDuration = 36000; // 60 = 1 second at 60fps
        public static readonly float TimeSlowFactor = 0.25f; // 0.25f = 25% normal speed
        public static readonly bool SlowPlayerToo = false; // Players remain at normal speed
        public static readonly float RippleMaxRadius = 800f;
        public static readonly int RippleLifetime = 120; // 120 = 2 seconds
        public static readonly int ItemValue = Item.sellPrice(gold: 15);
        public static readonly int ItemRarity = ItemRarityID.Red;

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.value = ItemValue;
            Item.rare = ItemRarity;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ChronosPlayer>().hasChronosWatch = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var player = Main.LocalPlayer.GetModPlayer<ChronosPlayer>();
            
            string statusText;
            if (player.bulletTimeActive)
            {
                int remainingFrames = player.bulletTimeRemaining;
                float remainingSeconds = remainingFrames / 60f;
                statusText = $"BULLET TIME ACTIVE: {remainingSeconds:F1}s remaining";
            }
            else if (player.bulletTimeCooldown > 0)
            {
                int cooldownFrames = player.bulletTimeCooldown;
                float cooldownSeconds = cooldownFrames / 60f;
                statusText = $"Cooldown: {cooldownSeconds:F1}s";
            }
            else
            {
                statusText = "Ready to use";
            }

            string keyName = "V"; // Default fallback
            try
            {
                var keybind = ModContent.GetInstance<ChronosSystem>().BulletTimeKey;
                if (keybind != null && keybind.GetAssignedKeys().Count > 0)
                {
                    keyName = keybind.GetAssignedKeys()[0].ToString();
                }
            }
            catch
            {
                // Fallback to default if keybind not available
            }

            TooltipLine keyLine = new TooltipLine(Mod, "BulletTimeKey", 
                $"Press '{keyName}' to activate bullet time");
            keyLine.OverrideColor = Color.Gold;
            tooltips.Add(keyLine);

            TooltipLine statusLine = new TooltipLine(Mod, "BulletTimeStatus", statusText);
            statusLine.OverrideColor = player.bulletTimeActive ? Color.Cyan : 
                                     player.bulletTimeCooldown > 0 ? Color.Red : Color.LimeGreen;
            tooltips.Add(statusLine);

            TooltipLine durationLine = new TooltipLine(Mod, "BulletTimeDuration", 
                $"Freezes enemies for {BulletTimeDuration / 60f:F0} seconds, slows item use");
            durationLine.OverrideColor = Color.LightBlue;
            tooltips.Add(durationLine);
        }

        // Example recipe - adjust as needed
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 15);
            recipe.AddIngredient(ItemID.FragmentSolar, 10);
            recipe.AddIngredient(ItemID.GoldWatch, 1);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}