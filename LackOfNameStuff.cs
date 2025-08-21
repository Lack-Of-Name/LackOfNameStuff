using System.IO;
using Terraria.ModLoader;
using LackOfNameStuff.Systems;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using LackOfNameStuff.Items.Tools;

namespace LackOfNameStuff
{
    public class LackOfNameStuff : Mod
    {
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            ChronosNetworkHandler.HandlePacket(this, reader, whoAmI);
        }
    }

    public class RewindPlayer : ModPlayer
    {
        // Store last 600 ticks (10 seconds) of positions
        private Queue<Vector2> positionHistory = new Queue<Vector2>();
        private const int MaxHistoryLength = 600; // 10 seconds at 60 FPS

        public override void PostUpdate()
        {
            // Record current position every tick
            positionHistory.Enqueue(Player.position);
            
            // Keep only the last 10 seconds
            while (positionHistory.Count > MaxHistoryLength)
            {
                positionHistory.Dequeue();
            }
        }

        public void TriggerRewind()
        {
            if (positionHistory.Count > 0)
            {
                // Get the oldest position (10 seconds ago)
                Vector2 rewindPosition = positionHistory.Peek();
                
                // Teleport player
                Player.Teleport(rewindPosition);
                
                // Add some visual/audio feedback
                for (int i = 0; i < 30; i++)
                {
                    Vector2 dustPos = Player.position + new Vector2(Main.rand.Next(Player.width), Main.rand.Next(Player.height));
                    Dust dust = Dust.NewDustDirect(dustPos, 0, 0, DustID.MagicMirror);
                    dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
                    dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                }
                
                // Play sound
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item6, Player.position);
            }
        }

        // Reset position history when player dies or world changes
        public override void OnRespawn()
        {
            positionHistory.Clear();
        }
        
        public override void OnEnterWorld()
        {
            positionHistory.Clear();
        }
    }

    public class TemporalPickaxePlayer : ModPlayer
    {
        public int BlocksMinedWithTemporalPickaxe = 0;
        private const int MaxBlocks = 5000; // BigNumber for speed calculation
        
        public float GetSpeedBonus()
        {
            // Calculate speed bonus: (BlocksMined / 5000) * 1000, capped at 1000%
            return Math.Min(1000f, (float)BlocksMinedWithTemporalPickaxe / MaxBlocks * 1000f);
        }

        // Clean method - removed duplicates
        public void OnBlockMined()
        {
            if (Player.HeldItem.ModItem is TemporalPickaxe)
            {
                BlocksMinedWithTemporalPickaxe++;

                // Visual feedback every 100 blocks
                if (BlocksMinedWithTemporalPickaxe % 100 == 0)
                {
                    CombatText.NewText(Player.getRect(), Color.Cyan, $"+{GetSpeedBonus():F1}% speed!", true);
                }
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["BlocksMinedWithTemporalPickaxe"] = BlocksMinedWithTemporalPickaxe;
        }

        public override void LoadData(TagCompound tag)
        {
            BlocksMinedWithTemporalPickaxe = tag.GetInt("BlocksMinedWithTemporalPickaxe");
        }
    }
}