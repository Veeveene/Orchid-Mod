using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Content.Shapeshifter.Weapons.Symbiote;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Shapeshifter.Projectiles.Symbiote
{
	public class SymbioteToadProj : OrchidModShapeshifterProjectile
	{
		public int Count;
		public float TongueProgress;
		private static Texture2D TextureMain;
		public List<Vector2> OldPosition;
		public List<float> OldRotation;

		public override void SafeSetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 600;
			Projectile.scale = 1f;
			Projectile.penetrate = -1;
			Projectile.alpha = 255;
			TextureMain ??= ModContent.Request<Texture2D>(Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			OldPosition = new List<Vector2>();
			OldRotation = new List<float>();
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Count = 0;
			TongueProgress = 0f;
			MeleeHit = true;
		}

		public override void AI()
		{
			Player player = Owner;
			if (!Initialized)
			{
				Initialized = true;
				OldPosition.Add(Projectile.Center - Owner.Center + Owner.velocity + new Vector2(0f, -1f));
				OldRotation.Add(Projectile.rotation);
				Vector2 initialVelocity = Projectile.velocity;

				int maxCount = 30; // number of segments
				for (int i = 0; i < maxCount; i++)
				{ // Calculates where every segment will be
					Vector2 target = new Vector2(Projectile.ai[0], Projectile.ai[1]);
					Vector2 position = OldPosition[OldPosition.Count - 1];
					Vector2 direction = Vector2.Normalize(target - position);
					Projectile.velocity = Vector2.Normalize(Projectile.velocity * (maxCount - i) + direction * i) * 4f;
					OldPosition.Add(position + Projectile.velocity);
					OldRotation.Add(Projectile.velocity.ToRotation() + MathHelper.PiOver2);

					if ((position + Projectile.velocity).Distance(target) < 4f && OldPosition.Count > maxCount / 2f)
					{
						break;
					}
				}

				Projectile.velocity = Vector2.Normalize(initialVelocity); // so enemies are KB'd in the right direction 
			}

			OrchidShapeshifter shapeshifter = Shapeshifter(player);
			TongueProgress += shapeshifter.GetShapeshifterMeleeSpeed();

			if (shapeshifter.IsShapeshifted)
			{
				if (shapeshifter.Shapeshift is not SymbioteToad)
				{
					Projectile.Kill();
				}
			}
			else
			{
				Projectile.Kill();
			}

			if (TongueProgress < OldPosition.Count)
			{ // Calculated the number of segments that should be displayed, and sets the projectile position accordingly
				Count = (int)(Math.Sin(TongueProgress * (MathHelper.Pi / (float)OldPosition.Count)) * OldPosition.Count);
				
				if (Count >= OldPosition.Count)
				{
					Count = OldPosition.Count - 1;
				}

				Projectile.Center = player.Center + new Vector2(6f * Projectile.ai[2], 0f) + OldPosition[Count];
			}
			else
			{
				Projectile.Kill();
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{ // turn back on tile collide
			if (TongueProgress < OldPosition.Count * 0.5f)
			{
				SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
				TongueProgress = OldPosition.Count - TongueProgress;
				Projectile.netUpdate = true;
			}
			return false;
		}

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidShapeshifter shapeshifter)
		{
			if (Projectile.localAI[0] == 0)
			{ // spawn a fly on the first hit
				Projectile.localAI[0]++;
				int projectileType = ModContent.ProjectileType<SymbioteToadProjAlt>();
				int count = 0; // can't have more than 5 flies on a player
				int countTotal = 0; // can't have more than 10 flies total
				foreach (Projectile projectile in Main.projectile)
				{
					if (projectile.type == projectileType && projectile.active && projectile.owner == player.whoAmI)
					{
						countTotal++;

						if ((int)projectile.ai[0] == player.whoAmI)
						{
							count++;
						}
					}
				}

				if (count < 5 && countTotal < 10)
				{
					Vector2 targetLocation = Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(24f, 64f);
					int damage = shapeshifter.GetShapeshifterDamage(Projectile.damage * 0.67f);
					Vector2 spawnPosition = Projectile.Center + targetLocation;
					Projectile newProjectile = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), spawnPosition, Vector2.Zero, projectileType, damage, 0f, player.whoAmI, player.whoAmI, targetLocation.X, targetLocation.Y);
					newProjectile.CritChance = shapeshifter.GetShapeshifterCrit(Projectile.CritChance);
				}
			}
		}

		public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
		{
			Vector2 drawPosition = Owner.Center + Projectile.Center - Main.screenPosition;
			Rectangle rectangle = TextureMain.Bounds;
			rectangle.Height /= 2;
			rectangle.Y += rectangle.Height;

			for (int i = 0; i <= Count; i++)
			{
				if (i == Count)
				{
					rectangle.Y = 0;
				}

				Vector2 drawPosition2 = Owner.Center + OldPosition[i] - Main.screenPosition;
				spriteBatch.Draw(TextureMain, drawPosition2, rectangle, lightColor, OldRotation[i], rectangle.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0f);
			}

			return false;
		}
	}
}