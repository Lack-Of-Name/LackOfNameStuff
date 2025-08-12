using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace LackOfNameStuff.Items.Weapons.Ranged
{
    public class StickGun : ModItem
    {
        // === CUSTOMIZABLE VARIABLES ===
        
        // Basic Item Properties
        private static readonly int BaseDamage = 50;
        private static readonly int BaseKnockback = 2;
        private static readonly int BaseCritChance = 4;
        private static readonly int ItemRarity = ItemRarityID.Pink;
        private static readonly int ItemValue = Item.sellPrice(gold: 10);
        
        // Weapon Properties
        private static readonly int UseTimeValue = 1; // Basically infinite attack speed
        private static readonly int UseAnimationValue = 1;
        private static readonly bool AutoReuse = true;
        private static readonly int UseStyleValue = ItemUseStyleID.Shoot;
        
        // Projectile Properties
        private static readonly int ProjectileType = ProjectileID.Bullet; // Change this to your custom projectile
        private static readonly float ShootSpeed = 16f;
        private static readonly bool ConsumeAmmo = true; // Now actually uses ammo
        private static readonly int AmmoType = AmmoID.Bullet;
        
        // Ammo Save Properties
        private static readonly bool HasAmmoSave = true;
        private static readonly float AmmoSaveChance = 0.8f; // 66% chance to not consume ammo (adjust as needed)
        
        // Visual/Audio Properties
        private static readonly SoundStyle UseSound = SoundID.Item11; // Gun sound
        private static readonly float Scale = 1f;
        
        // Special Properties
        private static readonly bool NoSpread = true;
        private static readonly bool DamageEqualsCrit = true;
        private static readonly float SpreadMultiplier = 0f; // 0 = no spread, 1 = normal spread
        private static readonly int BypassIFrames = 0; // 0 = bypass all I-frames, higher values = ignore fewer I-frames
        
        // Tooltip customization
        private static readonly string CustomTooltip = "Bypasses I-frames\nDamage equals critical strike chance\nHigh attack speed with great accuracy\n80% chance to not consume ammo";

        public override void SetDefaults()
        {
            // Basic item properties
            Item.damage = BaseDamage;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 40;
            Item.height = 20;
            Item.useTime = UseTimeValue;
            Item.useAnimation = UseAnimationValue;
            Item.useStyle = UseStyleValue;
            Item.noMelee = true;
            Item.knockBack = BaseKnockback;
            Item.value = ItemValue;
            Item.rare = ItemRarity;
            Item.UseSound = UseSound;
            Item.autoReuse = AutoReuse;
            Item.shoot = ProjectileType;
            Item.shootSpeed = ShootSpeed;
            Item.useAmmo = ConsumeAmmo ? AmmoType : AmmoID.None;
            Item.crit = BaseCritChance;
            Item.scale = Scale;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // Add custom tooltip
            if (!string.IsNullOrEmpty(CustomTooltip))
            {
                tooltips.Add(new TooltipLine(Mod, "CustomTooltip", CustomTooltip));
            }

            // Add conditional crafting hint to tooltip
            Player player = Main.LocalPlayer;
            if (player?.name == "Hero")
            {
                tooltips.Add(new TooltipLine(Mod, "ConditionalCraft", "[c/00FF00:Special crafting available for Hero!]"));
                tooltips.Add(new TooltipLine(Mod, "MoneyAbuseWarning", "[c/FF0000:Don't abuse your crafting privelages for infinite gold!]"));
            }
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            if (DamageEqualsCrit)
            {
                // Set damage equal to the player's total critical strike chance
                int totalCrit = (int)player.GetTotalCritChance(DamageClass.Ranged);
                
                // Override the damage calculation
                damage = StatModifier.Default;
                damage.Base = totalCrit;
            }
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            if (HasAmmoSave)
            {
                // Return false to NOT consume ammo based on the save chance
                return Main.rand.NextFloat() >= AmmoSaveChance;
            }
            return true; // Always consume ammo if ammo save is disabled
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (NoSpread)
            {
                // Calculate perfect aim direction (no spread)
                Vector2 targetDirection = velocity.SafeNormalize(Vector2.UnitX);
                velocity = targetDirection * ShootSpeed;
            }
            else if (SpreadMultiplier != 1f)
            {
                // Apply custom spread multiplier
                float spread = MathHelper.ToRadians(5f) * SpreadMultiplier; // Base 5 degree spread
                velocity = velocity.RotatedByRandom(spread);
            }

            // If damage equals crit is enabled, update damage here too
            if (DamageEqualsCrit)
            {
                damage = (int)player.GetTotalCritChance(DamageClass.Ranged);
            }

            // Create the projectile
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            return false; // Return false to prevent default projectile creation
        }

        public override Vector2? HoldoutOffset()
        {
            // Adjust the weapon's position when held (optional)
            return new Vector2(-2f, 0f);
        }

        public override void AddRecipes()
        {
            // Default recipe for everyone else
            Recipe defaultRecipe = CreateRecipe();
            defaultRecipe.AddIngredient(ItemID.IllegalGunParts, 1);
            defaultRecipe.AddIngredient(ItemID.SoulofFright, 10);
            defaultRecipe.AddIngredient(ItemID.SoulofMight, 10);
            defaultRecipe.AddIngredient(ItemID.SoulofSight, 10);
            defaultRecipe.AddTile(TileID.MythrilAnvil);
            defaultRecipe.AddCondition(new Condition("Not World Hero", () => Main.LocalPlayer?.name != "Hero"));
            defaultRecipe.Register();

            // Special recipe for Hero
            Recipe heroRecipe = CreateRecipe();
            heroRecipe.AddIngredient(ItemID.Wood, 1);
            heroRecipe.AddTile(TileID.Anvils);
            heroRecipe.AddCondition(new Condition("World Hero?", () => Main.LocalPlayer?.name == "Hero"));
            heroRecipe.Register();
        }

        // Optional: Custom behavior when the item is in the player's inventory
        public override void UpdateInventory(Player player)
        {
            // Add any passive effects here if needed
        }

        // Optional: Custom behavior when the item is held
        public override void HoldItem(Player player)
        {
            // Add any effects while holding the weapon
        }
    }

    // Custom Recipe Conditions (if you need more complex conditions)
    public class CustomRecipeConditions : ModSystem
    {
        // Example of a custom condition
        public static Condition PlayerIsHero = new Condition("World Hero?", () => Main.LocalPlayer?.name == "Hero");
        
        // Example of a more complex condition
        public static Condition PlayerHasSpecialStatus = new Condition("Player has special status", () => {
            Player player = Main.LocalPlayer;
            return player?.name == "Hero" && player.statLifeMax >= 500;
        });

        public override void PostSetupContent()
        {
            // You can register custom conditions here if needed
        }
    }
}