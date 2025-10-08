using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using LackOfNameStuff.Buffs;

namespace LackOfNameStuff.Projectiles
{
	// Simple non-damaging bolt that applies TemporalShock on hit
	public class TemporalBolt : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 10;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.aiStyle = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 180; // 3s
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10; // short cooldown to ensure hit registers
		}

		public override void AI()
		{
			// Rotate to velocity
			if (Projectile.velocity.LengthSquared() > 0.1f)
				Projectile.rotation = Projectile.velocity.ToRotation();

			// Light and dust for a temporal feel
			Lighting.AddLight(Projectile.Center, 0.2f, 0.3f, 0.5f);
			if (Main.rand.NextBool(3))
			{
				var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric,
					-Projectile.velocity.X * 0.1f, -Projectile.velocity.Y * 0.1f, 150, Color.Cyan, 1.1f);
				d.noGravity = true;
			}
		}

		// Ensure the projectile can actually register a hit: keep at least 1 damage
		public override void OnSpawn(IEntitySource source)
		{
			if (Projectile.damage <= 0)
				Projectile.damage = 1;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			// Apply TemporalShock debuff for a few seconds (scaled in Expert/Master by the buff itself)
			target.AddBuff(ModContent.BuffType<TemporalShock>(), 240); // 4s
			SoundEngine.PlaySound(SoundID.Item93, target.Center); // zappy hit

			// Brief burst
			for (int i = 0; i < 10; i++)
			{
				var d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Electric, 0, 0, 120, Color.Cyan, 1.2f);
				d.velocity = Main.rand.NextVector2Circular(2.5f, 2.5f);
				d.noGravity = true;
			}
			Projectile.Kill(); // Ensure the projectile disappears after hitting
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			// Small zap on tile
			SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
			for (int i = 0; i < 6; i++)
			{
				var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric);
				d.velocity *= 0.7f;
				d.noGravity = true;
			}
			return true;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			// Soft glow trail using a simple rectangle texture
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 origin = tex.Size() * 0.5f;
			for (int i = 1; i < Projectile.oldPos.Length; i++)
			{
				if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i - 1] == Vector2.Zero) continue;
				float progress = 1f - i / (float)Projectile.oldPos.Length;
				Color col = new Color(80, 200, 255, 0) * (0.3f + 0.4f * progress);
				Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
				Main.EntitySpriteDraw(tex, drawPos, null, col, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
			}
			return true;
		}
	}
}
