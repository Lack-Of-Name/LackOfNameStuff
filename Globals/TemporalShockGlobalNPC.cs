using LackOfNameStuff.Buffs;
using LackOfNameStuff.Common;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace LackOfNameStuff.Globals
{
    // Applies Temporal Shock's defense reduction effect when the target has the debuff
    public class TemporalShockGlobalNPC : GlobalNPC
    {
        // We don't need per-instance fields, keep this lightweight
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            // If the NPC has Temporal Shock, scale its effective defense to 65%
            if (npc.HasBuff(ModContent.BuffType<TemporalShock>()))
            {
                // DefenseEffectiveness scales how much defense reduces damage.
                // Pull from config and fall back to the buff default.
                float multiplier = ModContent.GetInstance<BalanceConfig>()?.TemporalShockDefenseEffectivenessMultiplier
                    ?? TemporalShock.DefaultDefenseEffectivenessMultiplier;
                modifiers.DefenseEffectiveness *= multiplier;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (npc.HasBuff(ModContent.BuffType<TemporalShock>()))
            {
                // Add a cyan tint and subtle pulsing to make it obvious.
                drawColor = Color.Lerp(drawColor, new Color(80, 180, 255), 0.35f);

                // Add a faint electric light so it pops in dark areas.
                Lighting.AddLight(npc.Center, 0.1f, 0.25f, 0.35f);

                // Occasional spark arcs around the NPC.
                if (Main.rand.NextBool(6))
                {
                    var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, Terraria.ID.DustID.Electric,
                        Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f), 120, new Color(120, 220, 255), 1f);
                    dust.noGravity = true;
                    dust.fadeIn = 1.2f;
                }
            }
        }
    }
}
