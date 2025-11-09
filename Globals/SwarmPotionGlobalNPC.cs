using System;
using Terraria;
using Terraria.ModLoader;
using LackOfNameStuff.Players;

namespace LackOfNameStuff.Globals
{
    public class SwarmPotionGlobalNPC : GlobalNPC
    {
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (player is null || !player.active)
            {
                return;
            }

            var swarmPlayer = player.GetModPlayer<SwarmPotionPlayer>();
            if (!swarmPlayer.SwarmPotionActive)
            {
                return;
            }

            spawnRate = Math.Max(1, spawnRate / SwarmPotionPlayer.SpawnRateMultiplier);
            maxSpawns = Math.Max(maxSpawns, SwarmPotionPlayer.MaxSpawnOverride);
        }
    }
}
