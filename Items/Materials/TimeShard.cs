// TimeShard.cs - Updated with orange/yellow glow
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LackOfNameStuff.Items.Materials
{
    public class TimeShard : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.scale = 0.1f;
            Item.maxStack = 999;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Orange;
            Item.ammo = Item.type; // Makes it usable as ammo for the Temporal Launcher
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Time Shard");
            // Tooltip.SetDefault("A crystallized fragment of time itself\n'Pulses with temporal energy'\nCan be used as ammunition for temporal weapons");
        }

        public override void PostUpdate()
        {
            // Visual effects with orange/yellow theme
            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Electric);
                dust.velocity = Vector2.Zero;
                dust.scale = 0.8f;
                dust.fadeIn = 1f;
                dust.noGravity = true;
                // Orange to yellow color mix
                dust.color = Color.Lerp(Color.Orange, Color.Yellow, Main.rand.NextFloat());
            }
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            float customScale = 0.1f;
            Vector2 position = Item.position - Main.screenPosition + new Vector2(Item.width / 2, Item.height - texture.Height * customScale / 2f);

            // Calculate pulsing glow intensity
            float glowIntensity = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6) * 0.3f + 0.7f;
            
            // Draw glow effect first (behind the main sprite)
            Color glowColor = Color.Lerp(Color.Orange, Color.Yellow, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4) * 0.5f + 0.5f);
            glowColor *= glowIntensity * 0.8f;
            glowColor.A = 0; // Make it additive
            
            // Draw multiple glow layers for more intensity
            for (int i = 0; i < 3; i++)
            {
                float glowScale = customScale * (1.5f + i * 0.3f);
                float glowAlpha = (1f - i * 0.3f) * glowIntensity;
                
                spriteBatch.Draw(
                    texture,
                    position,
                    null,
                    glowColor * glowAlpha,
                    rotation + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2) * 0.1f, // Slight rotation wobble
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