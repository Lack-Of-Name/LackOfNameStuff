using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.IO;
using System.Collections.Generic;
using LackOfNameStuff.Items.Accessories;
using LackOfNameStuff.Systems;
using LackOfNameStuff.Effects;
using LackOfNameStuff.Buffs;

namespace LackOfNameStuff.Players
{
    public class ChronosPlayer : ModPlayer
    {
        public bool hasChronosWatch = false;
        public int bulletTimeCooldown = 0;
        
        // Visual effects
        public List<BulletTimeRipple> activeRipples = new List<BulletTimeRipple>();
        
        // Screen effect intensity for VCR effect
        public float screenEffectIntensity = 0f;
        
        // VCR scanlines effect
        private float scanlineOffset = 0f;
        private float chromaticAberrationIntensity = 0f;
        private float noiseIntensity = 0f;

        // Convenience properties
        public bool IsInBulletTime => Player.HasBuff<BulletTimeBuff>();
        public int BulletTimeRemaining => Player.HasBuff<BulletTimeBuff>() ? Player.buffTime[Player.FindBuffIndex(ModContent.BuffType<BulletTimeBuff>())] : 0;

        public override void ResetEffects()
        {
            hasChronosWatch = false;
        }

        public override void PostUpdate()
        {
            // Handle bullet time activation (only for players with the item)
            if (hasChronosWatch && ModContent.GetInstance<ChronosKeybindSystem>().BulletTimeKey.JustPressed)
            {
                TryActivateBulletTime();
            }

            // Update cooldown
            if (bulletTimeCooldown > 0)
            {
                bulletTimeCooldown--;
            }
            
            // Update visual effects based on buff presence
            UpdateVisualEffects();
            
            // Update ripple effects
            UpdateRippleEffects();
        }

        private void UpdateVisualEffects()
        {
            // Update screen effect intensity based on buff
            if (IsInBulletTime)
            {
                screenEffectIntensity = System.Math.Min(screenEffectIntensity + 0.05f, 1f);
                chromaticAberrationIntensity = System.Math.Min(chromaticAberrationIntensity + 0.03f, 0.8f);
                noiseIntensity = System.Math.Min(noiseIntensity + 0.02f, 0.3f);
            }
            else
            {
                screenEffectIntensity = System.Math.Max(screenEffectIntensity - 0.03f, 0f);
                chromaticAberrationIntensity = System.Math.Max(chromaticAberrationIntensity - 0.02f, 0f);
                noiseIntensity = System.Math.Max(noiseIntensity - 0.01f, 0f);
            }
            
            // Update scanline animation
            scanlineOffset += 2f;
            if (scanlineOffset > Main.screenHeight)
                scanlineOffset = 0f;
        }

        private void TryActivateBulletTime()
        {
            // Add debugging
            Mod.Logger.Info($"Player {Player.whoAmI} trying to activate bullet time - InBulletTime: {IsInBulletTime}, Cooldown: {bulletTimeCooldown}");
            
            // Check if we can activate (not already in bullet time and not on personal cooldown)
            if (IsInBulletTime || bulletTimeCooldown > 0)
            {
                Mod.Logger.Info($"Player {Player.whoAmI} bullet time activation blocked - InBulletTime: {IsInBulletTime}, PersonalCooldown: {bulletTimeCooldown}");
                return;
            }

            Mod.Logger.Info($"Player {Player.whoAmI} attempting bullet time activation");

            // Create initial ripple effect (always do this for immediate feedback)
            CreateActivationRipple();

            // Play sound effect (always do this for immediate feedback)
            SoundEngine.PlaySound(SoundID.Item29.WithVolumeScale(0.8f).WithPitchOffset(-0.3f), Player.Center);

            // Send packet to apply bullet time to all players
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Client sends request to server - DON'T set cooldown yet, let server decide
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)0); // Message type: Request bullet time activation
                packet.Write(Player.whoAmI);
                packet.Send();
                Mod.Logger.Info($"Client {Player.whoAmI} sent bullet time request to server");
            }
            else
            {
                // Single player or server - check global state before applying
                bool anyPlayerInBulletTime = false;
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active && Main.player[i].HasBuff(ModContent.BuffType<BulletTimeBuff>()))
                    {
                        anyPlayerInBulletTime = true;
                        break;
                    }
                }
                
                if (!anyPlayerInBulletTime)
                {
                    // Set cooldown only if we're actually going to activate
                    bulletTimeCooldown = ChronosWatch.CooldownDuration;
                    ApplyBulletTimeToAllPlayers();
                    Mod.Logger.Info($"Server/SinglePlayer {Player.whoAmI} applied bullet time directly");
                }
                else
                {
                    Mod.Logger.Info($"Server/SinglePlayer {Player.whoAmI} - bullet time already active globally");
                }
            }
        }

        private void ApplyBulletTimeToAllPlayers()
        {
            // Apply buff to all active players
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active)
                {
                    Main.player[i].AddBuff(ModContent.BuffType<BulletTimeBuff>(), ChronosWatch.BulletTimeDuration);
                }
            }
            
            // If we're the server, sync this to all clients
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)1); // Message type: Sync bullet time activation
                packet.Write(Player.whoAmI);
                packet.Write(ChronosWatch.BulletTimeDuration);
                packet.Send();
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

        // VCR Screen Effect Rendering
        public override void ModifyScreenPosition()
        {
            if (screenEffectIntensity > 0f)
            {
                // Slight screen shake during bullet time
                float shakeIntensity = screenEffectIntensity * 0.5f;
                Main.screenPosition.X += Main.rand.NextFloat(-shakeIntensity, shakeIntensity);
                Main.screenPosition.Y += Main.rand.NextFloat(-shakeIntensity, shakeIntensity);
            }
        }

        // Method to be called by network handler for visual effects
        public void CreateNetworkRipple(Vector2 position, int activatingPlayer)
        {
            activeRipples.Add(new BulletTimeRipple(position, ChronosWatch.RippleLifetime, true));
            if (activatingPlayer != Player.whoAmI)
            {
                SoundEngine.PlaySound(SoundID.Item29.WithVolumeScale(0.4f).WithPitchOffset(-0.3f), position);
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

        // Properties for ChronosWatch tooltip compatibility
        public bool bulletTimeActive => IsInBulletTime;
        public int bulletTimeRemaining => BulletTimeRemaining;
    }
}