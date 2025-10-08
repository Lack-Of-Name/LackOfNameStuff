// TemporalProgressionSystem.cs - Fixed version with proper recipe timing
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
            // Get Calamity mod reference for other systems to use
            calamityMod = ModLoader.GetMod("CalamityMod");
            
            if (calamityMod == null)
            {
                // Log warning if Calamity isn't loaded (but don't error out)
                ModLoader.GetMod("LackOfNameStuff").Logger.Warn("Calamity Mod not found - temporal progression recipes will use fallback materials.");
            }
        }

        // Use AddRecipes instead of PostSetupContent for recipe creation
        public override void AddRecipes()
        {
            // Get Calamity mod reference again to be safe
            calamityMod = ModLoader.GetMod("CalamityMod");
            
            // Add crafting recipes
            AddTemporalRecipes();
        }

        private void AddTemporalRecipes()
        {
            // Eternal Shard Recipe (Post-Providence)
            // Time Shards + Unholy Essence + Exodium Clusters
            Recipe eternalShardRecipe = Recipe.Create(ModContent.ItemType<EternalShard>(), 3);
            eternalShardRecipe.AddIngredient(ModContent.ItemType<TimeShard>(), 3);
            
            // Add Calamity materials if available
            if (calamityMod != null)
            {
                if (calamityMod.TryFind("UnholyEssence", out ModItem unholyEssence))
                    eternalShardRecipe.AddIngredient(unholyEssence.Type, 9);
                else
                    eternalShardRecipe.AddIngredient(ItemID.SoulofNight, 15); // Fallback
                
                if (calamityMod.TryFind("ExodiumCluster", out ModItem exodiumCluster))
                    eternalShardRecipe.AddIngredient(exodiumCluster.Type, 3);
                else
                    eternalShardRecipe.AddIngredient(ItemID.LunarBar, 5); // Fallback
            }
            else
            {
                // Fallback ingredients if Calamity isn't loaded
                eternalShardRecipe.AddIngredient(ItemID.SoulofNight, 15);
                eternalShardRecipe.AddIngredient(ItemID.LunarBar, 5);
            }
            
            eternalShardRecipe.AddTile(TileID.LunarCraftingStation);
            eternalShardRecipe.Register();

            // Time Gem Recipe (Post-Polterghast)
            // Eternal Shards + Ruinous Souls + Nightmare Fuel/Endothermic Energy
            Recipe timeGemRecipe = Recipe.Create(ModContent.ItemType<TimeGem>(), 1);
            timeGemRecipe.AddIngredient(ModContent.ItemType<EternalShard>(), 8);
            
            if (calamityMod != null)
            {
                if (calamityMod.TryFind("RuinousSoul", out ModItem ruinousSoul))
                    timeGemRecipe.AddIngredient(ruinousSoul.Type, 8);
                else
                    timeGemRecipe.AddIngredient(ItemID.SoulofMight, 10); // Fallback
                
                // Try Nightmare Fuel first, then Endothermic Energy
                if (calamityMod.TryFind("NightmareFuel", out ModItem nightmareFuel))
                    timeGemRecipe.AddIngredient(nightmareFuel.Type, 4);
                else if (calamityMod.TryFind("EndothermicEnergy", out ModItem endothermicEnergy))
                    timeGemRecipe.AddIngredient(endothermicEnergy.Type, 4);
                else
                    timeGemRecipe.AddIngredient(ItemID.SoulofFright, 10); // Fallback
            }
            else
            {
                // Fallback ingredients
                timeGemRecipe.AddIngredient(ItemID.SoulofMight, 10);
                timeGemRecipe.AddIngredient(ItemID.SoulofFright, 10);
            }
            
            timeGemRecipe.AddTile(TileID.LunarCraftingStation);
            timeGemRecipe.Register();

            // Eternal Gem Recipe (Post-DoG/Yharon/Exo)
            // Time Gems + Cosmilite + Auric Tesla + Exo Prisms
            Recipe eternalGemRecipe = Recipe.Create(ModContent.ItemType<EternalGem>(), 3);
            eternalGemRecipe.AddIngredient(ModContent.ItemType<TimeGem>(), 3);
            
            if (calamityMod != null)
            {
                if (calamityMod.TryFind("CosmiliteBar", out ModItem cosmiliteBar))
                    eternalGemRecipe.AddIngredient(cosmiliteBar.Type, 3);
                else
                    eternalGemRecipe.AddIngredient(ItemID.LunarBar, 15); // Fallback: use a sane amount comparable to post-ML
                
                if (calamityMod.TryFind("AuricTeslaBar", out ModItem auricTesla))
                    eternalGemRecipe.AddIngredient(auricTesla.Type, 3);
                else
                    eternalGemRecipe.AddIngredient(ItemID.FragmentSolar, 10); // Fallback: fragments as substitute
                
                if (calamityMod.TryFind("ExoPrism", out ModItem exoPrism))
                    eternalGemRecipe.AddIngredient(exoPrism.Type, 1);
                else
                    eternalGemRecipe.AddIngredient(ItemID.MoonLordLegs, 3); // Fallback (rare drop)
                
                // Try to use Draedon's Forge if available
                if (calamityMod.TryFind("DraedonsForge", out ModTile draedonsForge))
                    eternalGemRecipe.AddTile(draedonsForge.Type);
                else
                    eternalGemRecipe.AddTile(TileID.LunarCraftingStation);
            }
            else
            {
                // Fallback ingredients
                eternalGemRecipe.AddIngredient(ItemID.LunarBar, 15);
                eternalGemRecipe.AddIngredient(ItemID.FragmentSolar, 10);
                eternalGemRecipe.AddIngredient(ItemID.MoonLordLegs, 1);
                eternalGemRecipe.AddTile(TileID.LunarCraftingStation);
            }
            
            eternalGemRecipe.Register();
        }

        // Helper method to check if player has defeated specific Calamity bosses
        public static bool HasDefeatedProvidence()
        {
            if (calamityMod == null) return NPC.downedMoonlord;
            
            // Use the CalamityIntegration class we fixed earlier
            return CalamityIntegration.DownedProvidence;
        }

        public static bool HasDefeatedPolterghast()
        {
            if (calamityMod == null) return NPC.downedMoonlord;
            
            return CalamityIntegration.DownedPolterghast;
        }

        public static bool HasDefeatedDoG()
        {
            if (calamityMod == null) return NPC.downedMoonlord;
            
            return CalamityIntegration.DownedDoG;
        }
    }
}