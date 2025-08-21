// Clean TemporalPickaxeGlobalTile.cs
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using LackOfNameStuff.Items.Tools; // Ensure TemporalPickaxe is in this namespace

namespace LackOfNameStuff.Globals
{
    public class TemporalPickaxeGlobalTile : GlobalTile
    {
        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (!fail && !effectOnly)
            {
                // Attempt to find the closest player to the broken tile
                Player closestPlayer = null;
                float minDist = float.MaxValue;
                Vector2 tileWorldPos = new Vector2(i * 16, j * 16);

                foreach (Player p in Main.player)
                {
                    if (p.active)
                    {
                        float dist = Vector2.Distance(p.Center, tileWorldPos);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            closestPlayer = p;
                        }
                    }
                }

                if (closestPlayer != null && closestPlayer.HeldItem.ModItem is TemporalPickaxe)
                {
                    var modPlayer = closestPlayer.GetModPlayer<TemporalPickaxePlayer>();
                    modPlayer.OnBlockMined();
                }
            }
        }
    }
}