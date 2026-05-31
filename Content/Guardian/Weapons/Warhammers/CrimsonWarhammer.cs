using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
	public class CrimsonWarhammer : OrchidModGuardianHammer
	{

		public override void SafeSetDefaults()
		{
			Item.width = 36;
			Item.height = 36;
			Item.value = Item.sellPrice(0, 0, 54, 0);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.DD2_MonkStaffSwing;
			Item.knockBack = 9f;
			Item.shootSpeed = 10f;
			Item.damage = 124;
			Item.useTime = 28;
			Range = 28;
			GuardStacks = 1;
			SwingSpeed = 1.5f;
			ReturnSpeed = 0.9f;
			BlockDuration = 225;
		}

		public override bool ThrowAI(Player player, OrchidGuardian guardian, Projectile projectile, bool weak, bool OffHand)
		{

			if (Main.rand.NextBool(5))
			{
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Crimstone, Scale: Main.rand.NextFloat(0.8f, 1f));
				dust.velocity = dust.velocity * 0.25f + projectile.velocity * 0.2f;
				dust.noGravity = true;
			}

			return true;
		}

		public override void OnThrowHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak, bool OffHand)
		{
			if (!Weak) guardian.modPlayer.TryHeal(5);
		}

		public override void OnMeleeHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged, bool OffHand)
		{
			guardian.modPlayer.TryHeal(2);
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.Anvils);
			recipe.AddIngredient(ItemID.CrimtaneBar, 10);
			recipe.AddIngredient(ItemID.TissueSample, 5);
			recipe.Register();
		}
	}
}
