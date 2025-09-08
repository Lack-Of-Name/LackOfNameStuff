// EternalGem.cs - Post-DoG/Yharon/Exo final tier material
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LackOfNameStuff.Items.Materials
{
    public class EternalGem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 40;
            Item.scale = 0.16f;
            Item.maxStack = 999;
            Item.value = Item.buyPrice(platinum: 1);
            Item.rare = ItemRarityID.LightPurple;
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Eternal Gem");
            // Tooltip.SetDefault("The ultimate fusion of time and space\n'Radiates with the power of eternity itself'\nUsed to craft the most powerful temporal equipment");
        }

        public override void PostUpdate()
        {
            // Visual effects with rainbow/prismatic theme
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.RainbowMk2);
                dust.velocity = Vector2.Zero;
                dust.scale = 1.3f;
                dust.fadeIn = 1.6f;
                dust.noGravity = true;
                // Rainbow color cycling
                dust.color = Main.hslToRgb((Main.GlobalTimeWrappedHourly * 0.5f) % 1f, 1f, 0.7f);
            }
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            float customScale = 0.16f;
            Vector2 position = Item.position - Main.screenPosition + new Vector2(Item.width / 2, Item.height - texture.Height * customScale / 2f);

            // Calculate pulsing glow intensity
            float glowIntensity = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6) * 0.6f + 1.2f;
            
            // Draw rainbow glow effect first (behind the main sprite)
            Color glowColor = Main.hslToRgb((Main.GlobalTimeWrappedHourly * 0.7f) % 1f, 1f, 0.8f);
            glowColor *= glowIntensity * 1.2f;
            glowColor.A = 0; // Make it additive
            
            // Draw multiple glow layers for more intensity
            for (int i = 0; i < 6; i++)
            {
                float glowScale = customScale * (2f + i * 0.6f);
                float glowAlpha = (1f - i * 0.15f) * glowIntensity;
                
                // Cycle through rainbow colors for each layer
                Color layerColor = Main.hslToRgb(((Main.GlobalTimeWrappedHourly * 0.7f) + i * 0.15f) % 1f, 1f, 0.8f);
                layerColor *= glowAlpha;
                layerColor.A = 0;
                
                spriteBatch.Draw(
                    texture,
                    position,
                    null,
                    layerColor,
                    rotation + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f + i * 0.5f) * 0.3f, // Complex rotation pattern
                    texture.Size() * 0.5f,
                    glowScale,
                    SpriteEffects.None,
                    0f
                );
            }

            // Draw the main sprite
            spriteBatch.Draw(
                texture,
                position,
                null,
                lightColor,
                rotation,
                texture.Size() * 0.5f,
                customScale,
                SpriteEffects.None,
                0f
            );

            return false; // Stop Terraria from drawing the default big sprite
        }
    }
}