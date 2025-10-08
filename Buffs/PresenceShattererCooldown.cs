using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LackOfNameStuff.Buffs
{
    // A visible cooldown so players know when they can fire again
    public class PresenceShattererCooldown : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = false;
            Main.buffNoSave[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = false; // show timer
        }
    }
}
