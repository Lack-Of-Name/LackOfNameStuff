// TimeShard.cs
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
            Item.rare = ItemRarityID.Cyan;

            // Add a subtle glow effect
            Item.glowMask = 0; // Set to 0 for no glow, or assign a valid glow mask ID if you have one
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Time Shard");
            // Tooltip.SetDefault("A crystallized fragment of time itself\n'Pulses with temporal energy'");
        }

        public override void PostUpdate()
        {
            // Add some visual effects
            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Electric);
                dust.velocity = Vector2.Zero;
                dust.scale = 0.8f;
                dust.fadeIn = 1f;
                dust.noGravity = true;
            }
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            float customScale = 0.1f;

            Vector2 position = Item.position - Main.screenPosition + new Vector2(Item.width / 2, Item.height - texture.Height * customScale / 2f);
            spriteBatch.Draw(
                texture,
                position,
                null,
                lightColor,
                rotation,
                texture.Size() * 0.5f, // center origin
                customScale,
                SpriteEffects.None,
                0f
            );

            return false; // <- stop Terraria from drawing the default big sprite
        }
    }
}