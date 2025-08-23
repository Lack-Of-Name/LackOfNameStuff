using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Players;
using LackOfNameStuff.Projectiles;
using LackOfNameStuff.Globals;

namespace LackOfNameStuff.Items.Weapons.Magic
{
    public class TemporalOrbLauncher : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Set temporal weapon properties here
            var temporalData = Item.GetGlobalItem<TemporalWeaponData>();
            temporalData.TemporalWeapon = true;
            temporalData.TemporalBuffDamage = 1.5f;    // 50% more damage
            temporalData.TemporalBuffSpeed = 1.3f;     // 30% faster
            temporalData.TemporalBuffCrit = 20f;       // +20% crit chance
            temporalData.TemporalBuffKnockback = 1.4f; // 40% more knockback
            // Just other setstaticdefault stuff
            Item.ResearchUnlockCount = 1;
            Item.SetNameOverride("Temporal Orb Launcher");
        }
        public override void SetDefaults()
        {
            Item.damage = 45;
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.Cyan;
            Item.UseSound = SoundID.Item43; // Magical sound
            Item.shoot = ModContent.ProjectileType<TemporalOrb>();
            Item.shoot = ModContent.ProjectileType<TemporalOrb>();
            Item.shootSpeed = 8f;
            Item.mana = 15;
            Item.autoReuse = true;
        }

        public override bool CanUseItem(Player player)
        {
            // Can only be used during Chronos time
            var chronosPlayer = player.GetModPlayer<ChronosPlayer>();
            return chronosPlayer.screenEffectIntensity > 0.1f;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Tooltip0", "Creates orbs that float in place during Chronos time"));
            tooltips.Add(new TooltipLine(Mod, "Tooltip1", "When time resumes, all orbs home in on enemies"));
            tooltips.Add(new TooltipLine(Mod, "Tooltip2", "'Set your trap in frozen time'"));
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