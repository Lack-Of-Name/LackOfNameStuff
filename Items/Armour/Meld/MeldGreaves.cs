using LackOfNameStuff.Common;
using LackOfNameStuff.Items.Materials;
using LackOfNameStuff.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LackOfNameStuff.Items.Armour.Meld
{
    [AutoloadEquip(EquipType.Legs)]
    public class MeldGreaves : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 18;
            Item.value = Item.sellPrice(gold: 22);
            Item.rare = ItemRarityID.Cyan;
            Item.defense = 24;
        }

        public override void UpdateEquip(Player player)
        {
            DamageClass rogue = ResolveRogueDamage();
            player.GetDamage(rogue) += 0.10f;
            player.moveSpeed += 0.18f;
            player.jumpSpeedBoost += 0.6f;
            player.GetModPlayer<MeldPlayer>().hasMeldGreaves = true;
        }

        public override void AddRecipes()
        {
            if (!CalamityIntegration.TryGetHideOfAstrumDeus(out int hideType))
            {
                return;
            }

            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<MeldAmalgamate>(), 16);
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
