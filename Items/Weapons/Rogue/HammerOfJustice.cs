using System.Collections.Generic;
using LackOfNameStuff.Common;
using LackOfNameStuff.Players;
using LackOfNameStuff.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace LackOfNameStuff.Items.Weapons.Rogue
{
    public class HammerOfJustice : ModItem
    {
        private DamageClass ResolveDamageClass()
        {
            if (CalamityIntegration.CalamityLoaded &&
                CalamityIntegration.CalamityMod.TryFind("RogueDamageClass", out DamageClass rogueDamage))
            {
                return rogueDamage;
            }

            return DamageClass.Ranged;
        }

        public override void SetDefaults()
        {
            Item.width = 58;
            Item.height = 58;
            Item.damage = 670;
            Item.DamageType = ResolveDamageClass();
            Item.useTime = 29;
            Item.useAnimation = 29;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 11f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.value = Item.sellPrice(platinum: 1, gold: 20);
            Item.rare = ItemRarityID.Red;
            Item.hammer = 220;
            Item.scale = 1.25f;
            Item.crit = 14;
        }

        public override void HoldItem(Player player)
        {
            player.GetModPlayer<HammerOfJusticePlayer>().HasHammerEquipped = true;
        }

        public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.Knockback *= 1.25f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.LocalPlayer == null)
            {
                return;
            }

            TooltipLine nameLine = tooltips.Find(line => line.Name == "ItemName" && line.Mod == "Terraria");
            if (nameLine != null)
            {
                Color pink = new Color(252, 111, 241);
                Color green = new Color(99, 171, 37);
                float t = (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.5 + 0.5);
                nameLine.OverrideColor = Color.Lerp(pink, green, t);
            }

            HammerOfJusticePlayer hammerPlayer = Main.LocalPlayer.GetModPlayer<HammerOfJusticePlayer>();
            ChronosKeybindSystem keybinds = ModContent.GetInstance<ChronosKeybindSystem>();

            string dashBind = GetKeybindName(keybinds.HammerDashKey, "G");
            string parryBind = GetKeybindName(keybinds.HammerParryKey, "H");

            TooltipLine dashLine = new TooltipLine(Mod, "HammerDash", $"Press '{dashBind}' to unleash an omnidirectional justice dash");
            dashLine.OverrideColor = new Color(255, 198, 92);
            tooltips.Add(dashLine);

            if (hammerPlayer.DashCooldownTimer > 0)
            {
                float seconds = hammerPlayer.DashCooldownTimer / 60f;
                TooltipLine dashCooldown = new TooltipLine(Mod, "HammerDashCooldown", $"Dash cooldown: {seconds:F1}s");
                dashCooldown.OverrideColor = Color.OrangeRed;
                tooltips.Add(dashCooldown);
            }
            else
            {
                TooltipLine dashReady = new TooltipLine(Mod, "HammerDashReady", "Dash ready");
                dashReady.OverrideColor = Color.LimeGreen;
                tooltips.Add(dashReady);
            }

            TooltipLine parryLine = new TooltipLine(Mod, "HammerParry", $"Tap '{parryBind}' to parry and reflect nearby projectiles");
            parryLine.OverrideColor = new Color(190, 225, 255);
            tooltips.Add(parryLine);

            if (hammerPlayer.ParryCooldownTimer > 0 && hammerPlayer.ParryChainWindowTimer <= 0)
            {
                float seconds = hammerPlayer.ParryCooldownTimer / 60f;
                TooltipLine parryCooldown = new TooltipLine(Mod, "HammerParryCooldown", $"Parry cooldown: {seconds:F1}s");
                parryCooldown.OverrideColor = Color.Crimson;
                tooltips.Add(parryCooldown);
            }
            else if (hammerPlayer.ParryChainWindowTimer > 0)
            {
                float seconds = hammerPlayer.ParryChainWindowTimer / 60f;
                TooltipLine parryChain = new TooltipLine(Mod, "HammerParryChain", $"Chain window: {seconds:F1}s");
                parryChain.OverrideColor = Color.LightGoldenrodYellow;
                tooltips.Add(parryChain);
            }
            else
            {
                TooltipLine parryReady = new TooltipLine(Mod, "HammerParryReady", "Parry ready");
                parryReady.OverrideColor = Color.LimeGreen;
                tooltips.Add(parryReady);
            }

            TooltipLine stealthLine = new TooltipLine(Mod, "HammerStealthStrike", "Chain dashes to unleash a reality-splitting Hammerfall");
            stealthLine.OverrideColor = new Color(255, 180, 90);
            tooltips.Add(stealthLine);
        }

        private static string GetKeybindName(ModKeybind keybind, string fallback)
        {
            try
            {
                if (keybind != null && keybind.GetAssignedKeys().Count > 0)
                {
                    return keybind.GetAssignedKeys()[0].ToString();
                }
            }
            catch
            {
                // ignore
            }

            return fallback;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.TurtleShell, 2);
            recipe.AddIngredient(ItemID.ChlorophyteBar, 18);
            recipe.AddIngredient(ItemID.FragmentSolar, 12);
            recipe.AddIngredient(ItemID.FragmentVortex, 8);
            recipe.AddIngredient(ModContent.ItemType<Items.Materials.EternalGem>(), 1);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();

            if (CalamityIntegration.TryGetCalamityItem("CosmiliteBar", out int cosmiliteBar))
            {
                Recipe calamityRecipe = CreateRecipe();
                calamityRecipe.AddIngredient(cosmiliteBar, 14);
                if (CalamityIntegration.TryGetCalamityItem("YharonSoulFragment", out int yharonSoul))
                {
                    calamityRecipe.AddIngredient(yharonSoul, 5);
                }
                calamityRecipe.AddIngredient(ItemID.TurtleShell, 1);
                calamityRecipe.AddIngredient(ModContent.ItemType<Items.Materials.EternalGem>(), 1);
                calamityRecipe.AddTile(TileID.LunarCraftingStation);
                calamityRecipe.Register();
            }
        }
    }
}
