using Microsoft.Xna.Framework;
using OrchidMod.Content.Guardian.Projectiles.Shields;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	public class BijouShield : OrchidModGuardianShield
	{

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 10, 0, 0);
			Item.width = 28;
			Item.height = 34;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item101;
			Item.knockBack = 6f;
			Item.damage = 120;
			Item.rare = ItemRarityID.Pink;
			Item.useTime = 28;
			distance = 40f;
			slamDistance = 90f;
			blockDuration = 200;
		}

		public override void Slam(Player player, Projectile shield, bool WeakSlam)
		{
			if (IsLocalPlayer(player) && !WeakSlam)
			{
				Projectile anchor = GetAnchor(player).Projectile;
				int type = ModContent.ProjectileType<BijouShieldProj>();
				Vector2 dir = Vector2.Normalize(Main.MouseWorld - player.Center) * 5f;
				Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), anchor.Center, dir, type, (int)(shield.damage), Item.knockBack, player.whoAmI);
				newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
			}
		}
	}
}
