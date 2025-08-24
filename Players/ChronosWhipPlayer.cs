using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Players;

namespace LackOfNameStuff.Players
{
    public class ChronosWhipPlayer : ModPlayer
    {
        private Dictionary<int, Vector2> markedEnemies = new Dictionary<int, Vector2>();
        private int snapCooldown = 0;

        public void AddMarkedEnemy(int npcIndex, Vector2 position)
        {
            markedEnemies[npcIndex] = position;
        }

        public override void PostUpdateMiscEffects()
        {
            if (snapCooldown > 0) 
                snapCooldown--;

            var chronosPlayer = Player.GetModPlayer<ChronosPlayer>();
            
            // When bullet time ends, snap enemies back
            if (!chronosPlayer.bulletTimeActive && markedEnemies.Count > 0 && snapCooldown <= 0)
            {
                SnapMarkedEnemies();
                snapCooldown = 120; // 2 second cooldown
            }
        }

        private void SnapMarkedEnemies()
        {
            foreach (var kvp in markedEnemies.ToList())
            {
                int npcIndex = kvp.Key;
                Vector2 markedPosition = kvp.Value;
                
                if (npcIndex < Main.maxNPCs && Main.npc[npcIndex].active)
                {
                    NPC npc = Main.npc[npcIndex];
                    Vector2 pullVector = markedPosition - npc.Center;
                    
                    // Don't snap if too far (enemy might have teleported)
                    if (pullVector.Length() < 400f)
                    {
                        // Snap enemy back with damage
                        npc.velocity += pullVector * 0.3f;
                        
                        // Deal damage on snap
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int snapDamage = (int)(Player.GetWeaponDamage(Player.HeldItem) * 0.75f);
                            int hitDir = pullVector.X > 0 ? -1 : 1;
                            NPC.HitInfo hitInfo = new NPC.HitInfo()
                            {
                                Damage = snapDamage,
                                Knockback = 3f,
                                HitDirection = hitDir,
                                Crit = false
                            };

                            npc.StrikeNPC(hitInfo, fromNet: false);
                            if (Main.netMode == NetmodeID.Server)
                            {
                                NetMessage.SendStrikeNPC(npc, hitInfo, -1); 
                            }
                        }
                        
                        // Visual snap effect
                        CreateSnapEffect(npc.Center, markedPosition);
                    }
                }
            }
            
            markedEnemies.Clear();
        }

        private void CreateSnapEffect(Vector2 current, Vector2 marked)
        {
            // Line of dust between positions
            Vector2 direction = Vector2.Normalize(marked - current);
            float distance = Vector2.Distance(current, marked);
            
            for (float d = 0; d < distance; d += 8f)
            {
                Vector2 dustPos = current + direction * d;
                Dust dust = Dust.NewDustDirect(dustPos, 4, 4, DustID.Electric);
                dust.velocity = Vector2.Zero;
                dust.scale = 0.8f;
                dust.noGravity = true;
                dust.color = Color.Cyan;
                dust.fadeIn = 0.8f;
            }
            
            SoundEngine.PlaySound(SoundID.Item30, current);
        }

        public override void ResetEffects()
        {
            // Keep marked enemies between frames
        }
    }
}