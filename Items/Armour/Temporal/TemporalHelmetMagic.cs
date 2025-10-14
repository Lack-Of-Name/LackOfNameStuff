// TemporalHelmetMagic.cs - Magic Variant
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Items.Materials;
using LackOfNameStuff.Items.Armour.Temporal;

namespace LackOfNameStuff.Items.Armour.Temporal
{
    [AutoloadEquip(EquipType.Head)]
    public class TemporalHelmetMagic : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.defense = 10; // Lower defense, higher magic bonuses
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Temporal Crown");
            // Tooltip.SetDefault("Provides magical resonance");
            
            ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
        }

        public override void UpdateEquip(Player player)
        {
            // Magic-focused bonuses
            player.GetDamage(DamageClass.Magic) += 0.20f;
            player.GetCritChance(DamageClass.Magic) += 12;
            player.statManaMax2 += 60;
            player.manaCost -= 0.10f; // 10% reduced mana cost

            // Set helmet type
            player.GetModPlayer<Players.TemporalPlayer>().helmetType = "Magic";
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

            // Magic set bonus
            player.setBonus = "Temporal Sage: Magic attacks pierce through time\n" +
                            "Chance for decreased Chronos Watch cooldown\n" +
                            "Spells have chance to not consume mana\n" +
                            "-15% mana cost, 17% chance not to consume mana";
            
            // Magic-specific bonuses (scale slightly by tier)
            float extraManaRed = 0.03f * (tier - 1);
            player.manaCost -= 0.15f + extraManaRed; // Additional mana reduction per tier
            // Increase free-cast chance slightly with tier (17% base -> ~23% at T4)
            int freeCastChance = 6 - (tier - 1); // 6,5,4,3 -> 16.7%, 20%, 25%, 33% (approx)
            freeCastChance = System.Math.Clamp(freeCastChance, 3, 6);
            if (Main.rand.NextBool(freeCastChance))
                player.manaFlower = true;
            
            // Time immunity
            player.buffImmune[BuffID.Slow] = true;
            player.buffImmune[BuffID.Webbed] = true;
            player.buffImmune[BuffID.Frozen] = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 8);
            recipe.AddIngredient(ItemID.FragmentNebula, 6);
            recipe.AddIngredient(ModContent.ItemType<TimeShard>(), 5);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}