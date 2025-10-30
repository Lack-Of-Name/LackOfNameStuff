using System;
using LackOfNameStuff.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LackOfNameStuff.Globals
{
    public class GersonGlobalNPC : GlobalNPC
    {
        public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
        {
            Player player = Main.LocalPlayer;
            if (player == null || !player.active)
            {
                return;
            }

            bool hasGerson = player.GetModPlayer<GersonPlayer>().HasGersonName;
            int discountPrice(int value) => (int)Math.Max(1, value * 0.8f);

            for (int i = 0; i < items.Length; i++)
            {
                Item item = items[i];
                if (item == null || item.type <= ItemID.None)
                {
                    continue;
                }

                if (!hasGerson)
                {
                    if (item.shopCustomPrice.HasValue && item.value > 0 && item.shopCustomPrice.Value == discountPrice(item.value))
                    {
                        item.shopCustomPrice = null;
                    }

                    continue;
                }

                int value = item.value;
                if (value <= 0)
                {
                    continue;
                }

                item.shopCustomPrice = discountPrice(value);
            }
        }

        public override void GetChat(NPC npc, ref string chat)
        {
            if (!npc.townNPC || npc.type == NPCID.OldMan)
            {
                return;
            }

            Player player = Main.LocalPlayer;
            if (player == null || !player.active)
            {
                return;
            }

            GersonPlayer gerson = player.GetModPlayer<GersonPlayer>();
            if (!gerson.HasGersonName)
            {
                return;
            }

            if (!Main.rand.NextBool(4))
            {
                return;
            }

            string baseLine = npc.type switch
            {
                NPCID.Merchant => "I'll shave a little extra off the top for you, shell-buyer.",
                NPCID.Clothier => "Your shell is still intact? Miraculous.",
                NPCID.Nurse => "Payment up front, but I'll let the mossy ones slide this once.",
                NPCID.PartyGirl => "Gyaa ha ha! Let's make it a discount party!",
                _ => "Gyaa ha ha... you've earned a mossy markdown."
            };

            chat = baseLine;
        }
    }
}
