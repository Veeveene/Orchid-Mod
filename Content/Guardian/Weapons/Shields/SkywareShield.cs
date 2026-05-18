using Microsoft.Xna.Framework;
using OrchidMod.Content.Guardian.Projectiles.Shields;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	public class SkywareShield : OrchidModGuardianShield
	{

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.width = 30;
			Item.height = 38;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item9;
			Item.knockBack = 7f;
			Item.damage = 76;
			Item.rare = ItemRarityID.Green;
			Item.useTime = 25;
			Item.shootSpeed = 8f;
			distance = 45f;
			slamDistance = 65f;
			blockDuration = 120;
		}

		public override void Slam(Player player, Projectile shield, bool WeakSlam)
		{
			if (IsLocalPlayer(player) && !WeakSlam)
			{
				Projectile anchor = GetAnchor(player).Projectile;
				int type = ModContent.ProjectileType<SkywareShieldProj>();
				for (int i = 0; i < 2 + Main.rand.Next(2); i++)
				{
					Vector2 dir = Vector2.Normalize(Main.MouseWorld - player.Center).RotatedByRandom(MathHelper.ToRadians(20f)) * Item.shootSpeed;
					Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), anchor.Center + Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(16f), dir, type, (int)(shield.damage * 0.7f), Item.knockBack, player.whoAmI);
					newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
				}
			}
		}
	}
}
