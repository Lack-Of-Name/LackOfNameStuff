using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Players;
using LackOfNameStuff.Projectiles;

namespace LackOfNameStuff.Items.Weapons
{
    public class TemporalOrbLauncher : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 45;
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.Cyan;
            Item.UseSound = SoundID.Item43; // Magical sound
                        Item.shoot = ModContent.ProjectileType<TemporalOrb>();
            // If TemporalOrb is in a different namespace, use the fully qualified name:
            // Item.shoot = ModContent.ProjectileType<LackOfNameStuff.Projectiles.TemporalOrb>();
            Item.shoot = ModContent.ProjectileType<TemporalOrb>();
            Item.shootSpeed = 8f;
            Item.mana = 15;
        }

        public override bool CanUseItem(Player player)
        {
            // Can only be used during Chronos time
            var chronosPlayer = player.GetModPlayer<ChronosPlayer>();
            return chronosPlayer.screenEffectIntensity > 0.1f;
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Temporal Orb Launcher");
            // Tooltip.SetDefault("Creates orbs that float in place during Chronos time\nWhen time resumes, all orbs home in on enemies\n'Set your trap in frozen time'");
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 6);
            recipe.AddIngredient(ItemID.FragmentNebula, 12);
            recipe.AddIngredient(ModContent.ItemType<Items.Materials.TimeShard>(), 8);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}