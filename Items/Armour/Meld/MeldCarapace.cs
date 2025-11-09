using LackOfNameStuff.Common;
using LackOfNameStuff.Items.Materials;
using LackOfNameStuff.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LackOfNameStuff.Items.Armour.Meld
{
    [AutoloadEquip(EquipType.Body)]
    public class MeldCarapace : ModItem
    {
        public override void SetStaticDefaults()
        {
            ArmorIDs.Body.Sets.HidesArms[Item.bodySlot] = false;
            ArmorIDs.Body.Sets.HidesHands[Item.bodySlot] = false;
            ArmorIDs.Body.Sets.HidesTopSkin[Item.bodySlot] = true;
            ArmorIDs.Body.Sets.HidesBottomSkin[Item.bodySlot] = true;
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 20;
            Item.value = Item.sellPrice(gold: 28);
            Item.rare = ItemRarityID.Cyan;
            Item.defense = 30;
        }

        public override void UpdateEquip(Player player)
        {
            DamageClass rogue = ResolveRogueDamage();
            player.GetDamage(rogue) += 0.18f;
            player.endurance += 0.05f;
            player.statLifeMax2 += 40;
            player.GetModPlayer<MeldPlayer>().hasMeldCarapace = true;
        }

        public override void AddRecipes()
        {
            if (!CalamityIntegration.TryGetHideOfAstrumDeus(out int hideType))
            {
                return;
            }

            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<MeldAmalgamate>(), 20);
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
