using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using LackOfNameStuff.Items.Materials;
using LackOfNameStuff.Players;
using LackOfNameStuff.Common;
using Microsoft.Xna.Framework;

namespace LackOfNameStuff.Globals
{
    public class TimeShardGlobalNPC : GlobalNPC
    {
        // Use ModifyNPCLoot instead of OnKill for guaranteed drops
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            // Moon Lord drops - GUARANTEED Time Shards (Entry-level)
            if (npc.type == NPCID.MoonLordCore)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TimeShard>(), 1, 3, 6)); // 100% chance, 3-6 shards
            }
            
            // Lunatic Cultist drops - GUARANTEED Time Shards
            if (npc.type == NPCID.CultistBoss)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TimeShard>(), 1, 2, 4)); // 100% chance, 2-4 shards
            }
            
            // Dungeon Guardian drops - GUARANTEED Time Shards
            if (npc.type == NPCID.DungeonGuardian)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TimeShard>(), 2, 1, 3)); // 50% chance, 1-3 shards
            }
            
            // Celestial Pillar enemies - RARE Time Shard drops
            if (IsCelestialEnemy(npc.type))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TimeShard>(), 15, 1, 2)); // 6.67% chance, 1-2 shards
            }

            // Calamity boss integration (if Calamity is loaded)
            if (TemporalProgressionSystem.calamityMod != null)
            {
                AddCalamityBossDrops(npc, npcLoot);
            }
        }

        private bool IsCelestialEnemy(int npcType)
        {
            return npcType == NPCID.SolarCorite || npcType == NPCID.SolarCrawltipedeHead || 
                   npcType == NPCID.SolarDrakomire || npcType == NPCID.SolarSroller ||
                   npcType == NPCID.SolarSpearman || npcType == NPCID.NebulaBeast ||
                   npcType == NPCID.NebulaHeadcrab || npcType == NPCID.NebulaSoldier ||
                   npcType == NPCID.StardustCellBig || npcType == NPCID.StardustJellyfishBig ||
                   npcType == NPCID.StardustSoldier || npcType == NPCID.StardustSpiderBig ||
                   npcType == NPCID.VortexHornet || npcType == NPCID.VortexLarva ||
                   npcType == NPCID.VortexRifleman || npcType == NPCID.VortexSoldier;
        }

        private void AddCalamityBossDrops(NPC npc, NPCLoot npcLoot)
        {
            var calamity = TemporalProgressionSystem.calamityMod;
            
            // Providence drops - Eternal Shards
            if (calamity.TryFind("Providence", out ModNPC providence) && npc.type == providence.Type)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EternalShard>(), 1, 4, 8)); // 100% chance, 4-8 eternal shards
            }
            
            // Polterghast drops - Time Gems  
            if (calamity.TryFind("Polterghast", out ModNPC polterghast) && npc.type == polterghast.Type)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TimeGem>(), 1, 2, 5)); // 100% chance, 2-5 time gems
            }
            
            // Devourer of Gods drops - Eternal Gems
            if (calamity.TryFind("DevourerofGodsHead", out ModNPC dogHead) && npc.type == dogHead.Type)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EternalGem>(), 1, 3, 6)); // 100% chance, 3-6 eternal gems
            }
            
            // Yharon drops - Eternal Gems
            if (calamity.TryFind("Yharon", out ModNPC yharon) && npc.type == yharon.Type)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EternalGem>(), 1, 4, 7)); // 100% chance, 4-7 eternal gems
            }
            
            // Supreme Calamitas drops - Eternal Gems (highest tier)
            if (calamity.TryFind("SupremeCalamitas", out ModNPC scal) && npc.type == scal.Type)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EternalGem>(), 1, 6, 10)); // 100% chance, 6-10 eternal gems
            }
        }

        // Keep OnKill for Chronos time drops (this is special logic)
        public override void OnKill(NPC npc)
        {
            // Skip if it's a boss (they already drop via ModifyNPCLoot)
            if (npc.boss) return;
            
            // Skip if NPC gives no experience (critters, etc.)
            if (npc.lifeMax <= 5) return;
            
            // Enemies killed during Chronos time
            Player killer = Main.player[Player.FindClosest(npc.position, npc.width, npc.height)];
            if (killer != null && killer.active)
            {
                var chronosPlayer = killer.GetModPlayer<ChronosPlayer>();
                if (chronosPlayer != null && chronosPlayer.screenEffectIntensity > 0.1f) // If in chronos time
                {
                    // Higher chance based on enemy value/difficulty
                    int dropChance = GetChronosDropChance(npc);
                    
                    if (Main.rand.NextBool(dropChance))
                    {
                        // Drop appropriate tier material based on progression
                        int materialType = GetAppropriateTimeMaterial();
                        
                        Item.NewItem(npc.GetSource_Loot(), npc.getRect(), materialType, 1);
                        
                        // Visual feedback - more noticeable
                        string materialName = GetMaterialName(materialType);
                        CombatText.NewText(npc.getRect(), GetMaterialColor(materialType), materialName, true);
                        
                        // Audio feedback
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, npc.position);
                        
                        // Extra visual effect with appropriate dust
                        CreateMaterialDust(npc, materialType);
                    }
                }
            }
        }

        private int GetChronosDropChance(NPC npc)
        {
            // Base 2% chance
            int dropChance = 50;
            
            // Increase chance for stronger enemies
            if (npc.lifeMax > 500) dropChance = 33; // 3% for stronger enemies
            if (npc.lifeMax > 1000) dropChance = 20; // 5% for very strong enemies
            if (npc.lifeMax > 2000) dropChance = 15; // 6.67% for elite enemies
            
            return dropChance;
        }

        private int GetAppropriateTimeMaterial()
        {
            // Return appropriate material based on progression
            if (TemporalProgressionSystem.HasDefeatedDoG())
                return ModContent.ItemType<EternalGem>();
            else if (TemporalProgressionSystem.HasDefeatedPolterghast())
                return ModContent.ItemType<TimeGem>();
            else if (TemporalProgressionSystem.HasDefeatedProvidence())
                return ModContent.ItemType<EternalShard>();
            else
                return ModContent.ItemType<TimeShard>();
        }

        private string GetMaterialName(int materialType)
        {
            if (materialType == ModContent.ItemType<EternalGem>())
                return "ETERNAL GEM!";
            else if (materialType == ModContent.ItemType<TimeGem>())
                return "TIME GEM!";
            else if (materialType == ModContent.ItemType<EternalShard>())
                return "ETERNAL SHARD!";
            else
                return "TIME SHARD!";
        }

        private Color GetMaterialColor(int materialType)
        {
            if (materialType == ModContent.ItemType<EternalGem>())
                return Color.Violet;
            else if (materialType == ModContent.ItemType<TimeGem>())
                return Color.Cyan;
            else if (materialType == ModContent.ItemType<EternalShard>())
                return Color.Purple;
            else
                return Color.Orange;
        }

        private void CreateMaterialDust(NPC npc, int materialType)
        {
            int dustType = DustID.Electric;
            Color dustColor = Color.Orange;
            
            if (materialType == ModContent.ItemType<EternalGem>())
            {
                dustType = DustID.RainbowMk2;
                dustColor = Color.Violet;
            }
            else if (materialType == ModContent.ItemType<TimeGem>())
            {
                dustType = DustID.IceTorch;
                dustColor = Color.Cyan;
            }
            else if (materialType == ModContent.ItemType<EternalShard>())
            {
                dustType = DustID.Shadowflame;
                dustColor = Color.Purple;
            }
            
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, dustType);
                dust.velocity = Main.rand.NextVector2Circular(4f, 4f);
                dust.scale = 1.3f;
                dust.color = dustColor;
                dust.noGravity = true;
            }
        }
    }
}