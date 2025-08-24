using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Globals;

namespace LackOfNameStuff.Items.Weapons.Melee
{
    public class ChronosWhip : ModItem
    {
        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Item.damage = 65;
            Item.DamageType = DamageClass.MeleeNoSpeed; // Whips use MeleeNoSpeed
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item152; // Whip sound
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<ChronosWhipProjectile>();
            Item.shootSpeed = 12f; // Increased for better range

            var temporalData = Item.GetGlobalItem<TemporalWeaponData>();
            temporalData.TemporalWeapon = true;

            // Set the temporal buff values here
            temporalData.TemporalBuffDamage = 1.8f;
            temporalData.TemporalBuffSpeed = 1.8f;
            temporalData.TemporalBuffCrit = 30f;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 6);
            recipe.AddIngredient(ItemID.FragmentNebula, 8);
            recipe.AddIngredient(ModContent.ItemType<Items.Materials.TimeShard>(), 5);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}