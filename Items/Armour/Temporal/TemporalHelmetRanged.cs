// TemporalHelmetRanged.cs
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Items.Materials;

namespace LackOfNameStuff.Items.Armour.Temporal
{
    [AutoloadEquip(EquipType.Head)]
    public class TemporalHelmetRanged : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.defense = 21;
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Temporal Helmet");
            // Tooltip.SetDefault("Provides ranger prowess");
        }

        public override void UpdateEquip(Player player)
        {
            // 15% increased ranged damage
            player.GetDamage(DamageClass.Ranged) += 0.15f;
            // Mark helmet type for class-specific effects
            player.GetModPlayer<Players.TemporalPlayer>().helmetType = "Ranged";
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<TemporalChestplate>() && 
                   legs.type == ModContent.ItemType<TemporalLeggings>();
        }

        public override void UpdateArmorSet(Player player)
        {
            var temporalPlayer = player.GetModPlayer<Players.TemporalPlayer>();
            int tier = temporalPlayer.currentTier;
            
            // Set bonus effects
            temporalPlayer.hasTemporalSet = true;
            player.setBonus = "Temporal Mastery: Attacks have a chance to slow enemies\n" +
                            "Chance for decreased Chronos Watch cooldown\n" +
                            "Immunity to time-based debuffs\n" +
                            "+15% ranged attack speed, + 10% ranged crit chance";
            
            // Ranged-specific bonuses
            float extraAS = 0.03f * (tier - 1); // small per-tier bump
            float extraCrit = 2f * (tier - 1);
            player.GetAttackSpeed(DamageClass.Ranged) += 0.15f + extraAS;
            player.GetCritChance(DamageClass.Ranged) += 10f + extraCrit;

            // Time immunity
            player.buffImmune[BuffID.Slow] = true;
            player.buffImmune[BuffID.Webbed] = true;
            player.buffImmune[BuffID.Frozen] = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 8);
            recipe.AddIngredient(ItemID.FragmentVortex, 6);
            recipe.AddIngredient(ModContent.ItemType<TimeShard>(), 5);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}