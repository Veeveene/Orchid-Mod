using Microsoft.Xna.Framework;
using OrchidMod.Content.Guardian.Projectiles.Shields;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	public class NightShield : OrchidModGuardianShield
	{

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 2, 50, 0);
			Item.width = 34;
			Item.height = 42;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item73;
			Item.knockBack = 8f;
			Item.damage = 108;
			Item.rare = ItemRarityID.Orange;
			Item.useTime = 38;
			distance = 45f;
			slamDistance = 150f;
			blockDuration = 180;
		}

		public override void Slam(Player player, Projectile shield, bool WeakSlam)
		{
			if (IsLocalPlayer(player) && !WeakSlam)
			{
				Projectile anchor = GetAnchor(player).Projectile;
				int type = ModContent.ProjectileType<NightShieldProjAlt>();
				Vector2 dir = Vector2.Normalize(Main.MouseWorld - player.Center) * 10f;
				Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), anchor.Center, dir, type, (int)(shield.damage * 0.6f), Item.knockBack, player.whoAmI);
				newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
			}
		}
	}
}
