using Microsoft.Xna.Framework;
using OrchidMod.Common.ModObjects;
using OrchidMod.Content.Guardian.Projectiles.Katars;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Katars
{
	public class ThoriumGraniteKatar : OrchidModGuardianKatar
	{
		int toSpawn = 0;
		int spawnDelay = 0;

		public override void SafeSetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.knockBack = 7f;
			Item.damage = 67;
			Item.value = Item.sellPrice(0, 0, 50, 0);
			Item.rare = ItemRarityID.Orange;
			Item.useTime = 20;
			JabVelocity = 20f;
			ParryDuration = 10;
			ParryDashSpeed = 25f;
			ParryDashMomentum = 0.28f;
		}

		public override Color GetColor()
		{
			return new Color(109, 221, 255);
		}

		public override void OnHitParry(Player player, OrchidGuardian guardian, NPC target, Projectile projectile)
		{
			if (OrchidModProjectile.IsValidTarget(target))
			{
				toSpawn += 2;
			}
		}

		public override void ExtraAIKatar(Player player, OrchidGuardian guardian, Projectile anchor, bool offHandKatar)
		{
			if (toSpawn > 0 && spawnDelay <= 0)
			{
				toSpawn--;
				spawnDelay = 15;
				int count = 0;
				int lowestTimeLeft = 630;
				Projectile lowestTimeLeftProjectile = null;
				int projectileType = ModContent.ProjectileType<ThoriumGraniteKatarProjectile>();

				foreach (Projectile katarOrb in Main.projectile)
				{
					if (katarOrb.type == projectileType && katarOrb.owner == player.whoAmI && katarOrb.active && (katarOrb.ai[2] < 0f || katarOrb.ai[1] > 1f))
					{
						count++;

						if (katarOrb.timeLeft < lowestTimeLeft)
						{
							lowestTimeLeft = katarOrb.timeLeft;
							lowestTimeLeftProjectile = katarOrb;
						}
					}
				}

				if (count < 6)
				{
					int damage = guardian.GetGuardianDamage(Item.damage * 0.33f);
					Projectile newProjectile = Projectile.NewProjectileDirect(guardian.Player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, projectileType, damage, 0f, guardian.Player.whoAmI);
					newProjectile.CritChance = guardian.GetGuardianCrit(Item.crit);
					newProjectile.netUpdate = true;
				}
				else
				{
					toSpawn = 0;
				}
			}
			spawnDelay--;
		}

		public override void OnHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool fullyCharged)
		{
			if (fullyCharged)
			{
				int projectileType = ModContent.ProjectileType<ThoriumGraniteKatarProjectile>();
				foreach (Projectile proj in Main.projectile)
				{
					if (proj.active && proj.type == projectileType && proj.owner == player.whoAmI)
					{
						proj.ai[1] = Main.rand.Next(5, 25);
						proj.ai[2] = target.whoAmI;
						proj.netUpdate = true;
					}
				}
			}
		}
	}
}
