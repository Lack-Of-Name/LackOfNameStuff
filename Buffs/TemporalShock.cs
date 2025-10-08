using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LackOfNameStuff.Buffs
{
	// Temporal Shock: Generic temporal debuff applied to enemies. Reduces effective defense to 65%.
	public class TemporalShock : ModBuff
	{
		// Default fraction of normal defense effectiveness while shocked.
		public const float DefaultDefenseEffectivenessMultiplier = 0.65f;

		public override void SetStaticDefaults()
		{
			// Display and behavior flags
			Main.debuff[Type] = true;              // This is a debuff
			Main.pvpBuff[Type] = true;             // Allow affecting players in PvP if ever applied
			Main.buffNoSave[Type] = true;          // Do not save
			BuffID.Sets.LongerExpertDebuff[Type] = true; // Longer duration in Expert/Master
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = true; // Nurse can't remove
		}

		public override void Update(NPC npc, ref int buffIndex)
		{
			// Subtle visual cue while the debuff is active
			if (Main.rand.NextBool(5))
			{
				Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Electric,
					0f, 0f, 100, Color.Cyan, 1.1f);
				d.velocity = Main.rand.NextVector2Circular(1.6f, 1.6f);
				d.noGravity = true;
			}
		}
	}
}
