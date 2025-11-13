using System.Collections.Generic;
using LackOfNameStuff.Common;
using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;

namespace LackOfNameStuff.Systems
{
    public class LackOfNameKeybinds : ModSystem
    {
        private static readonly Dictionary<ModKeybind, string[]> DefaultBindings = new();

        public static ModKeybind BulletTime { get; private set; }
        public static ModKeybind HammerDash { get; private set; }
        public static ModKeybind HammerParry { get; private set; }
        public static ModKeybind Veinmine { get; private set; }

        public override void Load()
        {
            BulletTime = Register("BulletTime", "V");
            HammerDash = Register("HammerDash", "G");
            HammerParry = Register("HammerParry", "H");
            Veinmine = Register("Veinmine", "B");
        }

        public override void Unload()
        {
            BulletTime = null;
            HammerDash = null;
            HammerParry = null;
            Veinmine = null;
            DefaultBindings.Clear();
            KeybindHelper.ResetNotifications();
        }

        internal static string[] GetDefaults(ModKeybind keybind)
        {
            return keybind != null && DefaultBindings.TryGetValue(keybind, out var defaults)
                ? defaults
                : System.Array.Empty<string>();
        }

        private ModKeybind Register(string name, params string[] defaults)
        {
            string primary = (defaults != null && defaults.Length > 0) ? defaults[0] : Keys.None.ToString();

            ModKeybind keybind = KeybindLoader.RegisterKeybind(Mod, name, primary);
            DefaultBindings[keybind] = defaults ?? System.Array.Empty<string>();
            return keybind;
        }
    }
}
