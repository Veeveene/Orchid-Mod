using OrchidMod.Content.Guardian.Projectiles.Misc;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Misc
{
	public class ConsolariaLepusEggNest : OrchidModGuardianHammer, IGuardianCustomUIItem
	{
		string IGuardianCustomUIItem.TextureName => "EggBasket";
		
		public override void SafeSetDefaults()
		{
			Item.width = 34;
			Item.height = 22;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.DD2_MonkStaffSwing;
			Item.knockBack = 5f;
			Item.shootSpeed = 13f;
			Item.useTime = 30;
			Item.damage = 60;
			WaitChargeGain = 2f;
			Range = 20;
			CannotSwing = true;
			CannotBlock = true;
			hasSpecialHammerTexture = true;
			HoldOffset = -6f;
		}

		public override void OnThrow(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak, bool OffHand)
		{
			int projectileType = ModContent.ProjectileType<ConsolariaLepusEggNestProjectile>();
			Projectile newProjectile = Projectile.NewProjectileDirect(projectile.GetSource_FromAI(), projectile.Center, projectile.velocity, projectileType, projectile.damage, projectile.knockBack, projectile.owner);
			newProjectile.CritChance = projectile.CritChance;
			newProjectile.ai[1] = Weak ? 0f : 1f;
			projectile.Kill();
		}
	}
}
