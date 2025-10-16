using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;

namespace LackOfNameStuff.Systems
{
    public class ChronosKeybindSystem : ModSystem
    {
        public ModKeybind BulletTimeKey { get; private set; }
        public ModKeybind HammerDashKey { get; private set; }
        public ModKeybind HammerParryKey { get; private set; }

        public override void PostSetupContent()
        {
            BulletTimeKey = KeybindLoader.RegisterKeybind(Mod, "Bullet Time", Keys.V);
            HammerDashKey = KeybindLoader.RegisterKeybind(Mod, "Hammer Dash", Keys.G);
            HammerParryKey = KeybindLoader.RegisterKeybind(Mod, "Hammer Parry", Keys.H);
        }
    }
}