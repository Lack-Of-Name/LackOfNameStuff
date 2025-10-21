// CalamityIntegration.cs - Fixed version with proper reflection handling
using Terraria;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Reflection;

namespace LackOfNameStuff.Common
{
    public static class CalamityIntegration
    {
        public static Mod CalamityMod => ModLoader.GetMod("CalamityMod");
        public static bool CalamityLoaded => CalamityMod != null;

        private static Type _calamityPlayerType;
        private static MethodInfo _getModPlayerGeneric;
        private static FieldInfo _rogueStealthField;
        private static FieldInfo _rogueStealthMaxField;
        private static FieldInfo _rogueStealthRegenField;
        private static FieldInfo _rogueStealthCooldownField;
        private static FieldInfo _stealthStrikeAvailableField;
        private static bool _triedInitializeCalamityPlayer;

        // Boss defeat tracking
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
                        var fieldInfo = calamityWorld.GetType().GetField("downedProvidence");
                        if (fieldInfo != null)
                            return (bool)fieldInfo.GetValue(calamityWorld);
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
                        var fieldInfo = calamityWorld.GetType().GetField("downedPolterghast");
                        if (fieldInfo != null)
                            return (bool)fieldInfo.GetValue(calamityWorld);
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
                        var fieldInfo = calamityWorld.GetType().GetField("downedDoG");
                        if (fieldInfo != null)
                            return (bool)fieldInfo.GetValue(calamityWorld);
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
                        var fieldInfo = calamityWorld.GetType().GetField("downedYharon");
                        if (fieldInfo != null)
                            return (bool)fieldInfo.GetValue(calamityWorld);
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
                        var fieldInfo = calamityWorld.GetType().GetField("downedCalamitas");
                        if (fieldInfo != null)
                            return (bool)fieldInfo.GetValue(calamityWorld);
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

        private static bool EnsureCalamityPlayerReflection()
        {
            if (_calamityPlayerType != null)
            {
                return true;
            }

            if (_triedInitializeCalamityPlayer || !CalamityLoaded || CalamityMod.Code == null)
            {
                return _calamityPlayerType != null;
            }

            _triedInitializeCalamityPlayer = true;

            string[] candidateTypeNames = new[]
            {
                "CalamityMod.CalPlayer.CalamityPlayer",
                "CalamityMod.CalPlayer",
                "CalamityMod.CalamityPlayer"
            };

            foreach (string typeName in candidateTypeNames)
            {
                _calamityPlayerType = CalamityMod.Code.GetType(typeName);
                if (_calamityPlayerType != null)
                {
                    break;
                }
            }

            if (_calamityPlayerType == null)
            {
                return false;
            }

            _getModPlayerGeneric = typeof(Player).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "GetModPlayer" && m.IsGenericMethodDefinition && m.GetParameters().Length == 0);

            static FieldInfo FindField(Type type, params string[] names)
            {
                foreach (string name in names)
                {
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field != null)
                    {
                        return field;
                    }
                }

                return null;
            }

            _rogueStealthField = FindField(_calamityPlayerType, "rogueStealth", "stealth");
            _rogueStealthMaxField = FindField(_calamityPlayerType, "rogueStealthMax", "stealthMax");
            _rogueStealthRegenField = FindField(_calamityPlayerType, "rogueStealthRegen", "rogueStealthRegenRate", "stealthRegen", "stealthRegenRate");
            _rogueStealthCooldownField = FindField(_calamityPlayerType, "rogueStealthCooldown", "stealthCooldown");
            _stealthStrikeAvailableField = FindField(_calamityPlayerType, "stealthStrikeAvailable", "rogueStealthStrikeAvailable");

            return _getModPlayerGeneric != null && _calamityPlayerType != null;
        }

        private static bool TryGetCalamityPlayer(Player player, out object calamityPlayer)
        {
            calamityPlayer = null;

            if (!EnsureCalamityPlayerReflection())
            {
                return false;
            }

            if (_getModPlayerGeneric == null || _calamityPlayerType == null)
            {
                return false;
            }

            MethodInfo constructed = _getModPlayerGeneric.MakeGenericMethod(_calamityPlayerType);
            calamityPlayer = constructed.Invoke(player, null);
            return calamityPlayer != null;
        }

        public static bool TryAccessRogueStealthRegen(Player player, out float current, out Action<float> setter)
        {
            current = 0f;
            setter = null;

            if (!TryGetCalamityPlayer(player, out object calamityPlayer) || _rogueStealthRegenField == null)
            {
                return false;
            }

            object value = _rogueStealthRegenField.GetValue(calamityPlayer);
            if (value == null)
            {
                return false;
            }

            current = Convert.ToSingle(value);
            setter = newValue => _rogueStealthRegenField.SetValue(calamityPlayer, newValue);
            return true;
        }

        public static bool TrySetRogueStealthRegen(Player player, float value)
        {
            if (!TryGetCalamityPlayer(player, out object calamityPlayer) || _rogueStealthRegenField == null)
            {
                return false;
            }

            _rogueStealthRegenField.SetValue(calamityPlayer, value);
            return true;
        }

        public static bool TryGetRogueStealth(Player player, out float current, out float max)
        {
            current = 0f;
            max = 0f;

            if (!TryGetCalamityPlayer(player, out object calamityPlayer) || _rogueStealthField == null || _rogueStealthMaxField == null)
            {
                return false;
            }

            current = Convert.ToSingle(_rogueStealthField.GetValue(calamityPlayer));
            max = Convert.ToSingle(_rogueStealthMaxField.GetValue(calamityPlayer));
            return true;
        }

        public static bool TrySetRogueStealth(Player player, float value)
        {
            if (!TryGetCalamityPlayer(player, out object calamityPlayer) || _rogueStealthField == null)
            {
                return false;
            }

            _rogueStealthField.SetValue(calamityPlayer, value);
            return true;
        }

        public static bool TrySetRogueStealthCooldown(Player player, float value)
        {
            if (!TryGetCalamityPlayer(player, out object calamityPlayer) || _rogueStealthCooldownField == null)
            {
                return false;
            }

            if (_rogueStealthCooldownField.FieldType == typeof(int))
            {
                _rogueStealthCooldownField.SetValue(calamityPlayer, (int)value);
                return true;
            }

            if (_rogueStealthCooldownField.FieldType == typeof(float))
            {
                _rogueStealthCooldownField.SetValue(calamityPlayer, value);
                return true;
            }

            return false;
        }

        public static void DisableRogueStealthStrike(Player player)
        {
            if (!TryGetCalamityPlayer(player, out object calamityPlayer) || _stealthStrikeAvailableField == null)
            {
                return;
            }

            _stealthStrikeAvailableField.SetValue(calamityPlayer, false);
        }

        public static bool IsRogueStealthReady(Player player)
        {
            if (!TryGetCalamityPlayer(player, out object calamityPlayer) || _rogueStealthField == null || _rogueStealthMaxField == null)
            {
                return false;
            }

            float current = Convert.ToSingle(_rogueStealthField.GetValue(calamityPlayer));
            float max = Math.Max(0.0001f, Convert.ToSingle(_rogueStealthMaxField.GetValue(calamityPlayer)));
            return current >= max;
        }

        public static bool TryConsumeRogueStealthStrike(Player player)
        {
            if (!TryGetCalamityPlayer(player, out object calamityPlayer) || _rogueStealthField == null || _rogueStealthMaxField == null)
            {
                return false;
            }

            float current = Convert.ToSingle(_rogueStealthField.GetValue(calamityPlayer));
            float max = Math.Max(0.0001f, Convert.ToSingle(_rogueStealthMaxField.GetValue(calamityPlayer)));

            if (current < max)
            {
                return false;
            }

            _rogueStealthField.SetValue(calamityPlayer, 0f);

            if (_stealthStrikeAvailableField != null)
            {
                _stealthStrikeAvailableField.SetValue(calamityPlayer, false);
            }

            if (_rogueStealthCooldownField != null)
            {
                if (_rogueStealthCooldownField.FieldType == typeof(int))
                {
                    _rogueStealthCooldownField.SetValue(calamityPlayer, 60);
                }
                else if (_rogueStealthCooldownField.FieldType == typeof(float))
                {
                    _rogueStealthCooldownField.SetValue(calamityPlayer, 60f);
                }
            }

            return true;
        }

        public static bool RogueStealthIntegrationAvailable
        {
            get
            {
                if (!CalamityLoaded)
                {
                    return false;
                }

                return EnsureCalamityPlayerReflection() &&
                       _rogueStealthField != null &&
                       _rogueStealthMaxField != null;
            }
        }
    }
}