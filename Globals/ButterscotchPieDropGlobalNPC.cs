using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Items.Consumables;

namespace LackOfNameStuff.Globals
{
    public class ButterscotchPieDropGlobalNPC : GlobalNPC
    {
        private const int DropChance = 400; // 1 in 400 underground enemies

        public override void OnKill(NPC npc)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }

            if (!ShouldConsider(npc))
            {
                return;
            }

            if (!IsInUndergroundLayer(npc.Center))
            {
                return;
            }

            if (Main.rand.NextBool(DropChance))
            {
                Item.NewItem(npc.GetSource_Loot(), npc.getRect(), ModContent.ItemType<ButterscotchCinnamonPie>());
            }
        }

        private static bool ShouldConsider(NPC npc)
        {
            if (!npc.CanBeChasedBy())
            {
                return false;
            }

            if (npc.lifeMax <= 5)
            {
                return false;
            }

            return true;
        }

        private static bool IsInUndergroundLayer(Vector2 worldPosition)
        {
            float tileY = worldPosition.Y / 16f;
            return tileY > Main.worldSurface && tileY <= Main.rockLayer;
        }
    }
}
