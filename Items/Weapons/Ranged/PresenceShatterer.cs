using LackOfNameStuff.Buffs;
using LackOfNameStuff.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace LackOfNameStuff.Items.Weapons.Ranged
{
	// Mid-Calamity "stun gun" that applies TemporalShock via TemporalBolt. No damage, long visible cooldown.
	public class PresenceShatterer : ModItem
	{
		// Cooldown in ticks (60 = 1 second). Choose a big penalty without being obnoxious.
		// 10 seconds
		public const int CooldownTime = 60 * 10;

		public override void SetDefaults()
		{
			Item.width = 46;
			Item.height = 22;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 20; // animation speed, not the cooldown gate
			Item.useAnimation = 20;
			Item.noMelee = true;
			Item.autoReuse = false; // one shot per manual click
			Item.channel = false;
			Item.DamageType = DamageClass.Ranged;
			Item.damage = 0; // no damage
			Item.knockBack = 0f;
			Item.shoot = ModContent.ProjectileType<TemporalBolt>();
			Item.shootSpeed = 16f;
			Item.rare = ItemRarityID.Cyan; // mid-late rarity
			Item.value = Item.buyPrice(gold: 10);
			Item.UseSound = SoundID.Item85; // zappy charge
		}

		public override void SetStaticDefaults()
		{
			// Tooltip intentionally minimal; this is a utility weapon
			// DisplayName.SetDefault("Presence Shatterer");
			// Tooltip.SetDefault("Fires a temporal bolt that shocks foes\nApplies Temporal Shock, reducing their defense\nHas a long cooldown and deals no damage");
		}

		public override bool CanUseItem(Player player)
		{
			// Block usage if cooldown is active
			return !player.HasBuff(ModContent.BuffType<PresenceShattererCooldown>());
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// Spawn the TemporalBolt with player's aim; ensure no damage
			int proj = Projectile.NewProjectile(source, position, velocity, type, 0, 0f, player.whoAmI);

			// Apply visible cooldown to the player
			player.AddBuff(ModContent.BuffType<PresenceShattererCooldown>(), CooldownTime);

			// Prevent default shooting since we spawned manually (but we could also return true and rely on Item.damage=0)
			return false;
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-6f, 0f);
		}

		public override void AddRecipes()
		{
			// Mid-Calamity recipe (post-Providence or Polterghast); using Eternal Shard and Time Gem if available
			Recipe recipe = CreateRecipe();
			// Try to use our temporal materials if present
			int eternalShard = ModContent.TryFind("LackOfNameStuff/Items/Materials/EternalShard", out ModItem esItem)
				? esItem.Type : ItemID.FragmentVortex; // fallback
			int timeGem = ModContent.TryFind("LackOfNameStuff/Items/Materials/TimeGem", out ModItem tgItem)
				? tgItem.Type : ItemID.FragmentNebula; // fallback

			recipe.AddIngredient(eternalShard, 8);
			recipe.AddIngredient(timeGem, 6);
			recipe.AddIngredient(ItemID.Wire, 50);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
}

