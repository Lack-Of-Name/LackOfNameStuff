using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;

namespace LackOfNameStuff.Items.Weapons.Magic
{
    public class PrismaticStaff : ModItem
    {
        // === WEAPON CONFIGURATION ===
        public static readonly int WeaponDamage = 85;
        public static readonly int WeaponMana = 30;
        public static readonly int WeaponUseTime = 40;
        public static readonly int WeaponUseAnimation = 40;
        public static readonly float WeaponKnockback = 4f;
        public static readonly int WeaponCrit = 20;
        public static readonly int WeaponValue = Item.sellPrice(gold: 12);
        public static readonly int WeaponRarity = ItemRarityID.Pink;
        public static readonly int ProjectileType = ModContent.ProjectileType<PrismaticBolt>();
        public static readonly float ProjectileSpeed = 3f;
        public static readonly int ProjectileCount = 7;
        public static readonly float ProjectileSpread = 0.26f; // In radians
        public static readonly float ShotRandomness = 0.15f; // Additional random spread in radians
        public static readonly float ProjectileSpawnOffset = 0f; // Distance from player center to spawn projectiles
        
        // === DAYTIME ENHANCEMENT CONFIGURATION ===
        public static readonly float DaytimeDamageMultiplier = 1.2f; // 20% more damage during day
        public static readonly int DaytimeProjectileCount = 10; // More projectiles (vs 7 at night)
        public static readonly float DaytimeProjectileSpeed = 4.5f; // Faster projectiles
        public static readonly int DaytimeManaReduction = 25; // Less mana cost (vs 30)
        public static readonly float DaytimeKnockbackBonus = 2f; // Extra knockback

        public override void SetDefaults()
        {
            bool isDaytime = Main.dayTime;
            
            Item.damage = isDaytime ? (int)(WeaponDamage * DaytimeDamageMultiplier) : WeaponDamage;
            Item.DamageType = DamageClass.Magic;
            Item.mana = isDaytime ? DaytimeManaReduction : WeaponMana;
            Item.width = 64;
            Item.height = 64;
            Item.scale = 0.075f;
            Item.useTime = WeaponUseTime;
            Item.useAnimation = WeaponUseAnimation;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = isDaytime ? WeaponKnockback + DaytimeKnockbackBonus : WeaponKnockback;
            Item.value = WeaponValue;
            Item.rare = WeaponRarity;
            Item.UseSound = SoundID.Item125; // Empress of Light sound
            Item.autoReuse = true;
            Item.shoot = ProjectileType;
            Item.shootSpeed = isDaytime ? DaytimeProjectileSpeed : ProjectileSpeed;
            Item.crit = WeaponCrit;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            bool isDaytime = Main.dayTime;
            int projectileCount = isDaytime ? DaytimeProjectileCount : ProjectileCount;
            float currentSpeed = isDaytime ? DaytimeProjectileSpeed : ProjectileSpeed;
            
            for (int i = 0; i < projectileCount; i++)
            {
                Vector2 shootVel = velocity.SafeNormalize(Vector2.UnitX) * currentSpeed;
                
                // Apply base spread - tighter during day for more focused assault
                if (ProjectileSpread > 0)
                {
                    float spreadModifier = isDaytime ? 0.8f : 1f; // Tighter spread during day
                    float spread = ProjectileSpread * spreadModifier * (i - (projectileCount - 1) / 2f);
                    shootVel = shootVel.RotatedBy(spread);
                }
                
                // Add randomness to shot direction
                if (ShotRandomness > 0)
                {
                    float randomSpread = Main.rand.NextFloat(-ShotRandomness, ShotRandomness);
                    shootVel = shootVel.RotatedBy(randomSpread);
                }
                
                // Calculate spawn position with offset
                Vector2 spawnPosition = position;
                if (ProjectileSpawnOffset > 0)
                {
                    Vector2 offsetDirection = shootVel.SafeNormalize(Vector2.UnitX);
                    spawnPosition += offsetDirection * ProjectileSpawnOffset;
                }
                
                Projectile.NewProjectile(source, spawnPosition, shootVel, type, damage, knockback, player.whoAmI);
            }
            
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            bool isDaytime = Main.dayTime;
            string timeTooltip = isDaytime ? "Channels the fury of the solar empress" : "Harnesses prismatic light";
            
            TooltipLine timeLine = new TooltipLine(Mod, "TimeBasedPower", timeTooltip);
            timeLine.OverrideColor = isDaytime ? Color.Gold : Color.Cyan;
            tooltips.Add(timeLine);
        }

        public override void AddRecipes()
        {
            // Add recipe here
            
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 12);
            recipe.AddIngredient(ItemID.FragmentNebula, 8);
            recipe.AddIngredient(ItemID.SoulofLight, 5);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
            
        }
    }

    public class PrismaticBolt : ModProjectile
    {
        // === PROJECTILE CONFIGURATION ===
        public static readonly int ProjectileWidth = 16;
        public static readonly int ProjectileHeight = 16;
        public static readonly float MaxSpeed = 16f;
        public static readonly float Acceleration = 0.25f; // Speed increase per frame
        public static readonly float HomingStrength = 0.08f; // How quickly it turns toward target
        public static readonly float DetectionRange = 800f; // Pixels to search for enemies
        public static readonly int MaxTimeLeft = 1200; // Frames before despawn (600 = 10 seconds at 60fps)
        public static readonly bool PenetrateEnemies = true;
        public static readonly int MaxPenetrations = 1;
        public static readonly float LightIntensity = 2f;
        public static readonly bool CreateDust = true;
        public static readonly int DustFrequency = 3; // Every N frames
        
        // Projectile opacity control - always nearly invisible
        public static readonly float ProjectileOpacity = 0.1f; // Very low opacity
        
        // Trail configuration
        public static readonly int TrailLength = 20;
        public static readonly float TrailWidth = 10f;
        public static readonly float TrailScale = 1.75f; // Overall scale multiplier for trail
        public static readonly float TrailAlpha = 0.6f; // Maximum trail opacity
        public static readonly float TrailSaturation = 0.9f; // Base saturation for trail colors (0.0 = grayscale, 1.0 = full color)
        
        // Ricochet system
        public static readonly int MaxRicochets = 3;
        public static readonly float RicochetDamageReduction = 0.8f;
        
        // === DAYTIME PROJECTILE ENHANCEMENTS ===
        public static readonly float DaytimeMaxSpeed = 20f; // Faster than night (16f)
        public static readonly float DaytimeAcceleration = 0.35f; // Faster acceleration
        public static readonly float DaytimeHomingStrength = 0.12f; // Stronger homing
        public static readonly float DaytimeDetectionRange = 1000f; // Longer detection range
        public static readonly int DaytimeMaxRicochets = 5; // More ricochets
        public static readonly float DaytimeRicochetDamageReduction = 0.9f; // Less damage reduction per ricochet
        
        private int dustTimer = 0;
        private int spiralTimer = 0;
        private int ricochetsRemaining;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            bool isDaytime = Main.dayTime;
            
            Projectile.width = ProjectileWidth;
            Projectile.height = ProjectileHeight;
            Projectile.aiStyle = -1; // Custom AI
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = PenetrateEnemies ? MaxPenetrations : 1;
            Projectile.timeLeft = MaxTimeLeft;
            Projectile.light = isDaytime ? LightIntensity * 1.3f : LightIntensity;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            
            // Make projectile nearly invisible
            Projectile.alpha = (int)((1f - ProjectileOpacity) * 255f);
            
            // Initialize ricochet system with daytime consideration
            ricochetsRemaining = isDaytime ? DaytimeMaxRicochets : MaxRicochets;
        }

        public override void AI()
        {
            bool isDaytime = Main.dayTime;
            float currentMaxSpeed = isDaytime ? DaytimeMaxSpeed : MaxSpeed;
            float currentAcceleration = isDaytime ? DaytimeAcceleration : Acceleration;
            float currentHomingStrength = isDaytime ? DaytimeHomingStrength : HomingStrength;
            float currentDetectionRange = isDaytime ? DaytimeDetectionRange : DetectionRange;
            
            // Acceleration - gradually increase speed
            if (currentAcceleration > 0 && Projectile.velocity.Length() < currentMaxSpeed)
            {
                float currentSpeed = Projectile.velocity.Length();
                float newSpeed = Math.Min(currentSpeed + currentAcceleration, currentMaxSpeed);
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * newSpeed;
            }
            
            // Enhanced homing behavior with line-of-sight targeting
            NPC target = FindBestTarget(currentDetectionRange);
            
            if (target != null)
            {
                Vector2 targetDirection = target.Center - Projectile.Center;
                targetDirection.Normalize();
                
                // Gradually adjust velocity toward target
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDirection * currentMaxSpeed, currentHomingStrength);
            }
            
            // Limit max speed
            if (Projectile.velocity.Length() > currentMaxSpeed)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= currentMaxSpeed;
            }
            
            // Rotation follows movement direction
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // Create enhanced prismatic dust effect - more intense during day
            if (CreateDust)
            {
                dustTimer++;
                spiralTimer++;
                
                int dustFreq = isDaytime ? Math.Max(1, DustFrequency - 1) : DustFrequency; // More frequent dust during day
                if (dustTimer >= dustFreq)
                {
                    dustTimer = 0;
                    CreateAdvancedPrismaticDust(isDaytime);
                }
            }
        }

        // Enhanced target prioritization system with line-of-sight check
        private NPC FindBestTarget(float detectionRange)
        {
            NPC bestTarget = null;
            float bestScore = float.MaxValue;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && npc.CanBeChasedBy())
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < detectionRange)
                    {
                        // Line-of-sight check
                        if (!HasLineOfSight(Projectile.Center, npc.Center))
                            continue;
                        
                        float score = distance;
                        
                        // Prioritize bosses (highest priority)
                        if (npc.boss) score *= 0.3f;
                        
                        // Prioritize damaged enemies
                        float healthRatio = (float)npc.life / npc.lifeMax;
                        if (healthRatio < 0.8f) score *= 0.7f;
                        
                        // Prioritize enemies with higher life (more threatening)
                        if (npc.lifeMax > 1000) score *= 0.8f;
                        
                        // Slight preference for enemies in movement direction
                        Vector2 toEnemy = (npc.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        Vector2 movementDir = Projectile.velocity.SafeNormalize(Vector2.Zero);
                        float directionBonus = Vector2.Dot(toEnemy, movementDir);
                        if (directionBonus > 0) score *= (1f - directionBonus * 0.2f);
                        
                        if (score < bestScore)
                        {
                            bestScore = score;
                            bestTarget = npc;
                        }
                    }
                }
            }
            
            return bestTarget;
        }

        // Line-of-sight check to prevent targeting through walls
        private bool HasLineOfSight(Vector2 start, Vector2 end)
        {
            Vector2 direction = end - start;
            float distance = direction.Length();
            direction.Normalize();
            
            // Check points along the line for solid tiles
            int steps = (int)(distance / 16f); // Check every 16 pixels (1 tile)
            for (int i = 1; i < steps; i++)
            {
                Vector2 checkPoint = start + direction * (i * 16f);
                int tileX = (int)(checkPoint.X / 16f);
                int tileY = (int)(checkPoint.Y / 16f);
                
                // Bounds check
                if (tileX < 0 || tileY < 0 || tileX >= Main.maxTilesX || tileY >= Main.maxTilesY)
                    return false;
                
                Tile tile = Main.tile[tileX, tileY];
                if (tile != null && tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                {
                    return false; // Solid tile blocking line of sight
                }
            }
            
            return true; // Clear line of sight
        }

        // Advanced particle system with spiraling effects
        private void CreateAdvancedPrismaticDust(bool isDaytime = false)
        {
            int dustMultiplier = isDaytime ? 2 : 1; // More particles during day
            
            // Base rainbow dust (night) or solar dust (day)
            for (int i = 0; i < 2 * dustMultiplier; i++)
            {
                int dustType = isDaytime ? DustID.YellowTorch : DustID.RainbowMk2;
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType);
                dust.velocity = Projectile.velocity * (isDaytime ? 0.4f : 0.3f);
                dust.scale = Main.rand.NextFloat(isDaytime ? 1f : 0.8f, isDaytime ? 1.5f : 1.2f);
                dust.noGravity = true;
                
                // During day, make dust more solar-colored
                if (isDaytime)
                {
                    dust.color = Color.Lerp(Color.White, Color.Gold, Main.rand.NextFloat(0.3f, 1f));
                }
            }
            
            // More intense effects during daytime
            if (isDaytime && Main.rand.NextBool(2))
            {
                // Extra bright white/gold core during day
                Dust brightDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch);
                brightDust.velocity = Projectile.velocity * 0.2f;
                brightDust.scale = 1.5f;
                brightDust.noGravity = true;
                brightDust.color = Color.Lerp(Color.White, Color.Gold, 0.7f);
            }
            
            // Spiraling dust pattern
            if (spiralTimer % 4 == 0)
            {
                float spiralAngle = spiralTimer * 0.2f;
                int spiralCount = isDaytime ? 3 : 2; // More spirals during day
                
                for (int i = 0; i < spiralCount; i++)
                {
                    Vector2 spiralOffset = new Vector2(
                        (float)Math.Cos(spiralAngle + i * MathHelper.TwoPi / spiralCount) * 12f,
                        (float)Math.Sin(spiralAngle + i * MathHelper.TwoPi / spiralCount) * 12f
                    );
                    
                    Vector2 dustPos = Projectile.Center + spiralOffset;
                    int dustType = isDaytime ? DustID.YellowTorch : DustID.RainbowMk2;
                    Dust dust = Dust.NewDustDirect(dustPos, 0, 0, dustType);
                    dust.velocity = Vector2.Zero;
                    dust.scale = isDaytime ? 1.2f : 1.0f;
                    dust.noGravity = true;
                    dust.fadeIn = 0.8f;
                    
                    if (isDaytime)
                    {
                        dust.color = Color.Lerp(Color.White, Color.Gold, Main.rand.NextFloat(0.4f, 1f));
                    }
                    
                    // Add some variety with different dust types occasionally
                    if (Main.rand.NextBool(4))
                    {
                        int extraDustType = isDaytime ? DustID.Enchanted_Gold : DustID.YellowTorch;
                        Dust extraDust = Dust.NewDustDirect(dustPos, 0, 0, extraDustType);
                        extraDust.velocity = Main.rand.NextVector2Circular(2f, 2f);
                        extraDust.scale = isDaytime ? 1f : 0.8f;
                        extraDust.noGravity = true;
                    }
                }
            }
            
            // Occasional luxury effects - more frequent during day
            int luxChance = isDaytime ? 4 : 6;
            if (Main.rand.NextBool(luxChance))
            {
                Dust luxDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Enchanted_Gold);
                luxDust.velocity = Main.rand.NextVector2Circular(4f, 4f);
                luxDust.scale = isDaytime ? 1.4f : 1.2f;
                luxDust.noGravity = true;
                luxDust.color = isDaytime ? Color.Gold : Color.White;
            }
        }

        // Enhanced trail rendering with different colors for day/night
        public override bool PreDraw(ref Color lightColor)
        {
            DrawEnhancedPrismaticTrail();
            return true;
        }

        private void DrawEnhancedPrismaticTrail()
        {
            Vector2[] trailPositions = Projectile.oldPos;
            
            if (trailPositions.Length < 2)
                return;

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            
            bool isDaytime = Main.dayTime;
            
            if (isDaytime)
            {
                // Solar/Empress of Light style trails during day
                DrawSolarTrail();
            }
            else
            {
                // Rainbow prismatic trails at night
                DrawPrismaticTrail();
            }
        }

        private void DrawSolarTrail()
        {
            Vector2[] trailPositions = Projectile.oldPos;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            
            float scaleMultiplier = 1.2f; // Bigger trails during day
            
            // Draw solar-themed trail layers
            DrawSolarTrailLayer(TrailScale * 1.5f * scaleMultiplier, TrailAlpha * 0.3f, Color.Gold, 0.4f);     // Outer glow
            DrawSolarTrailLayer(TrailScale * scaleMultiplier, TrailAlpha * 0.8f, Color.White, 0.8f);          // Main trail
            DrawSolarTrailLayer(TrailScale * 0.6f * scaleMultiplier, TrailAlpha, Color.White, 1f);            // Inner bright core
        }

        private void DrawPrismaticTrail()
        {
            Vector2[] trailPositions = Projectile.oldPos;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            
            // Calculate base color for synchronized rainbow effect
            float time = Main.GlobalTimeWrappedHourly * 1.5f;
            float baseHue = (time + Projectile.whoAmI * 0.1f) % 1f;
            
            // Draw multiple rainbow trail layers for depth
            DrawRainbowTrailLayer(TrailScale * 1.5f, TrailAlpha * 0.3f, baseHue, 0.8f, 0.7f); // Outer glow
            DrawRainbowTrailLayer(TrailScale, TrailAlpha * 0.8f, baseHue, 0.9f, 0.8f);         // Main trail
            DrawRainbowTrailLayer(TrailScale * 0.6f, TrailAlpha, baseHue, 1f, 0.9f);          // Inner bright core
        }

        private void DrawSolarTrailLayer(float scale, float alpha, Color baseColor, float intensity)
        {
            Vector2[] trailPositions = Projectile.oldPos;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            
            for (int i = 0; i < trailPositions.Length - 1; i++)
            {
                if (trailPositions[i] == Vector2.Zero)
                    continue;
                
                float progress = 1f - (float)i / trailPositions.Length;
                float layerScale = progress * scale;
                float layerAlpha = progress * alpha * intensity;
                
                Color trailColor = Color.Lerp(baseColor, Color.Yellow, 0.3f) * layerAlpha;
                
                Vector2 drawPos = trailPositions[i] + Projectile.Size * 0.5f - Main.screenPosition;
                
                Main.EntitySpriteDraw(
                    texture,
                    drawPos,
                    null,
                    trailColor,
                    Projectile.oldRot[i],
                    origin,
                    layerScale,
                    SpriteEffects.None,
                    0
                );
            }
        }

        private void DrawRainbowTrailLayer(float scale, float alpha, float baseHue, float saturation, float lightness)
        {
            Vector2[] trailPositions = Projectile.oldPos;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            
            for (int i = 0; i < trailPositions.Length - 1; i++)
            {
                if (trailPositions[i] == Vector2.Zero)
                    continue;
                
                float progress = 1f - (float)i / trailPositions.Length;
                float layerScale = progress * scale;
                float layerAlpha = progress * alpha;
                
                // Use the synchronized base hue with slight offset for trail segments
                float segmentHue = (baseHue + i * 0.02f) % 1f;
                Color trailColor = Main.hslToRgb(segmentHue, saturation, lightness);
                
                trailColor *= layerAlpha;
                
                Vector2 drawPos = trailPositions[i] + Projectile.Size * 0.5f - Main.screenPosition;
                
                Main.EntitySpriteDraw(
                    texture,
                    drawPos,
                    null,
                    trailColor,
                    Projectile.oldRot[i],
                    origin,
                    layerScale,
                    SpriteEffects.None,
                    0
                );
            }
        }

        // Enhanced ricochet system
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (ricochetsRemaining > 0)
            {
                ricochetsRemaining--;
                
                // Bounce logic with slight randomization
                Vector2 newVelocity = oldVelocity;
                if (Projectile.velocity.X != oldVelocity.X)
                    newVelocity.X = -oldVelocity.X * Main.rand.NextFloat(0.8f, 1.2f);
                if (Projectile.velocity.Y != oldVelocity.Y)
                    newVelocity.Y = -oldVelocity.Y * Main.rand.NextFloat(0.8f, 1.2f);
                
                Projectile.velocity = newVelocity;
                
                // Reduce damage on ricochet - less reduction during day
                bool isDaytime = Main.dayTime;
                float damageReduction = isDaytime ? DaytimeRicochetDamageReduction : RicochetDamageReduction;
                Projectile.damage = (int)(Projectile.damage * damageReduction);
                
                // Enhanced ricochet effects
                CreateRicochetEffects();
                
                // Sound effect
                SoundEngine.PlaySound(SoundID.Item10.WithVolumeScale(0.6f), Projectile.Center);
                
                return false; // Don't kill projectile
            }
            
            // Final impact effect when out of ricochets
            CreateFinalImpactEffect();
            return true;
        }

        private void CreateRicochetEffects()
        {
            Vector2 impactCenter = Projectile.Center;
            bool isDaytime = Main.dayTime;
            int effectMultiplier = isDaytime ? 2 : 1;
            
            if (isDaytime)
            {
                // Solar-themed ricochet effects
                for (int i = 0; i < 8 * effectMultiplier; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
                    dust.position = impactCenter + Main.rand.NextVector2Circular(6f, 6f);
                    dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
                    dust.scale = Main.rand.NextFloat(1f, 2.2f);
                    dust.noGravity = true;
                    dust.alpha = 0;
                    dust.color = Color.Lerp(Color.White, Color.Gold, Main.rand.NextFloat(0.3f, 1f));
                }
            }
            else
            {
                // Rainbow ricochet burst at night
                for (int i = 0; i < 8 * effectMultiplier; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RainbowMk2);
                    dust.position = impactCenter + Main.rand.NextVector2Circular(6f, 6f);
                    dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
                    dust.scale = Main.rand.NextFloat(1f, 1.8f);
                    dust.noGravity = true;
                    dust.alpha = 0;
                }
            }
            
            // Bright sparks - more during day
            for (int i = 0; i < 5 * effectMultiplier; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Silver);
                dust.position = impactCenter + Main.rand.NextVector2Circular(4f, 4f);
                dust.velocity = Main.rand.NextVector2Circular(8f, 8f);
                dust.scale = Main.rand.NextFloat(0.8f, isDaytime ? 1.6f : 1.4f);
                dust.noGravity = true;
            }
        }

        private void CreateFinalImpactEffect()
        {
            Vector2 impactCenter = Projectile.Center;
            bool isDaytime = Main.dayTime;
            
            // Enhanced final impact - more intense during day
            int dustCount = isDaytime ? 25 : 15;
            
            if (isDaytime)
            {
                // Solar-themed final impact
                for (int i = 0; i < dustCount; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
                    dust.position = impactCenter + Main.rand.NextVector2Circular(8f, 8f);
                    dust.velocity = Main.rand.NextVector2Circular(8f, 8f);
                    dust.scale = Main.rand.NextFloat(1.2f, 2.5f);
                    dust.noGravity = true;
                    dust.fadeIn = 1f;
                    dust.alpha = 50;
                    dust.color = Color.Lerp(Color.White, Color.Gold, Main.rand.NextFloat(0.2f, 1f));
                }
            }
            else
            {
                // Rainbow prismatic dust burst at night
                for (int i = 0; i < dustCount; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RainbowMk2);
                    dust.position = impactCenter + Main.rand.NextVector2Circular(8f, 8f);
                    dust.velocity = Main.rand.NextVector2Circular(8f, 8f);
                    dust.scale = Main.rand.NextFloat(1.2f, 2f);
                    dust.noGravity = true;
                    dust.fadeIn = 1f;
                    dust.alpha = 50;
                }
            }
            
            // Bright white/silver sparks
            for (int i = 0; i < (isDaytime ? 15 : 10); i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Silver);
                dust.position = impactCenter + Main.rand.NextVector2Circular(6f, 6f);
                dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
                dust.scale = Main.rand.NextFloat(0.8f, isDaytime ? 1.8f : 1.4f);
                dust.noGravity = true;
                dust.fadeIn = 0.8f;
            }
            
            // Additional glowing white/gold dust
            for (int i = 0; i < (isDaytime ? 12 : 8); i++)
            {
                int dustType = isDaytime ? DustID.YellowTorch : DustID.WhiteTorch;
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType);
                dust.position = impactCenter + Main.rand.NextVector2Circular(10f, 10f);
                dust.velocity = Main.rand.NextVector2Circular(4f, 4f);
                dust.scale = Main.rand.NextFloat(1f, isDaytime ? 2.2f : 1.8f);
                dust.noGravity = true;
                dust.alpha = 100;
                dust.color = isDaytime ? Color.Gold : Color.White;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Enhanced hit effect
            Vector2 hitCenter = target.Center;
            bool isDaytime = Main.dayTime;
            int baseEffectCount = isDaytime ? 35 : 25; // More effects during day
            
            if (isDaytime)
            {
                // Solar-themed hit effects during day
                for (int i = 0; i < baseEffectCount; i++)
                {
                    int dustType = Main.rand.Next(new int[] { DustID.YellowTorch, DustID.Enchanted_Gold, DustID.WhiteTorch });
                    Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, dustType);
                    dust.position = hitCenter + Main.rand.NextVector2Circular(target.width * 0.8f, target.height * 0.8f);
                    dust.velocity = Main.rand.NextVector2Circular(15f, 15f);
                    dust.scale = Main.rand.NextFloat(2.2f, 3.5f);
                    dust.noGravity = true;
                    dust.alpha = 0;
                    
                    // Solar color scheme
                    dust.color = Color.Lerp(Color.White, Color.Gold, Main.rand.NextFloat(0.2f, 1f));
                }
            }
            else
            {
                // Rainbow prismatic explosion at night
                for (int i = 0; i < baseEffectCount; i++)
                {
                    int dustType = Main.rand.Next(new int[] { DustID.RainbowMk2, DustID.RainbowRod, 267 });
                    Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, dustType);
                    dust.position = hitCenter + Main.rand.NextVector2Circular(target.width * 0.8f, target.height * 0.8f);
                    dust.velocity = Main.rand.NextVector2Circular(15f, 15f);
                    dust.scale = Main.rand.NextFloat(1.8f, 3f);
                    dust.noGravity = true;
                    dust.alpha = 0;
                    
                    // Enhanced smooth rainbow effect
                    float time = Main.GlobalTimeWrappedHourly * 2f;
                    float hue = (time + i * 0.04f + Main.rand.NextFloat(0f, 0.1f)) % 1f;
                    dust.color = Main.hslToRgb(hue, 1f, 0.8f);
                }
            }
            
            // Bright explosion center
            for (int i = 0; i < (isDaytime ? 20 : 15); i++)
            {
                int dustType = isDaytime ? DustID.YellowTorch : DustID.YellowTorch;
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, dustType);
                dust.position = hitCenter + Main.rand.NextVector2Circular(target.width * 0.3f, target.height * 0.3f);
                dust.velocity = Main.rand.NextVector2Circular(12f, 12f);
                dust.scale = Main.rand.NextFloat(isDaytime ? 2f : 1.5f, isDaytime ? 3f : 2.5f);
                dust.noGravity = true;
                dust.alpha = 0;
                dust.color = isDaytime ? Color.Gold : Color.Yellow;
            }
            
            // White/gold sparkles
            for (int i = 0; i < (isDaytime ? 18 : 12); i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Enchanted_Gold);
                dust.position = hitCenter + Main.rand.NextVector2Circular(target.width * 0.5f, target.height * 0.5f);
                dust.velocity = Main.rand.NextVector2Circular(8f, 8f);
                dust.scale = Main.rand.NextFloat(1f, isDaytime ? 2.5f : 2f);
                dust.noGravity = true;
            }
        }
    }
}