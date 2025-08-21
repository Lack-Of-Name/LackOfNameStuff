// TimeShard.cs
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace LackOfNameStuff.Items.Materials
{
    public class TimeShard : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
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
    }
}