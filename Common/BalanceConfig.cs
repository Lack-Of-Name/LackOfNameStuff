using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace LackOfNameStuff.Common
{
    // Centralized balance knobs for the mod.
    public class BalanceConfig : ModConfig
    {
        // This is a server-side config so the host controls balance in MP.
        public override ConfigScope Mode => ConfigScope.ServerSide;

        // How effective defense is while an NPC has Temporal Shock.
        // 1.0 = normal defense, 0.65 = 65% as effective (i.e., 35% reduced impact).
        [LabelKey("Mods.LackOfNameStuff.Config.TemporalShockDefenseEffectiveness.Label")]
        [TooltipKey("Mods.LackOfNameStuff.Config.TemporalShockDefenseEffectiveness.Tooltip")]
        [Range(0f, 1f)]
        public float TemporalShockDefenseEffectivenessMultiplier { get; set; } = Buffs.TemporalShock.DefaultDefenseEffectivenessMultiplier;
    }
}
