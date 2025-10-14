// TemporalHelmetSummoner.cs - Summoner Variant
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Items.Materials;
using LackOfNameStuff.Items.Armour.Temporal;

namespace LackOfNameStuff.Items.Armour.Temporal
{
    [AutoloadEquip(EquipType.Head)]
    public class TemporalHelmetSummoner : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.defense = 6;
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Temporal Diadem");
            // Tooltip.SetDefault("Provides minion mastery");
            
            ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
        }

        public override void UpdateEquip(Player player)
        {
            // Summoner-focused bonuses
            player.GetDamage(DamageClass.Summon) += 0.25f;
            player.maxMinions += 2;
            player.maxTurrets += 1;

            // Set helmet type
            player.GetModPlayer<Players.TemporalPlayer>().helmetType = "Summoner";
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
            temporalPlayer.hasTemporalSet = true;

            // Summoner set bonus
            player.setBonus = "Temporal Commander: Minions gain temporal abilities\n" +
                            "Increased minion damage during bullet-time\n" +
                            "Minions occasionally slow enemies\n" +
                            "+1 minion slot, + 10% summon class damage";
            
            // Summoner-specific bonuses (scale slightly by tier)
            int extraSlots = (tier >= 3) ? 1 : 0; // +1 more slot at tier 3+
            float extraDamage = 0.04f * (tier - 1);
            player.maxMinions += 1 + extraSlots; // Additional minion slots
            player.GetDamage(DamageClass.Summon) += 0.10f + extraDamage; // Additional damage
            
            // Time immunity
            player.buffImmune[BuffID.Slow] = true;
            player.buffImmune[BuffID.Webbed] = true;
            player.buffImmune[BuffID.Frozen] = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 8);
            recipe.AddIngredient(ItemID.FragmentStardust, 6);
            recipe.AddIngredient(ModContent.ItemType<TimeShard>(), 5);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}