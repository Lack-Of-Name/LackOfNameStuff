using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

using Terraria.ModLoader;
using LackOfNameStuff.Players;

namespace LackOfNameStuff.Systems
{
    public class VCREffectSystem : ModSystem
    {
        private static Texture2D _pixelTexture;

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
                DrawVCREffect(spriteBatch, chronosPlayer.screenEffectIntensity);
            }
        }

        private void DrawVCREffect(SpriteBatch spriteBatch, float intensity)
        {
            // Get screen dimensions
            int screenWidth = Main.screenWidth;
            int screenHeight = Main.screenHeight;
            
            // Get our pixel texture
            Texture2D pixelTexture = GetPixelTexture();
            
            // VCR Scanlines Effect
            DrawScanlines(spriteBatch, pixelTexture, screenWidth, screenHeight, intensity);
            
            // Screen border darkening (vignette effect)
            DrawVignette(spriteBatch, pixelTexture, screenWidth, screenHeight, intensity);
            
            // Static noise overlay
            DrawStaticNoise(spriteBatch, pixelTexture, screenWidth, screenHeight, intensity);
            
            // Color desaturation overlay
            DrawDesaturationOverlay(spriteBatch, pixelTexture, screenWidth, screenHeight, intensity);
        }

        private void DrawScanlines(SpriteBatch spriteBatch, Texture2D pixelTexture, int screenWidth, int screenHeight, float intensity)
        {
            Color scanlineColor = Color.Black * (intensity * 0.3f);
            int scanlineSpacing = 4;
            
            for (int y = 0; y < screenHeight; y += scanlineSpacing)
            {
                Rectangle scanlineRect = new Rectangle(0, y, screenWidth, 2);
                spriteBatch.Draw(pixelTexture, scanlineRect, scanlineColor);
            }
        }

        private void DrawVignette(SpriteBatch spriteBatch, Texture2D pixelTexture, int screenWidth, int screenHeight, float intensity)
        {
            int vignetteSize = (int)(200 * intensity);
            
            // Top vignette
            for (int i = 0; i < vignetteSize; i++)
            {
                float alpha = (float)(vignetteSize - i) / vignetteSize * intensity * 0.4f;
                Color fadeColor = Color.Black * alpha;
                Rectangle line = new Rectangle(0, i, screenWidth, 1);
                spriteBatch.Draw(pixelTexture, line, fadeColor);
            }
            
            // Bottom vignette
            for (int i = 0; i < vignetteSize; i++)
            {
                float alpha = (float)i / vignetteSize * intensity * 0.4f;
                Color fadeColor = Color.Black * alpha;
                Rectangle line = new Rectangle(0, screenHeight - vignetteSize + i, screenWidth, 1);
                spriteBatch.Draw(pixelTexture, line, fadeColor);
            }
            
            // Left vignette
            for (int i = 0; i < vignetteSize; i++)
            {
                float alpha = (float)(vignetteSize - i) / vignetteSize * intensity * 0.2f;
                Color fadeColor = Color.Black * alpha;
                Rectangle line = new Rectangle(i, 0, 1, screenHeight);
                spriteBatch.Draw(pixelTexture, line, fadeColor);
            }
            
            // Right vignette
            for (int i = 0; i < vignetteSize; i++)
            {
                float alpha = (float)i / vignetteSize * intensity * 0.2f;
                Color fadeColor = Color.Black * alpha;
                Rectangle line = new Rectangle(screenWidth - vignetteSize + i, 0, 1, screenHeight);
                spriteBatch.Draw(pixelTexture, line, fadeColor);
            }
        }

        private void DrawStaticNoise(SpriteBatch spriteBatch, Texture2D pixelTexture, int screenWidth, int screenHeight, float intensity)
        {
            if (intensity < 0.5f) return; // Only show noise at higher intensities
            
            int noisePixels = (int)(500 * intensity);
            
            for (int i = 0; i < noisePixels; i++)
            {
                int x = Main.rand.Next(screenWidth);
                int y = Main.rand.Next(screenHeight);
                float brightness = Main.rand.NextFloat(0.1f, 0.3f);
                Color noiseColor = Color.White * brightness;
                Rectangle noisePixel = new Rectangle(x, y, 2, 2);
                spriteBatch.Draw(pixelTexture, noisePixel, noiseColor);
            }
        }

        private void DrawDesaturationOverlay(SpriteBatch spriteBatch, Texture2D pixelTexture, int screenWidth, int screenHeight, float intensity)
        {
            // Add a slight blue/cyan tint to simulate old monitor look
            Color tintColor = Color.Cyan * (intensity * 0.08f);
            Rectangle fullScreen = new Rectangle(0, 0, screenWidth, screenHeight);
            spriteBatch.Draw(pixelTexture, fullScreen, tintColor);
            
            // Add slight horizontal distortion lines
            if (intensity > 0.7f)
            {
                for (int i = 0; i < 3; i++)
                {
                    int y = Main.rand.Next(screenHeight);
                    Color distortColor = Color.White * (intensity * 0.1f);
                    Rectangle distortLine = new Rectangle(0, y, screenWidth, 1);
                    spriteBatch.Draw(pixelTexture, distortLine, distortColor);
                }
            }
        }
    }
}