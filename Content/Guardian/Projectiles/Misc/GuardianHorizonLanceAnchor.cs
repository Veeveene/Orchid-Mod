using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common;
using OrchidMod.Content.Guardian.Weapons.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Player;
using Terraria.Localization;
using OrchidMod.Content.General.Prefixes;

namespace OrchidMod.Content.Guardian.Projectiles.Misc
{
	public class GuardianHorizonLanceAnchor : OrchidModGuardianParryAnchor
	{
		public int TimeSpent = 0;
		public bool Ding = false;
		public bool Blast = false;
		public bool Reinforced = false;
		public bool NeedNetUpdate = false;
		public int SelectedItem { get; set; } = -1;
		public Item HorizonLanceItem => Main.player[Projectile.owner].inventory[SelectedItem];
		public bool Worn => Projectile.ai[1] > 0f; // Standard buff remaining duration
		Vector2 visualSway = Vector2.Zero;
		int flagLength = 0;

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(SelectedItem);
			writer.Write(Reinforced);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			SelectedItem = reader.ReadInt32();
			Reinforced = reader.ReadBoolean();
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			var owner = Main.player[Projectile.owner];
			if (owner.HeldItem.ModItem is HorizonLance) overPlayers.Add(index); // Display the flag over the player if it is being held
		}

		public override void SafeSetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 60;
			Projectile.penetrate = -1;
			Projectile.netImportant = true;
			Projectile.alpha = 255;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
			Projectile.netImportant = true;
		}

		public void OnChangeSelectedItem(Player owner)
		{
			OrchidGuardian guardian = owner.GetModPlayer<OrchidGuardian>();
			Projectile.ai[0] = 0f;
			guardian.GuardianItemCharge = 0;
			if (owner.inventory[owner.selectedItem].ModItem is HorizonLance) SelectedItem = owner.selectedItem;
			Projectile.netUpdate = true;
		}

		public override void AI()
		{
			var owner = Main.player[Projectile.owner];
			OrchidGuardian guardian = owner.GetModPlayer<OrchidGuardian>();

			if (SelectedItem < 0 || HorizonLanceItem == null || HorizonLanceItem.ModItem is not HorizonLance || !owner.active || owner.dead)
			{
				Projectile.Kill();
				return;
			}
			else
			{
				bool heldStandard = owner.HeldItem.ModItem is HorizonLance;

				if (IsLocalOwner)
				{
					if (NeedNetUpdate)
					{
						NeedNetUpdate = false;
						Projectile.netUpdate = true;
					}

					if (heldStandard)
					{
						if (Main.MouseWorld.X > owner.Center.X && owner.direction != 1) owner.ChangeDir(1);
						else if (Main.MouseWorld.X < owner.Center.X && owner.direction != -1) owner.ChangeDir(-1);
					}
					else if (!Worn)
					{
						Projectile.Kill();
						return;
					}
				}
				else
				{
					if (!heldStandard)
					{
						Projectile.ai[0] = 0f;
					}

					if (Projectile.ai[0] == 0f)
					{ // Adresses a visual issue
						guardian.GuardianItemCharge = 0;
					}
				}

				TimeSpent++;
				Projectile.timeLeft = 5;

				if (Worn)
				{ // Handles buffs given to nearby players, npcs, etc
					Projectile.ai[1]--;
					guardian.GuardianCurrentStandardAnchor = Projectile;

					if (Reinforced)
					{
						guardian.GuardianStandardStats.lifeRegen += 6;
						visualSway = visualSway * 0.95f - owner.velocity * 0.005f;
					}
				}
				else
				{
					Reinforced = false;
				}

				if (HorizonLanceItem.ModItem is HorizonLance guardianItem)
				{ 
					Projectile.localAI[0] = 0f; // used for block UI display
					if (Projectile.ai[0] < 0f)
					{ // Stabbing
						Vector2 puchDir = (Projectile.ai[2] + MathHelper.PiOver2).ToRotationVector2();
						if (puchDir.X > 0 && owner.direction != 1) owner.ChangeDir(1);
						else if (puchDir.X < 0 && owner.direction != -1) owner.ChangeDir(-1);

						float addedDistance = 28f;
						//lunge forward
						if (Projectile.ai[0] < -30)
						{
							addedDistance = 28f * (40f / -Projectile.ai[0]);
						}
						//fire, recoil back
						if (Projectile.ai[0] > -30)
						{
							addedDistance += 0.4f * Projectile.ai[0];

							if (!Blast)
							{
								Blast = true;
								SoundEngine.PlaySound(SoundID.Item105, owner.Center);
								int projectileType = ModContent.ProjectileType<GuardianHorizonLanceProj>();

								if (IsLocalOwner)
								{
									foreach (Projectile projectile in Main.projectile)
									{
										if (projectile.type == projectileType && projectile.active && projectile.owner == owner.whoAmI)
										{
											projectile.ai[1] = 1f;
										}
									}
								}

								int damage = guardian.GetGuardianDamage(HorizonLanceItem.damage);
								Projectile newProjectile = Projectile.NewProjectileDirect(HorizonLanceItem.GetSource_FromAI(), Projectile.Center + owner.velocity * 1.5f, Projectile.ai[2].ToRotationVector2(), projectileType, damage, HorizonLanceItem.knockBack, owner.whoAmI);
								newProjectile.CritChance = (int)(owner.GetCritChance<GuardianDamageClass>() + owner.GetCritChance<GenericDamageClass>() + HorizonLanceItem.crit);
								newProjectile.netUpdate = true;

								if (owner.boneGloveItem != null && !owner.boneGloveItem.IsAir && owner.boneGloveTimer == 0)
								{ // Bone glove compatibility, from vanilla code
									owner.boneGloveTimer = 60;
									Vector2 center = owner.Center;
									Vector2 vector = owner.DirectionTo(owner.ApplyRangeCompensation(0.2f, center, Main.MouseWorld)) * 10f;
									Projectile.NewProjectile(owner.GetSource_ItemUse(owner.boneGloveItem), center.X, center.Y, vector.X, vector.Y, ProjectileID.BoneGloveProj, 25, 5f, owner.whoAmI);
								}
							}
						}

						Projectile.Center = owner.MountedCenter.Floor() + new Vector2(4f * owner.direction, 2f) + Vector2.UnitY.RotatedBy(Projectile.ai[2]) * addedDistance;
						Projectile.rotation = Projectile.ai[2] - MathHelper.PiOver4 * 5f;
						float addedRotation = 0f;
						if (Projectile.ai[2] + MathHelper.PiOver2 > 0f)
						{
							addedRotation = (Projectile.ai[2] + owner.direction * MathHelper.PiOver2) * 0.5f;
						}
						addedRotation += (addedDistance - 28) * (puchDir.Y > 0 ? 0.06f : 0.12f) * owner.direction * puchDir.Y;
						owner.SetCompositeArmFront(true, addedDistance > 24 ? CompositeArmStretchAmount.Full : addedDistance > 18 ? CompositeArmStretchAmount.ThreeQuarters : CompositeArmStretchAmount.Quarter, MathHelper.PiOver4 * 2.2f * -owner.direction + addedRotation);

						Projectile.ai[0]++;

						if (Projectile.ai[0] >= 0f)
						{
							Projectile.ai[0] = 0f;
							Projectile.ai[2] = 0f;
						}
					}
					else if (Projectile.ai[0] > 1f)
					{ // Blocking
						Projectile.localAI[0] = 90f; // used for block UI display
						guardian.GuardianParry = true;
						guardian.GuardianParryBuffer = true;

						Projectile.ai[0]--;
						if (owner.immune)
						{
							if (owner.eocHit != -1)
							{
								guardian.DoParryItemParry(Main.npc[owner.eocHit]);
							}
							else
							{
								guardian.GuardianGuardRecharging += Projectile.ai[2] / (guardianItem.ParryDuration * guardianItem.Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianParryDuration);
								Rectangle rect = owner.Hitbox;
								rect.Y -= 64;
								CombatText.NewText(guardian.Player.Hitbox, Color.LightGray, "Interrupted", false, true);
							}
							Projectile.ai[0] = 0f;

							Vector2 pos = new Vector2(Projectile.position.X, Projectile.position.Y);
							for (int i = 0; i < 3; i++)
							{
								Dust dust = Dust.NewDustDirect(pos, 20, 20, DustID.Smoke);
								dust.scale *= 0.75f;
								dust.velocity *= 0.25f;
							}
						}
						else if (Projectile.ai[0] <= 0f)
						{
							Projectile.ai[0] = 0f;
						}

						Projectile.Center = owner.MountedCenter.Floor() + new Vector2(8f * owner.direction, -15);
						Projectile.rotation = MathHelper.PiOver4 * 0.15f * owner.direction - MathHelper.PiOver4;
						owner.SetCompositeArmFront(true, CompositeArmStretchAmount.Full, MathHelper.PiOver2 * -(0.6f + guardian.GuardianItemCharge * 0.0025f) * owner.direction);
						owner.SetCompositeArmBack(true, CompositeArmStretchAmount.Quarter, MathHelper.PiOver2 * -(1f + guardian.GuardianItemCharge * 0.0025f) * owner.direction);
					}
					else if (Projectile.ai[0] == 1f)
					{ // Being charged by the player
						Projectile.Center = owner.MountedCenter.Floor() + new Vector2((26f - guardian.GuardianItemCharge * 0.03f) * owner.direction, guardian.GuardianItemCharge * 0.045f);
						Projectile.rotation = MathHelper.PiOver4 * (1.75f + guardian.GuardianItemCharge * 0.0015f) * owner.direction - MathHelper.PiOver4;
						owner.SetCompositeArmFront(true, CompositeArmStretchAmount.ThreeQuarters, MathHelper.PiOver2 * -(0.6f - guardian.GuardianItemCharge * 0.0025f) * owner.direction);

						if (guardian.GuardianItemCharge < 180f)
						{
							guardian.GuardianItemCharge += 30f / guardianItem.Item.useTime * owner.GetTotalAttackSpeed(DamageClass.Melee);
							if (guardian.GuardianItemCharge > 180f) guardian.GuardianItemCharge = 180f;
						}

						if (guardian.GuardianItemCharge >= 180f && !Ding && IsLocalOwner)
						{
							Ding = true;
							if (ModContent.GetInstance<OrchidClientConfig>().GuardianAltChargeSounds) SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, owner.Center);
							else SoundEngine.PlaySound(SoundID.MaxMana, owner.Center);
						}

						if ((!owner.controlUseItem || !heldStandard) && IsLocalOwner)
						{
							if (guardian.GuardianItemCharge >= 180f)
							{ // Full charge
								SoundEngine.PlaySound(guardianItem.Item.UseSound, owner.Center);

								if (Projectile.ai[1] > 0 && !Reinforced)
								{
									Reinforced = true;
									CombatText.NewText(owner.Hitbox, new Color(175, 255, 175), Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.Reinforced"));
								}

								Projectile.ai[1] = guardianItem.StandardDuration * guardian.GuardianStandardTimer;
								guardian.AddGuard(3);

								foreach (Projectile proj in Main.projectile)
								{
									if (proj.type == ModContent.ProjectileType<GuardianStandardAnchor>() && proj.active && proj.owner == Projectile.owner)
									{
										proj.ai[1] = 30f;
										proj.netUpdate = true;
										break;
									}
								}

								// Stab starts
								Projectile.ai[2] = Vector2.Normalize(Main.MouseWorld - owner.MountedCenter).ToRotation() - MathHelper.PiOver2;
								Projectile.ai[0] = -50f;
								owner.itemTime = 51;
								owner.itemAnimation = 51;
							}
							else
							{ // Not enough charge = Reset to idle
								Projectile.ai[0] = 0f;
							}

							Blast = false;
							guardian.GuardianItemCharge = 0;
							Projectile.netUpdate = true;
						}
					}
					else if (Worn && !heldStandard)
					{ // Display on player back
						Projectile.Center = owner.MountedCenter.Floor();
						if (flagLength > Projectile.ai[1]) flagLength--;
						else if (flagLength < maxFlagLength)flagLength++;
					}
					else
					{ // Idle - Lance is held lower
						Ding = false;
						flagLength = 0;
						Projectile.Center = owner.MountedCenter.Floor() + new Vector2(18f * owner.direction, 14f);
						Projectile.rotation = MathHelper.PiOver4 * 2.2f * owner.direction - MathHelper.PiOver4;
						owner.SetCompositeArmFront(true, CompositeArmStretchAmount.Full, MathHelper.PiOver2 * -0.05f * owner.direction);
					}
				}
			}
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 5; i++)
			{
				Main.dust[Dust.NewDust(Projectile.Center, 0, 0, DustID.Smoke)].velocity *= 0.25f;
			}
		}

		public override bool? CanCutTiles() => false;

		public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
		{
			if (SelectedItem < 0 || SelectedItem > 58) return false;
			if (HorizonLanceItem.ModItem is not HorizonLance guardianItem) return false;
			if (!ModContent.HasAsset(guardianItem.LanceTexture)) return false;

			var player = Main.player[Projectile.owner];
			var color = Lighting.GetColor((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f), Color.White);
			float colorMult2 = 1f;
			if (Projectile.ai[1] < 30f && player.HeldItem.ModItem is not HorizonLance) colorMult2 *= Projectile.ai[1] / 30f;

			SpriteEffects effect = SpriteEffects.None;
			Vector2 posproj = Projectile.Center;
			float drawRotation = Projectile.rotation;
			if (player.gravDir == -1)
			{
				drawRotation = -drawRotation + MathHelper.Pi;
				posproj.Y = (player.Bottom.Floor() + player.position.Floor()).Y - posproj.Y + (posproj.Y - player.Center.Floor().Y) * 2f;
				effect = SpriteEffects.FlipHorizontally;
			}

			var texture = ModContent.Request<Texture2D>(Texture).Value;
			var drawPosition = Vector2.Transform(posproj - Main.screenPosition + Vector2.UnitY * player.gfxOffY, Main.GameViewMatrix.EffectMatrix);

			if (Worn && player.HeldItem.ModItem is not HorizonLance)
			{
				drawRotation = MathHelper.PiOver4 * 0.5f * -player.direction - MathHelper.PiOver4;
				if (player.gravDir == -1)
				{
					drawPosition.Y += 24f;
					drawRotation -= MathHelper.PiOver4 * player.direction;
					if (player.direction == -1)
					{
						drawRotation += MathHelper.Pi;
					}
				}

				drawPosition += new Vector2(-8f * player.direction, -12); 
				float colorMult = (float)Math.Sin(TimeSpent * 0.075f) * 0.1f + 0.9f;
				if (Reinforced) DrawHorizonFlag(drawPosition, drawRotation, player.direction == 1, visualSway);
				spriteBatch.Draw(texture, drawPosition, null, color * colorMult2, drawRotation, texture.Size() * 0.5f, Projectile.scale, effect, 0f);
				if (Reinforced) DrawHorizonFlag(drawPosition, drawRotation, player.direction != 1, visualSway);
				
			}
			else
			{
				spriteBatch.Draw(texture, drawPosition, null, color * colorMult2, drawRotation, texture.Size() * 0.5f, Projectile.scale, effect, 0f);
			}


			return false;
		}

		const int maxFlagLength = 24;

		/// <summary> Mimics the <c>HorizonGlow</c> shader, returning a color modified by the given phase similar to what the shader would provide. </summary>
		/// <remarks> Can take either a <c>float</c> or a <c>Color</c> as the input color. Will automatically cast <c>phase</c> to <c>float</c> if it is a <c>double</c>. <example>Example usage:
		/// <code> GetHorizonGlowColor(Math.Sin(Projectile.timeLeft * 0.05), 0.5f) </code> </example> </remarks>
		/// <param name="phase"> Color phase of the shader. 1 is orange, 0 is white, and -1 is blue. Will automatically cast to <c>float</c> if a <c>double</c> is passed, such as from <c>Math.Sin</c>.</param>
		/// <param name="lumi"> Brightness of the default greyscale color. Defaults to fully bright.</param>
		/// <param name="alpha"> Alpha of the default greyscale color.</param>
		public static Color GetHorizonGlowColor(float phase, float lumi = 1, float alpha = 0)
		{
			float coPhase = 2 * (1 - phase * phase);
			float red;
			float green;
			float blue;
			if (phase > 0)
			{
				red = lumi * (3.4f * phase + coPhase);
				blue = lumi * (((float)Math.Sqrt(lumi) * 0.67f + 0.04f) * phase + coPhase);
			}
			else
			{
				red = lumi * (0.82f * -phase + coPhase);
				blue = lumi * (3.15f * -phase + coPhase);
			}
			green = 1.5f - 0.77f / (lumi + 0.375f) + lumi * coPhase;
			return new (red, green, blue, alpha);
		}
		/// <param name = "color"> Specific color to be modified.</param>
		/// <inheritdoc cref="GetHorizonGlowColor(float, float, float)"/>
		public static Color GetHorizonGlowColor(float phase, Color color)
		{
			float coPhase = 2 * (1 - phase * phase);
			//i really wish i could make these into a float3 or float4 like the shader, but unfortunately xna's vector3 and vector4 don't recognize the alias of rgb
			//this is the more readable solution but it's kind of a mess, sorry
			float red = color.R / 255f;
			float green = color.G / 255f;
			float blue = color.B / 255f;
			float alpha = color.A / 255f;
			float lumi = 0.3f * red * red + 0.4f * green * green + 0.2f * blue * blue;
			float fade = red + green + blue + alpha;
			red = (red / 1.5f + lumi) * (0.6f + 0.4f * alpha);
			green = (red / 1.5f + lumi) * (0.6f + 0.4f * alpha);
			blue = (red / 1.5f + lumi) * (0.6f + 0.4f * alpha);
			if (phase > 0)
			{
				red *= 3.4f * phase + coPhase;
				blue *= ((float)Math.Sqrt(blue) * 0.67f + 0.04f) * phase + coPhase;
			}
			else
			{
				red *= 0.82f * -phase + coPhase;
				blue *= 3.15f * -phase + coPhase;
			}
			green = 1.5f - 0.77f / (green + 0.375f) + green * coPhase;
			return new Color(red, green, blue, alpha) * Math.Min(1f, fade);
		}
		/// <inheritdoc cref="GetHorizonGlowColor(float, Color)"/>
		public static Color GetHorizonGlowColor(double phase, float lumi = 1, float alpha = 0) => GetHorizonGlowColor((float)phase, lumi, alpha);
		/// <inheritdoc cref="GetHorizonGlowColor(float, Color)"/>
		public static Color GetHorizonGlowColor(double phase, Color color) => GetHorizonGlowColor((float)phase, color);

		void DrawHorizonFlag(Vector2 drawPosition, float rot, bool flip, Vector2 vSway)
		{
			var flagTexture = ModContent.Request<Texture2D>(Texture + "_Flag").Value;
			rot -= MathHelper.PiOver4;
			Vector2 pos = drawPosition + new Vector2(28, flip ? 10 : -10).RotatedBy(rot);
			rot += MathHelper.PiOver4 * Main.player[Projectile.owner].direction * 0.5f - vSway.X * 0.5f;
			vSway = vSway.RotatedBy(vSway.X * 0.5f);
			Rectangle rect = new (0, 20, 2, 10);
			var effect = flip ? SpriteEffects.FlipVertically : SpriteEffects.None;
			float swayMult = 0.5f;
			for (int i = 0; i < flagLength; i++)
			{
				float sway = (float)Math.Sin(Projectile.ai[1] / 20 + i * 0.5f);
				float length = 1 + vSway.Y * 0.5f * swayMult;
				// incredibly small flat value is added to length to prevent draw issues with very scrunched flags 
				Main.EntitySpriteDraw(flagTexture, pos, rect, GetHorizonGlowColor(Math.Sin(Projectile.ai[1] * 0.03f + i * 0.1f), 0.5f), rot, new Vector2(1, 5), new Vector2(length + 0.001f, 1), effect);
				//for the first and last 7 pieces, move the texture coordinates
				if (i < flagLength / 3) rect.X += 2;
				if (flagLength - i < flagLength / 3) rect.X -= 2;
				//the draw texture naturally faces rightwards, so:
				//X: add the length of the previous piece to continue drawing out
				//Y: add the current left/right sway values
				pos -= new Vector2(2 * length, (-vSway.X + sway) * swayMult).RotatedBy(rot);
				swayMult *= 1.05f;
			}
		}
	}
}
