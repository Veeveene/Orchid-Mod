using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common.ModObjects;
using OrchidMod.Content.Shapeshifter.Projectiles.Predator;
using OrchidMod.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Shapeshifter.Weapons.Predator
{
	public class PredatorUndine : OrchidModShapeshifterShapeshift
	{
		public bool LateralMovement = false;

		public override void SafeSetDefaults()
		{
			Item.width = 32;
			Item.height = 34;
			Item.value = Item.sellPrice(0, 1, 62, 0);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.NPCDeath4;
			Item.useTime = 15;
			Item.shootSpeed = 150f;
			Item.knockBack = 5f;
			Item.damage = 19;
			ShapeshiftWidth = 20;
			ShapeshiftHeight = 24;
			ShapeshiftType = ShapeshifterShapeshiftType.Predator;
			ShapeshiftTypeUI = ShapeshifterShapeshiftTypeUI.List;
			AutoReuseRight = true;
			GroundedWildshape = true;
		}

		public override void ShapeshiftGetUIInfo(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter, ref int uiCount, ref int uiCountMax)
		{
			uiCount = (int)Math.Floor(projectile.ai[0] * 0.5f);
			uiCountMax = 5;
		}

		public override void ShapeshiftAnchorOnShapeshiftFast(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter)
		{
			projectile.ai[0] = 4f;
		}

		public override void ShapeshiftAnchorOnShapeshift(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter)
		{
			anchor.Frame = 1;
			anchor.Timespent = 0;
			projectile.direction = player.direction;
			projectile.spriteDirection = player.direction;
			LateralMovement = false;

			for (int i = 0; i < 5; i++)
			{
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.WaterCandle);
				dust.scale *= Main.rand.NextFloat(2f, 2.5f);
				dust.velocity *= Main.rand.NextFloat(0.25f, 0.5f);
				dust.noGravity = true;
			}

			for (int i = 0; i < 8; i++)
			{
				Main.dust[Dust.NewDust(projectile.Center, 0, 0, DustID.Smoke)].velocity *= 0.5f;
			}
		}

		public override void OnKillAnchor(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter)
		{
			for (int i = 0; i < 5; i++)
			{
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.WaterCandle);
				dust.scale *= Main.rand.NextFloat(2f, 2.5f);
				dust.velocity *= Main.rand.NextFloat(0.25f, 0.5f);
				dust.noGravity = true;
			}

			for (int i = 0; i < 8; i++)
			{
				Main.dust[Dust.NewDust(projectile.Center, 0, 0, DustID.Smoke)].velocity *= 0.5f;
			}
		}

		public override bool ShapeshiftCanLeftClick(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) => anchor.IsLeftClick && anchor.CanLeftClick && (!anchor.IsRightClick || anchor.RightCLickCooldown > 0);

		public override void ShapeshiftOnLeftClick(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter)
		{
			Vector2 position = projectile.Center;
			Vector2 offSet = Vector2.Normalize(Main.MouseWorld - projectile.Center).RotatedByRandom(MathHelper.ToRadians(5f)) * Item.shootSpeed * Main.rand.NextFloat(0.8f, 1.2f) / 25f;

			bool foundTarget = false;
			for (int i = 0; i < 25; i++)
			{
				position += TileCollideShapeshifter(position, offSet, 2, 2, true, true, (int)player.gravDir);

				foreach (NPC npc in Main.npc)
				{
					if (OrchidModProjectile.IsValidTarget(npc))
					{
						if (position.Distance(npc.Center) < npc.width + 32f) // if the NPC is close to the projectile path, snaps to it.
						{
							foundTarget = true;
							position = npc.Center;
							break;
						}
					}
				}

				if (foundTarget)
				{
					break;
				}
			}

			int projectileType = ModContent.ProjectileType<PredatorUndineProj>();
			ShapeshifterNewProjectile(shapeshifter, position, offSet * 0.001f, projectileType, Item.damage, Item.crit, Item.knockBack, player.whoAmI);
			anchor.LeftCLickCooldown = Item.useTime - projectile.ai[1] * 0.5f;
		}

		public override bool ShapeshiftCanRightClick(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) => anchor.IsRightClick && anchor.CanRightClick;

		public override void ShapeshiftOnRightClick(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter)
		{
			int projectileType = ModContent.ProjectileType<PredatorUndineProjAlt>();

			float damage = 1f;
			int crit = shapeshifter.GetShapeshifterCrit(Item.crit);
			if (crit > 100)
			{ // damage increased by 200% of excess crit
				damage += (crit - 100) * 0.02f;
			}
			damage = damage * Item.damage;

			float closestDistance = 80f;
			NPC closestTarget = null;

			if (shapeshifter.modPlayer.LastHitNPC != null)
			{
				if (OrchidModProjectile.IsValidTarget(shapeshifter.modPlayer.LastHitNPC))
				{
					closestTarget = shapeshifter.modPlayer.LastHitNPC;
				}
			}

			foreach (NPC npc in Main.npc)
			{
				bool closestBoss = false;
				if (closestTarget != null)
				{
					if (closestTarget.boss)
					{
						closestBoss = true;
					}
				}

				if (OrchidModProjectile.IsValidTarget(npc) && Collision.CanHitLine(projectile.position + new Vector2(projectile.width * 0.5f, 0f), 2, 2, npc.position, npc.width, npc.height))
				{
					float distance = Main.MouseWorld.Distance(npc.Center);
					Point point = new Point((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y);
					if ((distance < closestDistance || (closestTarget == null && npc.Hitbox.Contains(point)) || (distance < 80f && npc.boss)) && (!closestBoss || npc.boss))
					{
						closestTarget = npc;
						closestDistance = distance;
					}
				}
			}

			if (closestTarget != null)
			{
				while (projectile.ai[0] >= 1f)
				{
					projectile.ai[0]--;
					Vector2 velocity = Vector2.Normalize(projectile.Center - closestTarget.Center).RotatedByRandom(1f) * 3.8f; // 3.8f is a magic number that makes the trail look good ...
					ShapeshifterNewProjectile(shapeshifter, projectile.Center, velocity, projectileType, damage, Item.crit, Item.knockBack, player.whoAmI, ai0: projectile.ai[0] * 15f, ai2: closestTarget.whoAmI);
				}

				anchor.RightCLickCooldown = 60;
			}
		}

		public override void ShapeshiftBuffs(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter)
		{
			Item.crit = (int)(projectile.ai[0] * 10f);
			shapeshifter.ShapeshifterMoveSpeedBonus += 0.05f * projectile.ai[1] + anchor.ai[1] * 0.033f;
		}

		public override void UpdateInventory(Player player)
		{
			OrchidShapeshifter shapeshifter = Shapeshifter(player);
			if (shapeshifter.IsShapeshifted)
			{
				if (shapeshifter.Shapeshift.Type != Type)
				{
					Item.crit = 0;
				}
			}
			else
			{
				Item.crit = 0;
			}
		}

		public override void ShapeshiftAnchorAI(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter)
		{
			// ai[0] holds passive stacks
			// ai[1] holds the number of recent crits
			// ai[2] holds the timer before crits reset
			// anchor.ai[0] holds the auto attack blink animation
			// anchor.ai[1] holds the movement passive bonus movement speed
			// anchor.ai[2] holds bonus jumps

			// MISC EFFECTS

			int velocityRef = (int)projectile.velocity.Length();
			if (velocityRef > 10) velocityRef = 10;
			if (Main.rand.NextBool(15 - velocityRef))
			{ // spawns dust, more while running
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Water);
				dust.velocity *= Main.rand.NextFloat(0.5f, 0.75f);
				dust.velocity.Y = Main.rand.NextFloat(-0.5f, -2f);
			}

			if (Main.rand.NextBool(25 - velocityRef) && projectile.ai[1] > 5)
			{ // spawns dust, more while running
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.WaterCandle);
				dust.scale *= Main.rand.NextFloat(2f, 2.5f);
				dust.velocity *= Main.rand.NextFloat(0.25f, 0.5f);
				dust.noGravity = true;
			}

			if (anchor.ai[1] > 0)
			{
				anchor.ai[1]--;
				if (anchor.ai[1] < 0)
				{
					anchor.ai[1] = 0;
				}
			}

			anchor.ai[0] -= 1f * shapeshifter.GetShapeshifterMeleeSpeed() * (1f + 0.05f * projectile.ai[1]);

			if (projectile.ai[2] <= 0f)
			{
				projectile.ai[1] = 0f;
			}
			else
			{
				projectile.ai[2] --;
			}

			bool grounded = IsGrounded(projectile, player, 8f);
			float speedMult = GetSpeedMult(player, shapeshifter, anchor, grounded);


			if (grounded)
			{
				anchor.ai[2] = 2;
			}

			// ANIMATION

			if (grounded && projectile.velocity.Y >= 0)
			{
				if (LateralMovement)
				{ // Player is moving left or right, cycle through frames
					if (anchor.Timespent % 4 == 0 && anchor.Timespent > 0)
					{
						anchor.Frame++;
						if (anchor.Frame == 8)
						{
							anchor.Frame = 1;
						}
					}
				}
				else
				{ // idle frame
					anchor.Timespent = 0;
					anchor.Frame = 0;
				}
			}
			else
			{ // Falling frame
				anchor.Timespent = 0;
				anchor.Frame = 5;
			}

			// MOVEMENT

			Vector2 intendedVelocity = projectile.velocity;
			GravityCalculations(ref intendedVelocity, player, shapeshifter);

			if (anchor.IsInputJump)
			{
				if (!grounded && anchor.ai[2] > 0 && anchor.JumpWithControlRelease(player))
				{ // double jump
					anchor.ai[2]--;

					if (anchor.IsInputLeft && !anchor.IsInputRight)
					{ // right directon jump
						TryJump(ref intendedVelocity, 9f, player, shapeshifter, anchor, speedEfficiency: 0f);
						intendedVelocity.X = -10f * speedMult * shapeshifter.ShapeshifterMoveSpeedMiscOverride;
					}
					else if (!anchor.IsInputLeft && anchor.IsInputRight)
					{ // right directon jump
						TryJump(ref intendedVelocity, 9f, player, shapeshifter, anchor, speedEfficiency : 0f);
						intendedVelocity.X = 10f * speedMult * shapeshifter.ShapeshifterMoveSpeedMiscOverride;
					}
					else
					{
						TryJump(ref intendedVelocity, 11f, player, shapeshifter, anchor);
					}

					SoundEngine.PlaySound(SoundID.DoubleJump, projectile.Center);

					for (int i = 0; i < 5; i++)
					{
						Main.dust[Dust.NewDust(projectile.Center, 0, 0, DustID.Smoke)].velocity *= 0.5f;
					}

					for (int i = 0; i < 3; i++)
					{
						Gore gore = Gore.NewGoreDirect(projectile.GetSource_FromAI(), projectile.Center + new Vector2(Main.rand.NextFloat(-24f, 0f), Main.rand.NextFloat(-24f, 0f)), Vector2.UnitY.RotatedByRandom(MathHelper.Pi), 61 + Main.rand.Next(3));
						gore.rotation = Main.rand.NextFloat(MathHelper.Pi);
						gore.velocity.Y += Main.rand.NextFloat(0.25f, 0.5f);
					}

					for (int i = 0; i < 20; i++)
					{
						Dust dust = Dust.NewDustDirect(projectile.Center, 0, 0, DustID.Water);
						dust.scale *= Main.rand.NextFloat(1f, 1.5f);
						dust.velocity *= Main.rand.NextFloat(0.5f, 0.75f);
						dust.velocity.Y = Main.rand.NextFloat(-1f, -5f);
					}

					for (int i = 0; i < 10; i++)
					{
						Dust dust = Dust.NewDustDirect(projectile.Center, 0, 0, DustID.WaterCandle);
						dust.scale *= Main.rand.NextFloat(2f, 2.5f);
						dust.velocity *= Main.rand.NextFloat(0.5f, 0.75f);
						dust.velocity += projectile.velocity * 0.25f;
						dust.noGravity = true;
					}
				}
				else
				{ // grounded jump
					anchor.JumpWithControlRelease(player);
					TryJump(ref intendedVelocity, 9.5f, player, shapeshifter, anchor, true);
				}
			}

			// Normal movement
			if (anchor.IsInputLeft || anchor.IsInputRight)
			{ // Player is inputting a movement key
				float acceleration = speedMult;
				if (!grounded) acceleration *= 0.5f;

				if (anchor.IsInputLeft && !anchor.IsInputRight)
				{ // Left movement
					TryAccelerate(ref intendedVelocity, shapeshifter, -4.3f, speedMult, 0.3f, acceleration);
					projectile.direction = -1;
					projectile.spriteDirection = -1;
					LateralMovement = true;
				}
				else if (anchor.IsInputRight && !anchor.IsInputLeft)
				{ // Right movement
					TryAccelerate(ref intendedVelocity, shapeshifter, 4.3f, speedMult, 0.3f, acceleration);
					projectile.direction = 1;
					projectile.spriteDirection = 1;
					LateralMovement = true;
				}
				else
				{ // Both keys pressed = no movement
					LateralMovement = false;
					TrySlowDown(ref intendedVelocity, 0.7f, player, shapeshifter, projectile);
				}
			}
			else
			{ // no movement input
				LateralMovement = false;
				TrySlowDown(ref intendedVelocity, 0.7f, player, shapeshifter, projectile);
			}

			FinalVelocityCalculations(ref intendedVelocity, projectile, player, true);

			// POSITION AND ROTATION VISUALS

			anchor.OldPosition.Add(projectile.Center);
			anchor.OldRotation.Add(projectile.rotation);
			anchor.OldFrame.Add(anchor.Frame);

			for (int i = 0; i < 2; i++)
			{
				if (anchor.OldPosition.Count > (projectile.ai[1] > 5 ? 7 : 5))
				{
					anchor.OldPosition.RemoveAt(0);
					anchor.OldRotation.RemoveAt(0);
					anchor.OldFrame.RemoveAt(0);
				}
			}
		}

		public override void ShapeshiftPreDraw(SpriteBatch spriteBatch, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Vector2 drawPosition, Rectangle drawRectangle, SpriteEffects effect, Player player, Color lightColor)
		{
			if (anchor.ai[0] > 0)
			{
				spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
				spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });

				OrchidShapeshifter shapeshifter = Shapeshifter(player);

				float scalemult = (float)Math.Sin(anchor.ai[0] * 0.2092f) * 0.125f + 1f;

				if (shapeshifter.ShapeshifterHairpin)
				{
					spriteBatch.Draw(anchor.TextureShapeshift, drawPosition, drawRectangle, Color.White * 0.75f, projectile.rotation, drawRectangle.Size() * 0.5f, projectile.scale * scalemult, effect, 0f);
					spriteBatch.Draw(anchor.TextureShapeshiftHairGray, drawPosition, drawRectangle, player.hairColor * 0.75f, projectile.rotation, drawRectangle.Size() * 0.5f, projectile.scale * scalemult, effect, 0f);
				}
				else
				{
					spriteBatch.Draw(anchor.TextureShapeshift, drawPosition, drawRectangle, Color.White * 0.75f, projectile.rotation, drawRectangle.Size() * 0.5f, projectile.scale * scalemult, effect, 0f);
					spriteBatch.Draw(anchor.TextureShapeshiftHair, drawPosition, drawRectangle, Color.White * 0.75f, projectile.rotation, drawRectangle.Size() * 0.5f, projectile.scale * scalemult, effect, 0f);
				}

				spriteBatch.End();
				spriteBatch.Begin(spriteBatchSnapshot);
			}
		}

		public override bool ShapeshiftFreeDodge(Player.HurtInfo info, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter)
		{
			if (projectile.ai[0] >= 10f)
			{
				shapeshifter.modPlayer.SetDodgeImmuneTime(40);
				SoundEngine.PlaySound(SoundID.Splash, projectile.Center);
				projectile.ai[0] = 0f;
				anchor.ai[1] = 30;
				anchor.NeedNetUpdate = true;

				for (int i = 0; i < 5; i++)
				{
					Dust dust = Dust.NewDustDirect(projectile.Center, 0, 0, DustID.Smoke);
					dust.scale *= Main.rand.NextFloat(1f, 1.5f);
					dust.velocity *= Main.rand.NextFloat(0.5f, 0.75f);
				}

				for (int i = 0; i < 20; i++)
				{
					Dust dust = Dust.NewDustDirect(projectile.Center, 0, 0, DustID.Water);
					dust.scale *= Main.rand.NextFloat(1f, 1.5f);
					dust.velocity *= Main.rand.NextFloat(0.5f, 0.75f);
					dust.velocity.Y = Main.rand.NextFloat(-1f, -5f);
				}

				for (int i = 0; i < 10; i++)
				{
					Dust dust = Dust.NewDustDirect(projectile.Center, 0, 0, DustID.WaterCandle);
					dust.scale *= Main.rand.NextFloat(2f, 2.5f);
					dust.velocity *= Main.rand.NextFloat(0.5f, 0.75f);
					dust.velocity += projectile.velocity * 0.25f;
					dust.noGravity = true;
				}

				int projectileType = ModContent.ProjectileType<PredatorUndineProjAlt>();

				float damage = 1f;
				int crit = shapeshifter.GetShapeshifterCrit(Item.crit);
				if (crit > 100)
				{ // damage increased by 200% of excess crit
					damage += (crit - 100) * 0.02f;
				}
				damage = damage * Item.damage;

				float closestDistance = 160f;
				NPC closestTarget = null;

				foreach (NPC npc in Main.npc)
				{
					float distance = projectile.Center.Distance(npc.Center);
					if (OrchidModProjectile.IsValidTarget(npc) && distance < closestDistance)
					{
						closestTarget = npc;
						closestDistance = distance;
					}
				}

				if (closestTarget != null)
				{
					for (int i = 0; i < 6; i++)
					{
						Vector2 velocity = Vector2.Normalize(projectile.Center - closestTarget.Center).RotatedByRandom(1f) * 3.8f; // 3.8f is a magic number that makes the trail look good ...
						Projectile newProjectile = ShapeshifterNewProjectile(shapeshifter, projectile.Center, velocity, projectileType, damage, Item.crit, Item.knockBack, player.whoAmI, ai0: (i * -15f) - 1f, ai2: closestTarget.whoAmI);
					}
				}

				return true;
			}

			return false;
		}
	}
}