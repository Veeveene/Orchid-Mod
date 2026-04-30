using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common;
using OrchidMod.Content.General.Prefixes;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Localization;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Player;

namespace OrchidMod.Content.Guardian
{
	public class GuardianKatarAnchor : OrchidModGuardianParryAnchor
	{
		public GuardianKatarAnchor LinkedKatarAnchor;
		public int LockedOwnerDir = 0;
		public bool OffHandKatar = false;
		public bool Ding = false;
		public bool NeedNetUpdate = false;
		public float KatarDashAngle = 0f;
		public int KatarDashTimer = 0;
		public float SlamTime = 0;

		public int KatarAnimFrame = 0;

		public int SelectedItem { get; set; } = -1;
		public Item KatarItem => Main.player[Projectile.owner].inventory[SelectedItem];
		public bool Blocking => Projectile.ai[0] > 0 && !Charging;
		public bool Slamming => Projectile.ai[0] < 0;
		public bool Charging => Projectile.ai[2] > 0;

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			if (!OffHandKatar || Blocking) overPlayers.Add(index);
		}

		// ...

		public override void SafeSetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
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
			KatarDashAngle = 0f;
			KatarDashTimer = 0;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(SelectedItem);
			writer.Write(OffHandKatar);
			writer.Write(KatarDashAngle);
			writer.Write(KatarDashTimer);
		}
		public override void ReceiveExtraAI(BinaryReader reader)
		{
			SelectedItem = reader.ReadInt32();
			OffHandKatar = reader.ReadBoolean();
			KatarDashAngle = reader.ReadSingle();
			KatarDashTimer = reader.ReadInt32();
		}

		public void OnChangeSelectedItem(Player owner)
		{
			SelectedItem = owner.selectedItem;
			Projectile.ai[0] = 0f;
			Projectile.ai[1] = 0f;
			Projectile.ai[2] = 0f;
			Projectile.localAI[1] = 0;
			Projectile.netUpdate = true;
			KatarDashAngle = 0f;
			KatarDashTimer = 0;
			owner.GetModPlayer<OrchidGuardian>().GuardianItemCharge = 0;
		}

		public override void AI()
		{
			var owner = Main.player[Projectile.owner];
			OrchidGuardian guardian = owner.GetModPlayer<OrchidGuardian>();

			if (!owner.active || owner.dead || SelectedItem < 0 || !(owner.HeldItem.ModItem is OrchidModGuardianKatar) || KatarItem == null || KatarItem.ModItem is not OrchidModGuardianKatar guardianItem)
			{
				if (IsLocalOwner) Projectile.Kill();
				return;
			}
			else
			{
				if (NeedNetUpdate)
				{
					NeedNetUpdate = false;
					Projectile.netUpdate = true;
				}

				Projectile.timeLeft = 5;
				if (OffHandKatar && IsLocalOwner) // Offhand is always loaded first; no need to do that twice
				{
					if (Main.projectile[guardianItem.GetAnchors(owner)[1]].ai[0] >= 0)
					{ // Lock the player direction while slamming
						if (Main.MouseWorld.X > owner.Center.X && owner.direction != 1) owner.ChangeDir(1);
						else if (Main.MouseWorld.X < owner.Center.X && owner.direction != -1) owner.ChangeDir(-1);
						LockedOwnerDir = owner.direction;
					}
					else owner.direction = LockedOwnerDir;
				}

				if (Blocking)
				{
					guardian.GuardianParry = true;
					guardian.GuardianParryBuffer = true;
					Projectile.ai[0]--;

					if (owner.immune)
					{
						if (owner.eocHit != -1 && owner.eocDash > 0)
						{
							guardian.DoParryItemParry(Main.npc[owner.eocHit]); // this resets both katars' parry state
						}
						else
						{
							Projectile.ai[0] = 0f;
							//refund remaining duration as guards if interrupted by owner becoming immune from another source
							guardian.GuardianGuardRecharging += Projectile.ai[0] / (guardianItem.ParryDuration * guardianItem.Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianParryDuration);
							Rectangle rect = owner.Hitbox;
							rect.Y -= 64;
							CombatText.NewText(guardian.Player.Hitbox, Color.LightGray, Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.Interrupted"), false, true);
							if (OffHandKatar) //disable mainhand guard on interrupt
							{
								for (int i = Projectile.whoAmI + 1; i < Main.maxProjectiles; i++)
								{
									if (Main.projectile[i].active && Main.projectile[i].owner == Projectile.owner && Main.projectile[i].ModProjectile is GuardianKatarAnchor mainhand)
									{
										if (mainhand.Blocking)
										{
											Main.projectile[i].ai[0] = 0f;
											break;
										}
										break;
									}
								}
							}
						}
					}
					else if (Projectile.ai[0] <= 0f)
					{
						spawnDusts();
						Projectile.ai[0] = 0f;
					}
				}


				if (KatarDashTimer > 0 || (OffHandKatar && LinkedKatarAnchor.KatarDashTimer > 0))
				{ // handles the player dash (after a parry)
					if (!OffHandKatar)
					{
						if (KatarDashTimer > 1)
						{
							Vector2 intendedVelocity = Vector2.UnitY.RotatedBy(KatarDashAngle) * -guardianItem.ParryDashSpeed;
							owner.velocity = intendedVelocity;
							owner.direction = intendedVelocity.X > 0 ? 1 : -1;
							owner.fallStart = (int)(owner.position.Y / 16);

							if (Main.rand.NextBool())
							{
								Dust dust = Dust.NewDustDirect(owner.position, owner.width, owner.height, DustID.Smoke);
								dust.noGravity = true;
							}

							if (KatarDashTimer == guardianItem.ParryDuration + 1)
							{ // spawn smoke and play sound on dash start
								guardianItem.PlayGuardSound(owner, guardian, Projectile);

								for (int i = 0; i < 5; i++)
								{
									Dust dust = Dust.NewDustDirect(owner.Center, 0, 0, DustID.Smoke);
									dust.scale *= Main.rand.NextFloat(1f, 1.5f);
									dust.velocity *= Main.rand.NextFloat(0.5f, 0.75f);
								}

								for (int i = 0; i < 3; i++)
								{
									Gore gore = Gore.NewGoreDirect(owner.GetSource_FromAI(), owner.Center + new Vector2(Main.rand.NextFloat(-24f, 0f), Main.rand.NextFloat(-24f, 0f)), Vector2.UnitY.RotatedByRandom(MathHelper.Pi), 61 + Main.rand.Next(3));
									gore.rotation = Main.rand.NextFloat(MathHelper.Pi);
									gore.scale *= Main.rand.NextFloat(0.4f, 0.66f);
									gore.velocity *= Main.rand.NextFloat(0.5f, 0.75f);
								}
							}
						}
						else
						{
							owner.velocity *= guardianItem.ParryDashMomentum;

							for (int i = 0; i < 5; i++)
							{
								Dust dust = Dust.NewDustDirect(owner.Center, 0, 0, DustID.Smoke);
								dust.scale *= Main.rand.NextFloat(1f, 1.5f);
								dust.velocity *= Main.rand.NextFloat(0.5f, 0.75f);
							}

							for (int i = 0; i < 3; i++)
							{
								Gore gore = Gore.NewGoreDirect(owner.GetSource_FromAI(), owner.Center + new Vector2(Main.rand.NextFloat(-24f, 0f), Main.rand.NextFloat(-24f, 0f)), Vector2.UnitY.RotatedByRandom(MathHelper.Pi), 61 + Main.rand.Next(3));
								gore.rotation = Main.rand.NextFloat(MathHelper.Pi);
								gore.scale *= Main.rand.NextFloat(0.4f, 0.66f);
								gore.velocity *= Main.rand.NextFloat(0.5f, 0.75f);
							}
						}

						Projectile.rotation = KatarDashAngle;
						Projectile.Center = owner.MountedCenter.Floor() + Vector2.UnitY.RotatedBy(KatarDashAngle) * -10f;
					}
					else
					{
						Projectile.rotation = LinkedKatarAnchor.KatarDashAngle;
						Projectile.Center = owner.MountedCenter.Floor() + Vector2.UnitY.RotatedBy(LinkedKatarAnchor.KatarDashAngle) * -10f + new Vector2(owner.direction * 3f, -3f);
					}

					KatarDashTimer--;
				}
				else if (Slamming || (OffHandKatar && LinkedKatarAnchor.Slamming))
				{
					Projectile projectile;
					if (OffHandKatar)
					{
						projectile = LinkedKatarAnchor.Projectile;
						float animTime = projectile.localAI[1] / LinkedKatarAnchor.SlamTime;
						float fistDist = projectile.ai[0] == -1f ? 18f : 23f;
						float addedDistance = (float)Math.Sin((animTime - 0.33f) * ((1 - animTime) * 5.5f - 4.4f) - 0.2f) * -animTime * fistDist;
						Projectile.Center = owner.MountedCenter.Floor() + new Vector2(4 * owner.direction, 0) + Vector2.UnitY.RotatedBy(projectile.ai[1]) * addedDistance + new Vector2(owner.direction * 3f, -3f); ;
					}
					else
					{
						projectile = Projectile;
						if (projectile.localAI[1] == 0f) // Register base slam length
						{
							SlamTime = (projectile.ai[0] == -1f ? 30f : 35f) / (guardianItem.JabSpeed * owner.GetAttackSpeed<MeleeDamageClass>());
							projectile.localAI[1] = SlamTime;
							guardian.GauntletPunchCooldown = (int)SlamTime / 2 - 1;
						}
						float animTime = Projectile.localAI[1] / SlamTime;
						float fistDist = Projectile.ai[0] == -1f ? 15f : 20f;
						float addedDistance = (float)Math.Sin((animTime - 0.33f) * ((1 - animTime) * 5.5f - 4.4f) - 0.2f) * -animTime * fistDist;
						Projectile.Center = owner.MountedCenter.Floor() + new Vector2(4 * owner.direction, 0) + Vector2.UnitY.RotatedBy(Projectile.ai[1]) * addedDistance;

						if (!IsLocalOwner)
						{ // Rotates the player in the direction of the punch for other clients
							Vector2 puchDir = (Projectile.ai[1] + MathHelper.PiOver2).ToRotationVector2();
							if (puchDir.X > 0 && owner.direction != 1) owner.ChangeDir(1);
							else if (puchDir.X < 0 && owner.direction != -1) owner.ChangeDir(-1);
						}
						else if (Projectile.localAI[1] == SlamTime)
						{ // Slam just started, make projectile
							int damage = guardian.GetGuardianDamage(guardianItem.Item.damage);
							bool charged = Projectile.ai[0] == -2f;
							if (guardianItem.OnJab(owner, guardian, Projectile, OffHandKatar, Ding, ref charged, ref damage))
							{
								if (owner.boneGloveItem != null && !owner.boneGloveItem.IsAir && owner.boneGloveTimer == 0)
								{ // Bone glove compatibility, from vanilla code
									owner.boneGloveTimer = 60;
									Vector2 center = owner.Center;
									Vector2 vector = owner.DirectionTo(owner.ApplyRangeCompensation(0.2f, center, Main.MouseWorld)) * 10f;
									Projectile.NewProjectile(owner.GetSource_ItemUse(owner.boneGloveItem), center.X, center.Y, vector.X, vector.Y, ProjectileID.BoneGloveProj, 25, 5f, owner.whoAmI);
								}

								int projectileType = ModContent.ProjectileType<KatarJabProjectile>();
								float strikeVelocity = guardianItem.JabVelocity * (charged ? 1f : 0.75f) * guardianItem.Item.GetGlobalItem<GuardianPrefixItem>().GetSlamDistance() * owner.GetTotalAttackSpeed(DamageClass.Melee);
								Vector2 velocity = Vector2.UnitY.RotatedBy((Main.MouseWorld - owner.MountedCenter).ToRotation() - MathHelper.PiOver2) * strikeVelocity * 0.25f;
								Projectile punchProj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, velocity, projectileType, 1, 1f, owner.whoAmI, charged ? 1f : 0f, OffHandKatar ? 1f : 0f);
								if (punchProj.ModProjectile is KatarJabProjectile jab)
								{
									jab.KatarItem = KatarItem.ModItem as OrchidModGuardianKatar;
									punchProj.damage = damage;
									punchProj.CritChance = (int)(owner.GetCritChance<GuardianDamageClass>() + owner.GetCritChance<GenericDamageClass>() + guardianItem.Item.crit);
									punchProj.knockBack = guardianItem.Item.knockBack;
									//punchProj.position += punchProj.velocity * 0.5f;
									punchProj.velocity += owner.velocity * 0.375f;

									if (!charged) punchProj.damage = (int)(punchProj.damage * guardianItem.SlamDamage);
									guardianItem.PlayPunchSound(owner, guardian, Projectile, charged);

									punchProj.netUpdate = true;
								}
								else punchProj.Kill();
							}
							Ding = false;
						}
					}

					if (projectile.ai[1] < 1f && projectile.ai[1] > -1f)
					{ // Offset the gauntlet when aiming down
						int offset = 2;
						if (projectile.ai[1] < 0.7f && projectile.ai[1] > -0.7f) offset += 2;
						if (projectile.ai[1] < 0.4f && projectile.ai[1] > -0.4f) offset += 2;
						Projectile.position.Y += offset;
						Projectile.position.X -= offset * owner.direction;
					}

					Projectile.rotation = projectile.ai[1];
					if (owner.direction == 1) Projectile.rotation += MathHelper.Pi;

					Projectile.localAI[1]--;
					if (projectile.localAI[1] <= (OffHandKatar ? 1 : 0))
					{
						if (!OffHandKatar)
						{
							Projectile.localAI[1] = 0f;
							Projectile.ai[0] = 0;
							Projectile.ai[1] = 0;
						}

						if (owner.direction == -1) Projectile.rotation += MathHelper.Pi; // weird issue fix, katars flips for 1 frame at the end of a punch when facing left
					}
				}
				else
				{
					if (Charging || OffHandKatar && LinkedKatarAnchor.Charging)
					{
						if (!OffHandKatar)
						{ // Unlike gauntlets, Katars aren't asynchronous, so the main hand katar will always be the "one charging"
							guardian.GuardianItemCharge += 30f / KatarItem.useTime * (owner.GetTotalAttackSpeed(DamageClass.Melee) * 2f - 1f) * guardianItem.ChargeSpeedMultiplier;
							if (guardian.GuardianItemCharge > 180f)
							{
								if (!Ding && IsLocalOwner)
								{
									if (ModContent.GetInstance<OrchidClientConfig>().GuardianAltChargeSounds) SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, owner.Center);
									else SoundEngine.PlaySound(SoundID.MaxMana, owner.Center);
									Ding = true;
								}
								guardian.GuardianItemCharge = 180f;
							}
						}

						if ((ModContent.GetInstance<OrchidClientConfig>().GuardianSwapGauntletImputs ? !Main.mouseRight : !Main.mouseLeft) && owner.whoAmI == Main.myPlayer)
						{
							if (!OffHandKatar)
							{
								if (guardian.GuardianItemCharge >= 180f) Projectile.ai[0] = -2f;
								else Projectile.ai[0] = -1f;

								guardian.GuardianItemCharge = 0;

								if (IsLocalOwner)
								{
									Projectile.ai[1] = Vector2.Normalize(Main.MouseWorld - owner.MountedCenter).ToRotation() - MathHelper.PiOver2;
									Projectile.ai[2] = 0f;
									Projectile.netUpdate = true;
								}
							}
						}
						else
						{
							Projectile.Center = owner.MountedCenter.Floor() + new Vector2((2 - guardian.GuardianItemCharge * 0.02f) * owner.direction, 4);
							if (OffHandKatar) Projectile.position += new Vector2(4 * owner.direction, -1);
							Projectile.rotation = MathHelper.PiOver2;
						}
					}
					else
					{
						Projectile.Center = owner.MountedCenter.Floor() + new Vector2(0f, 10f);
						if (OffHandKatar) Projectile.position.X += 4 * owner.direction;

						if (owner.velocity.X != 0)
						{
							Projectile.position.X -= 2 * owner.direction;
							Projectile.position.Y -= 4;
							Projectile.rotation = MathHelper.PiOver2 + MathHelper.PiOver4 * owner.direction * 0.75f;
						}
						else
						{
							Projectile.position.X += 2 * owner.direction;
							Projectile.rotation = MathHelper.Pi - MathHelper.PiOver4 * owner.direction;
						}
					}
				}

				if (!OffHandKatar)
				{ // Composite arm stuff for the front arm (the back arm is disabled while holding gauntlets)
					float rotation = (Projectile.Center + new Vector2(6 * owner.direction, Slamming ? 2 : Charging ? 8 : 6) - owner.MountedCenter.Floor()).ToRotation();
					CompositeArmStretchAmount compositeArmStretchAmount = CompositeArmStretchAmount.ThreeQuarters; // Tweak the arm based on punch direction if necessary
					if (owner.velocity.X != 0) compositeArmStretchAmount = CompositeArmStretchAmount.Quarter;
					if (Charging) compositeArmStretchAmount = CompositeArmStretchAmount.Quarter;
					if (Projectile.localAI[1] > 0.55f && (Projectile.ai[1] > -2.25f || Projectile.ai[1] < -4f)) compositeArmStretchAmount = CompositeArmStretchAmount.Full;
					owner.SetCompositeArmFront(true, compositeArmStretchAmount, rotation - MathHelper.PiOver2);
				}
			}

			guardianItem.ExtraAIKatar(owner, guardian, Projectile, OffHandKatar);
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 3; i++)
			{
				Main.dust[Dust.NewDust(Projectile.Center, 0, 0, DustID.Smoke)].velocity *= 0.25f;
			}
		}

		public void spawnDusts()
		{
			Vector2 pos = new Vector2(Projectile.position.X, Projectile.position.Y);
			for (int i = 0; i < 3; i++)
			{
				Dust dust = Dust.NewDustDirect(pos, 20, 20, DustID.Smoke);
				dust.scale *= 0.75f;
				dust.velocity *= 0.25f;
			}
		}

		public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
		{
			if (SelectedItem < 0 || SelectedItem > 58) return false;
			if (!(KatarItem.ModItem is OrchidModGuardianKatar guardianItem)) return false;
			if (!ModContent.HasAsset(guardianItem.KatarTexture)) return false;

			Player player = Main.player[Projectile.owner];
			OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
			Color color = Lighting.GetColor((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f), Color.White);

			if (guardianItem.PreDrawKatar(spriteBatch, Projectile, player, OffHandKatar, ref color))
			{
				Texture2D texture = guardianItem.GetKatarTexture(player, Projectile, OffHandKatar, out Rectangle? drawRectangle, KatarAnimFrame);

				var effect = SpriteEffects.None;
				if (player.direction != 1)
				{
					GuardianKatarAnchor anchor = OffHandKatar ? LinkedKatarAnchor : this;
					if (player.velocity.X != 0 && anchor.KatarDashTimer <= 0 || (player.GetModPlayer<OrchidGuardian>().GuardianItemCharge > 0 && anchor.Projectile.ai[2] != 0) || anchor.Slamming) effect = SpriteEffects.FlipVertically;
					else effect = SpriteEffects.FlipHorizontally;
				}

				float drawRotation = Projectile.rotation;
				Vector2 posproj = Projectile.Center;
				if (player.gravDir == -1)
				{
					drawRotation = -drawRotation;
					posproj.Y = (player.Bottom + player.position).Y - posproj.Y + (posproj.Y - player.Center.Y) * 2f;
					if (effect == SpriteEffects.FlipVertically)
					{
						effect = SpriteEffects.None;
					}
					else if (effect == SpriteEffects.FlipHorizontally)
					{
						effect = SpriteEffects.None;
						drawRotation += MathHelper.Pi;
					}
					else if (effect == SpriteEffects.None)
					{
						effect = SpriteEffects.FlipVertically;
					}
				}

				var drawPosition = Vector2.Transform(posproj - Main.screenPosition + Vector2.UnitY * player.gfxOffY, Main.GameViewMatrix.EffectMatrix);
				float rotation = Projectile.rotation;
				Vector2 origin = drawRectangle == null ? texture.Size() * 0.5f : drawRectangle.GetValueOrDefault().Size() * 0.5f;
				spriteBatch.Draw(texture, drawPosition, drawRectangle, color, drawRotation, origin, Projectile.scale, effect, 0f);

				Texture2D textureGlow = guardianItem.GetGlowmaskTexture(player, Projectile, OffHandKatar, out Rectangle? drawRectangleGlow);
				if (textureGlow != null)
				{
					Color glowColor = guardianItem.GetKatarGlowmaskColor(player, guardian, Projectile, lightColor);
					spriteBatch.Draw(textureGlow, drawPosition, drawRectangle, glowColor, drawRotation, origin, Projectile.scale, effect, 0f);
				}
			}
			guardianItem.PostDrawKatar(spriteBatch, Projectile, player, OffHandKatar, color);

			return false;
		}
	}
}