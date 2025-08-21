using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using LackOfNameStuff.Items.Materials;
using LackOfNameStuff.Players;
using Microsoft.Xna.Framework;

namespace LackOfNameStuff.Globals
{
    public class TimeShardGlobalNPC : GlobalNPC
    {
        // Use ModifyNPCLoot instead of OnKill for guaranteed drops
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            // Moon Lord drops - GUARANTEED
            if (npc.type == NPCID.MoonLordCore)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TimeShard>(), 1, 8, 12)); // 100% chance, 8-12 shards
            }
            
            // Lunatic Cultist drops - GUARANTEED  
            if (npc.type == NPCID.CultistBoss)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TimeShard>(), 1, 3, 5)); // 100% chance, 3-5 shards
            }
            
            // Dungeon Guardian drops - GUARANTEED
            if (npc.type == NPCID.DungeonGuardian)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TimeShard>(), 1, 2, 4)); // 100% chance, 2-4 shards
            }
            
            // Solar Pillar enemies - RARE drops
            if (npc.type == NPCID.SolarCorite || npc.type == NPCID.SolarCrawltipedeHead || 
                npc.type == NPCID.SolarDrakomire || npc.type == NPCID.SolarSroller ||
                npc.type == NPCID.SolarSpearman)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TimeShard>(), 10, 1, 1)); // 10% chance, 1 shard
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
                    int dropChance = 4; // 25% base chance
                    if (npc.lifeMax > 100) dropChance = 3; // 33% for stronger enemies
                    if (npc.lifeMax > 500) dropChance = 2; // 50% for very strong enemies
                    
                    if (Main.rand.NextBool(dropChance))
                    {
                        Item.NewItem(npc.GetSource_Loot(), npc.getRect(), ModContent.ItemType<TimeShard>(), 1);
                        
                        // Visual feedback - more noticeable
                        CombatText.NewText(npc.getRect(), Color.Cyan, "TIME SHARD!", true);
                        
                        // Audio feedback
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, npc.position);
                        
                        // Extra visual effect
                        for (int i = 0; i < 10; i++)
                        {
                            Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Electric);
                            dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
                            dust.scale = 1.2f;
                            dust.noGravity = true;
                        }
                    }
                }
            }
        }
        
        // Debug method - you can remove this later
        public override void SetStaticDefaults()
        {
            // This will help us debug - check console for Moon Lord ID
            Main.NewText($"Moon Lord Core ID: {NPCID.MoonLordCore}");
            Main.NewText($"Cultist Boss ID: {NPCID.CultistBoss}");
        }
    }
}