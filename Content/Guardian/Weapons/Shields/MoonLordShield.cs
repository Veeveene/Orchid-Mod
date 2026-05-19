using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Utilities;
using OrchidMod.Common.ModObjects;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	public class MoonLordShield : OrchidModGuardianShield
	{
		public float playerVelocity;
		public float originalHeight;
		public int TimeSpent;

		public Color Color => new Color((byte)(Main.DiscoR / 1.25f), (byte)(Main.DiscoG / 1.25f), (byte)(Main.DiscoB / 1.25f), 200);

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 10, 0, 0);
			Item.width = 40;
			Item.height = 52;
			Item.UseSound = SoundID.Item115;
			Item.knockBack = 6f;
			Item.damage = 1337;
			Item.rare = ItemRarityID.Red;
			Item.useTime = 24;
			distance = 26f;
			slamDistance = 150f;
			blockDuration = 600;
			parryRotation = 0f;
		}

		public override void Slam(Player player, Projectile shield, bool WeakSlam)
		{
			if (!WeakSlam)
			{
				playerVelocity = 0;
				originalHeight = 0;
				Player owner = Main.player[shield.owner];
				if (shield.ModProjectile is GuardianShieldAnchor anchor)
				{
					if (owner.mount.Type == MountID.None)
					{
						OrchidPlayer orchidPlayer = player.GetModPlayer<OrchidPlayer>();
						Vector2 playerDashVelocity = Vector2.UnitY.RotatedBy((player.Center - Main.MouseWorld).ToRotation() - MathHelper.PiOver2) * 20f;
						orchidPlayer.ForcedVelocityVector = playerDashVelocity;
						orchidPlayer.ForcedVelocityTimer = 5;
						orchidPlayer.ForcedVelocityUpkeep = 1f;
					}
				}
			}
		}


		public override void BlockStart(Player player, Projectile shield)
		{
			playerVelocity = 0;
			originalHeight = 0;
			player.velocity *= 0f;
			TimeSpent = 0;
		}

		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
		{
			spriteBatch.DrawSimpleItemGlowmaskInWorld(Item, Color, rotation, scale);
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			 Main.EntitySpriteDraw(ModContent.Request<Texture2D>(Texture + "_Glow").Value, position, frame, Color, 0, new Vector2(frame.Width / 2, frame.Height / 2), scale, SpriteEffects.None);
		}

		public override void ExtraAIShield(Projectile projectile)
		{
			if (projectile.ai[1] > 0f) // is slamming
			{
				projectile.rotation += projectile.ai[1] * 0.20944f; //approximately 4pi / 60, so it rotates 2 times as it decreases from the base value of 60
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.RainbowMk2, newColor: Color);
				dust.noGravity = true;
				dust.velocity *= 0.5f;
				dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.RainbowMk2, newColor: Color);
				dust.noGravity = true;
				dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Smoke, newColor: Color);
				dust.noGravity = true;
				dust.velocity *= 0.5f;
				dust.scale *= Main.rand.NextFloat(1f, 1.5f);
				dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Smoke, newColor: Color);
				dust.noGravity = true;
				dust.scale *= Main.rand.NextFloat(1f, 1.5f);
			}

			if (projectile.ai[0] > 0f && projectile.ModProjectile is GuardianShieldAnchor anchor) // is blocking
			{
				Player owner = Main.player[projectile.owner];
				Vector2 collision = Collision.TileCollision(owner.position + new Vector2((owner.width - Item.width) * 0.5f, owner.height), Vector2.UnitY * 12f, Item.width, 14, false, false, (int)owner.gravDir);
				if (playerVelocity != 0f && ((owner.velocity.X == 0f || owner.grapCount > 0 && owner.mount.Type != MountID.None) || collision != Vector2.UnitY * 12f)) // Player hit a tile, stop skating
				{
					projectile.ai[0] = 1f;
				}

				// playing is aiming down has no hook or mount, and is falling. Place the shield down and allow skating
				if ((anchor.aimedLocation.Y + 8f) > owner.Center.Y && (Math.Abs(anchor.aimedLocation.X - owner.Center.X) < 64f) && owner.grapCount == 0 && owner.mount.Type == MountID.None)
				{
					TimeSpent++;

					/*if (TimeSpent % 60 == 0)
					{
						OrchidGuardian guardian = Main.player[projectile.owner].GetModPlayer<OrchidGuardian>();
						if (TimeSpent % 120 == 0)
						{
							guardian.AddGuard(1);
						}
						else
						{
							guardian.AddSlam(1);
						}
					}*/
					OrchidGuardian guardian = Main.player[projectile.owner].GetModPlayer<OrchidGuardian>();
					//lol 360% increased regeneration
					guardian.GuardianGuardRecharge += 3.6f;
					guardian.GuardianSlamRecharge += 3.6f;

					if (projectile.ai[2] != -MathHelper.PiOver2)
					{
						anchor.aimedLocation = owner.Center.Floor() - new Vector2(projectile.width / 2f, projectile.height / 2f) + Vector2.UnitY * distance;
						projectile.ai[2] = -MathHelper.PiOver2; // networkedrotation
						projectile.rotation = -MathHelper.PiOver2;
						originalHeight = owner.position.Y;
						SoundEngine.PlaySound(SoundID.Item114, projectile.Center);
					}

					owner.velocity.Y = -0.4f;
					if (originalHeight != 0f) owner.position.Y = originalHeight;
					if (TimeSpent % 40 == 1) SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal, projectile.Center);

					if (playerVelocity != 0)
					{
						if (owner.direction < 0 && playerVelocity > 0 || owner.direction > 0 && playerVelocity < 0)
						{
							SoundEngine.PlaySound(SoundID.Item115, projectile.Center);
							playerVelocity *= -1f;
						}

						owner.velocity.Y -= -0.00001f;
						owner.fallStart = (int)(owner.position.Y / 16f);
						owner.fallStart2 = (int)(owner.position.Y / 16f);
						owner.velocity.X = playerVelocity;

						Dust dust = Dust.NewDustDirect(projectile.Center - new Vector2(13, 1), 6, 6, DustID.Smoke, newColor: Color);
						dust.noGravity = true;
						dust.velocity *= 0.5f;
						dust.velocity.Y = 1f;
						dust.scale *= Main.rand.NextFloat(1f, 1.5f);
						dust = Dust.NewDustDirect(projectile.Center + new Vector2(7, 1), 6, 6, DustID.Smoke, newColor: Color);
						dust.noGravity = true;
						dust.velocity *= 0.5f;
						dust.velocity.Y = 1f;
						dust.scale *= Main.rand.NextFloat(1f, 1.5f);

						dust = Dust.NewDustDirect(projectile.Center - new Vector2(13, 1), 6, 6, DustID.RainbowMk2, 0f, 0f, newColor: Color);
						dust.noGravity = true;
						dust.velocity.X = playerVelocity * 0.1f;
						dust.velocity.Y = (float)Math.Sin(TimeSpent * 0.25f) * 5f;
						dust = Dust.NewDustDirect(projectile.Center + new Vector2(7, 1), 6, 6, DustID.RainbowMk2, 0f, 0f, newColor: Color);
						dust.noGravity = true;
						dust.velocity.X = playerVelocity * 0.1f;
						dust.velocity.Y = (float)Math.Sin(TimeSpent * 0.25f) * 5f;

						projectile.rotation = projectile.ai[2] + 0.01f * playerVelocity;
					}
					else
					{
						playerVelocity = owner.velocity.X;
						if (playerVelocity != 0f)
						{
							if (Math.Abs(playerVelocity) < 8f) playerVelocity = 15f * Math.Sign(playerVelocity);
							SoundEngine.PlaySound(SoundID.Item115, projectile.Center);
						}

						Dust dust = Dust.NewDustDirect(projectile.Center - new Vector2(13, 1), 6, 6, DustID.RainbowMk2, 0f, 0f, newColor: Color);
						dust.noGravity = true;
						dust.velocity.X = (float)Math.Sin(TimeSpent * 0.25f) * 5f;
						dust.velocity.Y = 3f;
						dust = Dust.NewDustDirect(projectile.Center + new Vector2(7, 1), 6, 6, DustID.RainbowMk2, 0f, 0f, newColor: Color);
						dust.noGravity = true;
						dust.velocity.X = (float)Math.Sin(TimeSpent * 0.25f) * 5f;
						dust.velocity.Y = 3f;

						if (Main.rand.NextBool(3))
						{

							dust = Dust.NewDustDirect(projectile.Center - new Vector2(13, 1), 6, 6, DustID.Smoke, newColor: Color);
							dust.noGravity = true;
							dust.velocity *= 0.5f;
							dust.velocity.Y = 1f;
							dust.scale *= Main.rand.NextFloat(1f, 1.5f);
							dust = Dust.NewDustDirect(projectile.Center + new Vector2(7, 1), 6, 6, DustID.Smoke, newColor: Color);
							dust.noGravity = true;
							dust.velocity *= 0.5f;
							dust.velocity.Y = 1f;
							dust.scale *= Main.rand.NextFloat(1f, 1.5f);
						}
					}
				}
			}
		}
	}
}
