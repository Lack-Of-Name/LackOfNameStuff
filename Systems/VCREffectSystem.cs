using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using LackOfNameStuff.Players;

namespace LackOfNameStuff.Systems
{
    public class VCREffectSystem : ModSystem
    {
        private static Texture2D _pixelTexture;
        
        // Variables for unstable effects
        private static int _frameCounter = 0;
        private static float _glitchTimer = 0f;
        private static int[] _scanlineJitterOffsets = new int[100]; // Store jitter offsets for scanlines
        private static float _edgeCorruptionSeed = 0f;

        private static Texture2D GetPixelTexture()
        {
            if (_pixelTexture == null || _pixelTexture.IsDisposed)
            {
                _pixelTexture = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });
            }
            return _pixelTexture;
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            var chronosPlayer = Main.LocalPlayer.GetModPlayer<ChronosPlayer>();
            
            if (chronosPlayer.screenEffectIntensity > 0f)
            {
                UpdateEffectTimers();
                DrawVCREffect(spriteBatch, chronosPlayer.screenEffectIntensity);
            }
        }

        private void UpdateEffectTimers()
        {
            _frameCounter++;
            _glitchTimer += 0.1f;
            _edgeCorruptionSeed += 0.05f;
            
            // Update scanline jitter offsets occasionally
            if (_frameCounter % 3 == 0)
            {
                for (int i = 0; i < _scanlineJitterOffsets.Length; i++)
                {
                    if (Main.rand.NextFloat() < 0.1f) // 10% chance to jitter each scanline
                    {
                        _scanlineJitterOffsets[i] = Main.rand.Next(-3, 4);
                    }
                    else if (Main.rand.NextFloat() < 0.05f) // 5% chance to reset
                    {
                        _scanlineJitterOffsets[i] = 0;
                    }
                }
            }
        }

        private void DrawVCREffect(SpriteBatch spriteBatch, float intensity)
        {
            // Get screen dimensions
            int screenWidth = Main.screenWidth;
            int screenHeight = Main.screenHeight;
            
            // Get our pixel texture
            Texture2D pixelTexture = GetPixelTexture();
            
            // Chromatic aberration (drawn first, behind other effects)
            DrawChromaticAberration(spriteBatch, pixelTexture, screenWidth, screenHeight, intensity);
            
            // VCR Scanlines Effect (with jitter)
            DrawJitteredScanlines(spriteBatch, pixelTexture, screenWidth, screenHeight, intensity);
            
            // Glitch bars
            DrawGlitchBars(spriteBatch, pixelTexture, screenWidth, screenHeight, intensity);
            
            // Corrupted screen border (unstable vignette effect)
            DrawCorruptedVignette(spriteBatch, pixelTexture, screenWidth, screenHeight, intensity);
            
            // Static noise overlay
            DrawStaticNoise(spriteBatch, pixelTexture, screenWidth, screenHeight, intensity);
            
            // Dynamic color tint overlay
            DrawDynamicTintOverlay(spriteBatch, pixelTexture, screenWidth, screenHeight, intensity);
        }

        private void DrawJitteredScanlines(SpriteBatch spriteBatch, Texture2D pixelTexture, int screenWidth, int screenHeight, float intensity)
        {
            Color scanlineColor = Color.Black * (intensity * 0.3f);
            int scanlineSpacing = 4;
            int jitterIndex = 0;
            
            for (int y = 0; y < screenHeight; y += scanlineSpacing)
            {
                int xOffset = 0;
                if (jitterIndex < _scanlineJitterOffsets.Length)
                {
                    xOffset = (int)(_scanlineJitterOffsets[jitterIndex] * intensity);
                    jitterIndex++;
                }
                
                Rectangle scanlineRect = new Rectangle(xOffset, y, screenWidth - Math.Abs(xOffset), 2);
                if (scanlineRect.Width > 0)
                {
                    spriteBatch.Draw(pixelTexture, scanlineRect, scanlineColor);
                }
            }
        }

        private void DrawGlitchBars(SpriteBatch spriteBatch, Texture2D pixelTexture, int screenWidth, int screenHeight, float intensity)
        {
            if (intensity < 0.3f) return;
            
            int numGlitchBars = (int)(5 * intensity);
            
            for (int i = 0; i < numGlitchBars; i++)
            {
                if (Main.rand.NextFloat() < 0.3f) // 30% chance per bar per frame
                {
                    int y = Main.rand.Next(screenHeight - 20);
                    int height = Main.rand.Next(2, 8);
                    int xOffset = Main.rand.Next(-10, 11);
                    
                    // Create glitch colors (corrupted looking)
                    Color glitchColor = Main.rand.Next(4) switch
                    {
                        0 => Color.Red * (intensity * 0.4f),
                        1 => Color.Green * (intensity * 0.3f),
                        2 => Color.Blue * (intensity * 0.3f),
                        _ => Color.White * (intensity * 0.2f)
                    };
                    
                    Rectangle glitchRect = new Rectangle(Math.Max(0, xOffset), y, 
                                                       Math.Min(screenWidth, screenWidth - Math.Max(0, -xOffset)), height);
                    spriteBatch.Draw(pixelTexture, glitchRect, glitchColor);
                }
            }
        }

        private void DrawCorruptedVignette(SpriteBatch spriteBatch, Texture2D pixelTexture, int screenWidth, int screenHeight, float intensity)
        {
            int vignetteSize = (int)(200 * intensity);
            
            // Top vignette with corruption
            for (int i = 0; i < vignetteSize; i++)
            {
                float alpha = (float)(vignetteSize - i) / vignetteSize * intensity * 0.4f;
                
                // Add corruption to edge
                float corruptionNoise = (float)(Math.Sin(_edgeCorruptionSeed + i * 0.1f) * 3 * intensity);
                
                Color fadeColor = Color.Black * alpha;
                Rectangle line = new Rectangle(0, i, screenWidth, 1);
                spriteBatch.Draw(pixelTexture, line, fadeColor);
            }
            
            // Bottom vignette with corruption
            for (int i = 0; i < vignetteSize; i++)
            {
                float alpha = (float)i / vignetteSize * intensity * 0.4f;
                
                float corruptionNoise = (float)(Math.Sin(_edgeCorruptionSeed * 1.3f + i * 0.1f) * 3 * intensity);
                
                Color fadeColor = Color.Black * alpha;
                Rectangle line = new Rectangle(0, screenHeight - vignetteSize + i, screenWidth, 1);
                spriteBatch.Draw(pixelTexture, line, fadeColor);
            }
            
            // Left vignette with corruption
            for (int i = 0; i < vignetteSize; i++)
            {
                float alpha = (float)(vignetteSize - i) / vignetteSize * intensity * 0.2f;
                
                // Add subtle horizontal corruption
                float corruptionOffset = (float)(Math.Sin(_edgeCorruptionSeed * 0.7f + i * 0.05f) * 2 * intensity);
                
                Color fadeColor = Color.Black * alpha;
                Rectangle line = new Rectangle(i + (int)corruptionOffset, 0, 1, screenHeight);
                if (line.X >= 0 && line.X < screenWidth)
                {
                    spriteBatch.Draw(pixelTexture, line, fadeColor);
                }
            }
            
            // Right vignette with corruption
            for (int i = 0; i < vignetteSize; i++)
            {
                float alpha = (float)i / vignetteSize * intensity * 0.2f;
                
                // Add subtle horizontal corruption
                float corruptionOffset = (float)(Math.Sin(_edgeCorruptionSeed * 0.9f + i * 0.05f) * 2 * intensity);
                
                Color fadeColor = Color.Black * alpha;
                Rectangle line = new Rectangle(screenWidth - vignetteSize + i + (int)corruptionOffset, 0, 1, screenHeight);
                if (line.X >= 0 && line.X < screenWidth)
                {
                    spriteBatch.Draw(pixelTexture, line, fadeColor);
                }
            }
        }

        private void DrawStaticNoise(SpriteBatch spriteBatch, Texture2D pixelTexture, int screenWidth, int screenHeight, float intensity)
        {
            if (intensity < 0.5f) return;
            
            int noisePixels = (int)(800 * intensity); // Increased for more chaos
            
            for (int i = 0; i < noisePixels; i++)
            {
                int x = Main.rand.Next(screenWidth);
                int y = Main.rand.Next(screenHeight);
                float brightness = Main.rand.NextFloat(0.1f, 0.4f);
                
                // Occasionally make noise colored instead of white
                Color noiseColor = Main.rand.NextFloat() < 0.1f ? 
                    new Color(Main.rand.NextFloat(), Main.rand.NextFloat(), Main.rand.NextFloat()) * brightness :
                    Color.White * brightness;
                    
                Rectangle noisePixel = new Rectangle(x, y, Main.rand.Next(1, 3), Main.rand.Next(1, 3));
                spriteBatch.Draw(pixelTexture, noisePixel, noiseColor);
            }
        }

        private void DrawDynamicTintOverlay(SpriteBatch spriteBatch, Texture2D pixelTexture, int screenWidth, int screenHeight, float intensity)
        {
            // Dynamic color tint - shifts from cyan to red/orange based on intensity and time
            Color baseColor;
            
            if (intensity < 0.4f)
            {
                // Low intensity: cyan tint
                baseColor = Color.Cyan;
            }
            else if (intensity < 0.7f)
            {
                // Medium intensity: mix cyan and orange
                float mixFactor = (intensity - 0.4f) / 0.3f;
                baseColor = Color.Lerp(Color.Cyan, Color.Orange, mixFactor);
            }
            else
            {
                // High intensity: orange to red, with pulsing
                float pulseIntensity = (float)(Math.Sin(_glitchTimer * 8) * 0.3f + 0.7f);
                Color dangerColor = Color.Lerp(Color.Orange, Color.Red, (intensity - 0.7f) / 0.3f);
                baseColor = dangerColor * pulseIntensity;
            }
            
            Color tintColor = baseColor * (intensity * 0.08f);
            Rectangle fullScreen = new Rectangle(0, 0, screenWidth, screenHeight);
            spriteBatch.Draw(pixelTexture, fullScreen, tintColor);
            
            // Add more aggressive horizontal distortion lines at high intensity
            if (intensity > 0.7f)
            {
                for (int i = 0; i < 5; i++)
                {
                    int y = Main.rand.Next(screenHeight);
                    Color distortColor = Color.Red * (intensity * 0.15f);
                    Rectangle distortLine = new Rectangle(0, y, screenWidth, Main.rand.Next(1, 3));
                    spriteBatch.Draw(pixelTexture, distortLine, distortColor);
                }
            }
        }

        private void DrawChromaticAberration(SpriteBatch spriteBatch, Texture2D pixelTexture, int screenWidth, int screenHeight, float intensity)
        {
            if (intensity < 0.5f) return; // Only at higher intensities
            
            // Simple chromatic aberration using colored overlays with offsets
            float aberrationStrength = intensity * 3f;
            
            // Red channel offset (slightly left and up)
            Color redTint = Color.Red * (intensity * 0.03f);
            Rectangle redOffset = new Rectangle((int)(-aberrationStrength), (int)(-aberrationStrength * 0.5f), 
                                              screenWidth, screenHeight);
            spriteBatch.Draw(pixelTexture, redOffset, redTint);
            
            // Green channel (no offset - this is our "base")
            // We don't draw green separately as it would be too much
            
            // Blue channel offset (slightly right and down)  
            Color blueTint = Color.Blue * (intensity * 0.03f);
            Rectangle blueOffset = new Rectangle((int)(aberrationStrength), (int)(aberrationStrength * 0.5f), 
                                               screenWidth, screenHeight);
            spriteBatch.Draw(pixelTexture, blueOffset, blueTint);
        }
    }
}