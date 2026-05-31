using Terraria;
using Terraria.ID;
using OrchidMod.Common.ModObjects;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
	public class HellWarhammer : OrchidModGuardianHammer
	{
		public override void SafeSetDefaults()
		{
			Item.width = 38;
			Item.height = 38;
			Item.value = Item.sellPrice(0, 0, 54, 0);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.DD2_MonkStaffSwing;
			Item.knockBack = 10f;
			Item.shootSpeed = 10f;
			Item.damage = 142;
			Range = 33;
			GuardStacks = 2;
			SwingChargeGain = 1.5f;
			ReturnSpeed = 1f;
			BlockDuration = 180;
		}

		public override bool ThrowAI(Player player, OrchidGuardian guardian, Projectile projectile, bool weak, bool OffHand)
		{
			if (Main.rand.NextBool(3))
			{
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Torch, Scale: Main.rand.NextFloat(1.5f, 2f));
				dust.velocity = dust.velocity * 0.25f + projectile.velocity * 0.2f;
				dust.noGravity = true;
			}
			return true;
		}


		public override void OnThrowHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak, bool OffHand)
		{
			if (!Weak && IsLocalPlayer(player)) OrchidModProjectile.spawnGenericExplosion(projectile, (int)(projectile.damage), 10f, 250, 0, true, true);
			target.AddBuff(BuffID.OnFire, 180);
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.Anvils);
			recipe.AddIngredient(ItemID.HellstoneBar, 20);
			recipe.Register();
		}
	}
}
