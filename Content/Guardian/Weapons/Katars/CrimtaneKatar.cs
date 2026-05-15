using Microsoft.Xna.Framework;
using OrchidMod.Common.ModObjects;
using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Katars
{
	public class CrimtaneKatar : OrchidModGuardianKatar
	{
		public int count = 0;

		public override void SafeSetDefaults()
		{
			Item.width = 32;
			Item.height = 34;
			Item.knockBack = 6f;
			Item.damage = 53;
			Item.value = Item.sellPrice(0, 0, 27, 0);
			Item.rare = ItemRarityID.Blue;
			Item.useTime = 30;
			JabVelocity = 16f;
			ParryDuration = 10;
		}

		public override Color GetColor()
		{
			return new Color(237, 28, 36);
		}

		public override void OnHitParry(Player player, OrchidGuardian guardian, NPC target, Projectile projectile)
		{
			if (OrchidModProjectile.IsValidTarget(target))
			{
				if (count < 3)
				{ // Recovers up to 0.5 guard
					guardian.GuardianGuardRecharging += 0.167f;
				}
				count++;
			}
		}

		public override bool PreGuard(Player player, OrchidGuardian guardian, Projectile anchor)
		{
			count = 0;
			return base.PreGuard(player, guardian, anchor);
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.Anvils);
			recipe.AddIngredient(ItemID.CrimtaneBar, 10);
			recipe.Register();
		}
	}
}
