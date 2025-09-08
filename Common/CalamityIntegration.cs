// CalamityIntegration.cs - Helper class for Calamity Mod integration
using Terraria;
using Terraria.ModLoader;
using System;

namespace LackOfNameStuff.Common
{
    public static class CalamityIntegration
    {
        public static Mod CalamityMod => ModLoader.GetMod("CalamityMod");
        public static bool CalamityLoaded => CalamityMod != null;

        // Boss defeat tracking (you'll need to adjust these based on Calamity's actual system)
        public static bool DownedProvidence
        {
            get
            {
                if (!CalamityLoaded) return false;
                
                // Try to get Calamity's world data
                try
                {
                    // This is a placeholder - you'll need to check Calamity's source for the actual field names
                    // Calamity typically stores boss defeats in a ModWorld class
                    if (CalamityMod.TryFind("CalamityWorld", out ModSystem calamityWorld))
                    {
                        // Use reflection to get the boss defeat status
                        var field = calamityWorld.GetType().GetField("downedProvidence");
                        if (field != null)
                            return (bool)field.GetValue(calamityWorld);
                    }
                }
                catch (Exception)
                {
                    // Fallback if reflection fails
                }
                
                return NPC.downedMoonlord; // Fallback
            }
        }

        public static bool DownedPolterghast
        {
            get
            {
                if (!CalamityLoaded) return false;
                
                try
                {
                    if (CalamityMod.TryFind("CalamityWorld", out ModSystem calamityWorld))
                    {
                        var field = calamityWorld.GetType().GetField("downedPolterghast");
                        if (field != null)
                            return (bool)field.GetValue(calamityWorld);
                    }
                }
                catch (Exception)
                {
                    // Fallback
                }
                
                return NPC.downedMoonlord; // Fallback
            }
        }

        public static bool DownedDoG
        {
            get
            {
                if (!CalamityLoaded) return false;
                
                try
                {
                    if (CalamityMod.TryFind("CalamityWorld", out ModSystem calamityWorld))
                    {
                        var field = calamityWorld.GetType().GetField("downedDoG");
                        if (field != null)
                            return (bool)field.GetValue(calamityWorld);
                    }
                }
                catch (Exception)
                {
                    // Fallback
                }
                
                return NPC.downedMoonlord; // Fallback
            }
        }

        public static bool DownedYharon
        {
            get
            {
                if (!CalamityLoaded) return false;
                
                try
                {
                    if (CalamityMod.TryFind("CalamityWorld", out ModSystem calamityWorld))
                    {
                        var field = calamityWorld.GetType().GetField("downedYharon");
                        if (field != null)
                            return (bool)field.GetValue(calamityWorld);
                    }
                }
                catch (Exception)
                {
                    // Fallback
                }
                
                return NPC.downedMoonlord; // Fallback
            }
        }

        public static bool DownedSCal
        {
            get
            {
                if (!CalamityLoaded) return false;
                
                try
                {
                    if (CalamityMod.TryFind("CalamityWorld", out ModSystem calamityWorld))
                    {
                        var field = calamityWorld.GetType().GetField("downedCalamitas");
                        if (field != null)
                            return (bool)field.GetValue(calamityWorld);
                    }
                }
                catch (Exception)
                {
                    // Fallback
                }
                
                return NPC.downedMoonlord; // Fallback
            }
        }

        // Material helper methods
        public static bool TryGetCalamityItem(string itemName, out int itemType)
        {
            itemType = 0;
            if (!CalamityLoaded) return false;
            
            if (CalamityMod.TryFind(itemName, out ModItem item))
            {
                itemType = item.Type;
                return true;
            }
            return false;
        }

        public static bool TryGetCalamityTile(string tileName, out int tileType)
        {
            tileType = 0;
            if (!CalamityLoaded) return false;
            
            if (CalamityMod.TryFind(tileName, out ModTile tile))
            {
                tileType = tile.Type;
                return true;
            }
            return false;
        }

        public static bool TryGetCalamityNPC(string npcName, out int npcType)
        {
            npcType = 0;
            if (!CalamityLoaded) return false;
            
            if (CalamityMod.TryFind(npcName, out ModNPC npc))
            {
                npcType = npc.Type;
                return true;
            }
            return false;
        }

        // Progression tier determination
        public static int GetCurrentProgressionTier()
        {
            if (DownedSCal || DownedYharon || DownedDoG)
                return 4; // Eternal Gem tier
            else if (DownedPolterghast)
                return 3; // Time Gem tier
            else if (DownedProvidence)
                return 2; // Eternal Shard tier
            else if (NPC.downedMoonlord)
                return 1; // Time Shard tier
            else
                return 0; // No temporal materials yet
        }

        public static string GetProgressionTierName(int tier)
        {
            return tier switch
            {
                4 => "Eternal Gem",
                3 => "Time Gem", 
                2 => "Eternal Shard",
                1 => "Time Shard",
                _ => "None"
            };
        }
    }
}