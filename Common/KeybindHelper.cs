using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using LackOfNameStuff.Systems;

namespace LackOfNameStuff.Common
{
    public static class KeybindHelper
    {
        private static readonly HashSet<ModKeybind> NotifiedUnboundKeys = new();

        public static bool HasBinding(ModKeybind keybind)
        {
            if (keybind == null)
            {
                return false;
            }

            try
            {
                return keybind.GetAssignedKeys().Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public static bool JustPressed(ModKeybind keybind)
        {
            return HasBinding(keybind) && keybind.JustPressed;
        }

        public static string GetBindDisplay(ModKeybind keybind, string fallback = null)
        {
            if (keybind == null)
            {
                return fallback ?? Language.GetTextValue("Mods.LackOfNameStuff.Keybinds.Unbound");
            }

            IReadOnlyList<string> assigned;
            try
            {
                assigned = keybind.GetAssignedKeys();
            }
            catch
            {
                assigned = System.Array.Empty<string>();
            }

            if (assigned.Count > 0)
            {
                return string.Join("/", assigned);
            }

            if (fallback != null)
            {
                return fallback;
            }

            string[] defaults = LackOfNameKeybinds.GetDefaults(keybind);
            if (defaults.Length > 0)
            {
                return Language.GetTextValue("Mods.LackOfNameStuff.Keybinds.UnboundWithDefault", string.Join("/", defaults));
            }

            return Language.GetTextValue("Mods.LackOfNameStuff.Keybinds.Unbound");
        }

        public static bool EnsureBound(ModKeybind keybind, Player player, string localizationKey = "Mods.LackOfNameStuff.Keybinds.BindReminder")
        {
            if (HasBinding(keybind))
            {
                return true;
            }

            if (keybind != null && NotifiedUnboundKeys.Add(keybind) && Main.netMode != Terraria.ID.NetmodeID.Server)
            {
                string actionName = keybind.DisplayName.Value;
                string message = Language.GetTextValue(localizationKey, actionName);
                Main.NewText(message, Color.OrangeRed);
            }

            return false;
        }

        public static void ResetNotifications()
        {
            NotifiedUnboundKeys.Clear();
        }
    }
}
