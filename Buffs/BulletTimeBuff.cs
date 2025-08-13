using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using LackOfNameStuff.Worlds;

namespace LackOfNameStuff.Buffs
{
    public class BulletTimeBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false; // This is not a debuff
            Main.pvpBuff[Type] = false; // Not a PvP buff
            Main.buffNoSave[Type] = true; // Don't save this buff (it shouldn't persist)
            BuffID.Sets.LongerExpertDebuff[Type] = false; // Not affected by expert mode
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // This buff serves as both a visual indicator and a fallback sync mechanism
            // The actual effects are handled by the global systems, but we can add
            // per-player effects here if needed

            // Optional: Add some visual dust or effects to show the player is in bullet time
            if (Main.rand.NextBool(30)) // Every ~0.5 seconds
            {
                Dust dust = Dust.NewDustDirect(
                    player.position, 
                    player.width, 
                    player.height,
                    DustID.Electric,
                    0f, 0f, 100, 
                    Color.Cyan, 
                    0.8f
                );
                dust.velocity *= 0.3f;
                dust.noGravity = true;
            }

            // Fallback: If world state somehow gets out of sync, this buff acts as the authority
            // If the buff is active but world state says bullet time is inactive, reactivate it
            if (!ChronosWorld.GlobalBulletTimeActive && player.buffTime[buffIndex] > 0)
            {
                // Buff is active but world state is not - sync the world state to match the buff
                // This should rarely happen, but it's a good fallback
                ChronosWorld.ActivateBulletTime(player);
            }
        }

        public override bool RightClick(int buffIndex)
        {
            // Allow players to right-click to remove the buff early (if they want to cancel bullet time)
            // Only allow the person who activated it to cancel it
            Player player = Main.LocalPlayer;
            if (ChronosWorld.GlobalBulletTimeOwner == player)
            {
                ChronosWorld.DeactivateBulletTime();
                return true; // Remove the buff
            }
            return false; // Don't remove the buff
        }
    }
}