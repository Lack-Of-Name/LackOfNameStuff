using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using System;
using System.Linq;
using Terraria.GameContent;
using Terraria.ModLoader;
using LackOfNameStuff.Players;
using LackOfNameStuff.Effects;
using LackOfNameStuff.Worlds;

namespace LackOfNameStuff.Systems
{
    public class ChronosSystem : ModSystem
    {
        public ModKeybind BulletTimeKey { get; private set; }
        
        // Use world-based global state instead of local state
        public static bool GlobalBulletTimeActive => ChronosWorld.GlobalBulletTimeActive;
        public static int GlobalBulletTimeRemaining => ChronosWorld.GlobalBulletTimeRemaining;
        public static Player GlobalBulletTimeOwner => ChronosWorld.GlobalBulletTimeOwner;
        public static Vector2 GlobalBulletTimeOrigin => ChronosWorld.GlobalBulletTimeOrigin;
        
        // Global screen effect intensity for ALL players
        public static float GlobalScreenEffectIntensity { get; private set; } = 0f;

        public override void PostSetupContent()
        {
            BulletTimeKey = KeybindLoader.RegisterKeybind(Mod, "Bullet Time", Keys.V);
        }

        // Update global bullet time state every frame
        public override void PostUpdateEverything()
        {
            // World handles bullet time state updates now
            UpdateGlobalVisualEffects();
        }

        private void UpdateGlobalVisualEffects()
        {
            // Update GLOBAL screen effect intensity for ALL players
            if (GlobalBulletTimeActive)
            {
                GlobalScreenEffectIntensity = Math.Min(GlobalScreenEffectIntensity + 0.05f, 1f);
            }
            else
            {
                GlobalScreenEffectIntensity = Math.Max(GlobalScreenEffectIntensity - 0.03f, 0f);
            }
        }

        // Handle drawing effects for ALL players during global bullet time
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            // Draw effects for ALL players when global bullet time is active or effects are fading out
            var localChronosPlayer = Main.LocalPlayer.GetModPlayer<ChronosPlayer>();
            
            // Always draw effects if global bullet time is active or effects are fading out
            if (GlobalBulletTimeActive || GlobalScreenEffectIntensity > 0)
            {
                DrawPlayerEffects(spriteBatch, localChronosPlayer);
            }
            
            // Also draw ripple effects from other players
            DrawAllPlayerRipples(spriteBatch);
        }

        private void DrawAllPlayerRipples(SpriteBatch spriteBatch)
        {
            // Draw ripple effects from all players, not just the local player
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active && i != Main.myPlayer)
                {
                    var otherChronosPlayer = Main.player[i].GetModPlayer<ChronosPlayer>();
                    DrawRippleEffects(spriteBatch, otherChronosPlayer);
                }
            }
        }

        private void DrawPlayerEffects(SpriteBatch spriteBatch, ChronosPlayer chronosPlayer)
        {
            // Draw ripple effects (still use local player's ripples)
            DrawRippleEffects(spriteBatch, chronosPlayer);
            
            // Draw screen effects using GLOBAL intensity for consistent effects across all players
            DrawScreenEffects(spriteBatch, GlobalScreenEffectIntensity);
        }

        private void DrawRippleEffects(SpriteBatch spriteBatch, ChronosPlayer chronosPlayer)
        {
            foreach (var ripple in chronosPlayer.activeRipples)
            {
                DrawSingleRipple(spriteBatch, ripple);
            }
        }

        private void DrawSingleRipple(SpriteBatch spriteBatch, BulletTimeRipple ripple)
        {
            // Use a simple circle approximation with existing textures
            Texture2D rippleTexture = TextureAssets.MagicPixel.Value;
            
            Vector2 drawPosition = ripple.Position - Main.screenPosition;
            float radius = ripple.GetRadius();
            float opacity = ripple.GetOpacity();
            
            Color rippleColor = ripple.IsActivation ? 
                Color.Cyan * opacity : 
                Color.Gold * opacity;
            
            // Draw a series of circles to approximate a ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                
                spriteBatch.Draw(
                    rippleTexture,
                    drawPosition + offset,
                    new Rectangle(0, 0, 1, 1),
                    rippleColor,
                    0f,
                    Vector2.Zero,
                    new Vector2(8f, 8f),
                    SpriteEffects.None,
                    0f
                );
            }
        }

        private void DrawScreenEffects(SpriteBatch spriteBatch, float intensity)
        {
            if (intensity > 0)
            {
                // Draw screen tint/overlay
                Rectangle screenRect = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
                Color screenTint = Color.Blue * (intensity * 0.1f);
                
                spriteBatch.Draw(
                    TextureAssets.MagicPixel.Value,
                    screenRect,
                    new Rectangle(0, 0, 1, 1),
                    screenTint
                );
                
                // Add scan lines
                DrawScanLines(spriteBatch, intensity);
            }
        }

        private void DrawScanLines(SpriteBatch spriteBatch, float intensity)
        {
            // Create subtle scan line effect
            for (int y = 0; y < Main.screenHeight; y += 4)
            {
                Rectangle lineRect = new Rectangle(0, y, Main.screenWidth, 1);
                Color lineColor = Color.Cyan * (intensity * 0.05f);
                
                spriteBatch.Draw(
                    TextureAssets.MagicPixel.Value,
                    lineRect,
                    new Rectangle(0, 0, 1, 1),
                    lineColor
                );
            }
        }
    }
}