using Microsoft.Xna.Framework;
using OrchidMod.Content.Guardian.Projectiles.Warhammers;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
	public class DesertWarhammer : OrchidModGuardianHammer
	{

		public override void SafeSetDefaults()
		{
			Item.width = 34;
			Item.height = 36;
			Item.value = Item.sellPrice(0, 0, 30, 0);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.DD2_MonkStaffSwing;
			Item.knockBack = 9f;
			Item.shootSpeed = 12f;
			Item.damage = 109;
			Item.useTime = 26;
			Range = 30;
			GuardStacks = 1;
			ReturnSpeed = 1f;
			SwingChargeGain = 1.5f;
			BlockDuration = 180;
		}

		public override void OnMeleeHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged, bool OffHand)
		{
			BoneBurst(player, guardian, target, projectile, new Vector2(player.direction * 4, 0), true);
		}

		public override void OnThrowHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak, bool OffHand)
		{
			BoneBurst(player, guardian, target, projectile, projectile.velocity, Weak);
		}

		void BoneBurst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, Vector2 hitVelocity, bool Weak)
		{
			Projectile.perIDStaticNPCImmunity[ModContent.ProjectileType<DesertWarhammerProjectile>()][target.whoAmI] = Main.GameUpdateCount + 30;
			int type = ModContent.ProjectileType<DesertWarhammerProjectile>();
			for (int i = Weak ? 2 : 0; i < 3; i++)
			{
				Vector2 velocity = 
					hitVelocity * (0.15f * (i + Main.rand.NextFloat(1, 3)))
					* new Vector2(1, projectile.velocity.Y < 0 ? 1 : 0.5f)
					+ Main.rand.NextVector2Circular(1, 1)
					- new Vector2(0, 0.2f * (Main.rand.NextFloat(2, 4) - i));
				
				if (IsLocalPlayer(player))
					Projectile.NewProjectile(player.GetSource_ItemUse(Item), projectile.Center, velocity, type, guardian.GetGuardianDamage(Item.damage * 0.35f), 1f, player.whoAmI);
				for (int j = 0; j < 2; j++)
				{
					Dust.NewDust(projectile.position, projectile.width, projectile.height, Main.rand.NextBool() ? DustID.Gold : DustID.Dirt, hitVelocity.X * 0.5f, -1);
				}
			}
			SoundEngine.PlaySound(SoundID.DD2_SkeletonHurt.WithPitchOffset(-0.2f), projectile.Center);
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.Anvils);
			recipe.AddIngredient(ItemID.FossilOre, 15);
			recipe.Register();
		}
	}
}
