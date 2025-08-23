// TemporalHelmet.cs
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
            Item.defense = 22;
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Temporal Helmet");
            // Tooltip.SetDefault("Provides temporal awareness");
        }

        public override void UpdateEquip(Player player)
        {
            // 15% increased ranged damage
            player.GetDamage(DamageClass.Ranged) += 0.15f;
            
            // Temporal awareness - see enemy health bars and show temporal effects
            player.GetModPlayer<Players.TemporalPlayer>().temporalAwareness = true;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<TemporalChestplate>() && 
                   legs.type == ModContent.ItemType<TemporalLeggings>();
        }

        public override void UpdateArmorSet(Player player)
        {
            var temporalPlayer = player.GetModPlayer<Players.TemporalPlayer>();
            
            // Set bonus effects
            temporalPlayer.hasTemporalSet = true;
            player.setBonus = "Temporal Mastery: Attacks have a chance to slow enemies\n" +
                            "Enhances Chronos Watch effectiveness\n" +
                            "Immunity to time-based debuffs";
            
            // Time immunity
            player.buffImmune[BuffID.Slow] = true;
            player.buffImmune[BuffID.Webbed] = true;
            player.buffImmune[BuffID.Frozen] = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 8);
            recipe.AddIngredient(ItemID.FragmentSolar, 6);
            recipe.AddIngredient(ModContent.ItemType<TimeShard>(), 5);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}