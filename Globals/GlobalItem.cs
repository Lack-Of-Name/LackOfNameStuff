// GlobalItem to store and handle temporal weapon data
using Terraria;
using Terraria.ModLoader;
using LackOfNameStuff.Players;
using System.Drawing;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace LackOfNameStuff.Globals
{
    public class TemporalWeaponData : GlobalItem
    {
        // Temporal weapon properties
        public bool TemporalWeapon = false;
        public float TemporalBuffDamage = 1.6f;     // Multiplier (1.3f = 30% more damage)
        public float TemporalBuffSpeed = 3f;      // Multiplier (1.8f = 80% faster)
        public float TemporalBuffCrit = 15f;         // Additive (15f = +15% crit chance)
        public float TemporalBuffKnockback = 1.2f;  // Multiplier (1.2f = 20% more knockback)
        public int TemporalBuffMana = -10;            // Additive mana cost reduction (-10 = 10 less mana)

        public override bool InstancePerEntity => true;

        // Check if an item is a temporal weapon
        public static bool IsTemporalWeapon(Item item)
        {
            return item.GetGlobalItem<TemporalWeaponData>().TemporalWeapon;
        }

        // Get temporal weapon data
        public static TemporalWeaponData GetTemporalData(Item item)
        {
            return item.GetGlobalItem<TemporalWeaponData>();
        }

        // Apply damage multiplier
        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            if (TemporalWeapon)
            {
                var temporalPlayer = player.GetModPlayer<TemporalPlayer>();
                if (temporalPlayer.hasTemporalSet)
                {
                    damage *= TemporalBuffDamage;
                }
            }
        }

        // Apply crit chance bonus
        public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
        {
            if (TemporalWeapon)
            {
                var temporalPlayer = player.GetModPlayer<TemporalPlayer>();
                if (temporalPlayer.hasTemporalSet && TemporalBuffCrit != 0f)
                {
                    crit += TemporalBuffCrit;
                }
            }
        }

        // Apply use speed multiplier
        public override float UseSpeedMultiplier(Item item, Player player)
        {
            if (TemporalWeapon)
            {
                var temporalPlayer = player.GetModPlayer<TemporalPlayer>();
                if (temporalPlayer.hasTemporalSet && TemporalBuffSpeed != 1.0f)
                {
                    return TemporalBuffSpeed;
                }
            }
            return base.UseSpeedMultiplier(item, player);
        }

        // Apply knockback multiplier
        public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
        {
            if (TemporalWeapon)
            {
                var temporalPlayer = player.GetModPlayer<TemporalPlayer>();
                if (temporalPlayer.hasTemporalSet && TemporalBuffKnockback != 1.0f)
                {
                    knockback *= TemporalBuffKnockback;
                }
            }
        }

        // Apply mana cost reduction
        public override void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult)
        {
            if (TemporalWeapon)
            {
                var temporalPlayer = player.GetModPlayer<TemporalPlayer>();
                if (temporalPlayer.hasTemporalSet && TemporalBuffMana != 0)
                {
                    reduce += TemporalBuffMana;
                }
            }
        }

        // Optional: Visual effects for temporal weapons
        public override void HoldItem(Item item, Player player)
        {
            if (TemporalWeapon)
            {
                var temporalPlayer = player.GetModPlayer<TemporalPlayer>();
                if (temporalPlayer.hasTemporalSet && Main.rand.NextBool(30))
                {
                    CreateTemporalWeaponEffect(player, GetTemporalColor(temporalPlayer.helmetType));
                }
            }
        }

        private void CreateTemporalWeaponEffect(Player player, Color effectColor)
        {
            Dust dust = Dust.NewDustDirect(
                player.position + new Microsoft.Xna.Framework.Vector2(player.width * 0.5f, player.height * 0.3f),
                10, 10, Terraria.ID.DustID.Electric);
            dust.velocity = Main.rand.NextVector2Circular(1f, 1f);
            dust.scale = 0.8f;
            dust.noGravity = true;
            dust.color = effectColor;
            dust.alpha = 150;
        }

        private Color GetTemporalColor(string helmetType)
        {
            return helmetType switch
            {
                "Ranged" => Color.Cyan,
                "Melee" => Color.Orange,
                "Magic" => Color.Magenta,
                "Summoner" => Color.Purple,
                _ => Color.Yellow
            };
        }
    }
}