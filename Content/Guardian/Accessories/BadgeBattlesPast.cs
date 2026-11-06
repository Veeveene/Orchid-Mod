using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Accessories
{
	public class BadgeBattlesPast : OrchidModGuardianEquipable
	{
		float damageIncrease = 0f;
		int timer = 0;

		public override void SafeSetDefaults()
		{
			Item.width = 30;
			Item.height = 30;
			Item.value = Item.sellPrice(0, 1, 50, 0);
			Item.rare = ItemRarityID.Green;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			int projectileType = ModContent.ProjectileType<GuardianShieldAnchor>();
			if (player.ownedProjectileCounts[projectileType] > 0)
			{
				OrchidGuardian guardian = Guardian(player);
				Projectile proj = Main.projectile.FirstOrDefault(i => i.active && i.owner == player.whoAmI && i.type == projectileType);
				if (proj.localAI[0] - proj.ai[0] < 2)
				{
					damageIncrease = 0f;
					timer = 0;
				}

				if (proj.ai[0] > 0)
				{
					if (damageIncrease < 1f)
					{
						damageIncrease += 0.008f;

						if (damageIncrease > 1f)
						{
							damageIncrease = 1f;
						}
					}
					timer = 30;
				}
				else if (timer > 0)
				{
					timer--;
					if (timer <= 0 || proj.ai[1] > 0)
					{
						damageIncrease = 0f;
						timer = 0;
					}
				}
				player.GetDamage<GuardianDamageClass>() += damageIncrease / (1 + guardian.GuardianBronzeShieldDamage);
			}
			else
			{
				timer = 0;
				damageIncrease = 0f;
			}
		}
	}
}