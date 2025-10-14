// TemporalPlayer.cs - Fixed missile triggering and improved progression system
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using LackOfNameStuff.Players;
using System;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.ModLoader.IO;
using LackOfNameStuff.Projectiles;
using Terraria.Localization;

namespace LackOfNameStuff.Players
{
    public class TemporalPlayer : ModPlayer
    {
        public bool hasTemporalSet = false;
        public string helmetType = "None";
        private int missileCooldown = 0;
        // When true, suppress auto-unlocks from inventory/pickups/crafting (useful for testing downgrades)
        public bool debugLockTier = false;
        
        // Progression system - these are permanent unlocks per player
        public int unlockedTier = 1; // Highest tier this player has unlocked (starts at 1)
        public bool hasUnlockedTimeShard = true;    // Tier 1 - everyone starts with this
        public bool hasUnlockedEternalShard = false; // Tier 2
        public bool hasUnlockedTimeGem = false;      // Tier 3  
        public bool hasUnlockedEternalGem = false;   // Tier 4
        
        // Current tier is now based on unlocked progression
        public int currentTier => unlockedTier;
        
        public override void ResetEffects()
        {
            hasTemporalSet = false;
            helmetType = "None";
        }

        // Check for material acquisition/crafting
        public override void OnEnterWorld()
        {
            // Check inventory for materials when entering world (in case they got items while offline)
            CheckForMaterialsInInventory();
        }

        public override void PostUpdateMiscEffects()
        {
            if (missileCooldown > 0)
                missileCooldown--;
                
            // Periodically check for new materials (every 60 ticks = 1 second)
            if (Main.GameUpdateCount % 60 == 0)
            {
                CheckForMaterialsInInventory();
            }
        }

        private void CheckForMaterialsInInventory()
        {
            if (debugLockTier) return;
            bool tierChanged = false;
            
            // Check for Eternal Shard (Tier 2)
            if (!hasUnlockedEternalShard && PlayerHasMaterial("Eternal Shard"))
            {
                UnlockTier(2, "Eternal Shard");
                tierChanged = true;
            }
            
            // Check for Time Gem (Tier 3)
            if (!hasUnlockedTimeGem && PlayerHasMaterial("Time Gem"))
            {
                UnlockTier(3, "Time Gem");
                tierChanged = true;
            }
            
            // Check for Eternal Gem (Tier 4)
            if (!hasUnlockedEternalGem && PlayerHasMaterial("Eternal Gem"))
            {
                UnlockTier(4, "Eternal Gem");
                tierChanged = true;
            }

            // Visual feedback when tier changes
            if (tierChanged)
            {
                CreateTierUpgradeEffect();
            }
        }

        private bool PlayerHasMaterial(string materialName)
        {
            // Check player's inventory for the material
            for (int i = 0; i < Player.inventory.Length; i++)
            {
                if (Player.inventory[i] != null && !Player.inventory[i].IsAir)
                {
                    // Alternative: Check by ModContent.ItemType specific item classes
                    try
                    {
                        if (materialName == "Eternal Shard" && Player.inventory[i].type == ModContent.ItemType<Items.Materials.EternalShard>())
                            return true;
                        if (materialName == "Time Gem" && Player.inventory[i].type == ModContent.ItemType<Items.Materials.TimeGem>())
                            return true;
                        if (materialName == "Eternal Gem" && Player.inventory[i].type == ModContent.ItemType<Items.Materials.EternalGem>())
                            return true;
                    }
                    catch (Exception)
                    {
                        // If the ModContent.ItemType fails, fall back to name checking
                        if (materialName == "Eternal Shard" && Player.inventory[i].Name.Contains("Eternal Shard"))
                            return true;
                        if (materialName == "Time Gem" && Player.inventory[i].Name.Contains("Time Gem"))
                            return true;
                        if (materialName == "Eternal Gem" && Player.inventory[i].Name.Contains("Eternal Gem"))
                            return true;
                    }
                }
            }
            
            // Also check piggy bank, safe, etc.
            for (int i = 0; i < Player.bank.item.Length; i++)
            {
                if (Player.bank.item[i] != null && !Player.bank.item[i].IsAir)
                {
                    if (materialName == "Eternal Shard" && Player.bank.item[i].Name.Contains("Eternal Shard"))
                        return true;
                    if (materialName == "Time Gem" && Player.bank.item[i].Name.Contains("Time Gem"))
                        return true;
                    if (materialName == "Eternal Gem" && Player.bank.item[i].Name.Contains("Eternal Gem"))
                        return true;
                }
            }
            
            return false;
        }

        private void UnlockTier(int tier, string materialName)
        {
            switch (tier)
            {
                case 2:
                    hasUnlockedEternalShard = true;
                    break;
                case 3:
                    hasUnlockedTimeGem = true;
                    break;
                case 4:
                    hasUnlockedEternalGem = true;
                    break;
            }
            
            unlockedTier = Math.Max(unlockedTier, tier);
            
            // Send congratulations message (localized)
            string upgradeText = Language.GetTextValue("Mods.LackOfNameStuff.Common.Progression.TierUpgrade", materialName, tier);
            Main.NewText(upgradeText, GetTierColor(tier));
            
            // Play upgrade sound
            SoundEngine.PlaySound(SoundID.Item4.WithVolumeScale(0.8f).WithPitchOffset(0.2f), Player.Center);
            
            // Sync in multiplayer
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)10); // Message type: Tier unlock
                packet.Write(Player.whoAmI);
                packet.Write(tier);
                packet.Send();
            }
        }

        private void CreateTierUpgradeEffect()
        {
            // Dramatic visual effect when tier upgrades
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    Player.position - new Vector2(20, 20), 
                    Player.width + 40, 
                    Player.height + 40, 
                    DustID.Electric
                );
                dust.velocity = Main.rand.NextVector2Circular(8f, 8f);
                dust.scale = Main.rand.NextFloat(1.2f, 2f);
                dust.noGravity = true;
                dust.color = GetTierColor(currentTier);
            }
            
            // Ring effect expanding outward
            for (int i = 0; i < 12; i++)
            {
                float angle = (float)i / 12f * MathHelper.TwoPi;
                Vector2 velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 6f;
                
                Dust dust = Dust.NewDustDirect(Player.Center, 4, 4, DustID.Electric);
                dust.velocity = velocity;
                dust.scale = 1.5f;
                dust.noGravity = true;
                dust.color = GetTierColor(currentTier);
            }
        }

        // Hook into the crafting system
        public void PostItemCraft(Recipe recipe, Item item)
        {
            if (debugLockTier) return;
            // Check if the crafted item is one of our progression materials
            CheckCraftedItem(item);
        }
        
        // Also check when items are picked up
        public override bool OnPickup(Item item)
        {
            if (debugLockTier) return base.OnPickup(item);
            CheckCraftedItem(item);
            return base.OnPickup(item);
        }

        private void CheckCraftedItem(Item item)
        {
            if (debugLockTier) return;
            if (item == null || item.IsAir) return;

            // Check for tier upgrades immediately when materials are obtained
            if (!hasUnlockedEternalShard && item.Name.Contains("Eternal Shard"))
            {
                UnlockTier(2, "Eternal Shard");
                string msg = Language.GetTextValue("Mods.LackOfNameStuff.Common.Progression.Tier2Hint");
                Main.NewText(msg, Color.Purple);
            }
            else if (!hasUnlockedTimeGem && item.Name.Contains("Time Gem"))
            {
                UnlockTier(3, "Time Gem");
                string msg = Language.GetTextValue("Mods.LackOfNameStuff.Common.Progression.Tier3Hint");
                Main.NewText(msg, Color.Cyan);
            }
            else if (!hasUnlockedEternalGem && item.Name.Contains("Eternal Gem"))
            {
                UnlockTier(4, "Eternal Gem");
                string msg = Language.GetTextValue("Mods.LackOfNameStuff.Common.Progression.Tier4Hint");
                Main.NewText(msg, Color.White);
            }
        }

        // Method for other systems that might want to check progression
        public bool HasUnlockedTier(int tier)
        {
            return unlockedTier >= tier;
        }

        // Method to force unlock a tier (for admin commands, cheats, etc.)
        public void ForceUnlockTier(int tier)
        {
            // Backwards-compatible: now delegates to SetTier to allow lowering and proper flag sync
            SetTier(tier, showEffects: true);
        }

        // Set the temporal tier exactly (1-4), syncing all unlock flags. Only shows effects on upgrades.
        public void SetTier(int tier, bool showEffects = false)
        {
            int oldTier = unlockedTier;
            tier = Math.Clamp(tier, 1, 4);

            // Sync boolean unlocks to match the exact tier
            hasUnlockedEternalShard = tier >= 2;
            hasUnlockedTimeGem = tier >= 3;
            hasUnlockedEternalGem = tier >= 4;
            unlockedTier = tier;

            // Only play upgrade visuals when actually increasing tier
            if (showEffects && tier > oldTier)
            {
                CreateTierUpgradeEffect();
            }
        }

        // Only trigger from direct player attacks (item/melee swing), not projectile hits
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Only trigger if player has temporal armor set equipped
            if (!hasTemporalSet || missileCooldown > 0) return;

            // Ignore target dummies entirely
            if (target.type == NPCID.TargetDummy) return;

            // Apply class-specific effect first
            ApplyClassSpecificEffect(target, hit, damageDone);

            // Then trigger universal missile system
            TriggerTemporalMissiles(target);
        }

        // Prevent missiles triggering more missiles; allow other projectiles to trigger
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Ignore target dummies entirely
            if (target.type == NPCID.TargetDummy) return;

            // Check if this projectile is one of our temporal missiles
            if (proj.type == ModContent.ProjectileType<TemporalMissile>() || 
                proj.type == ModContent.ProjectileType<TemporalSubMissile>())
            {
                // Do NOT trigger missiles from temporal projectiles
                return;
            }

            // For other projectiles, apply the temporal set bonus if equipped
            if (hasTemporalSet && missileCooldown <= 0)
            {
                // Apply class-specific effect
                ApplyClassSpecificEffect(target, hit, damageDone);
                
                // Trigger missiles from player projectiles (but not temporal ones)
                TriggerTemporalMissiles(target);
            }
        }

        private void ApplyClassSpecificEffect(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var chronosPlayer = Player.GetModPlayer<ChronosPlayer>();
            bool hasChronosWatch = chronosPlayer?.hasChronosWatch == true;
            int tier = currentTier;
            
            switch (helmetType)
            {
                case "Ranged":
                    // Ranged: Critical hits extend bullet time duration
                    if (hit.Crit && hasChronosWatch && chronosPlayer.bulletTimeActive)
                    {
                        // Extend bullet time more at higher tiers
                        int extra = 30 + 6 * (tier - 1); // 0.5s base -> +0.1s per tier
                        chronosPlayer.ExtendBulletTime(extra);
                        CreateClassEffect(target, Color.Cyan);
                    }
                    break;

                case "Melee":
                    // Melee: Kills reduce bullet time cooldown
                    if (target.life <= 0 && hasChronosWatch)
                    {
                        int reduce = 60 + 12 * (tier - 1); // -1.0s base, +0.2s per tier
                        chronosPlayer.bulletTimeCooldown = Math.Max(chronosPlayer.bulletTimeCooldown - reduce, 0);
                        CreateClassEffect(target, Color.Orange);
                    }
                    break;

                case "Magic":
                    // Magic: High damage hits reduce mana costs temporarily
                    if (damageDone > target.lifeMax * 0.1f) // If hit deals 10%+ of enemy max HP
                    {
                        int duration = 300 + 60 * (tier - 1); // 5s base, +1s per tier
                        Player.AddBuff(ModContent.BuffType<Buffs.TemporalCasting>(), duration);
                        CreateClassEffect(target, Color.Magenta);
                    }
                    break;

                case "Summoner":
                    // Summoner: Hits grant minions temporary damage boost
                    int dur = 240 + 60 * (tier - 1); // 4s base, +1s per tier
                    Player.AddBuff(ModContent.BuffType<Buffs.TemporalMinions>(), dur);
                    CreateClassEffect(target, Color.Purple);
                    break;
            }
        }

        private void TriggerTemporalMissiles(NPC target)
        {
            if (target == null || !target.active || target.friendly || target.type == NPCID.TargetDummy)
                return;

            // Set cooldown based on tier (higher tier = shorter cooldown for quick succession)
            int[] cooldowns = { 0, 60, 50, 40, 30 }; // Slightly conservative but faster at higher tiers
            missileCooldown = cooldowns[Math.Min(currentTier, 4)];

            // Calculate base damage based on tier and class
            int baseDamage = GetMissileBaseDamage();

            // Determine number of missiles based on tier
            int missiles = currentTier switch { 1 => 2, 2 => 3, 3 => 4, 4 => 5, _ => 2 };

            // Launch missiles from an arc above player, fanning out slightly
            float arcWidth = MathHelper.ToRadians(40f); // total arc angle
            float startAngle = -MathHelper.PiOver2 - arcWidth / 2f; // centered above
            float angleStep = missiles > 1 ? arcWidth / (missiles - 1) : 0f;
            float radius = 40f;

            for (int i = 0; i < missiles; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector2 offset = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * radius;
                Vector2 spawnPos = Player.Center + offset;

                // Initial velocity: outward along the arc plus a bit upward; homing takes over
                Vector2 initialVel = offset.SafeNormalize(Vector2.UnitY) * -6f + new Vector2(0, -2f);

                Projectile.NewProjectile(
                    Player.GetSource_Accessory(Player.armor[0]),
                    spawnPos,
                    initialVel,
                    ModContent.ProjectileType<TemporalMissile>(),
                    baseDamage,
                    3f,
                    Player.whoAmI,
                    ai0: target.whoAmI,
                    ai1: currentTier
                );

                CreateMissileLaunchEffect(spawnPos);
            }

            // Audio feedback
            SoundEngine.PlaySound(SoundID.Item61.WithVolumeScale(0.7f), Player.Center);
        }

        private int GetMissileBaseDamage()
        {
            // Base damage scales with tier and player's highest damage class
            int tierMultiplier = currentTier * 25; // 25/50/75/100 per tier
            
            // Get player's highest damage stat
            float highestDamage = Math.Max(
                Math.Max(Player.GetDamage(DamageClass.Melee).Multiplicative, Player.GetDamage(DamageClass.Ranged).Multiplicative),
                Math.Max(Player.GetDamage(DamageClass.Magic).Multiplicative, Player.GetDamage(DamageClass.Summon).Multiplicative)
            );

            return (int)(tierMultiplier * highestDamage);
        }

        private void CreateClassEffect(NPC target, Color effectColor)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Electric);
                dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
                dust.scale = 0.8f;
                dust.noGravity = true;
                dust.color = effectColor;
            }
        }

        private void CreateMissileLaunchEffect(Vector2 position)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(position, 8, 8, DustID.Electric);
                dust.velocity = Main.rand.NextVector2Circular(4f, 4f);
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                dust.noGravity = true;
                dust.color = GetTierColor(currentTier);
            }
        }

        private Color GetTierColor(int tier)
        {
            return tier switch
            {
                1 => Color.Orange,     // Time Shard
                2 => Color.Purple,     // Eternal Shard
                3 => Color.Cyan,       // Time Gem
                4 => Color.White,      // Eternal Gem (rainbow in actual implementation)
                _ => Color.Yellow
            };
        }

        // Save/Load the progression data
        public override void SaveData(TagCompound tag)
        {
            tag["unlockedTier"] = unlockedTier;
            tag["hasUnlockedEternalShard"] = hasUnlockedEternalShard;
            tag["hasUnlockedTimeGem"] = hasUnlockedTimeGem;
            tag["hasUnlockedEternalGem"] = hasUnlockedEternalGem;
            tag["debugLockTier"] = debugLockTier;
        }

        public override void LoadData(TagCompound tag)
        {
            unlockedTier = tag.GetInt("unlockedTier");
            hasUnlockedEternalShard = tag.GetBool("hasUnlockedEternalShard");
            hasUnlockedTimeGem = tag.GetBool("hasUnlockedTimeGem");
            hasUnlockedEternalGem = tag.GetBool("hasUnlockedEternalGem");
            debugLockTier = tag.ContainsKey("debugLockTier") && tag.GetBool("debugLockTier");
            
            // Fallback: if no save data, start at tier 1
            if (unlockedTier == 0) unlockedTier = 1;
        }
    }
}