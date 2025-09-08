// TimeGem.cs - Post-Polterghast material
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LackOfNameStuff.Items.Materials
{
    public class TimeGem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 36;
            Item.scale = 0.14f;
            Item.maxStack = 999;
            Item.value = Item.buyPrice(gold: 15);
            Item.rare = ItemRarityID.Purple;
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Time Gem");
            // Tooltip.SetDefault("A perfectly cut gemstone containing temporal essence\n'Shimmers with cosmic energy'\nUsed to craft cosmic-tier temporal equipment");
        }

        public override void PostUpdate()
        {
            // Visual effects with cosmic blue/cyan theme
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.IceTorch);
                dust.velocity = Vector2.Zero;
                dust.scale = 1.1f;
                dust.fadeIn = 1.4f;
                dust.noGravity = true;
                // Cyan to light blue color mix
                dust.color = Color.Lerp(Color.Cyan, Color.LightBlue, Main.rand.NextFloat());
            }
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            float customScale = 0.14f;
            Vector2 position = Item.position - Main.screenPosition + new Vector2(Item.width / 2, Item.height - texture.Height * customScale / 2f);

            // Calculate pulsing glow intensity
            float glowIntensity = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5) * 0.5f + 1f;
            
            // Draw glow effect first (behind the main sprite)
            Color glowColor = Color.Lerp(Color.Cyan, Color.LightBlue, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3.5f) * 0.5f + 0.5f);
            glowColor *= glowIntensity * 1f;
            glowColor.A = 0; // Make it additive
            
            // Draw multiple glow layers for more intensity
            for (int i = 0; i < 5; i++)
            {
                float glowScale = customScale * (1.8f + i * 0.5f);
                float glowAlpha = (1f - i * 0.2f) * glowIntensity;
                
                spriteBatch.Draw(
                    texture,
                    position,
                    null,
                    glowColor * glowAlpha,
                    rotation + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2.5f) * 0.2f, // More pronounced rotation wobble
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