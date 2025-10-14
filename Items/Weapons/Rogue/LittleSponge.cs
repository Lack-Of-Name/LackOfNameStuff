using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Common;
using LackOfNameStuff.Projectiles;

namespace LackOfNameStuff.Items.Weapons.Rogue
{
    public class LittleSponge : ModItem
    {
        private DamageClass ResolveDamageClass()
        {
            if (CalamityIntegration.CalamityLoaded &&
                CalamityIntegration.CalamityMod.TryFind("RogueDamageClass", out DamageClass rogueDamage))
            {
                return rogueDamage;
            }

            return DamageClass.Ranged;
        }

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.damage = 142;
            Item.DamageType = ResolveDamageClass();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.knockBack = 5f;
            Item.UseSound = SoundID.Item1;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<KrisbladeProjectile>();
            Item.shootSpeed = 16f;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.buyPrice(gold: 15);
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 10);
            recipe.AddIngredient(ItemID.FragmentStardust, 8);
            recipe.AddIngredient(ModContent.ItemType<Items.Materials.TimeGem>(), 5);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();

            if (CalamityIntegration.TryGetCalamityItem("CosmiliteBar", out int cosmiliteBar))
            {
                Recipe calamityRecipe = CreateRecipe();
                calamityRecipe.AddIngredient(cosmiliteBar, 8);

                if (CalamityIntegration.TryGetCalamityItem("RuinousSoul", out int ruinousSoul))
                {
                    calamityRecipe.AddIngredient(ruinousSoul, 5);
                }

                calamityRecipe.AddIngredient(ModContent.ItemType<Items.Materials.TimeGem>(), 6);
                calamityRecipe.AddTile(TileID.LunarCraftingStation);
                calamityRecipe.Register();
            }
        }
    }
}
