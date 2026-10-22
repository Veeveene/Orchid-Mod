using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common.Attributes;
using OrchidMod.Common.ModObjects;
using OrchidMod.Content.Guardian.Projectiles.Quarterstaves;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	[CrossmodContent("ThoriumMod")]
	public class ThoriumViscountQuarterstaff : OrchidModGuardianQuarterstaff
	{
		public int IsCounterAttacking = 0;
		public Vector2 Velocity = Vector2.Zero;
		public const float BatSpeed = 1.5f;

		public override void SafeSetDefaults()
		{
			Item.width = 42;
			Item.height = 42;
			Item.value = Item.sellPrice(0, 0, 40, 0);
			Item.rare = ItemRarityID.Green;
			Item.useTime = 30;
			ParryDuration = 80;
			Item.knockBack = 12f;
			Item.damage = 65;
			CounterSpeed = 0.5f;
			InvincibilityDuration = 80;
			CounterKnockback = 0f;
			CounterHits = 20; // Normal damage is 0.67f for 3 hits = 200% of item damage.
			CounterDamage = 0.125f; // This is 20 hits of 0.125f = 250% of item damage.
		}

		public override void ExtraAIQuarterstaffCounterattacking(Player player, OrchidGuardian guardian, Projectile projectile)
		{
			OrchidPlayer orchidPlayer = player.GetModPlayer<OrchidPlayer>();
			orchidPlayer.HideAllDrawLayers = true; // turns the player invisible while counterattacking
			orchidPlayer.PlayerImmunity = 10; // So the player gets a few additional iframes at the end of the attack
			IsCounterAttacking = 2;

			player.velocity = Velocity;

			// 8 Direction movement controls
			if (player.controlLeft && !player.controlRight)
			{
				if (player.controlUp && !player.controlDown)
				{ // Top Left
					Velocity.X -= BatSpeed * 0.714f;
					Velocity.Y -= BatSpeed * 0.714f;
				}
				else if (!player.controlUp && player.controlDown)
				{ // Bottom Left
					Velocity.X -= BatSpeed * 0.714f;
					Velocity.Y += BatSpeed * 0.714f;
				}
				else
				{ // Left
					Velocity.X -= BatSpeed;
				}
			}
			else if (!player.controlLeft && player.controlRight)
			{
				if (player.controlUp && !player.controlDown)
				{ // Top Right
					Velocity.X += BatSpeed * 0.714f;
					Velocity.Y -= BatSpeed * 0.714f;
				}
				else if (!player.controlUp && player.controlDown)
				{ // Bottom Right
					Velocity.X += BatSpeed * 0.714f;
					Velocity.Y += BatSpeed * 0.714f;
				}
				else
				{ // Right
					Velocity.X += BatSpeed;
				}
			}
			else if (player.controlUp && !player.controlDown)
			{ // Up
				Velocity.Y -= BatSpeed;
			}
			else if (!player.controlUp && player.controlDown)
			{ // Down
				Velocity.Y += BatSpeed;
			}
			else
			{ // No input
				Velocity *= 0.9f;
			}

			if (Velocity.Length() > 6f)
			{
				Velocity = Vector2.Normalize(Velocity) * 6f;
			}

			player.velocity = Velocity;
			player.maxFallSpeed = 0f;
			player.noFallDmg = true;
			player.fallStart = (int)(player.position.Y / 16);
		}

		public override void QuarterstaffModifyHitNPC(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, ref NPC.HitModifiers modifiers, bool jabAttack, bool counterAttack, bool firstHit)
		{
			if (IsCounterAttacking > 0)
			{ // Ignores some defense while counterattacking
				modifiers.Defense.Flat -= 25;
			}
		}

		public override void ExtraAIQuarterstaff(Player player, OrchidGuardian guardian, Projectile projectile)
		{
			if (IsCounterAttacking > 0)
			{
				IsCounterAttacking --;
			}
		}

		public override void OnParryQuarterstaff(Player player, OrchidGuardian guardian, Entity aggressor, Projectile anchor)
		{
			IsCounterAttacking = 2;
			Velocity = player.velocity;

			int projType = ModContent.ProjectileType<ThoriumViscountQuarterstaffProjectile>();
			foreach (Projectile projectile in Main.projectile)
			{ // clears existing bat swarms
				if (projectile.type == projType && projectile.active && projectile.owner == player.whoAmI)
				{
					projectile.Kill();
				}
			}

			Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, player.velocity, projType, 0, 0f, player.whoAmI);
		}

		public override bool PreDrawQuarterstaff(SpriteBatch spriteBatch, Projectile projectile, Player player, ref Color lightColor)
		{ // Hide Quarterstaff while counterattacking
			if (IsCounterAttacking > 0) return false;
			return base.PreDrawQuarterstaff(spriteBatch, projectile, player, ref lightColor);
		}
	}
}
