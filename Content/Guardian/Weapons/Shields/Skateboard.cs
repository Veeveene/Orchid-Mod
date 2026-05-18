using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Assets;
using System;
using Microsoft.Xna.Framework.Input;
using OrchidMod.Common.ModObjects;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	public class Skateboard : OrchidModGuardianShield
	{
		public static readonly SoundStyle SoundTrick = new(OrchidAssets.SoundsPath + "SkateTrick");
		public Texture2D TextureWheels;
		public float playerVelocity;
		public int TimeSpent = 0;
		public int AirTime = 0;

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 2, 0, 0);
			Item.width = 26;
			Item.height = 42;
			Item.UseSound = SoundID.Item1;
			Item.knockBack = 13f;
			Item.damage = 93;
			Item.rare = ItemRarityID.Green;
			Item.useTime = 24;
			distance = 28f;
			slamDistance = 50f;
			blockDuration = 240;
			TextureWheels ??= ModContent.Request<Texture2D>(Texture + "_Wheels", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			ShieldFrames = 4;
			parryRotation = 0f;
		}

		public override void BlockStart(Player player, Projectile shield)
		{
			playerVelocity = 0;
		}

		public override void SlamHitFirst(Player player, Projectile shield, NPC npc, bool WeakSlam)
		{
			Player owner = Main.player[shield.owner];
			if (shield.ModProjectile is GuardianShieldAnchor anchor && !WeakSlam)
			{
				if (anchor.aimedLocation.Y > owner.Center.Y && (Math.Abs(anchor.aimedLocation.X - owner.Center.X) < 48f) && owner.grapCount == 0 && owner.mount.Type == MountID.None)
				{
					owner.velocity.Y -= 10f;
					owner.jump = 0;
					if (owner.velocity.Y > -5) owner.velocity.Y = -5f;
					if (owner.velocity.Y - Math.Abs(owner.velocity.X / 2f) < -15f)
					{
						SoundEngine.PlaySound(SoundTrick, shield.Center);
					}
				}
			}
		}

		public override void PostDrawShield(SpriteBatch spriteBatch, Projectile projectile, Player player, Color lightColor)
		{ // Draw the wheels
			var effect = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			for (int i = -1; i < 2; i+= 2)
			{
				GuardianShieldAnchor anchor = projectile.ModProjectile as GuardianShieldAnchor;
				if (anchor != null && anchor.ShieldAnimFrame % 2 == 0)
				{
					Vector2 drawPosition = projectile.Center - new Vector2(4f, 8f * i).RotatedBy(projectile.rotation) * (anchor.ShieldAnimFrame == 2 ? -1f : 1f) - Main.screenPosition;
					spriteBatch.Draw(TextureWheels, drawPosition, null, lightColor * (projectile.ai[0] > 0f ? 1f : 0.5f), projectile.rotation + TimeSpent * 0.05f, TextureWheels.Size() * 0.5f, projectile.scale, effect, 0f);
				}
				
			}
		}

		public override void ExtraAIShield(Projectile projectile)
		{
			TimeSpent++;
			if (projectile.ai[0] > 0f && projectile.ModProjectile is GuardianShieldAnchor anchor) // is blocking
			{
				Player owner = Main.player[projectile.owner];
				if (playerVelocity != 0f && (owner.velocity.X == 0f || owner.grapCount > 0 && owner.mount.Type != MountID.None)) // Player hit a tile, stop skating
				{
					projectile.ai[0] = 1f;
				}

				// playing is aiming down has no hook or mount, and is falling. Place the shield down and allow skating
				if (anchor.aimedLocation.Y > owner.Center.Y && (Math.Abs(anchor.aimedLocation.X - owner.Center.X) < 32f) && owner.grapCount == 0 && owner.mount.Type == MountID.None && (owner.velocity.Y > 1f || playerVelocity != 0f))
				{
					anchor.aimedLocation = owner.Center.Floor() - new Vector2(projectile.width / 2f, projectile.height / 2f) + Vector2.UnitY * distance;
					projectile.ai[2] = -MathHelper.PiOver2; // networkedrotation
					projectile.rotation = -MathHelper.PiOver2;

					// Collision with the ground, do skating stuff
					Vector2 collision = Collision.TileCollision(owner.position + new Vector2((owner.width - Item.width) * 0.5f, owner.height), Vector2.UnitY * 12f, Item.width, 14, false, owner.controlDown, (int)owner.gravDir);
					if (collision != Vector2.UnitY * 12f)
					{
						owner.fallStart = (int)(owner.position.Y / 16f);
						owner.fallStart2 = (int)(owner.position.Y / 16f);
						owner.position.Y += (collision.Y - 1.7f);
						owner.velocity.X = playerVelocity;
						owner.velocity.Y = 0.1f;

						if (playerVelocity < 0) TimeSpent -= 10;
						else if (playerVelocity > 0) TimeSpent += 9;
						
						// owner.eocDash = 0;

						AirTime = 0;
						anchor.ShieldAnimFrame = 0;
						

						if (Main.rand.NextBool(4)) SoundEngine.PlaySound(SoundID.Item55, projectile.Center);
						//
						// if (AirTime == 0 && Main.keyState.IsKeyDown(Keys.Space))
						// {
						// 	owner.velocity.Y = -8f;
						// 	owner.position.Y -= collision.Y + 1.7f;
						// 	owner.wingTime = 0f;
						// 	owner.eocDash = 0;
						// 	SoundEngine.PlaySound(SoundID.Item32);
						// }
					}
					else
					{
						AirTime++;
						if (AirTime % 4 == 0) anchor.ShieldAnimFrame++;
						if (anchor.ShieldAnimFrame > 3) anchor.ShieldAnimFrame = 0;
						
						if (playerVelocity == 0) SoundEngine.PlaySound(SoundID.Item53, projectile.Center);
						playerVelocity = owner.velocity.X;
						if (playerVelocity < 1f) playerVelocity = 8f * owner.direction;
						if (Math.Abs(playerVelocity) < 8f) playerVelocity = 8f * Math.Sign(playerVelocity);
						owner.velocity.X = playerVelocity;
						
						projectile.ai[0]++;
					}
				}
			}
			else
			{
				AirTime = 0;
				((GuardianShieldAnchor)projectile.ModProjectile).ShieldAnimFrame = 0;
			}
		}
	}
}
