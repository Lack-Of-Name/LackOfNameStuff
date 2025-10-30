using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Common;

namespace LackOfNameStuff.Players
{
    public class GersonPlayer : ModPlayer
    {
        public bool HasGersonName { get; private set; }

        public bool HasSupremeGersonBlessing => HasGersonName && CalamityIntegration.DownedSCal;

        public override void Initialize()
        {
            EvaluateName();
        }

        public override void OnEnterWorld()
        {
            EvaluateName();
        }

        public override void PreUpdate()
        {
            EvaluateName();
        }

        public override void PostUpdate()
        {
            if (!HasGersonName || Main.netMode == NetmodeID.Server)
            {
                return;
            }

            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.GreenFairy);
                dust.velocity = new Vector2(Main.rand.NextFloat(-1.4f, 1.4f), Main.rand.NextFloat(-2.2f, -0.3f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.05f, 1.45f);
                dust.fadeIn = 1.1f;
            }
        }

        private void EvaluateName()
        {
            string name = Player.name?.Trim() ?? string.Empty;
            HasGersonName = name.Equals("Gerson", StringComparison.OrdinalIgnoreCase) ||
                            name.Equals("Gerson Boom", StringComparison.OrdinalIgnoreCase);
        }
    }
}
