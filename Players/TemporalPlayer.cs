// TemporalPlayer.cs - Enhanced ModPlayer for Multi-Class Armor
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using LackOfNameStuff.Players;
using System;
using Terraria.DataStructures;

namespace LackOfNameStuff.Players
{
    public class TemporalPlayer : ModPlayer
    {
        public bool hasTemporalSet = false;
        public bool temporalAwareness = false;
        public string helmetType = "None";
        private int temporalProcCooldown = 0;
        private int meleeShockwaveCooldown = 0;
        private int minionSlowCooldown = 0;

        public override void ResetEffects()
        {
            hasTemporalSet = false;
            temporalAwareness = false;
            helmetType = "None";
        }

        public override void PostUpdateMiscEffects()
        {
            if (temporalProcCooldown > 0)
                temporalProcCooldown--;
            if (meleeShockwaveCooldown > 0)
                meleeShockwaveCooldown--;
            if (minionSlowCooldown > 0)
                minionSlowCooldown--;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasTemporalSet || temporalProcCooldown > 0) return;

            // Get Chronos Watch status for proc chance modification
            var chronosPlayer = Player.GetModPlayer<ChronosPlayer>();
            bool hasChronosWatch = chronosPlayer?.hasChronosWatch == true;

            // Base proc chance
            int baseProcChance = hasChronosWatch ? 3 : 4; // 33% with watch, 25% without

            switch (helmetType)
            {
                case "Ranged":
                    HandleRangedProc(target, hit, baseProcChance);
                    break;
                case "Melee":
                    HandleMeleeProc(target, hit, baseProcChance);
                    break;
                case "Magic":
                    HandleMagicProc(target, hit, baseProcChance);
                    break;
                case "Summoner":
                    // Summoner effects are handled in OnHitNPCWithProj
                    break;
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasTemporalSet) return;

            // Handle summoner minion effects
            if (helmetType == "Summoner" && proj.minion && minionSlowCooldown <= 0)
            {
                if (Main.rand.NextBool(5)) // 20% chance for minions to proc
                {
                    target.AddBuff(ModContent.BuffType<Buffs.TemporalSlow>(), 120); // 2 seconds
                    minionSlowCooldown = 120; // 2 second cooldown
                    CreateTemporalEffect(target, Color.Purple);
                }
            }
        }

        private void HandleRangedProc(NPC target, NPC.HitInfo hit, int procChance)
        {
            if (Main.rand.NextBool(procChance))
            {
                target.AddBuff(ModContent.BuffType<Buffs.TemporalSlow>(), 240); // 4 seconds
                temporalProcCooldown = 60;
                CreateTemporalEffect(target, Color.Cyan);
            }
        }

        private void HandleMeleeProc(NPC target, NPC.HitInfo hit, int procChance)
        {
            if (Main.rand.NextBool(procChance) && meleeShockwaveCooldown <= 0)
            {
                // Create temporal shockwave
                CreateMeleeShockwave(target.Center);
                meleeShockwaveCooldown = 120; // 2 second cooldown
                temporalProcCooldown = 30; // Shorter cooldown for melee
            }
        }

        private void HandleMagicProc(NPC target, NPC.HitInfo hit, int procChance)
        {
            if (Main.rand.NextBool(procChance))
            {
                // Pierce through time - affect nearby enemies
                float pierceRadius = 100f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && Vector2.Distance(npc.Center, target.Center) < pierceRadius)
                    {
                        npc.AddBuff(ModContent.BuffType<Buffs.TemporalSlow>(), 180); // 3 seconds
                        CreateTemporalEffect(npc, Color.Magenta);
                    }
                }
                temporalProcCooldown = 90; // 1.5 second cooldown
            }
        }

        private void CreateMeleeShockwave(Vector2 center)
        {
            // Visual shockwave effect
            for (int ring = 0; ring < 2; ring++)
            {
                int numDust = 20;
                float ringRadius = 60f + (ring * 30f);
                
                for (int i = 0; i < numDust; i++)
                {
                    float angle = (float)i / numDust * MathHelper.TwoPi;
                    Vector2 dustPos = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * ringRadius;
                    
                    Dust dust = Dust.NewDustDirect(dustPos, 0, 0, DustID.Electric);
                    dust.velocity = Vector2.Zero;
                    dust.scale = 1.2f - (ring * 0.2f);
                    dust.noGravity = true;
                    dust.color = Color.Orange;
                    dust.fadeIn = 1f;
                }
            }

            // Damage and slow nearby enemies
            float shockwaveRadius = 90f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && Vector2.Distance(npc.Center, center) < shockwaveRadius)
                {
                    npc.AddBuff(ModContent.BuffType<Buffs.TemporalSlow>(), 300); // 5 seconds
                    
                    // Additional knockback
                    Vector2 knockback = Vector2.Normalize(npc.Center - center) * 8f;
                    npc.velocity += knockback;
                }
            }
        }

        private void CreateTemporalEffect(NPC target, Color effectColor)
        {
            // Standard temporal effect
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Electric);
                dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
                dust.scale = 1.0f;
                dust.noGravity = true;
                dust.color = effectColor;
            }
        }

        public override void PostUpdateEquips()
        {
            // Enhance Chronos Watch when wearing Temporal Set
            if (hasTemporalSet)
            {
                var chronosPlayer = Player.GetModPlayer<ChronosPlayer>();
                if (chronosPlayer?.hasChronosWatch == true)
                {
                    // Class-specific enhancements
                    switch (helmetType)
                    {
                        case "Ranged":
                            // Faster cooldown recovery
                            if (chronosPlayer.bulletTimeCooldown > 0 && Main.rand.NextBool(3))
                                chronosPlayer.bulletTimeCooldown--;
                            break;
                        case "Melee":
                            // Stronger bullet time effect (L melee effect - boring class)
                            if (chronosPlayer.bulletTimeActive)
                                chronosPlayer.screenEffectIntensity = MathHelper.Min(chronosPlayer.screenEffectIntensity * 1.3f, 1.0f);
                            break;
                        case "Magic":
                            // Faster cooldown recovery
                            if (chronosPlayer.bulletTimeCooldown > 0 && Main.rand.NextBool(3))
                                chronosPlayer.bulletTimeCooldown--;
                            break;
                        case "Summoner":
                            // Minions gain bullet time benefits
                            if (chronosPlayer.bulletTimeActive)
                            {
                                // Increase minion damage during bullet time
                                Player.GetDamage(DamageClass.Summon) += 0.25f;
                            }
                            break;
                    }
                }
            }
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            // Class-specific glow effects when wearing full set
            if (hasTemporalSet && Main.rand.NextBool(20))
            {
                Color glowColor = helmetType switch
                {
                    "Ranged" => Color.Cyan,
                    "Melee" => Color.Orange,
                    "Magic" => Color.Magenta,
                    "Summoner" => Color.Purple,
                    _ => Color.Yellow
                };

                Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Electric);
                dust.velocity = Main.rand.NextVector2Circular(1f, 1f);
                dust.scale = 0.6f;
                dust.noGravity = true;
                dust.color = glowColor;
                dust.alpha = 200;
            }
        }
    }
}