using System.IO;
using Terraria.ModLoader;
using LackOfNameStuff.Systems;

namespace LackOfNameStuff
{
    public class LackOfNameStuff : Mod
    {
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            ChronosNetworkHandler.HandlePacket(this, reader, whoAmI);
        }
    }
}