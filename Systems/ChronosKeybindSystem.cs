using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;

namespace LackOfNameStuff.Systems
{
    public class ChronosKeybindSystem : ModSystem
    {
        public ModKeybind BulletTimeKey { get; private set; }

        public override void PostSetupContent()
        {
            BulletTimeKey = KeybindLoader.RegisterKeybind(Mod, "Bullet Time", Keys.V);
        }
    }
}