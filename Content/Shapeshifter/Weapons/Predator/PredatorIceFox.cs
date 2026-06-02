using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Content.Shapeshifter.Projectiles.Predator;
using OrchidMod.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Shapeshifter.Weapons.Predator
{
	public class PredatorIceFox : OrchidModShapeshifterShapeshift
	{
		public bool LateralMovement = false;

		public override void SafeSetDefaults()
		{
			Item.width = 30;
			Item.height = 36;
			Item.value = Item.sellPrice(0, 0, 40, 0);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.NPCDeath4;
			Item.useTime = 40;
			Item.shootSpeed = 48f;
			Item.knockBack = 5f;
			Item.damage = 21;
			ShapeshiftWidth = 20;
			ShapeshiftHeight = 24;
			ShapeshiftType = ShapeshifterShapeshiftType.Predator;
			ShapeshiftTypeUI = ShapeshifterShapeshiftTypeUI.List;
			GroundedWildshape = true;
			ShapeshiftImmunities.Add(BuffID.Chilled);
		}

		public override void ShapeshiftGetUIInfo(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter, ref int uiCount, ref int uiCountMax)
		{
			uiCount = 0;
			uiCountMax = 3;
			int temp = 0;
			while (temp < projectile.ai[0])
			{
				temp += 240;
				if (temp <= projectile.ai[0])
				{
					uiCount++;
				}
			}
		}

		public override Color GetColor(Color lightColor, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter, bool isHairColor)
		{
			if (projectile.ai[1] > 0)
			{ // Right click phaseshift is active
				return new Color(54, 150, 248) * 0.25f;
			}
			return base.GetColor(lightColor, projectile, anchor, player, shapeshifter, isHairColor);
		}

		public override void ShapeshiftPostDraw(SpriteBatch spriteBatch, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Vector2 drawPosition, Rectangle drawRectangle, SpriteEffects effect, Player player, Color lightColor)
		{
			if (projectile.ai[1] > 0)
			{
				lightColor = new Color(54, 150, 248) * 0.75f;
				spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
				spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });

				spriteBatch.Draw(anchor.TextureShapeshift, drawPosition, drawRectangle, lightColor * 1.5f, projectile.rotation, drawRectangle.Size() * 0.5f, projectile.scale, effect, 0f);
				spriteBatch.Draw(anchor.TextureShapeshiftHairGray, drawPosition, drawRectangle, lightColor * 1.5f, projectile.rotation, drawRectangle.Size() * 0.5f, projectile.scale, effect, 0f);

				spriteBatch.End();
				spriteBatch.Begin(spriteBatchSnapshot);
			}
		}

		public override void ShapeshiftAnchorOnShapeshiftFast(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter)
		{
			projectile.ai[0] = 180f; //first dash charge after 1 second
		}

		public override void ShapeshiftAnchorOnShapeshift(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter)
		{
			shapeshifter.ShapeshifterPredatorFoxSpeed = 180;

			anchor.Frame = 1;
			anchor.Timespent = 0;
			projectile.direction = player.direction;
			projectile.spriteDirection = player.direction;
			LateralMovement = false;

			for (int i = 0; i < 8; i++)
			{
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.IceTorch, Scale: Main.rand.NextFloat(1f, 1.4f));
				dust.noGravity = true;
				dust.noLight = true;
			}
		}

		public override void OnKillAnchor(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter)
		{
			shapeshifter.ShapeshifterPredatorFoxSpeed = 180;

			for (int i = 0; i < 5; i++)
			{
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.IceTorch, Scale: Main.rand.NextFloat(1f, 1.4f));
				dust.noGravity = true;
				dust.noLight = true;
			}
		}

		public override void ShapeshiftOnLeftClick(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter)
		{
			int projectileType = ModContent.ProjectileType<PredatorIceFoxProj>();
			float ai1 = 0f;
			float ai0 = 0f;
			int count = 0;
			bool found2 = false;
			foreach (Projectile proj in Main.projectile)
			{
				if (proj.active && proj.owner == player.whoAmI && proj.type == projectileType && proj.ai[1] >= 0f)
				{
					count++;
					ai0 = proj.ai[0];
					if (proj.ai[1] == 0f && ai1 == 0f) ai1 = 1f;
					if (proj.ai[1] == 1f && !found2) ai1 = 2f;
					if (proj.ai[1] == 2f)
					{
						found2 = true;
						if (ai1 == 2f)
						{
							ai1 = 0f;
						}
					}
				}
			}

			if (count < 3)
			{ // Spawn a flame if less than 3 currently exist
				ShapeshifterNewProjectile(shapeshifter, projectile.Center, Vector2.Zero, projectileType, Item.damage, Item.crit, 0f, player.whoAmI, ai0, ai1);

				projectile.ai[2] = 30;
				anchor.LeftCLickCooldown = Item.useTime;
				anchor.NeedNetUpdate = true;
				SoundEngine.PlaySound(SoundID.DD2_DarkMageAttack, projectile.Center);


				for (int i = 0; i < 5; i++)
				{
					Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.IceTorch, Scale: Main.rand.NextFloat(1f, 1.4f));
					dust.noGravity = true;
					dust.noLight = true;
				}
			}
		}

		public override bool ShapeshiftCanRightClick(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) => Main.mouseRight && (Main.mouseRightRelease || AutoReuseRight) && anchor.CanRightClick && projectile.ai[0] >= 240;

		public override void ShapeshiftOnRightClick(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter)
		{
			projectile.ai[0] -= 240;

			anchor.RightCLickCooldown = 180;
			anchor.NeedNetUpdate = true;
			projectile.ai[1] = 90;
			SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal, projectile.Center);

			for (int i = 0; i < 20; i++)
			{
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.IceTorch, Scale: Main.rand.NextFloat(1.4f, 2f));
				dust.noGravity = true;
				dust.velocity += projectile.velocity;
			}
		}

		public override bool ShapeshiftCanJump(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) => anchor.JumpWithControlRelease(player) && projectile.ai[0] >= 180 && !IsGrounded(projectile, player, 4f);

		public override void ShapeshiftOnJump(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter)
		{
			for (int i = 0; i < 10; i++)
			{
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.IceTorch, Scale: Main.rand.NextFloat(1.4f, 2f));
				dust.noGravity = true;
			}

			projectile.ai[0] -= 240;
			Vector2 position = projectile.position;
			Vector2 offSet = Vector2.Normalize(Main.MouseWorld - projectile.Center) * 8f * GetSpeedMult(player, shapeshifter, anchor);

			for (int i = 0; i < 24; i++)
			{
				if (AnyTileCollideShapeshifter(position, offSet, projectile.width, projectile.height, true, true, (int)player.gravDir))
				{
					break;
				}
				else
				{
					position += offSet;
					Dust dust = Dust.NewDustDirect(position, projectile.width, projectile.height, DustID.IceTorch, Scale: Main.rand.NextFloat(1.4f, 2f));
					dust.noGravity = true;
				}
			}

			anchor.Teleport(position + new Vector2(projectile.width, projectile.height) * 0.5f);
			projectile.velocity = offSet;
			projectile.velocity.Y *= 0.5f;
			anchor.NeedNetUpdate = true;
			anchor.LeftCLickCooldown = Item.useTime;
			projectile.ai[2] = 30;
			SoundEngine.PlaySound(SoundID.DD2_DarkMageAttack, projectile.Center);
			SetCameraLerp(player, 0.1f, 8);

			// Spawn 3 foxfire flames after the dash and makes the old ones remain in place
			int projectileType = ModContent.ProjectileType<PredatorIceFoxProj>();

			foreach (Projectile proj in Main.projectile)
			{
				if (proj.active && proj.owner == player.whoAmI && proj.type == projectileType)
				{
					proj.ai[1] = -2f;
				}
			}

			for (int j = 0; j < 3; j++)
			{
				ShapeshifterNewProjectile(shapeshifter, projectile.Center, Vector2.Zero, projectileType, Item.damage, Item.crit, 0f, player.whoAmI, 0f, j);
			}

			SoundEngine.PlaySound(SoundID.Item28, projectile.Center);
		}

		public override void ShapeshiftAnchorAI(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter)
		{
			// MISC EFFECTS

			bool grounded = IsGrounded(projectile, player, 8f);
			float speedMult = GetSpeedMult(player, shapeshifter, anchor, grounded);

			if ((int)projectile.ai[0] < 721)
			{ // Increases the dash timer
				projectile.ai[0]++;

				if ((int)projectile.ai[0] % 240 == 0)
				{ // Spawns a projectile following the player when the dash is ready
					SoundStyle soundStyle = SoundID.Item30;
					soundStyle.Volume *= 0.5f;
					SoundEngine.PlaySound(soundStyle, projectile.Center);
				}
			}

			if (projectile.ai[1] > 0)
			{ // Right click shift lighting and damage
				projectile.damage = shapeshifter.GetShapeshifterDamage(Item.damage * 0.5f);
				projectile.CritChance = shapeshifter.GetShapeshifterCrit(Item.crit);
				projectile.friendly = true;

				projectile.ai[1]--;
				Color color = Color.Aqua * ((float)Math.Sin(projectile.ai[1] * 0.1046f) * 0.2f + 0.2f);
				Lighting.AddLight(projectile.Center, color.R / 255f, color.G / 255f, color.B / 255f);

				if (Main.rand.NextBool(2))
				{
					Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.IceTorch, Scale: Main.rand.NextFloat(1f, 1.4f));
					dust.noGravity = true;
					dust.noLight = true;

				}

				if (projectile.ai[1] <= 0)
				{
					SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal, projectile.Center);

					for (int i = 0; i < 20; i++)
					{
						Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.IceTorch, Scale: Main.rand.NextFloat(1.4f, 2f));
						dust.noGravity = true;
						dust.velocity += projectile.velocity;
					}
				}
			}
			else
			{
				projectile.damage = 0;
				projectile.CritChance = 0;
				projectile.friendly = false;
			}

			if (projectile.ai[2] > 0)
			{ // Left click lighting
				anchor.Projectile.ai[2]--;
				Color color = Color.Aqua * (float)Math.Sin(projectile.ai[2] * 0.1046f) * 0.2f;
				Lighting.AddLight(projectile.Center, color.R / 255f, color.G / 255f, color.B / 255f);
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
			{ // Jump while no charge ready
				TryJump(ref intendedVelocity, 9.5f, player, shapeshifter, anchor, true);
			}

			// Normal movement
			if (anchor.IsInputLeft || anchor.IsInputRight)
			{ // Player is inputting a movement key
				float acceleration = speedMult;
				if (!grounded) acceleration *= 0.5f;

				if (anchor.IsInputLeft && !anchor.IsInputRight)
				{ // Left movement
					TryAccelerate(ref intendedVelocity, shapeshifter, -4f, speedMult, 0.3f, acceleration);
					projectile.direction = -1;
					projectile.spriteDirection = -1;
					LateralMovement = true;
				}
				else if (anchor.IsInputRight && !anchor.IsInputLeft)
				{ // Right movement
					TryAccelerate(ref intendedVelocity, shapeshifter, 4f, speedMult, 0.3f, acceleration);
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
				if (anchor.OldPosition.Count > (projectile.ai[1] > 0 ? 8 : 5))
				{
					anchor.OldPosition.RemoveAt(0);
					anchor.OldRotation.RemoveAt(0);
					anchor.OldFrame.RemoveAt(0);
				}
			}
		}

		public override void ShapeshiftPreDraw(SpriteBatch spriteBatch, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Vector2 drawPosition, Rectangle drawRectangle, SpriteEffects effect, Player player, Color lightColor)
		{
			if (projectile.ai[2] > 0)
			{
				spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
				spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });

				float scalemult = (float)Math.Sin(projectile.ai[2] * 0.1046f) * 0.125f + 1f;
				spriteBatch.Draw(anchor.TextureShapeshift, drawPosition, drawRectangle, new Color(54, 150, 248) * 0.75f, projectile.rotation, drawRectangle.Size() * 0.5f, projectile.scale * scalemult, effect, 0f);
				spriteBatch.Draw(anchor.TextureShapeshiftHair, drawPosition, drawRectangle, new Color(54, 150, 248) * 0.75f, projectile.rotation, drawRectangle.Size() * 0.5f, projectile.scale * scalemult, effect, 0f);

				spriteBatch.End();
				spriteBatch.Begin(spriteBatchSnapshot);
			}
		}

		public override bool ShapeshiftFreeDodge(Player.HurtInfo info, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter)
		{
			if (projectile.ai[1] > 0)
			{
				shapeshifter.modPlayer.SetDodgeImmuneTime();
				return true;
			}

			return false;
		}
	}
}