using Microsoft.Xna.Framework;
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
        
        // Screen effect intensity
        public float screenEffectIntensity = 0f;

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
            }
            else
            {
                screenEffectIntensity = System.Math.Max(screenEffectIntensity - 0.03f, 0f);
            }
        }

        private void TryActivateBulletTime()
        {
            // Check if we can activate (not already in bullet time and not on cooldown)
            if (IsInBulletTime || bulletTimeCooldown > 0)
                return;

            // Apply bullet time buff to ALL players
            ApplyBulletTimeToAllPlayers();
            
            // Set our cooldown immediately so we can't reactivate
            bulletTimeCooldown = ChronosWatch.CooldownDuration;

            // Create initial ripple effect
            CreateActivationRipple();

            // Play sound effect
            SoundEngine.PlaySound(SoundID.Item29.WithVolumeScale(0.8f).WithPitchOffset(-0.3f), Player.Center);
            
            // Send activation message to all players for visual effects
            SendBulletTimeActivationToOtherPlayers();
        }

        private void ApplyBulletTimeToAllPlayers()
        {
            // In multiplayer, we need to send this to all players
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Send packet to server to apply buff to all players
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)0); // Message type: Apply bullet time to all players
                packet.Write(Player.whoAmI);
                packet.Write(ChronosWatch.BulletTimeDuration);
                packet.Send();
            }
            else
            {
                // Single player or server - apply directly
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active)
                    {
                        Main.player[i].AddBuff(ModContent.BuffType<BulletTimeBuff>(), ChronosWatch.BulletTimeDuration);
                    }
                }
                
                // If we're the server, also send to clients
                if (Main.netMode == NetmodeID.Server)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)0); // Message type: Apply bullet time to all players
                    packet.Write(Player.whoAmI);
                    packet.Write(ChronosWatch.BulletTimeDuration);
                    packet.Send();
                }
            }
        }

        private void SendBulletTimeActivationToOtherPlayers()
        {
            // Send visual effect trigger to other players
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)1); // Message type: Visual effects for activation
                packet.Write(Player.whoAmI);
                packet.Write(Player.Center.X);
                packet.Write(Player.Center.Y);
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

        // Handle multiplayer packets
        public void HandlePacket(BinaryReader reader, int whoAmI)
        {
            byte messageType = reader.ReadByte();
            
            if (messageType == 0) // Apply bullet time to all players
            {
                int activatingPlayer = reader.ReadInt32();
                int duration = reader.ReadInt32();
                
                // Apply buff to all players
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active)
                    {
                        Main.player[i].AddBuff(ModContent.BuffType<BulletTimeBuff>(), duration);
                    }
                }
            }
            else if (messageType == 1) // Visual effects for activation
            {
                int activatingPlayer = reader.ReadInt32();
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                Vector2 position = new Vector2(x, y);
                
                // Only create effects if it wasn't this player who activated it
                if (activatingPlayer != Player.whoAmI)
                {
                    activeRipples.Add(new BulletTimeRipple(position, ChronosWatch.RippleLifetime, true));
                    SoundEngine.PlaySound(SoundID.Item29.WithVolumeScale(0.4f).WithPitchOffset(-0.3f), position);
                }
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