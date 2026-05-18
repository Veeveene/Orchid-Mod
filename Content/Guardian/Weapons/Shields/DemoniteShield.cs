using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using OrchidMod.Content.Guardian.Projectiles.Shields;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	public class DemoniteShield : OrchidModGuardianShield
	{

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 0, 54, 0);
			Item.width = 30;
			Item.height = 38;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item60.WithPitchOffset(0.2f).WithVolumeScale(0.7f);
			Item.knockBack = 8f;
			Item.damage = 116;
			Item.shootSpeed = 4.5f;
			Item.rare = ItemRarityID.Blue;
			Item.useTime = 30;
			distance = 45f;
			slamDistance = 60f;
			blockDuration = 160;
			shouldFlip = true;
		}

		public override void Slam(Player player, Projectile shield, bool WeakSlam)
		{
			if (!WeakSlam)
			{
				Vector2 dir = Vector2.Zero;
				if (Main.MouseWorld != player.Center) dir = Vector2.Normalize(Main.MouseWorld - player.Center) * Item.shootSpeed;
				if (IsLocalPlayer(player))
				{
					OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
					Projectile anchor = GetAnchor(player).Projectile;
					int type = ModContent.ProjectileType<DemoniteShieldProjectile>();
					Projectile projectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), anchor.Center, dir, type, guardian.GetGuardianDamage(Item.damage), Item.knockBack, player.whoAmI);
					projectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
				}
				for (int i = 0; i < 10; i++)
				{
					Dust.NewDustDirect(shield.position, shield.width, shield.height, DustID.Demonite, 0f, 0f, 200, default, 1.2f).velocity += dir;
				}
			}
		}

		public override void ExtraAIShield(Projectile projectile)
		{
			if (projectile.ai[0] > 0 && Main.rand.NextBool(15))
				Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Demonite, 0f, 0f, 200, default, 1.2f);
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.Anvils);
			recipe.AddIngredient(ItemID.DemoniteBar, 10);
			recipe.AddIngredient(ItemID.ShadowScale, 5);
			recipe.Register();
		}
	}
}
