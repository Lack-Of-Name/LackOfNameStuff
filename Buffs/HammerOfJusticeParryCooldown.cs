using Terraria;
using Terraria.ModLoader;

namespace LackOfNameStuff.Buffs
{
    public class HammerOfJusticeParryCooldown : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
