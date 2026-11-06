using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Projectiles.Misc
{
	public class StaffRocketProjectile : OrchidModGuardianProjectile
	{
		public override void SafeSetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.friendly = false;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 60;
			Projectile.scale = 1f;
			Projectile.penetrate = 1;
			Projectile.alpha = 255;
		}

		public override void AI()
		{
			Projectile.Center = Owner.MountedCenter;
			Owner.maxFallSpeed = 17f;

			if (!Initialized)
			{
				Initialized = true;
				Owner.RemoveAllGrapplingHooks();
				SoundEngine.PlaySound(SoundID.DD2_KoboldIgnite.WithPitchOffset(-0.1f).WithVolumeScale(1.2f));

				if (Projectile.velocity == Vector2.Zero || Projectile.velocity.HasNaNs())
					Projectile.velocity = Main.rand.NextVector2Unit();
				
				for (int i = 0; i < 3; i++)
				{
					Gore gore = Gore.NewGoreDirect(Owner.GetSource_FromAI(), Projectile.Center + new Vector2(Main.rand.NextFloat(-24f, 0f), Main.rand.NextFloat(-24f, 0f)), Vector2.UnitY.RotatedByRandom(MathHelper.Pi), 61 + Main.rand.Next(3));
					gore.rotation = Main.rand.NextFloat(MathHelper.Pi);
				}

				for (int i = 0; i < 3; i++)
				{
					Gore gore = Gore.NewGoreDirect(Owner.GetSource_FromAI(), Projectile.Center + new Vector2(Main.rand.NextFloat(-24f, 0f), Main.rand.NextFloat(-24f, 0f)), Vector2.UnitY.RotatedByRandom(MathHelper.Pi), 61 + Main.rand.Next(3));
					gore.rotation = Main.rand.NextFloat(MathHelper.Pi);
					gore.velocity = -Vector2.Normalize(Projectile.velocity).RotatedByRandom(MathHelper.ToRadians(20f)) * Main.rand.NextFloat(3f, 6f);
				}
			}

			if (Main.rand.NextBool(Projectile.timeLeft > 40 ? 1 : 4) && Projectile.timeLeft > 15)
			{

				int dustType = Owner.GetModPlayer<OrchidGuardian>().GuardianStaffRocket;

				switch (dustType)
				{
					default: // 1 or incorrect
						dustType = DustID.FireworkFountain_Red;
						break;
					case 2:
						dustType = DustID.FireworkFountain_Green;
						break;
					case 3:
						dustType = DustID.FireworkFountain_Blue;
						break;
					case 4:
						dustType = DustID.FireworkFountain_Yellow;
						break;
				}


				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType);
				dust.velocity = -Vector2.Normalize(Projectile.velocity).RotatedByRandom(MathHelper.ToRadians(20f)) * Main.rand.NextFloat(3f, 6f);
				dust.position += dust.velocity * 5f;
				dust.scale *= Main.rand.NextFloat(0.7f, 1f);

				if (Main.rand.NextBool(10))
				{
					Gore gore = Gore.NewGoreDirect(Owner.GetSource_FromAI(), Projectile.Center + new Vector2(Main.rand.NextFloat(-12f, 0f), Main.rand.NextFloat(-12f, 0f)), Vector2.UnitY.RotatedByRandom(MathHelper.Pi), 61 + Main.rand.Next(3));
					gore.rotation = Main.rand.NextFloat(MathHelper.Pi);
					gore.scale *= Main.rand.NextFloat(0.4f, 0.7f);
				}
			}

			if (Main.rand.NextBool(Projectile.timeLeft > 30 ? 2 : 5))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
				dust.velocity = -Vector2.Normalize(Projectile.velocity).RotatedByRandom(MathHelper.ToRadians(20f)) * Main.rand.NextFloat(4f, 8f);
				dust.position += dust.velocity * 3f;
				dust.scale *= Main.rand.NextFloat(0.8f, 1.3f);
			}
			if (Projectile.ai[0] == 1f)
			{ // stops the owner when hitting (counterattacking) a target
				Projectile.ai[0] = 2f;

				Owner.velocity.X *= 0.2f;
				Owner.velocity.Y *= 0.4f;

				if (Projectile.timeLeft > 30)
				{
					Projectile.timeLeft = 30;
				}
			}

			if (Owner.immune && IsLocalOwner && Projectile.ai[0] == 0f)
			{
				Projectile.ai[0] = 1f;
				Projectile.netUpdate = true;
			}

			if (Projectile.timeLeft > 30)
			{
				Owner.velocity = Vector2.Normalize(Projectile.velocity) * ((60 - Math.Max(Projectile.timeLeft, 40)) / 1.25f) * (1f + (Owner.moveSpeed - 1f) * 0.5f); // 1f > 16f velocity over 30 frames 
				Owner.fallStart = (int)(Owner.position.Y / 16);
			}
		}
	}
}