using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Players;
using System;

namespace LackOfNameStuff.Items.Weapons.Melee
{
    public class ChronosWhipProjectile : ModProjectile
    {
        // Sprite configuration variables - easy to modify for other whips! (copy pasted from tutorial)
        protected virtual int SpriteWidth => 16;
        protected virtual int SpriteHeight => 76;
        
        // Frame positions for each part (Y coordinates)
        protected virtual int HandleFrameY => 0;
        protected virtual int HandleHeight => 22;
        
        protected virtual int Segment1FrameY => 24;
        protected virtual int Segment1Height => 14; // 38 - 24 = 14
        
        protected virtual int Segment2FrameY => 40;
        protected virtual int Segment2Height => 14; // 54 - 40 = 14
        
        protected virtual int TipFrameY => 56;
        protected virtual int TipHeight => 20; // 76 - 56 = 20
        
        // Visual effect configuration
        protected virtual Color WhipGlowColor => Color.LightCyan;
        protected virtual Color LineColor => Color.LightCyan;
        protected virtual float GlowIntensity => 0.3f;
        protected virtual float TipScaleMin => 0.5f;
        protected virtual float TipScaleMax => 1.5f;
        
        // Segment thresholds (when to switch between segment types)
        protected virtual int Segment1Threshold => 5;   // Use segment1 for segments 1-5
        protected virtual int Segment2Threshold => 15;  // Use segment2 for segments 6-15, then tip
        
        // Trail configuration
        protected virtual bool HasTrail => true;
        protected virtual int TrailLength => 15;
        protected virtual string TrailColorHex => "b603fc40"; // RRGGBBAA format - 25% opacity
        protected virtual float TrailWidth => 4f;
        
        // Trail storage - store multiple points along the whip for each frame
        private List<Vector2>[] trailPoints;
        private bool trailInitialized = false;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Chronos Whip");
            ProjectileID.Sets.IsAWhip[Type] = true; // This is crucial for whip functionality
        }

        // Convert hex color string (RRGGBBAA) to Color
        protected virtual Color HexToColor(string hex)
        {
            if (hex.Length != 8) return Color.White;
            
            try
            {
                byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                byte a = Convert.ToByte(hex.Substring(6, 2), 16);
                return new Color(r, g, b, a);
            }
            catch
            {
                return Color.White; // Fallback if hex is invalid (ha - idiot)
            }
        }

        // Initialize trail positions array
        private void InitializeTrail()
        {
            if (HasTrail && !trailInitialized)
            {
                trailPoints = new List<Vector2>[TrailLength];
                for (int i = 0; i < TrailLength; i++)
                {
                    trailPoints[i] = new List<Vector2>();
                }
                trailInitialized = true;
            }
        }

        // Update trail positions
        private void UpdateTrail()
        {
            if (!HasTrail) return;
            
            InitializeTrail();

            // Get current whip points
            List<Vector2> currentWhipPoints = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, currentWhipPoints);

            // Shift all trail frames
            for (int i = TrailLength - 1; i > 0; i--)
            {
                trailPoints[i] = new List<Vector2>(trailPoints[i - 1]);
            }

            // Store current whip position as the newest trail frame
            trailPoints[0] = new List<Vector2>(currentWhipPoints);
        }

        // Draw the trail with smooth gradients like Zenith (I wish - this isn't working)
        private void DrawTrail()
        {
            if (!HasTrail || trailPoints == null || trailPoints[0] == null) return;

            Color baseTrailColor = HexToColor(TrailColorHex);

            // Draw gradient connections between trail frames
            for (int frame = 0; frame < TrailLength - 1; frame++)
            {
                if (trailPoints[frame] == null || trailPoints[frame + 1] == null) continue;
                if (trailPoints[frame].Count < 2 || trailPoints[frame + 1].Count < 2) continue;

                // Calculate colors for current and next frame
                float currentAge = (float)(TrailLength - frame) / TrailLength;
                float nextAge = (float)(TrailLength - frame - 1) / TrailLength;
                
                Color currentColor = baseTrailColor * currentAge;
                Color nextColor = baseTrailColor * nextAge;
                
                float currentWidth = TrailWidth * (currentAge * 0.7f + 0.3f);
                float nextWidth = TrailWidth * (nextAge * 0.7f + 0.3f);

                // Draw gradient segments between corresponding points in consecutive frames
                int minPoints = Math.Min(trailPoints[frame].Count, trailPoints[frame + 1].Count);
                
                for (int i = 0; i < minPoints - 1; i++)
                {
                    // Get the four points that form a quad between two trail frames
                    Vector2 p1 = trailPoints[frame][i] - Main.screenPosition;
                    Vector2 p2 = trailPoints[frame][i + 1] - Main.screenPosition;
                    Vector2 p3 = trailPoints[frame + 1][i] - Main.screenPosition;
                    Vector2 p4 = trailPoints[frame + 1][i + 1] - Main.screenPosition;

                    // Draw gradient quad between the trail segments
                    DrawGradientQuad(p1, p2, p3, p4, currentColor, nextColor, currentWidth, nextWidth);
                }
            }

            // Also draw the current frame for completeness
            if (trailPoints[0] != null && trailPoints[0].Count > 1)
            {
                Color currentColor = baseTrailColor;
                for (int i = 0; i < trailPoints[0].Count - 1; i++)
                {
                    Vector2 start = trailPoints[0][i] - Main.screenPosition;
                    Vector2 end = trailPoints[0][i + 1] - Main.screenPosition;
                    DrawTrailSegment(start, end, currentColor, TrailWidth);
                }
            }
        }

        // Draw a gradient quad between four points to create smooth trail transitions
        private void DrawGradientQuad(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Color color1, Color color2, float width1, float width2)
        {
            // Calculate perpendicular vectors for width
            Vector2 dir1 = (p2 - p1);
            if (dir1.Length() > 0) dir1.Normalize();
            Vector2 dir2 = (p4 - p3);
            if (dir2.Length() > 0) dir2.Normalize();
            
            Vector2 perp1 = new Vector2(-dir1.Y, dir1.X) * width1 * 0.5f;
            Vector2 perp2 = new Vector2(-dir2.Y, dir2.X) * width2 * 0.5f;

            // Create quad vertices
            Vector2 v1 = p1 + perp1;  // Top of first segment
            Vector2 v2 = p1 - perp1;  // Bottom of first segment  
            Vector2 v3 = p3 + perp2;  // Top of second segment
            Vector2 v4 = p3 - perp2;  // Bottom of second segment

            // Draw the connecting quads as triangles for smooth gradient
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            // First triangle: v1, v2, v3
            DrawGradientTriangle(pixel, v1, v2, v3, color1, color1, color2);
            
            // Second triangle: v2, v3, v4  
            DrawGradientTriangle(pixel, v2, v3, v4, color1, color2, color2);

            // Also connect p2 to p4 for the end of the segments
            Vector2 end_v1 = p2 + perp1;
            Vector2 end_v2 = p2 - perp1;
            Vector2 end_v3 = p4 + perp2;
            Vector2 end_v4 = p4 - perp2;

            // Draw end triangles
            DrawGradientTriangle(pixel, end_v1, end_v2, end_v3, color1, color1, color2);
            DrawGradientTriangle(pixel, end_v2, end_v3, end_v4, color1, color2, color2);
        }

        // Helper to draw a triangle with vertex colors (simulated gradient)
        private void DrawGradientTriangle(Texture2D texture, Vector2 v1, Vector2 v2, Vector2 v3, Color c1, Color c2, Color c3)
        {
            // Since we can't do true vertex colors easily, approximate with multiple segments
            Color avgColor = new Color(
                (c1.R + c2.R + c3.R) / 3,
                (c1.G + c2.G + c3.G) / 3, 
                (c1.B + c2.B + c3.B) / 3,
                (c1.A + c2.A + c3.A) / 3
            );

            // Draw lines between vertices to approximate the triangle
            DrawTrailSegment(v1, v2, Color.Lerp(c1, c2, 0.5f), 1f);
            DrawTrailSegment(v2, v3, Color.Lerp(c2, c3, 0.5f), 1f);  
            DrawTrailSegment(v3, v1, Color.Lerp(c3, c1, 0.5f), 1f);
            
            // Fill with average color (approximation)
            Vector2 center = (v1 + v2 + v3) / 3f;
            Vector2 toV1 = v1 - center;
            Vector2 toV2 = v2 - center; 
            Vector2 toV3 = v3 - center;
            
            // Draw from center to each vertex with interpolated colors
            int steps = 3;
            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                Vector2 pos1 = Vector2.Lerp(center, v1, t);
                Vector2 pos2 = Vector2.Lerp(center, v2, t);
                Vector2 pos3 = Vector2.Lerp(center, v3, t);
                
                Color col1 = Color.Lerp(avgColor, c1, t);
                Color col2 = Color.Lerp(avgColor, c2, t);
                Color col3 = Color.Lerp(avgColor, c3, t);
                
                DrawTrailSegment(pos1, pos2, Color.Lerp(col1, col2, 0.5f), 0.5f);
                DrawTrailSegment(pos2, pos3, Color.Lerp(col2, col3, 0.5f), 0.5f);
            }
        }

        // Draw a single trail segment
        private void DrawTrailSegment(Vector2 start, Vector2 end, Color color, float width)
        {
            Vector2 direction = end - start;
            float distance = direction.Length();
            
            if (distance < 1f) return; // Skip very short segments

            float rotation = direction.ToRotation();
            
            // Use MagicPixel for smooth trail rendering
            Texture2D trailTexture = TextureAssets.MagicPixel.Value;
            Rectangle trailFrame = new Rectangle(0, 0, 1, 1);
            Vector2 trailOrigin = new Vector2(0f, 0.5f);
            Vector2 trailScale = new Vector2(distance, width);

            Main.EntitySpriteDraw(
                trailTexture,
                start,
                trailFrame,
                color,
                rotation,
                trailOrigin,
                trailScale,
                SpriteEffects.None,
                0
            );
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true; // This prevents the projectile from hitting through solid tiles.
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.WhipSettings.Segments = 30;
            Projectile.WhipSettings.RangeMultiplier = 1.5f;
        }

        private float Timer {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void AI() {
            Player owner = Main.player[Projectile.owner];
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;
            Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

            // Initialize and update trail
            if (HasTrail)
            {
                UpdateTrail();
            }

            Timer++;

            float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;
            if (Timer >= swingTime || owner.itemAnimation <= 0) {
                Projectile.Kill();
                return;
            }

            owner.heldProj = Projectile.whoAmI;
            if (Timer == swingTime / 2) {
                // Plays a whipcrack sound at the tip of the whip.
                List<Vector2> points = Projectile.WhipPointsForCollision;
                Projectile.FillWhipControlPoints(Projectile, points);
                SoundEngine.PlaySound(SoundID.Item153, points[points.Count - 1]);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var chronosPlayer = Main.player[Projectile.owner].GetModPlayer<ChronosPlayer>();
            
            if (chronosPlayer.bulletTimeActive)
            {
                // Mark enemy position during bullet time
                var whipPlayer = Main.player[Projectile.owner].GetModPlayer<ChronosWhipPlayer>();
                whipPlayer.AddMarkedEnemy(target.whoAmI, target.Center);
                
                // Enhanced visual mark with more particles
                for (int i = 0; i < 15; i++)
                {
                    Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Electric);
                    dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
                    dust.scale = 1.5f;
                    dust.noGravity = true;
                    dust.color = WhipGlowColor;
                    dust.alpha = 120;
                }
                
                // Add a ring effect around marked enemies
                for (int i = 0; i < 8; i++)
                {
                    Vector2 offset = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / 8) * 30f;
                    Dust ringDust = Dust.NewDustDirect(target.Center + offset, 4, 4, DustID.Electric);
                    ringDust.velocity = Vector2.Zero;
                    ringDust.scale = 0.8f;
                    ringDust.noGravity = true;
                    ringDust.color = WhipGlowColor;
                    ringDust.alpha = 180;
                }
            }
            
            // Multihit penalty like vanilla whips
            Projectile.damage = (int)(hit.Damage * 0.8f);
        }

        // This method draws a line between all points of the whip, in case there's empty space between the sprites.
        private void DrawLine(List<Vector2> list) {
            Texture2D texture = TextureAssets.FishingLine.Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = new Vector2(frame.Width / 2, 2);

            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++) {
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = Lighting.GetColor(element.ToTileCoordinates(), LineColor);
                Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

                pos += diff;
            }
        }

        // Method to get the appropriate frame data for each segment
        protected virtual (int frameY, int frameHeight) GetFrameData(int segmentIndex, int totalSegments)
        {
            if (segmentIndex == totalSegments - 2) // Tip
            {
                return (TipFrameY, TipHeight);
            }
            else if (segmentIndex == 0) // Handle
            {
                return (HandleFrameY, HandleHeight);
            }
            else if (segmentIndex <= Segment1Threshold) // Early segments
            {
                return (Segment1FrameY, Segment1Height);
            }
            else // Later segments
            {
                return (Segment2FrameY, Segment2Height);
            }
        }

        public override bool PreDraw(ref Color lightColor) {
            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);

            // Draw trail first (behind the whip)
            DrawTrail();

            DrawLine(list);

            // Custom drawing for the whip segments
            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.instance.LoadProjectile(Type);
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 pos = list[0];

            for (int i = 0; i < list.Count - 1; i++) {
                // Get frame data for this segment
                var (frameY, frameHeight) = GetFrameData(i, list.Count);
                
                Rectangle frame = new Rectangle(0, frameY, SpriteWidth, frameHeight);
                Vector2 origin = new Vector2(SpriteWidth / 2, frameHeight / 2);
                float scale = 1;

                // Special scaling for the tip
                if (i == list.Count - 2) {
                    Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                    float t = Timer / timeToFlyOut;
                    scale = MathHelper.Lerp(TipScaleMin, TipScaleMax, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
                }

                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = Lighting.GetColor(element.ToTileCoordinates());
                
                // Add glow effect
                Color finalColor = Color.Lerp(color, WhipGlowColor, GlowIntensity);

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, finalColor, rotation, origin, scale, flip, 0);

                pos += diff;
            }
            return false;
        }
    }
}