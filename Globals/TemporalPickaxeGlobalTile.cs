using Terraria;
using Terraria.ModLoader;
using LackOfNameStuff.Items.Tools;

namespace LackOfNameStuff.Globals
{
    public class TemporalPickaxeGlobalTile : GlobalTile
    {
        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (fail || effectOnly) return;

            Player player = Main.LocalPlayer;
            if (player.HeldItem?.ModItem is TemporalPickaxe)
            {
                player.GetModPlayer<TemporalPickaxePlayer>().OnBlockMined();
            }
        }
    }
}