using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace MyMod.Projectiles
{
    public class ExplosiveBullet : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.aiStyle = ProjAIStyleID.Arrow; // Changed from ProjAIStyleID.Bullet
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Explode when hitting terrain
            Explode();
            return true; // Kill the projectile
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Explode when hitting an enemy
            Explode();
        }

        private void Explode()
        {
            // Play explosion sound (smaller explosion than arrow)
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f, Pitch = 0.2f }, Projectile.position);

            // Create explosion visual effect
            for (int i = 0; i < 15; i++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, speed * 2.5f, Scale: 1.2f);
                d.noGravity = true;
            }

            // Create fire particles
            for (int i = 0; i < 10; i++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, speed * 1.8f, Scale: 1f);
                d.noGravity = true;
            }

            // Damage tiles in explosion radius (smaller than arrow)
            int explosionRadius = 2;
            Vector2 explosionCenter = Projectile.Center;
            
            for (int x = -explosionRadius; x <= explosionRadius; x++)
            {
                for (int y = -explosionRadius; y <= explosionRadius; y++)
                {
                    int tileX = (int)(explosionCenter.X / 16) + x;
                    int tileY = (int)(explosionCenter.Y / 16) + y;
                    
                    // Check if within circular radius
                    if (x * x + y * y <= explosionRadius * explosionRadius)
                    {
                        // Check if tile exists and is valid
                        if (WorldGen.InWorld(tileX, tileY))
                        {
                            Tile tile = Framing.GetTileSafely(tileX, tileY);
                            
                            // Only damage regular tiles, not walls or important tiles
                            if (tile.HasTile && Main.tileSolid[tile.TileType])
                            {
                                // Don't destroy important tiles like chests, altars, etc.
                                // Use TileLoader.CanExplode for better compatibility
                                if (TileLoader.CanExplode(tileX, tileY))
                                {
                                    WorldGen.KillTile(tileX, tileY, false, false, false);
                                    
                                    if (Main.netMode != NetmodeID.SinglePlayer)
                                    {
                                        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, tileX, tileY, 0f, 0, 0, 0);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Damage enemies in explosion radius
            Rectangle explosionRect = new Rectangle(
                (int)(explosionCenter.X - explosionRadius * 16), 
                (int)(explosionCenter.Y - explosionRadius * 16),
                explosionRadius * 32, 
                explosionRadius * 32
            );

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.Hitbox.Intersects(explosionRect))
                {
                    float distance = Vector2.Distance(npc.Center, explosionCenter);
                    if (distance <= explosionRadius * 16)
                    {
                        // Calculate damage based on distance (closer = more damage)
                        int damage = (int)(Projectile.damage * (1f - distance / (explosionRadius * 16)));
                        damage = Math.Max(damage, Projectile.damage / 4); // Minimum 25% damage
                        
                        npc.SimpleStrikeNPC(damage, 0, false, 0, DamageClass.Ranged);
                    }
                }
            }
        }

        public override void SetStaticDefaults()
        {
            // Modern tModLoader handles display names automatically
            // Remove SetDefault calls
        }
    }
}