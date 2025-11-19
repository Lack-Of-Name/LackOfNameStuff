using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using LackOfNameStuff.Players;

namespace LackOfNameStuff.Items.Misc
{
    public class DefiledMirror : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.MagicMirror);
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.buyPrice(gold: 5);
        }

        public override bool CanUseItem(Player player)
        {
            var deathTracker = player.GetModPlayer<DefiledMirrorPlayer>();
            return deathTracker.TryGetLastDeathPosition(out _);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
            {
                return false;
            }

            var deathTracker = player.GetModPlayer<DefiledMirrorPlayer>();
            if (!deathTracker.TryGetLastDeathPosition(out Vector2 destination))
            {
                return false;
            }

            PerformTeleport(player, destination);
            deathTracker.InvalidateDeathPosition();
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (tooltips == null)
            {
                return;
            }

            Player localPlayer = Main.LocalPlayer;
            DefiledMirrorPlayer mirrorPlayer = localPlayer?.GetModPlayer<DefiledMirrorPlayer>();
            bool hasStoredDeath = mirrorPlayer != null && mirrorPlayer.TryGetLastDeathPosition(out _);

            string statusKey = hasStoredDeath
                ? "Mods.LackOfNameStuff.Items.DefiledMirror.StatusReady"
                : "Mods.LackOfNameStuff.Items.DefiledMirror.StatusDormant";

            string statusText = Language.GetTextValue(statusKey);
            if (string.IsNullOrWhiteSpace(statusText))
            {
                return;
            }

            Color statusColor = hasStoredDeath ? new Color(120, 200, 255) : new Color(180, 180, 200);
            TooltipLine statusLine = new TooltipLine(Mod, "DefiledMirrorStatus", statusText)
            {
                OverrideColor = statusColor
            };

            tooltips.Add(statusLine);
        }

        private static void PerformTeleport(Player player, Vector2 destination)
        {
            Vector2 source = player.Center;
            player.Teleport(destination, TeleportationStyleID.MagicConch);
            player.fallStart = (int)(destination.Y / 16f);

            for (int i = 0; i < 70; i++)
            {
                Dust.NewDust(destination, player.width, player.height, DustID.PurpleTorch, Scale: 1.2f);
                Dust.NewDust(source, player.width, player.height, DustID.Torch, Scale: 1.2f);
            }

            SoundEngine.PlaySound(SoundID.Item8, player.position);
        }
    }
}
