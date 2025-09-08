// TemporalProgressionSystem.cs - Handles Calamity integration and crafting recipes
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Items.Materials;

namespace LackOfNameStuff.Common
{
    public class TemporalProgressionSystem : ModSystem
    {
        // Calamity mod reference
        public static Mod calamityMod;
        
        public override void PostSetupContent()
        {
            // Get Calamity mod reference
            calamityMod = ModLoader.GetMod("CalamityMod");
            
            if (calamityMod == null)
            {
                // Log error if Calamity isn't loaded
                ModLoader.GetMod("LackOfNameStuff").Logger.Error("Calamity Mod is required for Temporal Progression items!");
                return;
            }

            // Add crafting recipes if Calamity is loaded
            AddTemporalRecipes();
        }

        private void AddTemporalRecipes()
        {
            // Eternal Shard Recipe (Post-Providence)
            // Time Shards + Unholy Essence + Exodium Clusters
            Recipe eternalShardRecipe = Recipe.Create(ModContent.ItemType<EternalShard>(), 3);
            eternalShardRecipe.AddIngredient(ModContent.ItemType<TimeShard>(), 3);
            
            // Add Calamity materials
            if (calamityMod.TryFind("UnholyEssence", out ModItem unholyEssence))
                eternalShardRecipe.AddIngredient(unholyEssence.Type, 9);
            
            if (calamityMod.TryFind("ExodiumCluster", out ModItem exodiumCluster))
                eternalShardRecipe.AddIngredient(exodiumCluster.Type, 3);
            
            // Use Lunar Crafting Station for crafting
            eternalShardRecipe.AddTile(TileID.LunarCraftingStation); // Fallback
            
            eternalShardRecipe.Register();

            // Time Gem Recipe (Post-Polterghast)
            // Eternal Shards + Ruinous Souls + Nightmare Fuel/Endothermic Energy
            Recipe timeGemRecipe = Recipe.Create(ModContent.ItemType<TimeGem>(), 1);
            timeGemRecipe.AddIngredient(ModContent.ItemType<EternalShard>(), 8);
            
            if (calamityMod.TryFind("RuinousSoul", out ModItem ruinousSoul))
                timeGemRecipe.AddIngredient(ruinousSoul.Type, 8);
            
            if (calamityMod.TryFind("NightmareFuel", out ModItem nightmareFuel))
                timeGemRecipe.AddIngredient(nightmareFuel.Type, 4);
            else if (calamityMod.TryFind("EndothermicEnergy", out ModItem endothermicEnergy))
                timeGemRecipe.AddIngredient(endothermicEnergy.Type, 4);
            
            timeGemRecipe.AddTile(TileID.LunarCraftingStation);
            
            timeGemRecipe.Register();

            // Eternal Gem Recipe (Post-DoG/Yharon/Exo)
            // Time Gems + Cosmilite + Auric Tesla + Exo Prisms
            Recipe eternalGemRecipe = Recipe.Create(ModContent.ItemType<EternalGem>(), 3);
            eternalGemRecipe.AddIngredient(ModContent.ItemType<TimeGem>(), 3);
            
            if (calamityMod.TryFind("CosmiliteBar", out ModItem cosmiliteBar))
                eternalGemRecipe.AddIngredient(cosmiliteBar.Type, 3);
            
            if (calamityMod.TryFind("AuricTeslaBar", out ModItem auricTesla))
                eternalGemRecipe.AddIngredient(auricTesla.Type, 3);
            
            if (calamityMod.TryFind("ExoPrism", out ModItem exoPrism))
                eternalGemRecipe.AddIngredient(exoPrism.Type, 1);
            
            if (calamityMod.TryFind("DraedonsForge", out ModTile draedonsForge3))
                eternalGemRecipe.AddTile(draedonsForge3.Type);
            else
                eternalGemRecipe.AddTile(TileID.LunarCraftingStation);
            
            eternalGemRecipe.Register();
        }

        // Helper method to check if player has defeated specific Calamity bosses
        public static bool HasDefeatedProvidence()
        {
            if (calamityMod == null) return false;
            
            // Check Calamity's downedBoss system
            if (calamityMod.TryFind("CalamityGlobalNPC", out ModSystem calamityGlobalNPCSystem))
            {
                if (calamityMod == null) return false;
                //stuff is handled in CalamityIntegration.cs
                return NPC.downedMoonlord; // Placeholder
            }
            
            return NPC.downedMoonlord; // Fallback
        }

        public static bool HasDefeatedPolterghast()
        {
            if (calamityMod == null) return false;
            // Similar implementation as above for Polterghast
            return NPC.downedMoonlord; // Placeholder
        }

        public static bool HasDefeatedDoG()
        {
            if (calamityMod == null) return false;
            // Similar implementation as above for Devourer of Gods
            return NPC.downedMoonlord; // Placeholder
        }
    }
}