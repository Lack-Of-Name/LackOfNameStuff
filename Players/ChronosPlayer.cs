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
using LackOfNameStuff.Worlds;

namespace LackOfNameStuff.Players
{
    public class ChronosPlayer : ModPlayer
    {
        public bool hasChronosWatch = false;
        public int bulletTimeCooldown = 0;
        
        // Track global bullet time state for visual effects
        public bool wasGlobalBulletTimeActive = false;
        
        // Visual effects - only ripples are per-player now
        public List<BulletTimeRipple> activeRipples = new List<BulletTimeRipple>();

        // Convenience methods to check bullet time state
        public bool IsInBulletTime() => ChronosWorld.GlobalBulletTimeActive;
        public bool IsOwnBulletTime() => ChronosWorld.GlobalBulletTimeActive && ChronosWorld.GlobalBulletTimeOwner == Player;
        public int GetBulletTimeRemaining() => ChronosWorld.GlobalBulletTimeActive ? ChronosWorld.GlobalBulletTimeRemaining : 0;

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

            // Update cooldown
            if (bulletTimeCooldown > 0)
            {
                bulletTimeCooldown--;
            }
            
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
            bool isGlobalBulletTimeActive = ChronosWorld.GlobalBulletTimeActive;
            
            if (isGlobalBulletTimeActive && !wasGlobalBulletTimeActive)
            {
                // Global bullet time just activated
                // Only create effects if this player DIDN'T activate it themselves
                Player activatingPlayer = ChronosWorld.GlobalBulletTimeOwner;
                if (activatingPlayer != null && activatingPlayer != Player)
                {
                    // Create visual effects at the activating player's position
                    activeRipples.Add(new BulletTimeRipple(activatingPlayer.Center, ChronosWatch.RippleLifetime, true));
                    SoundEngine.PlaySound(SoundID.Item29.WithVolumeScale(0.4f).WithPitchOffset(-0.3f), activatingPlayer.Center);
                }
            }
            else if (!isGlobalBulletTimeActive && wasGlobalBulletTimeActive)
            {
                // Global bullet time just deactivated
                // Only create effects if this player DIDN'T deactivate it themselves
                bool thisPlayerJustDeactivated = (bulletTimeCooldown == ChronosWatch.CooldownDuration);
                
                if (!thisPlayerJustDeactivated)
                {
                    // Create deactivation effects at the origin point
                    Vector2 origin = ChronosWorld.GlobalBulletTimeOrigin;
                    if (origin != Vector2.Zero) // Make sure we have a valid origin
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                            activeRipples.Add(new BulletTimeRipple(origin + offset, ChronosWatch.RippleLifetime / 3, false));
                        }
                        SoundEngine.PlaySound(SoundID.Item25.WithVolumeScale(0.3f), origin);
                    }
                }
            }
            
            wasGlobalBulletTimeActive = isGlobalBulletTimeActive;
        }

        private void TryActivateBulletTime()
        {
            // Check if we can activate (check global state and local cooldown)
            if (ChronosWorld.GlobalBulletTimeActive || bulletTimeCooldown > 0)
                return;

            // Activate global bullet time through the world system
            ChronosWorld.ActivateBulletTime(Player);
            
            // Set our cooldown immediately so we can't reactivate
            bulletTimeCooldown = ChronosWatch.CooldownDuration;

            // Create initial ripple effect - this player creates their own effects
            CreateActivationRipple();

            // Play sound effect
            SoundEngine.PlaySound(SoundID.Item29.WithVolumeScale(0.8f).WithPitchOffset(-0.3f), Player.Center);
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

        // These methods are kept for backwards compatibility but are mostly unused now
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

        // Backwards compatibility properties for ChronosWatch tooltip
        public bool bulletTimeActive => IsOwnBulletTime();
        public int bulletTimeRemaining => GetBulletTimeRemaining();
    }
}