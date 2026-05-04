using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Content.Guardian.Weapons.Katars;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Projectiles.Katars
{
	public class ThoriumGraniteKatarProjectile : OrchidModGuardianProjectile
	{
		int TimeSpent = 0;

		public override void SafeSetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = false;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 36000;
			Projectile.scale = 1f;
			Projectile.penetrate = 5;
			Projectile.alpha = 255;
			Projectile.tileCollide = false;
			Projectile.netImportant = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
		}

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidGuardian guardian)
		{
			Projectile.localAI[1] = 20f;
			SoundEngine.PlaySound(SoundID.DD2_LightningBugZap.WithPitchOffset(Main.rand.NextFloat(-0.5f, -0.8f)).WithVolumeScale(0.3f), Projectile.Center);

			for (int i = 0; i < 5; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch);
				dust.scale *= Main.rand.NextFloat(1.5f, 2f);
				dust.velocity *= 1.25f;
				dust.velocity += Projectile.velocity;
				dust.noGravity = true;
			}
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 5; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch);
				dust.scale *= Main.rand.NextFloat(1.5f, 2f);
				dust.velocity *= 1.25f;
				dust.noGravity = true;
			}
		}

		public override void AI()
		{
			TimeSpent++;
			Projectile.localAI[1]--;
			Projectile.rotation += Projectile.localAI[0];

			if (Owner.HeldItem.type != ModContent.ItemType<ThoriumGraniteKatar>())
			{
				Projectile.Kill();
			}

			if (!Initialized)
			{
				Initialized = true;
				Projectile.frame = Main.rand.Next(3);
				Projectile.ai[2] = -1f;
				Projectile.localAI[0] = Main.rand.NextBool() ? -0.3f : 0.3f;
				Projectile.netUpdate = true;
				SoundEngine.PlaySound(SoundID.DD2_LightningBugZap.WithPitchOffset(Main.rand.NextFloat(0.3f, 0.5f)), Projectile.Center);

				for (int i = 0; i < 5; i++)
				{
					Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch);
					dust.scale *= Main.rand.NextFloat(1.5f, 2f);
					dust.velocity *= 1.25f;
					dust.noGravity = true;
				}
			}

			if (Main.rand.NextBool(10))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch);
				dust.velocity *= 0.5f;
				dust.scale *= Main.rand.NextFloat(1f, 1.5f);
				dust.noGravity = true;
			}

			if (Projectile.ai[2] < 0f || Projectile.ai[1] > 1f)
			{ // Hovering above the player
				int count = 0; // nb of more recent projectiles
				int countTotal = 0; // nb of other projectiles
				int highestTimeSpent = 0;
				foreach (Projectile projectile in Main.projectile)
				{
					if (projectile.active && projectile.type == Type && Projectile.owner == projectile.owner && (projectile.ai[2] < 0f || projectile.ai[1] > 1f))
					{
						countTotal++;

						if (projectile.ModProjectile is ThoriumGraniteKatarProjectile katarProj)
						{
							if (katarProj.TimeSpent < TimeSpent)
							{
								count++;
							}

							if (katarProj.TimeSpent > highestTimeSpent)
							{
								highestTimeSpent = katarProj.TimeSpent;
							}
						}
					}
				}

				Player owner = Owner;
				Vector2 targetPosition = owner.Center - Vector2.UnitY.RotatedBy(highestTimeSpent * -0.02f + (MathHelper.TwoPi / countTotal) * count) * (2f + Math.Max(owner.width, owner.height));
				Projectile.velocity = (targetPosition - Projectile.Center) * 0.1f + owner.velocity;

				if (Projectile.ai[1] > 1f)
				{
					Projectile.ai[1]--;
				}
			}
			else if (Projectile.localAI[1] <= 0f)
			{ // homing
				Projectile.friendly = true;
				if (Projectile.ai[1] != 0f)
				{
					SoundEngine.PlaySound(SoundID.DD2_LightningBugZap.WithPitchOffset(Main.rand.NextFloat(0.5f, 0.8f)), Projectile.Center);
					Projectile.ai[1] = 0f;
					Projectile.timeLeft = 600;

					for (int i = 0; i < 5; i ++)
					{
						Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch);
						dust.scale *= Main.rand.NextFloat(1.5f, 2f);
						dust.velocity *= 1.25f;
						dust.noGravity = true;
					}
				}

				NPC target = Main.npc[(int)Projectile.ai[2]];

				if (IsValidTarget(target))
				{
					Vector2 newVelocity = Vector2.Normalize(target.Center - Projectile.Center) * 0.8f;
					Projectile.velocity = Projectile.velocity * 0.92f + newVelocity;
				}
				else
				{
					NPC closestTarget = null;
					float distanceClosest = 1600f;
					foreach (NPC npc in Main.npc)
					{
						float distance = Projectile.Center.Distance(npc.Center);
						if (IsValidTarget(npc) && distance < distanceClosest)
						{
							closestTarget = npc;
							distanceClosest = distance;
						}
					}

					if (closestTarget != null)
					{
						Projectile.ai[2] = closestTarget.whoAmI;
						Projectile.ai[1] = 1f;
						Projectile.netUpdate = true;
					}
					else
					{
						Projectile.Kill();
					}
				}
			}
			else
			{
				if (Projectile.velocity.Length() > 10f)
				{
					Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 10f;
				}

				Projectile.velocity *= 0.9f;
			}
		}

		public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
		{
			float sineOffset = (float)Math.Sin(Projectile.timeLeft * 0.05f);
			Texture2D projTexture = TextureAssets.Projectile[Projectile.type].Value;
			float colorMult = 1f;
			if (Projectile.timeLeft < 20f) colorMult *= Projectile.timeLeft / 20f;

			Vector2 drawPosition = Projectile.Center - Main.screenPosition;
			drawPosition.X += sineOffset * 3f;

			Rectangle drawRectangle = projTexture.Bounds;
			drawRectangle.Height /= 3;
			drawRectangle.Y = Projectile.frame * drawRectangle.Height;

			spriteBatch.Draw(projTexture, drawPosition, drawRectangle, Color.White * colorMult, Projectile.rotation, drawRectangle.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}