using Terraria;
using Terraria.ModLoader;
using LackOfNameStuff.Players;

namespace LackOfNameStuff.Globals
{
    public class WulfrumVeinminerGlobalTile : GlobalTile
    {
        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (fail || effectOnly)
            {
                return;
            }

            Player player = Main.LocalPlayer;
            if (player == null || !player.active)
            {
                return;
            }

            player.GetModPlayer<WulfrumVeinminerPlayer>().HandleOreTileMined(i, j, type);
        }
    }
}
