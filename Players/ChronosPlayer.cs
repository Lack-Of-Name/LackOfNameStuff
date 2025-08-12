using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System;
using System.Collections.Generic;
using LackOfNameStuff.Items.Accessories;
using LackOfNameStuff.Systems;
using LackOfNameStuff.Effects;

namespace LackOfNameStuff.Players
{
    public class ChronosPlayer : ModPlayer
    {
        public bool hasChronosWatch = false;
        public bool bulletTimeActive = false;
        public int bulletTimeRemaining = 0;
        public int bulletTimeCooldown = 0;
        
        // Track global bullet time state for visual effects
        public bool wasGlobalBulletTimeActive = false;
        
        // Visual effects - only ripples are per-player now
        public List<BulletTimeRipple> activeRipples = new List<BulletTimeRipple>();
        public int lastActivationFrame = 0;

        public override void ResetEffects()
        {
            hasChronosWatch = false;
        }

        public override void PostUpdate()
        {
            // Handle bullet time activation (only for players with the item)
            if (hasChronosWatch && ModContent.GetInstance<ChronosSystem>().BulletTimeKey.JustPressed)
            {
                TryActivateBulletTime();
            }

            // Update personal bullet time state
            UpdateBulletTime();
            
            // Update visual effects based on GLOBAL bullet time (for ALL players)
            UpdateGlobalVisualEffects();
            
            // Update ripple effects
            UpdateRippleEffects();
        }

        // Players remain at normal speed during bullet time
        public override void PostUpdateMiscEffects()
        {
            // Players are NOT slowed down - they remain at normal speed
            // This method is intentionally empty to keep players at normal speed
        }

        private void UpdateGlobalVisualEffects()
        {
            // Check if global bullet time state changed
            bool isGlobalBulletTimeActive = ChronosSystem.GlobalBulletTimeActive;
            
            if (isGlobalBulletTimeActive && !wasGlobalBulletTimeActive)
            {
                // Global bullet time just activated
                if (!bulletTimeActive) // If this player didn't activate it
                {
                    Player activatingPlayer = ChronosSystem.GlobalBulletTimeOwner;
                    if (activatingPlayer != null)
                    {
                        // Create visual effects at the activating player's position
                        activeRipples.Add(new BulletTimeRipple(activatingPlayer.Center, ChronosWatch.RippleLifetime, true));
                        SoundEngine.PlaySound(SoundID.Item29.WithVolumeScale(0.4f).WithPitchOffset(-0.3f), activatingPlayer.Center);
                    }
                }
            }
            else if (!isGlobalBulletTimeActive && wasGlobalBulletTimeActive)
            {
                // Global bullet time just deactivated
                if (!bulletTimeActive) // If this player didn't deactivate it
                {
                    // Create deactivation effects at the origin point
                    Vector2 origin = ChronosSystem.GlobalBulletTimeOrigin;
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                        activeRipples.Add(new BulletTimeRipple(origin + offset, ChronosWatch.RippleLifetime / 3, false));
                    }
                    SoundEngine.PlaySound(SoundID.Item25.WithVolumeScale(0.3f), origin);
                }
            }
            
            wasGlobalBulletTimeActive = isGlobalBulletTimeActive;
        }

        private void TryActivateBulletTime()
        {
            // Check if we can activate
            if (bulletTimeActive || bulletTimeCooldown > 0)
                return;

            // Activate bullet time
            bulletTimeActive = true;
            bulletTimeRemaining = ChronosWatch.BulletTimeDuration;
            lastActivationFrame = (int)Main.GameUpdateCount;

            // Create initial ripple effect
            CreateActivationRipple();

            // Play sound effect
            SoundEngine.PlaySound(SoundID.Item29.WithVolumeScale(0.8f).WithPitchOffset(-0.3f), Player.Center);
        }

        private void UpdateBulletTime()
        {
            if (bulletTimeActive)
            {
                bulletTimeRemaining--;
                
                if (bulletTimeRemaining <= 0)
                {
                    // Deactivate bullet time
                    bulletTimeActive = false;
                    bulletTimeCooldown = ChronosWatch.CooldownDuration;
                    
                    // Create deactivation effect
                    CreateDeactivationEffect();
                    
                    // Play deactivation sound
                    SoundEngine.PlaySound(SoundID.Item25.WithVolumeScale(0.6f), Player.Center);
                }
            }
            else if (bulletTimeCooldown > 0)
            {
                bulletTimeCooldown--;
            }
        }

        private void UpdateRippleEffects()
        {
            // Update ripples
            for (int i = activeRipples.Count - 1; i >= 0; i--)
            {
                activeRipples[i].Update();
                if (activeRipples[i].IsExpired())
                {
                    activeRipples.RemoveAt(i);
                }
            }
        }

        private void CreateActivationRipple()
        {
            activeRipples.Add(new BulletTimeRipple(Player.Center, ChronosWatch.RippleLifetime, true));
        }

        private void CreateDeactivationEffect()
        {
            // Create multiple smaller ripples
            for (int i = 0; i < 5; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(50f, 50f);
                activeRipples.Add(new BulletTimeRipple(Player.Center + offset, ChronosWatch.RippleLifetime / 2, false));
            }
        }

        // These methods are kept for backwards compatibility but now trigger global effects
        public void ReceiveBulletTimeActivation(Vector2 position)
        {
            activeRipples.Add(new BulletTimeRipple(position, ChronosWatch.RippleLifetime, true));
            SoundEngine.PlaySound(SoundID.Item29.WithVolumeScale(0.4f).WithPitchOffset(-0.3f), position);
        }

        public void ReceiveBulletTimeDeactivation(Vector2 position)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                activeRipples.Add(new BulletTimeRipple(position + offset, ChronosWatch.RippleLifetime / 3, false));
            }
        }

        // Save/Load for persistence
        public override void SaveData(TagCompound tag)
        {
            tag["bulletTimeCooldown"] = bulletTimeCooldown;
        }

        public override void LoadData(TagCompound tag)
        {
            bulletTimeCooldown = tag.GetInt("bulletTimeCooldown");
        }
    }
}