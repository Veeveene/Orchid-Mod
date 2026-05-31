using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
	public class ChlorophyteWarhammer : OrchidModGuardianHammer
	{
		public override void SafeSetDefaults()
		{
			Item.width = 44;
			Item.height = 44;
			Item.value = Item.sellPrice(0, 5, 52, 0);
			Item.rare = ItemRarityID.Lime;
			Item.UseSound = SoundID.DD2_MonkStaffSwing;
			Item.knockBack = 10f;
			Item.shootSpeed = 13f;
			Item.damage = 228;
			Item.useTime = 25;
			Range = 45;
			GuardStacks = 2;
			SlamStacks = 1;
			SwingChargeGain = 1.5f;
			ReturnSpeed = 1.5f;
			BlockDuration = 300;
		}

		public override void OnThrowHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak, bool OffHand)
		{
			if (!Weak && IsLocalPlayer(player))
			{
				for (int i = 0; i < 6; i++)
				{
					Vector2 dir = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 6f * i).RotatedByRandom(MathHelper.ToRadians(15f)) * (2f + Main.rand.NextFloat(6f));
					Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), projectile.Center, dir, ProjectileID.SporeCloud, (int)(projectile.damage * 0.5f), Item.knockBack, player.whoAmI);
					newProjectile.DamageType = ModContent.GetInstance<GuardianDamageClass>();
					newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
				}
			}
		}

		public override void OnMeleeHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged, bool OffHand)
		{
			if (IsLocalPlayer(player))
			{
				for (int i = 0; i < 3; i++)
				{
					Vector2 dir = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 3f * i).RotatedByRandom(MathHelper.ToRadians(15f)) * (1f + Main.rand.NextFloat(4f));
					Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), projectile.Center, dir, ProjectileID.SporeCloud, (int)(projectile.damage * 0.5f), Item.knockBack, player.whoAmI);
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
