using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common.ModObjects;
using OrchidMod.Content.Shapeshifter.Weapons.Symbiote;
using OrchidMod.Utilities;
using System;
using System.IO;
using System.Runtime.Intrinsics.X86;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Shapeshifter.Projectiles.Symbiote
{
	public class SymbioteToadProjAlt : OrchidModShapeshifterProjectile
	{
		private bool ShouldHeal = false;
		private int Timespent = 0;
		private int Frame = 0;
		private int NPCTarget = 0;
		public int LastTargetHealth = 0;
		private static Texture2D TextureMain;
		private static Texture2D TextureGlow;
		private static Texture2D TextureGlow2;

		public override void SafeSetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 300;
			Projectile.tileCollide = false;
			Projectile.scale = 0.7f;
			Projectile.penetrate = -1;
			Projectile.alpha = 255;
			TextureMain ??= ModContent.Request<Texture2D>(Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			TextureGlow ??= ModContent.Request<Texture2D>(Texture + "_Glow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			TextureGlow2 ??= ModContent.Request<Texture2D>(Texture + "_Glow2", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
			Projectile.netImportant = true;
			Timespent = 0;
			Frame = 0;
			NPCTarget = -1;
			ShouldHeal = false;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(ShouldHeal);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			ShouldHeal = reader.ReadBoolean();
		}

		public override bool? CanCutTiles() => false;

		public override bool? CanHitNPC(NPC target)
		{
			if (target.whoAmI != NPCTarget) return false;
			return base.CanHitNPC(target);
		}

		public override void AI()
		{
			// Visuals & Fields updates

			Player player = Owner;
			OrchidShapeshifter shapeshifter = Shapeshifter(player);
			Timespent++;

			if (!Initialized)
			{ // spawn dusts
				Initialized = true;
				for (int i = 0; i < 3; i++)
				{
					Dust dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
					dust2.noGravity = true;
				}

				for (int i = 0; i < 3; i++)
				{
					Dust dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
					dust2.noGravity = true;
				}

				SoundStyle soundStyle = SoundID.Item63;
				soundStyle.Volume *= 0.25f;
				soundStyle.Pitch -= 0.3f;
				SoundEngine.PlaySound(soundStyle, Projectile.Center);
			}
			
			if (Main.rand.NextBool(15))
			{
				Dust dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
				dust2.scale *= Main.rand.NextFloat(0.75f, 1f);
				dust2.noGravity = true;
				dust2.noLightEmittence = true;
			}


			if (Timespent % 3 == 0)
			{
				Frame++;

				if (Frame > 3)
				{
					Frame = 0;
				}
			}

			if (shapeshifter.IsShapeshifted)
			{
				if (shapeshifter.Shapeshift is SymbioteToad toad)
				{
					if (IsLocalOwner)
					{ // random timeleft between 7 and 10 sec so not all flies die at the same time when shaping out
						Projectile.timeLeft = Main.rand.Next(420, 600);
					}
					else
					{ // Timeleft isn't synced, so other clients should have a bigger timeleft to make sure they don't see flies die locally
						Projectile.timeLeft = 600;
					}
				}
			}

			if (Projectile.localAI[0] > 0f)
			{ // The fly was flagged for kill because its target player took damage, kill it after a short delay
				Projectile.localAI[0]++;

				if (Projectile.localAI[0] >= 30f)
				{
					Projectile.Kill();
				}
			}

			Lighting.AddLight(Projectile.Center, 0.25f, 0.125f, 0f);
			Projectile.rotation = Projectile.velocity.X * 0.01f;

			// Targetting System

			Player targetedPlayer = Main.player[(int)Projectile.ai[0]];

			if (!targetedPlayer.dead && targetedPlayer.active)
			{
				if (IsLocalOwner)
				{ // the projectile owner keeps traps of other players health, and kills the flies accordingly when they take damage
					if (LastTargetHealth > targetedPlayer.statLife + 10)
					{ // the targetedPlayer took at least 10 damage since last frame, kill two flies
						int count = 0;

						foreach (Projectile projectile in Main.projectile)
						{
							if (projectile.type == Projectile.type && (int)projectile.ai[0] == targetedPlayer.whoAmI && projectile.active && projectile.localAI[0] != 0 && projectile.localAI[0] < 12f && projectile.owner == player.whoAmI)
							{
								count++;
								if (count >= 2)
								{
									break;
								}
							}
						}

						if (count < 2)
						{ // the fly is locally flagged for kil (sad!)
							Projectile.localAI[0] = 1f + count * 10;
							ShouldHeal = true;
							Projectile.netUpdate = true;
						}
					}
					LastTargetHealth = targetedPlayer.statLife;
				}

				if (Timespent % 40 == 0 && IsLocalOwner)
				{// Flies target locations based on their targeted player position
					float minDistance = 192f; // 12 tiles range
					NPC validTarget = null;

					if (Timespent >= 120)
					{
						foreach (NPC npc in Main.npc)
						{
							float distance = npc.Center.Distance(targetedPlayer.Center);
							if (IsValidTarget(npc) && distance < minDistance + npc.width && Collision.CanHitLine(targetedPlayer.Center, 2, 2, npc.position, npc.width, npc.height))
							{
								validTarget = npc;
								minDistance = distance;
							}
						}
					}

					if (validTarget != null)
					{ // A valid target has been found & the fly is ready to attack, target it
						NPCTarget = validTarget.whoAmI;
						Vector2 toNPC = validTarget.Center - targetedPlayer.Center;
						Vector2 newTargetLocation = toNPC + validTarget.velocity * 3f;
						Projectile.ai[1] = newTargetLocation.X;
						Projectile.ai[2] = newTargetLocation.Y;
						Projectile.netUpdate = true;
					} // else flies erratically around the targeted player
					else
					{
						Vector2 newTargetLocation = Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * -Main.rand.NextFloat(24f, 64f);
						Projectile.ai[1] = newTargetLocation.X;
						Projectile.ai[2] = newTargetLocation.Y;
						Projectile.netUpdate = true;
					}
				}

				Vector2 targetLocation = targetedPlayer.Center + new Vector2(Projectile.ai[1], Projectile.ai[2] - 16f);
				Vector2 toTarget = targetLocation - Projectile.Center;
				Projectile.velocity = Projectile.velocity * 0.9f + toTarget * 0.01f;
			}
			else if (!player.dead && player.active)
			{ // Projectiles go back to their owner (shapeshifter) if possible
				int count = 0; // can't have more than 5 flies on a player
				foreach(Projectile projectile in Main.projectile)
				{
					if (projectile.type == Projectile.type && (int)projectile.ai[0] == player.whoAmI && projectile.active && projectile.owner == player.whoAmI)
					{
						count++;
						if (count >= 5)
						{
							Projectile.Kill();
							return;
						}
					}
				}

				Projectile.ai[0] = player.whoAmI; // targets the owner player instead of dying if possible
				Projectile.netUpdate = true;
			}
			else
			{ // owner is dead. Fly dies. Sad!
				Projectile.Kill();
			}

			Projectile.direction = Projectile.velocity.X > 0f ? 1 : -1;
		}

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidShapeshifter shapeshifter)
		{ // after hitting a target, goes back to the owner
			NPCTarget = -1;
			Timespent = 1 + 40 * Main.rand.Next(3);
			Vector2 newTargetLocation = Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * -Main.rand.NextFloat(24f, 64f);
			Projectile.ai[1] = newTargetLocation.X;
			Projectile.ai[2] = newTargetLocation.Y;
			Projectile.netUpdate = true;
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 3; i++)
			{
				Dust dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
				dust2.noGravity = true;
			}

			for (int i = 0; i < 3; i++)
			{
				Dust dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
				dust2.noGravity = true;
			}

			SoundStyle soundStyle = SoundID.Item54;
			soundStyle.Pitch -= 1f;
			SoundEngine.PlaySound(soundStyle, Projectile.Center);

			if ((int)Projectile.ai[0] == Main.myPlayer && !Main.LocalPlayer.dead && ShouldHeal)
			{ // give some health back
				Main.LocalPlayer.GetModPlayer<OrchidPlayer>().TryHeal(Owner.GetModPlayer<OrchidShapeshifter>().GetShapeshifterHealing(3));
			}
		}

		public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
		{
			Vector2 drawPosition = Projectile.Center - Main.screenPosition;
			Rectangle rectangle = TextureMain.Bounds;
			rectangle.Height /= 4; // has 4 frames
			rectangle.Y += rectangle.Height * Frame;
			SpriteEffects effect = Projectile.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
			spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });

			float sine = (0.4f + ((float)Math.Sin(Timespent * 0.05f) * 0.15f));
			Color drawColor = new Color(229, 95, 0) * sine;
			spriteBatch.Draw(TextureGlow, drawPosition + new Vector2(Projectile.direction, 4f), null, drawColor, Projectile.rotation + sine * 0.33f, TextureGlow.Size() * 0.5f, Projectile.scale * 1.25f + sine * 0.5f, SpriteEffects.None, 0f);

			spriteBatch.End();
			spriteBatch.Begin(spriteBatchSnapshot);

			spriteBatch.Draw(TextureMain, drawPosition, rectangle, lightColor, Projectile.rotation, rectangle.Size() * 0.5f, Projectile.scale, effect, 0f);

			spriteBatch.End();
			spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });

			spriteBatch.Draw(TextureGlow2, drawPosition, rectangle, Color.White * 0.75f, Projectile.rotation, rectangle.Size() * 0.5f, Projectile.scale, effect, 0f);

			spriteBatch.End();
			spriteBatch.Begin(spriteBatchSnapshot);

			return false;
		}
	}
}