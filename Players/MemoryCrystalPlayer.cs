using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Players;

namespace LackOfNameStuff.Players
{
    public class MemoryCrystalPlayer : ModPlayer
    {
        public bool hasMemoryCrystal = false;
        private List<DamageMemory> storedDamage = new List<DamageMemory>();

        private struct DamageMemory
        {
            public int npcIndex;
            public int damage;
            public Vector2 position;
            public Color crystalColor;
            
            public DamageMemory(int npc, int dmg, Vector2 pos, Color color)
            {
                npcIndex = npc;
                damage = dmg;
                position = pos;
                crystalColor = color;
            }
        }

        public override void ResetEffects()
        {
            hasMemoryCrystal = false;
        }

        public override void PostUpdateMiscEffects()
        {
            if (!hasMemoryCrystal) return;

            var chronosPlayer = Player.GetModPlayer<ChronosPlayer>();
            
            // When bullet time ends, replay all stored damage
            if (!chronosPlayer.bulletTimeActive && storedDamage.Count > 0)
            {
                ReplayStoredDamage();
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)/* tModPorter If you don't need the Item, consider using OnHitNPC instead */
        {
            if (!hasMemoryCrystal) return;

            var chronosPlayer = Player.GetModPlayer<ChronosPlayer>();

            // Store damage during bullet time
            if (chronosPlayer.bulletTimeActive)
            {
                int damage = Player.GetWeaponDamage(Player.HeldItem); // <-- added

                Vector2 crystalPos = target.Center + new Vector2(0, -target.height * 0.6f);
                Color crystalColor = GetDamageColor(damage);

                storedDamage.Add(new DamageMemory(target.whoAmI, damage, crystalPos, crystalColor));

                // Create memory crystal visual above enemy
                CreateMemoryCrystal(crystalPos, crystalColor);
            }
        }

        private void CreateMemoryCrystal(Vector2 position, Color color)
        {
            // Create floating crystal effect
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(position + new Vector2(-4, -4), 8, 8, DustID.CrystalPulse);
                dust.velocity = Main.rand.NextVector2Circular(1f, 1f);
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                dust.noGravity = true;
                dust.color = color;
                dust.alpha = 100;
            }
        }

        private Color GetDamageColor(int damage)
        {
            // Color based on damage amount
            if (damage < 50) return Color.Blue;
            else if (damage < 100) return Color.Green;
            else if (damage < 200) return Color.Yellow;
            else if (damage < 500) return Color.Orange;
            else return Color.Red;
        }

        private void ReplayStoredDamage()
        {
            foreach (var memory in storedDamage)
            {
                if (memory.npcIndex < Main.maxNPCs && Main.npc[memory.npcIndex].active)
                {
                    NPC target = Main.npc[memory.npcIndex];
                    
                // Deal the stored damage
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int hitDir = target.Center.X > Player.Center.X ? 1 : -1;

                    NPC.HitInfo hitInfo = new NPC.HitInfo()
                    {
                        Damage = memory.damage,
                        Knockback = 3f,
                        HitDirection = hitDir,
                        Crit = false
                    };

                    target.StrikeNPC(hitInfo, fromNet: false);

                    if (Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendStrikeNPC(target, hitInfo, -1);
                    }
                }

                    
                    // Crystal explosion effect
                    CreateCrystalExplosion(memory.position, memory.crystalColor);
                }
            }
            
            storedDamage.Clear();
            SoundEngine.PlaySound(SoundID.Item27, Player.Center); // Crystal break sound
        }

        private void CreateCrystalExplosion(Vector2 position, Color color)
        {
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(position + new Vector2(-8, -8), 16, 16, DustID.CrystalPulse);
                dust.velocity = Main.rand.NextVector2Circular(4f, 4f);
                dust.scale = Main.rand.NextFloat(1.0f, 1.5f);
                dust.noGravity = true;
                dust.color = color;
                dust.fadeIn = 1.2f;
            }
        }
    }
}