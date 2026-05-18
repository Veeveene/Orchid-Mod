using Microsoft.Xna.Framework;
using OrchidMod.Content.Guardian.Projectiles.Shields;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	public class HallowedShield : OrchidModGuardianShield
	{
		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 4, 60, 0);
			Item.width = 38;
			Item.height = 46;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item28;
			Item.knockBack = 6f;
			Item.damage = 210;
			Item.rare = ItemRarityID.Pink;
			Item.useTime = 29;
			distance = 60f;
			slamDistance = 125f;
			blockDuration = 200;
		}

		public override void Slam(Player player, Projectile shield, bool WeakSlam)
		{
			if (IsLocalPlayer(player) && !WeakSlam)
			{
				Projectile anchor = GetAnchor(player).Projectile;
				int type = ModContent.ProjectileType<HallowedShieldProj>();
				Vector2 dir = Vector2.Normalize(Main.MouseWorld - player.Center).RotatedByRandom(MathHelper.ToRadians(5f)) * (8f + Main.rand.NextFloat(4f));
				Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), anchor.Center, dir, type, (int)(shield.damage * 0.5f), Item.knockBack, player.whoAmI);
				newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
			}
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.AddIngredient(ItemID.HallowedBar, 12);
			recipe.Register();
		}
	}
}
