using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common;
using OrchidMod.Content.General.Prefixes;
using OrchidMod.Content.Guardian.Weapons.Misc;
using OrchidMod.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.Player;

namespace OrchidMod.Content.Guardian.Projectiles.Misc
{
	public class GuardianNeedleAnchor : OrchidModGuardianParryAnchor
	{
		public List<Vector2> OldPosition;
		public List<float> OldRotation;

		public int TimeSpent = 0;
		public bool Ding = false;
		public bool NeedNetUpdate = false;
		public Rectangle[] HitBox;

		public int SelectedItem { get; set; } = -1;
		public Item GuardianItem => Main.player[Projectile.owner].inventory[SelectedItem];
		public Texture2D ItemTexture;

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(SelectedItem);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			SelectedItem = reader.ReadInt32();
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
			Projectile.localNPCHitCooldown = -1;
			Projectile.netImportant = true;

			HitBox = [new Rectangle(0, 0, 30, 30), new Rectangle(0, 0, 30, 30), new Rectangle(0, 0, 30, 30)];
			OldPosition = new List<Vector2>();
			OldRotation = new List<float>();
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overPlayers.Add(index);

		public void OnChangeSelectedItem(Player owner)
		{
			OrchidGuardian guardian = owner.GetModPlayer<OrchidGuardian>();
			Projectile.ai[0] = 0f;
			guardian.GuardianItemCharge = 0;
			SelectedItem = owner.selectedItem;
			Projectile.netUpdate = true;
			Projectile.friendly = false;

			if (GuardianItem.ModItem is GuardianNeedle guardianItem)
			{
				ItemTexture = ModContent.Request<Texture2D>(Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				ResetSize();
			}
			else if (IsLocalOwner)
			{
				Projectile.Kill();
			}
		}

		public override void AI()
		{
			var owner = Main.player[Projectile.owner];
			OrchidGuardian guardian = owner.GetModPlayer<OrchidGuardian>();

			if (SelectedItem < 0 || GuardianItem == null || GuardianItem.ModItem is not GuardianNeedle guardianItem || owner.HeldItem.ModItem is not GuardianNeedle || !owner.active || owner.dead)
			{ // Kills the projectile if anything is wrong
				Projectile.Kill();
				return;
			}
			else
			{
				if (IsLocalOwner)
				{ // Player rotation & Item netupdate
					if (Main.MouseWorld.X > owner.Center.X && owner.direction != 1) owner.ChangeDir(1);
					else if (Main.MouseWorld.X < owner.Center.X && owner.direction != -1) owner.ChangeDir(-1);
					if (NeedNetUpdate)
					{
						NeedNetUpdate = false;
						Projectile.netUpdate = true;
					}
				}
				else
				{
					if (Projectile.ai[0] == 0f)
					{ // Adresses a visual issue
						guardian.GuardianItemCharge = 0;
					}
				}

				TimeSpent++;
				Projectile.timeLeft = 5;

				if (Projectile.ai[2] > 0f)
				{ // Blocking
					Projectile.friendly = false;
					guardian.GuardianParry = true;
					guardian.GuardianParryBuffer = true;

					Projectile.Center = owner.MountedCenter.Floor() + new Vector2(10f * owner.direction, -2f);
					Projectile.rotation = MathHelper.PiOver4 * 0.55f * owner.direction - MathHelper.PiOver4;

					owner.SetCompositeArmFront(true, CompositeArmStretchAmount.Full, MathHelper.PiOver2 * -0.9f * owner.direction);
					owner.SetCompositeArmBack(true, CompositeArmStretchAmount.Quarter, MathHelper.PiOver2 * -1.4f * owner.direction);

					if (OldPosition.Count > 0)
					{
						OldPosition.RemoveAt(0);
						OldRotation.RemoveAt(0);
					}

					if (Projectile.ai[2] > 10f)
					{
						owner.velocity = (Projectile.ai[1] + MathHelper.PiOver2).ToRotationVector2() * 20f * (1f + (owner.moveSpeed - 1f) * 0.2f);
						owner.armorEffectDrawShadow = true;
						owner.fallStart = (int)(owner.position.Y / 16f);
						owner.fallStart2 = (int)(owner.position.Y / 16f);
					}
					else
					{
						owner.velocity *= 0.9f;
					}

					Projectile.ai[2]--;
					if (owner.immune)
					{
						if (owner.eocHit != -1)
						{
							guardian.DoParryItemParry(Main.npc[owner.eocHit]);
						}
						else
						{
							Projectile.ai[2] = 0f;
							guardian.GuardianGuardRecharging += Projectile.ai[2] / (guardianItem.ParryDuration * guardianItem.Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianParryDuration);
							Rectangle rect = owner.Hitbox;
							rect.Y -= 64;
							CombatText.NewText(guardian.Player.Hitbox, Color.LightGray, Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.Interrupted"), false, true);
						}
					}
					else if (Projectile.ai[2] <= 0f)
					{
						Projectile.ai[2] = 0f;
					}

					if (Projectile.scale > 1f)
					{
						ResetSize();
					}
				}
				else if (Projectile.ai[2] < 0f)
				{ // Counterattacking
					if (Projectile.ai[2] == -41f)
					{ // First frame of the swing
						Projectile.damage = guardian.GetGuardianDamage(GuardianItem.damage * 2f);
						Projectile.CritChance = guardian.GetGuardianCrit(GuardianItem.crit);
						Projectile.knockBack = GuardianItem.knockBack;
						Projectile.friendly = true;
						Projectile.ResetLocalNPCHitImmunity();
						SoundEngine.PlaySound(GuardianItem.UseSound, Projectile.Center);
						ResetHitStatus(true);
					}

					if (Projectile.ai[1] > -3.14f && Projectile.ai[1] < 0f)
					{ // Facing Right
						if (owner.direction != 1)
						{
							owner.ChangeDir(1);
						}
					}
					else
					{
						if (owner.direction != -1)
						{
							owner.ChangeDir(-1);
						}
					}

					Projectile.rotation = Projectile.ai[1] - MathHelper.PiOver4 + (float)Math.Cos(0.102f * (-Projectile.ai[2] - 10)) * 1.9f * -owner.direction + MathHelper.Pi;
					Projectile.Center = owner.MountedCenter.Floor() + Vector2.UnitY.RotatedBy(Projectile.ai[1] + (float)Math.Cos(0.102f * (-Projectile.ai[2] - 10)) * 1.8f * -owner.direction) * 24f;
					owner.SetCompositeArmFront(true, CompositeArmStretchAmount.Full, MathHelper.PiOver4 * owner.direction + Projectile.ai[1] + 0.1f - (float)Math.Cos(0.102f * (-Projectile.ai[2] - 10)) * owner.direction);
					owner.SetCompositeArmBack(true, CompositeArmStretchAmount.Full, Projectile.ai[1] - 0.1f + (float)Math.Cos(0.102f * (-Projectile.ai[2] - 10)) * 0.2f * owner.direction);

					// Trail
					OldPosition.Add(Projectile.Center);
					OldRotation.Add(Projectile.rotation);

					if (OldPosition.Count > 10)
					{
						OldPosition.RemoveAt(0);
						OldRotation.RemoveAt(0);
					}

					// Animation progress
					Projectile.ai[2] += 2f * owner.GetTotalAttackSpeed(DamageClass.Melee);

					if (Projectile.ai[2] >= -1f)
					{
						Projectile.ai[2] = 0f;
						Projectile.ai[1] = 0f;
						Projectile.friendly = false;
					}
				}
				else if (Projectile.ai[0] == 1f)
				{ // Being charged by the player
					if (guardian.GuardianItemCharge < 180f)
					{ // Increase guardian charge
						guardian.GuardianItemCharge += 30f / guardianItem.Item.useTime * owner.GetTotalAttackSpeed(DamageClass.Melee);
						if (guardian.GuardianItemCharge > 180f) guardian.GuardianItemCharge = 180f;
					}

					Projectile.Center = owner.MountedCenter.Floor() + new Vector2((16f - guardian.GuardianItemCharge * 0.03f) * owner.direction, guardian.GuardianItemCharge * 0.045f);
					Projectile.rotation = MathHelper.PiOver4 * (1.75f + guardian.GuardianItemCharge * 0.0015f) * owner.direction - MathHelper.PiOver4;
					owner.SetCompositeArmFront(true, CompositeArmStretchAmount.ThreeQuarters, MathHelper.PiOver2 * -(0.6f - guardian.GuardianItemCharge * 0.0025f) * owner.direction);

					if (OldPosition.Count > 0)
					{
						OldPosition.RemoveAt(0);
						OldRotation.RemoveAt(0);
					}

					if (IsLocalOwner)
					{
						if (guardian.GuardianItemCharge >= 180f && !Ding)
						{ // Sound cue when fully charged
							Ding = true;
							if (ModContent.GetInstance<OrchidClientConfig>().GuardianAltChargeSounds) SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, owner.Center);
							else SoundEngine.PlaySound(SoundID.MaxMana, owner.Center);
						}

						bool jabInput = Main.mouseRight;
						bool chargeInput = Main.mouseLeft;

						if (ModContent.GetInstance<OrchidClientConfig>().GuardianSwapGauntletImputs)
						{
							jabInput = Main.mouseLeft;
							chargeInput = Main.mouseRight;
						}

						if (!chargeInput)
						{
							if (guardian.GuardianItemCharge >= 180f || guardian.UseSlam(1, true))
							{ // dash
								/*
								if (Main.MouseWorld.X > owner.position.X)
								{
									Projectile.ai[1] = Vector2.Normalize(owner.MountedCenter + Vector2.UnitX - owner.MountedCenter).ToRotation() - MathHelper.PiOver2;
								}
								else
								{
									Projectile.ai[1] = Vector2.Normalize(owner.MountedCenter - Vector2.UnitX - owner.MountedCenter).ToRotation() - MathHelper.PiOver2;
								}
								*/
								Projectile.ai[1] = Vector2.Normalize(Main.MouseWorld - owner.MountedCenter).ToRotation() - MathHelper.PiOver2;

								Projectile.ai[0] = 51f;
								if (guardian.GuardianItemCharge < 180f)
								{
									guardian.UseSlam(1);
								}
							}
							else
							{ // Not fully charged, jab instead
								Projectile.ai[1] = Vector2.Normalize(Main.MouseWorld - owner.MountedCenter).ToRotation() - MathHelper.PiOver2;
								Projectile.ai[0] = -40f;
							}

							guardian.GuardianItemCharge = 0;
							Projectile.netUpdate = true;
						}
						else if (jabInput)
						{
							Projectile.ai[0] = -40f;
							Projectile.ai[1] = Vector2.Normalize(Main.MouseWorld - owner.MountedCenter).ToRotation() - MathHelper.PiOver2;
							Projectile.netUpdate = true;
						}
					}

					if (Projectile.scale > 1f)
					{
						ResetSize();
					}
				}
				else if (Projectile.ai[0] < 0)
				{ // Jabbing
					if (Projectile.ai[0] == -40f)
					{ // First frame of the jab
						Projectile.damage = guardian.GetGuardianDamage(GuardianItem.damage);
						Projectile.CritChance = guardian.GetGuardianCrit(GuardianItem.crit);
						Projectile.knockBack = GuardianItem.knockBack;
						Projectile.friendly = true;
						Projectile.ResetLocalNPCHitImmunity();
						SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.Center);
						ResetHitStatus(false);
						OldPosition.Clear();
						OldRotation.Clear();

						if (owner.boneGloveItem != null && !owner.boneGloveItem.IsAir && owner.boneGloveTimer == 0)
						{ // Bone glove compatibility, from vanilla code
							owner.boneGloveTimer = 60;
							Vector2 center = owner.Center;
							Vector2 vector = owner.DirectionTo(owner.ApplyRangeCompensation(0.2f, center, Main.MouseWorld)) * 10f;
							Projectile.NewProjectile(owner.GetSource_ItemUse(owner.boneGloveItem), center.X, center.Y, vector.X, vector.Y, ProjectileID.BoneGloveProj, 25, 5f, owner.whoAmI);
						}
					}

					if (Projectile.ai[1] > -3.14f && Projectile.ai[1] < 0f)
					{ // Facing Right
						if (owner.direction != 1)
						{
							owner.ChangeDir(1);
						}
					}
					else
					{
						if (owner.direction != -1)
						{
							owner.ChangeDir(-1);
						}
					}

					if (Projectile.ai[0] >= -30)
					{ // Returning
						Projectile.friendly = false;
						Projectile.rotation = Projectile.ai[1] - MathHelper.PiOver4 + (float)Math.Sin(0.1046f * (30 + Projectile.ai[0])) * 0.4f * -owner.direction + MathHelper.Pi;
						Projectile.Center = owner.MountedCenter.Floor() + Vector2.UnitY.RotatedBy(Projectile.ai[1]) * (38f - (float)Math.Sin(0.0523f * (30 + Projectile.ai[0])) * 24f);
						Projectile.position.Y -= (float)Math.Sin(0.0523f * (30 + Projectile.ai[0])) * 2f;
						owner.SetCompositeArmFront(true, CompositeArmStretchAmount.Quarter, MathHelper.PiOver4 * owner.direction + Projectile.ai[1] + 0.1f + (float)Math.Sin(0.1046f * (30 + Projectile.ai[0])) * 0.3f * owner.direction);
						owner.SetCompositeArmBack(true, CompositeArmStretchAmount.Full, Projectile.ai[1] - 0.1f + (float)Math.Sin(0.1046f * (30 + Projectile.ai[0])) * 0.2f * owner.direction);
					}
					else
					{ // Jabbing
						Projectile.Center = owner.MountedCenter.Floor() + Vector2.UnitY.RotatedBy(Projectile.ai[1]) * 3.8f * (Projectile.ai[0] + 40);
						Projectile.rotation = Projectile.ai[1] - MathHelper.PiOver4 + MathHelper.Pi;
						owner.SetCompositeArmFront(true, CompositeArmStretchAmount.None, MathHelper.PiOver4 * owner.direction + Projectile.ai[1] + 0.1f);
						owner.SetCompositeArmBack(true, CompositeArmStretchAmount.ThreeQuarters, Projectile.ai[1] - 0.1f);
					}

					// Trail
					OldPosition.Add(Projectile.Center);
					OldRotation.Add(Projectile.rotation);

					if (OldPosition.Count > 8)
					{
						OldPosition.RemoveAt(0);
						OldRotation.RemoveAt(0);
					}

					// Animation progress
					Projectile.ai[0] += 1.25f * owner.GetTotalAttackSpeed(DamageClass.Melee);

					if (Projectile.ai[0] >= 0)
					{
						if (guardian.GuardianItemCharge > 0)
						{
							Projectile.ai[0] = 1f;
						}
						else
						{
							Projectile.ai[0] = 0f;
						}

						Projectile.ai[1] = 0f;
						Projectile.friendly = false;

						if (Main.mouseLeft && Main.mouseRight)
						{ // Perfect jab loop while holding the attack
							Projectile.ai[0] = -40f;
							Projectile.ai[1] = Vector2.Normalize(Main.MouseWorld - owner.MountedCenter).ToRotation() - MathHelper.PiOver2;
							Projectile.netUpdate = true;

							if (guardian.GuardianItemCharge <= 0)
							{ // Fix an occurence where jabs loop and the player doesn't have any charge
								guardian.GuardianItemCharge = 1f;
							}
						}
					}
				}
				else if (Projectile.ai[0] > 1f)
				{ // Dash (charged attack)
					if (Projectile.ai[0] == 51f)
					{ // First frame of the swing
						Projectile.damage = guardian.GetGuardianDamage(GuardianItem.damage * 2f);
						Projectile.CritChance = guardian.GetGuardianCrit(GuardianItem.crit);
						Projectile.knockBack = GuardianItem.knockBack;
						Projectile.friendly = true;
						Projectile.ResetLocalNPCHitImmunity();
						SoundEngine.PlaySound(SoundID.DD2_GoblinBomberThrow, Projectile.Center);
						ResetHitStatus(true);

						for (int i = 0; i < 7; i++)
						{
							Dust dust = Dust.NewDustDirect(owner.Center, 0, 0, DustID.Smoke);
							dust.scale *= Main.rand.NextFloat(1f, 1.5f);
							dust.velocity *= Main.rand.NextFloat(0.5f, 0.75f);
						}

						for (int i = 0; i < 5; i++)
						{
							Gore gore = Gore.NewGoreDirect(owner.GetSource_FromAI(), owner.Center + new Vector2(Main.rand.NextFloat(-24f, 0f), Main.rand.NextFloat(-24f, 0f)), Vector2.UnitY.RotatedByRandom(MathHelper.Pi), 61 + Main.rand.Next(3));
							gore.rotation = Main.rand.NextFloat(MathHelper.Pi);
						}

						if (owner.boneGloveItem != null && !owner.boneGloveItem.IsAir && owner.boneGloveTimer == 0)
						{ // Bone glove compatibility, from vanilla code
							owner.boneGloveTimer = 60;
							Vector2 center = owner.Center;
							Vector2 vector = owner.DirectionTo(owner.ApplyRangeCompensation(0.2f, center, Main.MouseWorld)) * 10f;
							Projectile.NewProjectile(owner.GetSource_ItemUse(owner.boneGloveItem), center.X, center.Y, vector.X, vector.Y, ProjectileID.BoneGloveProj, 25, 5f, owner.whoAmI);
						}
					}

					if (Projectile.ai[1] > -3.14f && Projectile.ai[1] < 0f)
					{ // Facing Right
						if (owner.direction != 1)
						{
							owner.ChangeDir(1);
						}
					}
					else
					{
						if (owner.direction != -1)
						{
							owner.ChangeDir(-1);
						}
					}

					Projectile.Center = owner.MountedCenter.Floor() + Vector2.UnitY.RotatedBy(Projectile.ai[1]) * 3.8f * (-Projectile.ai[0] * 0.15f + 10);
					Projectile.rotation = Projectile.ai[1] - MathHelper.PiOver4 + MathHelper.Pi;
					owner.SetCompositeArmFront(true, CompositeArmStretchAmount.None, MathHelper.PiOver4 * owner.direction + Projectile.ai[1] + 0.1f);
					owner.SetCompositeArmBack(true, CompositeArmStretchAmount.ThreeQuarters, Projectile.ai[1] - 0.1f);

					if (Projectile.ai[0] > 20f)
					{
						owner.fallStart = (int)(owner.position.Y / 16f);
						owner.fallStart2 = (int)(owner.position.Y / 16f);
						owner.velocity = (Projectile.ai[1] + MathHelper.PiOver2).ToRotationVector2() * 15f * (1f + (owner.moveSpeed - 1f) * 0.2f);
						owner.armorEffectDrawShadow = true;
					}
					else
					{
						owner.velocity *= 0.9f;
					}

					if (guardian.modPlayer.PlayerImmunity < 10)
					{
						guardian.modPlayer.PlayerImmunity = 10;
					}

					// Trail
					OldPosition.Add(Projectile.Center);
					OldRotation.Add(Projectile.rotation);

					if (OldPosition.Count > 15)
					{
						OldPosition.RemoveAt(0);
						OldRotation.RemoveAt(0);
					}

					// Animation progress
					Projectile.ai[0] -= 2f;

					if (Projectile.ai[0] <= 1f)
					{
						Projectile.ai[0] = 0f;
						Projectile.ai[1] = 0f;
						Projectile.friendly = false;
					}
				}
				else
				{ // Idle - needle is held further and lower
					Ding = false;
					Projectile.Center = owner.MountedCenter.Floor() + new Vector2(8f * owner.direction, 12f);
					Projectile.rotation = MathHelper.PiOver4 * 2.2f * owner.direction - MathHelper.PiOver4;
					owner.SetCompositeArmFront(true, CompositeArmStretchAmount.Full, MathHelper.PiOver2 * -0.05f * owner.direction);

					if (Math.Abs(owner.velocity.Y) > 0.01f)
					{ // slightly lower the weapon while jumping
						Projectile.rotation += 0.25f * owner.direction;
						Projectile.position.Y += 2f;
					}

					if (OldPosition.Count > 0)
					{
						OldPosition.RemoveAt(0);
						OldRotation.RemoveAt(0);
					}
				}

				// Hitbox management for jabs and swings
				Vector2 position = Vector2.UnitY.RotatedBy(Projectile.rotation + MathHelper.PiOver4) * (Projectile.width - 15);
				HitBox[0].X = (int)(Projectile.Center.X - position.X * 0.5f) - 15;
				HitBox[0].Y = (int)(Projectile.Center.Y - position.Y * 0.5f) - 15;
				HitBox[1].X = (int)(Projectile.Center.X - position.X * 0.2f) - 15;
				HitBox[1].Y = (int)(Projectile.Center.Y - position.Y * 0.2f) - 15;
				HitBox[2].X = (int)(Projectile.Center.X - position.X * -0.1f) - 15;
				HitBox[2].Y = (int)(Projectile.Center.Y - position.Y * -0.1f) - 15;

				Projectile.velocity = Vector2.UnitX * 0.001f * owner.direction; // So enemies are KBd in the right direction

				// Hitbox display for testing
				/*
				if (Projectile.friendly)
				{
					for (int i = 0; i < 30; i++)
					{
						Dust.NewDustDirect(HitBox[0].TopLeft(), HitBox[0].Width, HitBox[0].Height, DustID.RedTorch).noGravity = true;
					}

					for (int i = 0; i < 30; i++)
					{
						Dust.NewDustDirect(HitBox[1].TopLeft(), HitBox[1].Width, HitBox[1].Height, DustID.GreenTorch).noGravity = true;
					}

					for (int i = 0; i < 30; i++)
					{
						Dust.NewDustDirect(HitBox[2].TopLeft(), HitBox[2].Width, HitBox[2].Height, DustID.BlueTorch).noGravity = true;
					}
				}
				*/
			}
		}

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidGuardian guardian)
		{
			if (GuardianItem.ModItem is GuardianNeedle guardianItem)
			{
				if (Projectile.ai[0] < 0f)
				{ // Jab
					if (FirstHit)
					{
						if (guardian.GuardianItemCharge > 0f)
						{
							guardian.GuardianItemCharge += 60f * player.GetTotalAttackSpeed(DamageClass.Melee);
							if (guardian.GuardianItemCharge > 180f)
							{
								guardian.GuardianItemCharge = 180f;
							}
						}
					}
				}
			}
		}

		public override void SafeModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (GuardianItem.ModItem is GuardianNeedle guardianItem)
			{
				Player player = Owner;
				OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();

				if (target.position.Y > player.Center.Y && Math.Abs(player.velocity.Y) != 0f)
				{
					player.velocity.Y = -7f * player.moveSpeed + (target.velocity.Y < 0 ? target.velocity.Y : 0);
					guardian.GuardianGuardRecharging += 0.5f;
					modifiers.FinalDamage *= 1.5f;
				}
			}
		}

		public void ResetSize()
		{
			int length = (int)Math.Sqrt(2 * (ItemTexture.Width * GuardianItem.scale * ItemTexture.Width * GuardianItem.scale));
			Projectile.width = length + 4;
			Projectile.height = length + 4;
			Projectile.scale = GuardianItem.scale;
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 5; i++)
			{
				Main.dust[Dust.NewDust(Projectile.Center, 0, 0, DustID.Smoke)].velocity *= 0.25f;
			}
		}

		public override bool? CanHitNPC(NPC target)
		{ // hitting wiith the end of the staff or spinning
			if (target.Hitbox.Intersects(HitBox[0]) || target.Hitbox.Intersects(HitBox[1]) || target.Hitbox.Intersects(HitBox[2]) || Projectile.ai[2] < 0f) return base.CanHitNPC(target);
			return false;
		}

		public override bool? CanCutTiles() => Projectile.friendly = true;

		public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
		{
			if (SelectedItem < 0 || SelectedItem > 58 || ItemTexture == null) return false;
			if (GuardianItem.ModItem is not GuardianNeedle guardianItem) return false;

			var player = Main.player[Projectile.owner];
			SpriteEffects effect = SpriteEffects.None;
			float rotationoffset = 0f;

			if (player.direction == -1)
			{
				effect = SpriteEffects.FlipVertically;
				rotationoffset = -MathHelper.PiOver2;
			}

			if (OldPosition.Count > 0)
			{ // attacking = draw trail
				spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
				spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });

				for (int i = 0; i < OldPosition.Count; i++)
				{
					Vector2 drawPositionTrail = OldPosition[i] - Main.screenPosition + Vector2.UnitY * player.gfxOffY;
					spriteBatch.Draw(ItemTexture, drawPositionTrail, null, lightColor * 0.05f * (i + 1), OldRotation[i] + rotationoffset, ItemTexture.Size() * 0.5f, Projectile.scale, effect, 0f);
				}

				spriteBatch.End();
				spriteBatch.Begin(spriteBatchSnapshot);
			}

			Vector2 drawPosition = Projectile.Center - Main.screenPosition + Vector2.UnitY * player.gfxOffY;
			spriteBatch.Draw(ItemTexture, drawPosition, null, lightColor, Projectile.rotation + rotationoffset, ItemTexture.Size() * 0.5f, Projectile.scale, effect, 0f);

			return false;
		}
	}
}