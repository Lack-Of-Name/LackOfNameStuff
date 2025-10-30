using LackOfNameStuff.Common;
using LackOfNameStuff.Items.Materials;
using LackOfNameStuff.Players;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace LackOfNameStuff.Items.Armour.Meld
{
    [AutoloadEquip(EquipType.Head)]
    public class MeldCoif : ModItem
    {
        public override void SetStaticDefaults()
        {
            ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = true;
            ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
            ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.value = Item.sellPrice(gold: 18);
            Item.rare = ItemRarityID.Cyan;
            Item.defense = 20;
        }

        public override void UpdateEquip(Player player)
        {
            DamageClass rogue = ResolveRogueDamage();
            player.GetDamage(rogue) += 0.14f;
            player.GetCritChance(rogue) += 8f;
            player.GetModPlayer<MeldPlayer>().hasMeldCoif = true;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<MeldCarapace>() &&
                   legs.type == ModContent.ItemType<MeldGreaves>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = Language.GetTextValue("Mods.LackOfNameStuff.Items.Armour.Meld.MeldCoif.SetBonus");
            player.GetModPlayer<MeldPlayer>().meldSetActive = true;
        }

        public override void AddRecipes()
        {
            if (!CalamityIntegration.TryGetHideOfAstrumDeus(out int hideType))
            {
                return;
            }

            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<MeldAmalgamate>(), 12);
            recipe.AddIngredient(hideType);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }

        private static DamageClass ResolveRogueDamage()
        {
            if (CalamityIntegration.CalamityLoaded &&
                CalamityIntegration.CalamityMod.TryFind("RogueDamageClass", out DamageClass rogueDamage))
            {
                return rogueDamage;
            }

            return DamageClass.Throwing;
        }
    }
}
