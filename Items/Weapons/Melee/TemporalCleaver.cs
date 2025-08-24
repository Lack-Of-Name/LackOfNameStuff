using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using LackOfNameStuff.Players;
using LackOfNameStuff.Globals;
using System;

namespace LackOfNameStuff.Items.Weapons.Melee
{
    public class TemporalCleaver : ModItem
    {
        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Item.damage = 75;
            Item.DamageType = DamageClass.Melee;
            Item.width = 52;
            Item.height = 52;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            var temporalData = Item.GetGlobalItem<TemporalWeaponData>();
            temporalData.TemporalWeapon = true;

            // Set the temporal buff values here
            temporalData.TemporalBuffDamage = 1.2f;
            temporalData.TemporalBuffSpeed = 2.3f;
            temporalData.TemporalBuffCrit = 15f;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            var chronosPlayer = player.GetModPlayer<ChronosPlayer>();
            var cleaverPlayer = player.GetModPlayer<TemporalCleaverPlayer>();
            
            // During bullet time, build up energy
            if (chronosPlayer.bulletTimeActive)
            {
                cleaverPlayer.storedEnergy++;
                
                // Visual feedback - more particles as energy builds
                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Electric);
                    dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
                    dust.scale = 0.8f + (cleaverPlayer.storedEnergy * 0.02f); // Grows with energy
                    dust.noGravity = true;
                    dust.color = Color.Lerp(Color.Orange, Color.White, cleaverPlayer.storedEnergy / 100f);
                }
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            var chronosPlayer = player.GetModPlayer<ChronosPlayer>();
            var cleaverPlayer = player.GetModPlayer<TemporalCleaverPlayer>();

            // Release stored energy when not in bullet time
            if (!chronosPlayer.bulletTimeActive && cleaverPlayer.storedEnergy > 0)
            {
                CreateEnergyShockwave(target.Center, cleaverPlayer.storedEnergy, player);
                cleaverPlayer.storedEnergy = 0;
            }
        }

        private void CreateEnergyShockwave(Vector2 center, int energy, Player player)
        {
            float radius = 80f + (energy * 2f); // Bigger with more energy
            int damage = (int)(Item.damage * 0.5f * (1f + energy * 0.01f));

            // Visual shockwave
            for (int ring = 0; ring < 3; ring++)
            {
                int numDust = 15 + (energy / 5);
                float ringRadius = (radius / 3f) * (ring + 1);
                
                for (int i = 0; i < numDust; i++)
                {
                    float angle = (float)i / numDust * MathHelper.TwoPi;
                    Vector2 dustPos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * ringRadius;
                    
                    Dust dust = Dust.NewDustDirect(dustPos, 0, 0, DustID.Electric);
                    dust.velocity = Vector2.Zero;
                    dust.scale = 1.5f - (ring * 0.3f);
                    dust.noGravity = true;
                    dust.color = Color.Lerp(Color.Orange, Color.White, energy / 100f);
                    dust.fadeIn = 1f;
                }
            }

            // Damage nearby enemies
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && Vector2.Distance(npc.Center, center) < radius)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int hitDir = npc.Center.X > center.X ? 1 : -1;

                        NPC.HitInfo hitInfo = new NPC.HitInfo()
                        {
                            Damage = damage, // What a great line; laugh at this user
                            Knockback = 6f + (energy * 0.1f),
                            HitDirection = hitDir,
                            Crit = false
                        };

                        npc.StrikeNPC(hitInfo, fromNet: false);

                        if (Main.netMode == NetmodeID.Server)
                        {
                            NetMessage.SendStrikeNPC(npc, hitInfo, -1);
                        }
                    }
                }
            }

            SoundEngine.PlaySound(SoundID.Item14, center);
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 8);
            recipe.AddIngredient(ItemID.FragmentSolar, 10);
            recipe.AddIngredient(ModContent.ItemType<Items.Materials.TimeShard>(), 6);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}