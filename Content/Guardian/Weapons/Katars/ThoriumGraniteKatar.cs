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
		public override void SafeSetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.knockBack = 7f;
			Item.damage = 63;
			Item.value = Item.sellPrice(0, 0, 50, 0);
			Item.rare = ItemRarityID.Orange;
			Item.useTime = 25;
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

				if (count < 5)
				{
					int damage = guardian.GetGuardianDamage(Item.damage * 0.33f);
					Projectile newProjectile = Projectile.NewProjectileDirect(guardian.Player.GetSource_ItemUse(Item), target.Center, Vector2.Zero, projectileType, damage, 0f, guardian.Player.whoAmI);
					newProjectile.CritChance = guardian.GetGuardianCrit(Item.crit);
					newProjectile.netUpdate = true;
				}
			}
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
