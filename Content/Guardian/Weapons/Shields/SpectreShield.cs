using Microsoft.Xna.Framework;
using OrchidMod.Content.Guardian.Projectiles.Shields;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	public class SpectreShield : OrchidModGuardianShield
	{
		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 6, 0, 0);
			Item.width = 38;
			Item.height = 46;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item88;
			Item.knockBack = 6f;
			Item.damage = 274;
			Item.rare = ItemRarityID.Yellow;
			Item.useTime = 26;
			Item.shootSpeed = 13f;
			distance = 65f;
			slamDistance = 140f;
			blockDuration = 240;
		}

		public override void Slam(Player player, Projectile shield, bool WeakSlam)
		{
			if (IsLocalPlayer(player) && !WeakSlam)
			{
				Projectile anchor = GetAnchor(player).Projectile;
				int type = ModContent.ProjectileType<SpectreShieldProj>();
				for (int i = 0; i < 3 + Main.rand.Next(5); i++)
				{
					Vector2 dir = Vector2.Normalize(Main.MouseWorld - player.Center).RotatedByRandom(MathHelper.ToRadians(30f)) * Item.shootSpeed;
					Projectile.NewProjectile(Item.GetSource_FromThis(), anchor.Center + Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(16f), dir, type, (int)(shield.damage * 0.4f), Item.knockBack, player.whoAmI);
				}
			}
		}

		public override void Reflect(Player player, Projectile shield, Projectile projectile, ref int GuardianShieldSpikeReflect)
		{
			if (IsLocalPlayer(player) && GuardianShieldSpikeReflect > 0)
			{
				Projectile anchor = GetAnchor(player).Projectile;
				int type = ModContent.ProjectileType<SpectreShieldProj>();
				Vector2 dir = Vector2.Normalize(Main.MouseWorld - player.Center).RotatedByRandom(MathHelper.ToRadians(30f)) * Item.shootSpeed;
				Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), anchor.Center + Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(16f), dir, type, (int)(shield.damage * 0.4f), Item.knockBack, player.whoAmI);
				newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
			}
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.AddIngredient(ItemID.SpectreBar, 18);
			recipe.Register();
		}
	}
}
