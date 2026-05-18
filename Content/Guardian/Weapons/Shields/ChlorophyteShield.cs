using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	public class ChlorophyteShield : OrchidModGuardianShield
	{
		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 5, 52, 0);
			Item.width = 38;
			Item.height = 46;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item1;
			Item.knockBack = 6f;
			Item.damage = 258;
			Item.rare = ItemRarityID.Lime;
			Item.useTime = 28;
			distance = 60f;
			slamDistance = 130f;
			blockDuration = 220;
		}

		public override void Slam(Player player, Projectile shield, bool WeakSlam)
		{
			if (IsLocalPlayer(player) && !WeakSlam)
			{
				Projectile anchor = GetAnchor(player).Projectile;
				int type = ProjectileID.SporeCloud;
				for (int i = 0; i < 1 + Main.rand.Next(3); i++)
				{
					Vector2 dir = Vector2.Normalize(Main.MouseWorld - player.Center).RotatedByRandom(MathHelper.ToRadians(10f)) * (8f + Main.rand.NextFloat(4f));
					Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), anchor.Center, dir, type, (int)(shield.damage * 0.4f), Item.knockBack, player.whoAmI);
					newProjectile.DamageType = ModContent.GetInstance<GuardianDamageClass>();
					newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
				}
			}
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.AddIngredient(ItemID.ChlorophyteBar, 12);
			recipe.Register();
		}
	}
}
