using Microsoft.Xna.Framework;
using OrchidMod.Content.Guardian.Projectiles.Shields;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	public class EnchantedPavise : OrchidModGuardianShield
	{

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.width = 28;
			Item.height = 38;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item1;
			Item.knockBack = 7f;
			Item.damage = 51;
			Item.rare = ItemRarityID.Blue;
			Item.useTime = 22;
			distance = 40f;
			slamDistance = 55f;
			blockDuration = 100;
			shouldFlip = true;
		}

		public override void Slam(Player player, Projectile shield, bool WeakSlam)
		{
			if (IsLocalPlayer(player) && !WeakSlam)
			{
				Projectile anchor = GetAnchor(player).Projectile;
				int type = ModContent.ProjectileType<EnchantedPaviseProj>();
				Vector2 dir = Vector2.Normalize(Main.MouseWorld - player.Center) * 0.1f;
				Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), anchor.Center, dir, type, (int)(shield.damage * 0.6f), Item.knockBack, player.whoAmI);
				newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
			}
		}
	}
}
