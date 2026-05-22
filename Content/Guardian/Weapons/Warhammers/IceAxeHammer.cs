using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
	public class IceAxeHammer : OrchidModGuardianHammer
	{
		public override void SafeSetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(0, 0, 18, 0);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.knockBack = 1f;
			Item.shootSpeed = 7f;
			Item.damage = 47;
			Item.useTime = 15;
			Range = 40;
			SlamStacks = 1;
			ReturnSpeed = 2f;
			SwingChargeGain = 1.5f;
			HitCooldown = 15;
			Penetrate = true;
			CannotBlock = true;
		}

		public override bool ThrowAI(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak)
		{
			if (projectile.ModProjectile is GuardianHammerAnchor anchor && anchor.range > 0)
			{
				projectile.rotation += projectile.velocity.Length() / 30f * (projectile.velocity.X > 0 ? 1f : -1f) * 1.2f;
				if (anchor.range % 10 == 0)
				{
					SoundEngine.PlaySound(SoundID.Item1.WithPitchOffset(Main.rand.NextFloat(-1f, -0.35f)).WithVolumeScale(0.3f), projectile.Center);
				}
			}

			return base.ThrowAI(player, guardian, projectile, Weak);
		}
	}
}
