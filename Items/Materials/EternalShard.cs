// EternalShard.cs - Post-Providence material
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LackOfNameStuff.Items.Materials
{
    public class EternalShard : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 32;
            Item.scale = 0.12f;
            Item.maxStack = 999;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Eternal Shard");
            // Tooltip.SetDefault("A fragment of crystallized eternity\n'Resonates with unholy power'\nUsed to craft advanced temporal equipment");
        }

        public override void PostUpdate()
        {
            // Visual effects with purple/dark energy theme
            if (Main.rand.NextBool(6))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Shadowflame);
                dust.velocity = Vector2.Zero;
                dust.scale = 0.9f;
                dust.fadeIn = 1.2f;
                dust.noGravity = true;
                // Dark purple to violet color mix
                dust.color = Color.Lerp(Color.DarkViolet, Color.Purple, Main.rand.NextFloat());
            }
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            float customScale = 0.12f;
            Vector2 position = Item.position - Main.screenPosition + new Vector2(Item.width / 2, Item.height - texture.Height * customScale / 2f);

            // Calculate pulsing glow intensity
            float glowIntensity = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4) * 0.4f + 0.8f;
            
            // Draw glow effect first (behind the main sprite)
            Color glowColor = Color.Lerp(Color.DarkViolet, Color.Purple, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3) * 0.5f + 0.5f);
            glowColor *= glowIntensity * 0.9f;
            glowColor.A = 0; // Make it additive
            
            // Draw multiple glow layers for more intensity
            for (int i = 0; i < 4; i++)
            {
                float glowScale = customScale * (1.6f + i * 0.4f);
                float glowAlpha = (1f - i * 0.25f) * glowIntensity;
                
                spriteBatch.Draw(
                    texture,
                    position,
                    null,
                    glowColor * glowAlpha,
                    rotation + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 1.5f) * 0.15f, // Slight rotation wobble
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