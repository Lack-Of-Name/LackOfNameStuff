using System.Collections.Generic;
using LackOfNameStuff.Common;
using Terraria.ModLoader;

namespace LackOfNameStuff.Systems
{
    public class LackOfNameKeybinds : ModSystem
    {
        private static readonly Dictionary<ModKeybind, string[]> DefaultBindings = new();

        public static ModKeybind BulletTime { get; private set; }
        public static ModKeybind HammerDash { get; private set; }
        public static ModKeybind HammerParry { get; private set; }

        public override void Load()
        {
            BulletTime = Register("BulletTime", "V");
            HammerDash = Register("HammerDash", "G");
            HammerParry = Register("HammerParry", "H");
        }

        public override void Unload()
        {
            BulletTime = null;
            HammerDash = null;
            HammerParry = null;
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
            ModKeybind keybind = KeybindLoader.RegisterKeybind(Mod, name, defaults);
            DefaultBindings[keybind] = defaults;
            return keybind;
        }
    }
}
