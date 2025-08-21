using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Players;

namespace LackOfNameStuff.Projectiles
{
    public class TemporalOrb : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600; // 10 seconds
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // Pass through tiles
        }

        private bool hasActivated = false;
        private Vector2 waitPosition;
        private int targetNPC = -1;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            var chronosPlayer = player.GetModPlayer<ChronosPlayer>();
            bool inChronosTime = chronosPlayer.screenEffectIntensity > 0.1f;

            if (!hasActivated && inChronosTime)
            {
                // During Chronos time: float out to position and wait
                if (Projectile.ai[0] == 0f) // First frame
                {
                    // Set the waiting position
                    waitPosition = Projectile.position + Projectile.velocity * 3f; // 3x the distance
                    Projectile.ai[0] = 1f;
                }

                // Move toward waiting position
                Vector2 direction = waitPosition - Projectile.Center;
                if (direction.Length() > 5f)
                {
                    direction.Normalize();
                    Projectile.velocity = direction * 4f;
                }
                else
                {
                    // Reached position, float in place
                    Projectile.velocity *= 0.95f;
                }

                // Visual effects during wait
                CreateWaitingDust();
            }
            else if (inChronosTime == false && !hasActivated)
            {
                // Chronos time just ended - activate homing!
                hasActivated = true;
                targetNPC = FindClosestEnemy();
                
                // Visual/audio feedback for activation
                CreateActivationEffects();
            }

            if (hasActivated)
            {
                // Homing behavior
                DoHomingAI();
            }

            // Rotation for visual effect
            Projectile.rotation += 0.1f;
        }

        private void CreateWaitingDust()
        {
            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric);
                dust.velocity = Vector2.Zero;
                dust.scale = 0.8f;
                dust.noGravity = true;
            }
        }

        private void CreateActivationEffects()
        {
            // Bright flash when activating
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Electric);
                dust.velocity = Main.rand.NextVector2Circular(5f, 5f);
                dust.scale = 1.2f;
                dust.noGravity = true;
            }
            
            // Sound effect
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item9, Projectile.position);
        }

        private int FindClosestEnemy()
        {
            float closestDistance = 800f; // Max targeting range
            int closestNPC = -1;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNPC = i;
                    }
                }
            }

            return closestNPC;
        }

        private void DoHomingAI()
        {
            if (targetNPC >= 0 && targetNPC < Main.maxNPCs && Main.npc[targetNPC].active)
            {
                NPC target = Main.npc[targetNPC];
                Vector2 direction = target.Center - Projectile.Center;
                
                if (direction.Length() > 10f)
                {
                    direction.Normalize();
                    float homingSpeed = 12f;
                    
                    // Accelerate toward target
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * homingSpeed, 0.08f);
                }
            }
            else
            {
                // Target lost, find new one
                targetNPC = FindClosestEnemy();
                if (targetNPC == -1)
                {
                    // No targets, just fly forward
                    Projectile.velocity *= 1.02f; // Slight acceleration
                }
            }

            // Homing trail effect
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric);
                dust.velocity = -Projectile.velocity * 0.3f;
                dust.scale = 0.6f;
                dust.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Impact effects
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric);
                dust.velocity = Main.rand.NextVector2Circular(4f, 4f);
                dust.scale = 0.9f;
                dust.noGravity = true;
            }
            
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
    }
}