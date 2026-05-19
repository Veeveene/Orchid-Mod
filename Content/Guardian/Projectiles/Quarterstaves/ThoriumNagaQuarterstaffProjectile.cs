using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using System;
using OrchidMod.Common;

namespace OrchidMod.Content.Guardian.Projectiles.Quarterstaves
{
	public class ThoriumNagaQuarterstaffProjectile : OrchidModGuardianProjectile
	{
		public override string Texture => $"Terraria/Images/Projectile_27";

		public override void SafeSetDefaults()
		{
			Projectile.width = 15;
			Projectile.height = 15;
			Projectile.timeLeft = 100;
			Projectile.scale = 1f;
			Projectile.penetrate = -1;
			Projectile.alpha = 255;
			Projectile.extraUpdates = 1;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 20;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			if (Projectile.timeLeft == 10)
			{
				Projectile.friendly = true;
				Projectile.velocity = Vector2.Zero;
				Projectile.extraUpdates = 0;
				SoundEngine.PlaySound(SoundID.Item21.WithPitchOffset(Strong ? -0.2f : 0.4f), Projectile.Center);
				float dustRot = Main.rand.NextFloat(MathHelper.TwoPi);
				bool ccw = Main.rand.NextBool();
				for (int i = 0; i < 3; i++)
				{
					float forkRot = dustRot + (MathHelper.TwoPi * i / 3);
					for (int j = Strong ? 75 : 45; j > 0; j--)
					{
						Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GreenFairy, newColor: Color.Navy);
						dust.velocity = Vector2.UnitX.RotatedBy(forkRot) * (j * 0.18f);
						dust.scale *= (Strong ? 2f : 1.5f) - j * 0.015f;
						dust.noGravity = true;
						if (ccw) forkRot -= 0.2f - j * 0.001f;
						else forkRot += 0.2f - j * 0.001f;
						dust.alpha = 127;
					}
				}
				for (int i = Strong ? 30 : 15; i > 0; i--)
				{
					Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.UltraBrightTorch);
					dust.scale *= 0.5f + Main.rand.NextFloat(Strong ? 0.8f : 0.4f);
					dust.velocity.Y *= 0.5f;
					dust.velocity.X *= 0.8f;
				}
			}
			else if (Projectile.timeLeft > 10)
			{
				Projectile.velocity *= 0.95f;
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GreenFairy, newColor: Color.Navy);
				dust.velocity = Vector2.UnitY.RotatedBy(Projectile.velocity.ToRotation()) * 0.4f * dust.velocity.Length() * (float)Math.Sin(Projectile.timeLeft / 2f);
				dust.noGravity = true;
				dust.alpha = 127;
			}

			if (Owner == Main.LocalPlayer)
			{
				bool attackInput = Main.mouseLeft && Main.mouseLeftRelease;
                if (ModContent.GetInstance<OrchidClientConfig>().GuardianSwapGauntletImputs) attackInput = Main.mouseRight && Main.mouseRightRelease;

				if (Projectile.timeLeft > 11 && attackInput) 
				{
					Projectile.timeLeft = 11;
					Strong = false;
				}
			}
		}

		public override void ModifyDamageHitbox(ref Rectangle hitbox)
		{
			int size = Strong ? 50 : 20;
			hitbox.X -= size;
			hitbox.Y -= size;
			hitbox.Width += size * 2;
			hitbox.Height += size * 2;
		}
	}
}