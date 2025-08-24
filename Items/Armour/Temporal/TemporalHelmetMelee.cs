// TemporalHelmetMelee.cs - Melee Variant
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Items.Materials;
using LackOfNameStuff.Items.Armour.Temporal;

namespace LackOfNameStuff.Items.Armour.Temporal
{
    [AutoloadEquip(EquipType.Head)]
    public class TemporalHelmetMelee : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.defense = 33; // Higher defense for melee
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Temporal Helm");
            // Tooltip.SetDefault("Provides temporal awareness and melee expertise");
            
            ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
        }

        public override void UpdateEquip(Player player)
        {
            // Melee-focused bonuses
            player.GetDamage(DamageClass.Melee) += 0.18f;
            player.GetCritChance(DamageClass.Melee) += 10;
            player.meleeScaleGlove = true; // Size bonus
            
            // Temporal awareness
            player.GetModPlayer<Players.TemporalPlayer>().temporalAwareness = true;
            player.GetModPlayer<Players.TemporalPlayer>().helmetType = "Melee";
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<TemporalChestplate>() && 
                   legs.type == ModContent.ItemType<TemporalLeggings>();
        }

        public override void UpdateArmorSet(Player player)
        {
            var temporalPlayer = player.GetModPlayer<Players.TemporalPlayer>();
            temporalPlayer.hasTemporalSet = true;

            // Melee set bonus
            player.setBonus = "Temporal Warrior: Melee attacks create temporal shockwaves\n" +
                            "Enhanced Chronos Watch effect\n" +
                            "Attacks have increased knockback and speed\n" +
                            "+15% melee damage, +2 knockback";
            
            // Melee-specific bonuses
            player.GetAttackSpeed(DamageClass.Melee) += 0.15f;
            player.GetKnockback(DamageClass.Melee) += 2f;
            
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