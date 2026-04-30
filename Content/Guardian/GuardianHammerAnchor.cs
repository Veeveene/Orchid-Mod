using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common;
using OrchidMod.Content.General.Prefixes;
using OrchidMod.Utilities;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian
{
	public class GuardianHammerAnchor : OrchidModGuardianAnchor
	{
		public List<Vector2> OldPosition;
		public List<float> OldRotation;
		public List<int> BlockedNPCs;

		public bool returning = false;
		public OrchidModGuardianHammer HammerItem;
		public Texture2D HammerTexture;
		public Texture2D HammerTextureGlow;

		public int range = 0;
		public int HitCount = 0;
		public bool penetrate;
		public bool WeakHit = false;
		public bool NeedNetUpdate = false;
		public bool FirstBlock = false;
		public int hitboxOffset;

		public int BlockDuration = 0;

		public bool Ding = false;
		
		public int HammerAnimFrame = 0;

		public bool WeakThrow => Projectile.ai[0] == 1;

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overPlayers.Add(index);

		public override void SafeSetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = false;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.scale = 1f;
			Projectile.timeLeft = 600;
			Projectile.alpha = 255;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			FirstBlock = false;

			HammerAnimFrame = 0;

			OldPosition = new List<Vector2>();
			OldRotation = new List<float>();
			BlockedNPCs = new List<int>();
		}

		public override void OnSpawn(IEntitySource source)
		{
			Player player = Main.player[Projectile.owner];
			OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
			Item item = player.inventory[player.selectedItem];

			if (item == null || !(item.ModItem is OrchidModGuardianHammer hammerItem))
			{
				if (Projectile.owner == Main.myPlayer) Projectile.Kill();
				return;
			}
			else
			{
				HammerItem = hammerItem;
				HammerTexture = TextureAssets.Item[hammerItem.Item.type].Value;
				//Projectile.width = (int)(HammerTexture.Width * hammerItem.Item.scale);
				//Projectile.height = (int)(HammerTexture.Height * hammerItem.Item.scale);
				hitboxOffset = (int)(HammerTexture.Width * guardian.GuardianWeaponScale * hammerItem.Item.scale / 2f);
				DrawOriginOffsetX = DrawOriginOffsetY = hitboxOffset;

				if (HammerItem.hasSpecialHammerTexture) HammerTexture = ModContent.Request<Texture2D>(hammerItem.HammerTexture, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

				if (ModContent.RequestIfExists<Texture2D>(hammerItem.Texture + "_Glow", out Asset<Texture2D> assetglow, AssetRequestMode.ImmediateLoad))
				{
					HammerTextureGlow = assetglow.Value;
				}


				//Projectile.position.X -= Projectile.width / 2;
				//Projectile.position.Y -= Projectile.height / 2;

				range = HammerItem.Range;
				penetrate = HammerItem.Penetrate;
				Projectile.netUpdate = true;
				Projectile.localNPCHitCooldown = hammerItem.HitCooldown;
			}
		}

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];
			OrchidGuardian guardian = owner.GetModPlayer<OrchidGuardian>();

			if (HammerItem != null)
			{
				Projectile.scale = HammerItem.Item.scale * guardian.GuardianWeaponScale;
				if (IsLocalOwner)
				{ // OnSpawn() is called too early, guardian.GuardianWeaponScale is always equal to 1f
					hitboxOffset = (int)(HammerTexture.Width * guardian.GuardianWeaponScale * HammerItem.Item.scale / 2f);
				}

				if (NeedNetUpdate)
				{
					NeedNetUpdate = false;
					Projectile.netUpdate = true;
				}

				if (BlockDuration != 0)
				{ // Blocking
					if (BlockDuration == (int)(HammerItem.BlockDuration * HammerItem.Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianBlockDuration + 10))
					{ // first frame of the block throw
						HammerItem.OnBlockThrow(owner, guardian, Projectile);
						if (Projectile.velocity.X > 0)
						{
							Projectile.spriteDirection = -Projectile.spriteDirection;
							Projectile.direction = Projectile.spriteDirection;
						}
					}

					Projectile.rotation += Projectile.velocity.Length() / 45f * (Projectile.velocity.X > 0 ? 1 : -1);

					if (BlockDuration <= HammerItem.BlockDuration * HammerItem.Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianBlockDuration)
					{ // hammers only starts to slow down 10 frames after being thrown
						Projectile.rotation += 0.25f * (Projectile.velocity.X > 0 ? 1 : -1);

						if (BlockDuration > 0)
						{
							Projectile.velocity *= 0.9f;
							Projectile.timeLeft++;
						}
						else
						{
							Projectile.tileCollide = false;
							float dist = Projectile.Center.Distance(owner.Center);
							Vector2 vel = Vector2.Normalize(owner.Center - Projectile.Center) * HammerItem.ReturnSpeed * BlockDuration * 0.2f;
							if (vel.Length() > 48f)
							{
								vel = Vector2.Normalize(vel) * 48f;
							}

							Projectile.velocity = -vel;

							if (dist < 30f && owner.whoAmI == Main.myPlayer)
							{
								Projectile.Kill();
							}
						}
					}

					Rectangle hitBox = Projectile.Hitbox; // larger hitbox for projectiles
					hitBox.X -= (int)(HammerTexture.Width / 2f) - 4;
					hitBox.Y -= (int)(HammerTexture.Width / 2f) - 4;
					hitBox.Width += HammerTexture.Width + 8;
					hitBox.Height += HammerTexture.Width + 8;

					for (int l = 0; l < Main.projectile.Length; l++)
					{
						Projectile proj = Main.projectile[l];
						if (proj.active && proj.hostile && proj.damage > 0 && !OrchidGuardian.ProjectilesBlockBlacklist.Contains(proj.type))
						{
							if (proj.Hitbox.Intersects(Projectile.Hitbox))
							{
								bool killProj = HammerItem.OnBlockProjectile(owner, guardian, Projectile, proj);
								guardian.OnBlockProjectile(Projectile, proj);
								if (!FirstBlock)
								{
									FirstBlock = true;
									guardian.OnBlockProjectileFirst(Projectile, proj);
									HammerItem.OnBlockFirstProjectile(owner, guardian, Projectile, proj);
									SoundEngine.PlaySound(SoundID.Item37.WithPitchOffset(Main.rand.NextFloat(0.4f, 0.6f)), owner.Center);
								}
								if (killProj) proj.Kill();
								SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
							}
						}
					}

					hitBox = Projectile.Hitbox;
					hitBox.X -= 2;
					hitBox.Y -= 2;
					hitBox.Width += 4;
					hitBox.Height += 4;

					for (int k = 0; k < Main.maxNPCs; k++)
					{
						NPC target = Main.npc[k];
						if (target.active && !target.dontTakeDamage && !target.friendly && target.Hitbox.Intersects(hitBox))
						{
							HammerItem.OnBlockContact(owner, guardian, target, Projectile);

							bool contained = false;
							foreach (BlockedEnemy blockedEnemy in guardian.GuardianBlockedEnemies)
							{
								if (blockedEnemy.npc == target)
								{ // Enemy already blocked, reset the timer
									blockedEnemy.time = 120;
									contained = true;
									break;
								}
							}

							if (!contained)
							{ // First time blocking an enemy
								guardian.GuardianBlockedEnemies.Add(new BlockedEnemy(target, 120));
								guardian.OnBlockNPCNew(Projectile, target);
								SoundEngine.PlaySound(SoundID.Dig, owner.Center);

								if (!BlockedNPCs.Contains(target.whoAmI))
								{
									HammerItem.OnBlockNPC(owner, guardian, target, Projectile);
									BlockedNPCs.Add(target.whoAmI);
								}
							}

							if (target.knockBackResist > 0f && BlockDuration > 0)
							{ // Push enemy if possible
								Vector2 push = target.Center - Projectile.Center;
								push.Normalize();
								target.velocity = push + Projectile.velocity;
							}

							guardian.OnBlockNPC(Projectile, target);
							if (!FirstBlock)
							{ // First block stuff
								FirstBlock = true;
								guardian.OnBlockNPCFirst(Projectile, target);
								HammerItem.OnBlockFirstNPC(owner, guardian, target, Projectile);
								SoundEngine.PlaySound(SoundID.Item37.WithPitchOffset(Main.rand.NextFloat(0.4f, 0.6f)), owner.Center);
							}
						}
					}

					OldPosition.Add(new Vector2(Projectile.Center.X, Projectile.Center.Y));
					OldRotation.Add(Projectile.rotation + MathHelper.PiOver2);
					if (OldPosition.Count > 10)
						OldPosition.RemoveAt(0);
					if (OldRotation.Count > 10)
						OldRotation.RemoveAt(0);

					BlockDuration--;

					if (BlockDuration == 0)
					{
						BlockDuration = -1;
					}
				}
				else if (Projectile.ai[1] <= 0) // Held
				{
					if (owner.dead || owner.HeldItem.ModItem is not OrchidModGuardianHammer hammerItem)
					{
						if (Projectile.owner == Main.myPlayer)
						{
							Projectile.Kill();
						}
					}
					else
					{
						if (Projectile.owner == Main.myPlayer)
						{
							if (Main.MouseWorld.X > owner.Center.X && owner.direction != 1) owner.ChangeDir(1);
							else if (Main.MouseWorld.X < owner.Center.X && owner.direction != -1) owner.ChangeDir(-1);
						}

						owner.itemAnimation = 1;
						Projectile.timeLeft = 600;
						Projectile.spriteDirection = -owner.direction;
						owner.heldProj = Projectile.whoAmI;

						if (guardian.GuardianItemCharge >= 180f && !Ding)
						{
							Ding = true;
							if (ModContent.GetInstance<OrchidClientConfig>().GuardianAltChargeSounds) SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, owner.Center);
							else SoundEngine.PlaySound(SoundID.MaxMana, owner.Center);
						}

						if (Projectile.ai[1] == 0)
						{
							if (WeakHit)
							{ // Projectiles just did a weak charge swing, kill it
								Projectile.Kill();
								return;
							}

							owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.Pi + guardian.GuardianItemCharge * 0.006f * Projectile.spriteDirection); // set arm position (90 degree offset since arm starts lowered)
							Vector2 armPosition = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, MathHelper.Pi - guardian.GuardianItemCharge * 0.006f * Projectile.spriteDirection) - (new Vector2(owner.Center.X, owner.Center.Y) - new Vector2(owner.Center.X, owner.Center.Y).Floor());
							Projectile.Center = armPosition - new Vector2(((hitboxOffset + hammerItem.HoldOffset) * 2 + 0.3f * guardian.GuardianItemCharge + (float)Math.Sin(MathHelper.Pi / 210f * guardian.GuardianItemCharge) * 10f) * owner.direction * 0.4f, ((hitboxOffset + hammerItem.HoldOffset) * 2 - (hitboxOffset + hammerItem.HoldOffset) * 0.014f * guardian.GuardianItemCharge) * 0.4f);

							if (guardian.GuardianItemCharge < 210f)
							{
								guardian.GuardianItemCharge += 30f / HammerItem.Item.useTime * owner.GetTotalAttackSpeed(DamageClass.Melee);

								if (guardian.GuardianItemCharge > 210f) guardian.GuardianItemCharge = 210f;
							}

							if (owner.whoAmI == Main.myPlayer)
							{
								if (!owner.controlUseItem)
								{
									if (owner.boneGloveItem != null && !owner.boneGloveItem.IsAir && owner.boneGloveTimer == 0)
									{ // Bone glove compatibility, from vanilla code
										owner.boneGloveTimer = 60;
										Vector2 center = owner.Center;
										Vector2 vector = owner.DirectionTo(owner.ApplyRangeCompensation(0.2f, center, Main.MouseWorld)) * 10f;
										Projectile.NewProjectile(owner.GetSource_ItemUse(owner.boneGloveItem), center.X, center.Y, vector.X, vector.Y, ProjectileID.BoneGloveProj, 25, 5f, owner.whoAmI);
									}

									if (guardian.GuardianItemCharge > 10f || hammerItem.CannotSwing)
									{ // Hammer is charged enough to be thrown (or can't be thrown)
										Projectile.ai[1] = 1;

										Vector2 dir = Vector2.Normalize(Main.MouseWorld - owner.Center) * HammerItem.Item.shootSpeed;

										if (guardian.ThrowLevel() < 4)
										{
											dir *= (0.3f * (guardian.ThrowLevel() + 2) / 3);
											Projectile.damage = (int)(Projectile.damage * 0.75f);
											Projectile.knockBack = (int)(Projectile.knockBack / 3f);
											Projectile.ai[0] = 1f;
										}

										Projectile.velocity = dir;
										Projectile.rotation = dir.ToRotation();
										Projectile.direction = Projectile.spriteDirection;
										Projectile.netUpdate = true;

										guardian.GuardianItemCharge = 0;
									}
									else
									{ // charged too little, hammer is swung
										Projectile.ai[1] = -61f;
										Projectile.netUpdate = true;
									}
								}
								else if (Main.mouseRight && !hammerItem.CannotSwing)
								{
									if (owner.boneGloveItem != null && !owner.boneGloveItem.IsAir && owner.boneGloveTimer == 0)
									{ // Bone glove compatibility, from vanilla code
										owner.boneGloveTimer = 60;
										Vector2 center = owner.Center;
										Vector2 vector = owner.DirectionTo(owner.ApplyRangeCompensation(0.2f, center, Main.MouseWorld)) * 10f;
										Projectile.NewProjectile(owner.GetSource_ItemUse(owner.boneGloveItem), center.X, center.Y, vector.X, vector.Y, ProjectileID.BoneGloveProj, 25, 5f, owner.whoAmI);
									}

									Projectile.ai[1] = -60f;
									Projectile.netUpdate = true;
								}
							}
						}
						else
						{
							if (Projectile.ai[1] < -60f) // Makes easier to sync the behaviour after a weak slam
							{
								Projectile.ai[1] = -60f;
								WeakHit = true;
								guardian.GuardianItemCharge = 0;
							}

							if (Projectile.ai[1] == -60f)
							{ // First frame of the swing
								SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.Center);
								Projectile.friendly = true;
								Projectile.netUpdate = true;
								ResetHitStatus(false);
								HammerItem.OnSwing(owner, guardian, Projectile, guardian.GuardianItemCharge >= 180f);
								Projectile.ResetLocalNPCHitImmunity();
								Projectile.localNPCHitCooldown = -1;
							}

							Projectile.velocity = Vector2.UnitX * 0.001f * owner.direction; // So enemies are KBd in the right direction

							float SwingOffset = (float)Math.Sin(MathHelper.Pi / 60f * Projectile.ai[1]);
							Vector2 arm = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, MathHelper.Pi - (guardian.GuardianItemCharge * 0.006f) * Projectile.spriteDirection);
							owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.Pi + (guardian.GuardianItemCharge * 0.006f + SwingOffset * (3f + guardian.GuardianItemCharge * 0.006f)) * Projectile.spriteDirection);
							Vector2 armPosition = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, MathHelper.Pi - (guardian.GuardianItemCharge * 0.006f + SwingOffset * (3f + guardian.GuardianItemCharge * 0.006f)) * Projectile.spriteDirection) - (new Vector2(owner.Center.X, owner.Center.Y) - new Vector2(owner.Center.X, owner.Center.Y).Floor());
							Projectile.Center = armPosition - new Vector2((hitboxOffset * 2 + 0.3f * guardian.GuardianItemCharge + (float)Math.Sin(MathHelper.Pi / 210f * guardian.GuardianItemCharge) * 10f) * owner.direction * 0.4f + (armPosition.X - arm.X) * (2.5f + hitboxOffset * 0.07f), (armPosition.Y - arm.Y) * -(1.1f + hitboxOffset * 0.03f) + (210f - guardian.GuardianItemCharge) * 0.075f);

							if (guardian.GuardianChain > 0f && Projectile.ai[1] < -20)
							{
								Vector2 chainDirection = Vector2.Normalize(Projectile.Center - armPosition);
								float chainOffset = guardian.GuardianChain;
								if (Projectile.ai[1] < -52) chainOffset = (chainOffset / 8f) * (Projectile.ai[1] + 60);
								if (Projectile.ai[1] > -35) chainOffset += (chainOffset / 15f) * (-Projectile.ai[1] - 35);

								Projectile.Center += chainDirection * chainOffset;
							}

							float toAdd = 30f / HammerItem.Item.useTime * HammerItem.SwingSpeed * owner.GetTotalAttackSpeed(DamageClass.Melee);
							if (Projectile.ai[1] < -40) Projectile.ai[1] += toAdd * 1.5f;
							else
							{
								Projectile.ai[1] += toAdd * 0.66f;
								Projectile.friendly = false;
								Projectile.netUpdate = true;
							}

							if (Projectile.ai[1] >= 0f)
							{
								Projectile.ai[1] = 0f;
								Projectile.friendly = false;
								Projectile.netUpdate = true;
							}
						}

					}
				}
				else // Thrown
				{
					if (HammerItem.ThrowAI(owner, guardian, Projectile, WeakThrow))
					{
						if (Projectile.timeLeft < 598 && range > 0) // Delay helps preventing the hammer from instantly despawning if launched from inside a tile
						Projectile.tileCollide = HammerItem.TileCollide;
						else Projectile.tileCollide = false;

						if (range == HammerItem.Range)
						{ // First frame of the throw
							SoundEngine.PlaySound(HammerItem.Item.UseSound, owner.Center);
							ResetHitStatus(!WeakThrow);
							Projectile.friendly = true;
							Projectile.netUpdate = true;
							HammerItem.OnThrow(owner, guardian, Projectile, WeakThrow);
							Projectile.ResetLocalNPCHitImmunity();
							if (!HammerItem.Penetrate) Projectile.localNPCHitCooldown = -1;
						}

						OldPosition.Add(new Vector2(Projectile.Center.X, Projectile.Center.Y));
						OldRotation.Add(Projectile.rotation);
						if (OldPosition.Count > 5)
							OldPosition.RemoveAt(0);
						if (OldRotation.Count > 5)
							OldRotation.RemoveAt(0);

						range--;

						if (range < 0)
						{
							float dist = Projectile.Center.Distance(owner.Center);
							Vector2 vel = Vector2.Normalize(owner.Center - Projectile.Center) * HammerItem.ReturnSpeed;

							if (range < -30)
							{
								vel *= 1 - (30 - range) * 0.15f;
								Projectile.velocity = -vel;
							}
							else
							{
								vel *= 0.5f;
								Projectile.velocity += vel;
							}

							if (dist < 30f && owner.whoAmI == Main.myPlayer) Projectile.Kill();

							if (range < -60)
							{
								Projectile.friendly = false;
								Projectile.netUpdate = true;
							}
						}

						if (WeakThrow)
							Projectile.rotation += 0.25f * (Projectile.velocity.X > 0 ? 1 : -1);
						else
							Projectile.rotation += Projectile.velocity.Length() / 30f * (Projectile.velocity.X > 0 ? 1f : -1f) * 1.2f;
					}
				}

				HammerItem.ExtraAI(owner, guardian, Projectile);
			}
		}

		public override void ModifyDamageHitbox(ref Rectangle hitbox)
		{
			hitbox.X -= hitboxOffset;
			hitbox.Y -= hitboxOffset;
			hitbox.Width += hitboxOffset * 2;
			hitbox.Height += hitboxOffset * 2;
		}

		public override void SafeModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (HammerItem != null)
			{
				if (BlockDuration != 0) // Block hit
				{
					modifiers.FinalDamage *= HammerItem.BlockDamage;
				}
				else if (Projectile.ai[1] < 0) // Swing hit
				{
					if (Main.LocalPlayer.GetModPlayer<OrchidGuardian>().GuardianItemCharge >= 180f)
					{
						modifiers.FinalDamage *= HammerItem.SwingDamage;
					}
					else
					{
						modifiers.FinalDamage *= HammerItem.SwingDamage * 1.5f;
					}
				}
				else // Throw hit
				{
					if (!HammerItem.Penetrate && target.lifeMax > 5)
					{
						modifiers.FinalDamage *= HammerItem.ThrowDamage * (1f - 0.25f * HitCount);
						if (HitCount < 3) HitCount++;
					}
					else
					{
						modifiers.FinalDamage *= HammerItem.ThrowDamage;
					}
				}
				OrchidGuardian guardian = Main.LocalPlayer.GetModPlayer<OrchidGuardian>();
				HammerItem.WarhammerModifyHitNPC(Owner, guardian, target, Projectile, ref modifiers, (Projectile.ai[1] < 0 ? guardian.GuardianItemCharge >= 180f : WeakThrow), Projectile.ai[1] < 0, BlockDuration != 0, FirstHit);
			}
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			if (Projectile.ai[1] < 0) // Less damage for melee hits
			{
				if (Main.LocalPlayer.GetModPlayer<OrchidGuardian>().GuardianItemCharge < 180f)
				{
					modifiers.FinalDamage *= 0.5f;
				}
				else
				{
					modifiers.FinalDamage *= 0.75f;
				}
			}

			if (HammerItem != null && Projectile.ai[1] > 0)
			{
				if (!HammerItem.Penetrate)
				{
					modifiers.FinalDamage *= 1f - 0.25f * HitCount;
					HitCount++;
					if (HitCount > 3)
					{
						HitCount = 3;
					}
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
			if (BlockDuration >0)
			{
				if (Projectile.velocity.X != oldVelocity.X)
					Projectile.velocity.X = -oldVelocity.X;
				if (Projectile.velocity.Y != oldVelocity.Y)
					Projectile.velocity.Y = -oldVelocity.Y;
			}
			else
			{
				if (HammerItem.TileBounce)
				{
					if (Projectile.velocity.X != oldVelocity.X)
						Projectile.velocity.X = -oldVelocity.X;
					if (Projectile.velocity.Y != oldVelocity.Y)
						Projectile.velocity.Y = -oldVelocity.Y;
				}
				else range = -40;
			}
			Player player = Main.player[Projectile.owner];
			OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
			HammerItem.OnThrowTileCollide(player, guardian, Projectile, oldVelocity);
			return false;
		}

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidGuardian guardian)
		{
			if (BlockDuration != 0) // Block hit
			{
				if (FirstHit)
				{
					HammerItem.OnBlockHitFirst(player, guardian, target, Projectile, hit.Knockback, hit.Crit);
				}
				HammerItem.OnBlockHit(player, guardian, target, Projectile, hit.Knockback, hit.Crit);
			}
			else if (Projectile.ai[1] > 0)
			{ // Throw
				bool weak = WeakThrow;
				if (FirstHit)
				{
					if (!weak)
					{
						guardian.AddSlam(HammerItem.SlamStacks);
						guardian.AddGuard(HammerItem.GuardStacks);
					}
					HammerItem.OnThrowHitFirst(player, guardian, target, Projectile, hit.Knockback, hit.Crit, weak);
				}
				HammerItem.OnThrowHit(player, guardian, target, Projectile, hit.Knockback, hit.Crit, weak);

				if (!penetrate && target.lifeMax > 5)
				{
					range = -40;
					Projectile.netUpdate = true;
				}
			}
			else if (BlockDuration == 0)
			{ // Melee Swing
				bool fullyCharged = guardian.GuardianItemCharge >= 180f;
				if (FirstHit)
				{
					HammerItem.OnMeleeHitFirst(player, guardian, target, Projectile, hit.Knockback, hit.Crit, fullyCharged);
					if (guardian.GuardianItemCharge > 0f)
					{
						guardian.GuardianItemCharge += 60f * HammerItem.SwingChargeGain * player.GetTotalAttackSpeed(DamageClass.Melee);
						if (guardian.GuardianItemCharge > 210f)
						{
							guardian.GuardianItemCharge = 210f;
						}
					}
				}
				HammerItem.OnMeleeHit(player, guardian, target, Projectile, hit.Knockback, hit.Crit, fullyCharged);
			}
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(HammerItem.Item.type);
			writer.Write(range);
			writer.Write(BlockDuration);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			int itemtype = reader.ReadInt32();
			range = reader.ReadInt32();
			BlockDuration = reader.ReadInt32();

			if (HammerItem == null)
			{
				OrchidGuardian guardian = Main.player[Projectile.owner].GetModPlayer<OrchidGuardian>();
				guardian.GuardianItemCharge = 0f;

				Item item = new Item();
				item.SetDefaults(itemtype);
				if (item.ModItem is OrchidModGuardianHammer hammerItem)
				{
					HammerItem = hammerItem;

					if (Main.netMode != NetmodeID.Server)
					{
						HammerTexture = TextureAssets.Item[hammerItem.Item.type].Value;
						hitboxOffset = (int)(HammerTexture.Width * hammerItem.Item.scale / 2f);
						DrawOriginOffsetX = DrawOriginOffsetY = hitboxOffset;
						//Projectile.width = (int)(HammerTexture.Width * hammerItem.Item.scale);
						//Projectile.height = (int)(HammerTexture.Height * hammerItem.Item.scale);
						
						if (HammerItem.hasSpecialHammerTexture) HammerTexture =  ModContent.Request<Texture2D>(hammerItem.HammerTexture, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
					}

					Projectile.scale = hammerItem.Item.scale * guardian.GuardianWeaponScale;

					//Projectile.position.X -= Projectile.width / 2;
					//Projectile.position.Y -= Projectile.height / 2;

					range = HammerItem.Range;
					penetrate = HammerItem.Penetrate;
				}
			}
		}

		public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
		{
			if (HammerTexture == null) return false;
			Player player = Main.player[Projectile.owner];
			OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
			Rectangle drawRectangle = HammerTexture.Frame(1, HammerItem.HammerFrames, 0, HammerAnimFrame % HammerItem.HammerFrames);

			if (HammerItem.PreDrawHammer(player, guardian, Projectile, spriteBatch, ref lightColor, ref HammerTexture, ref drawRectangle))
			{
				float rotationBonus = 0f;

				SpriteEffects effect;
				if (Projectile.spriteDirection == 1)
				{
					effect = SpriteEffects.FlipHorizontally;
					rotationBonus += MathHelper.PiOver2;
				}
				else
				{
					effect = SpriteEffects.None;
					rotationBonus -= MathHelper.PiOver2;
				}

				Vector2 posproj = Projectile.Center;
				//float rotaproj = Projectile.rotation;
				if (player.gravDir == -1)
				{
					if (Projectile.ai[1] <= 0)
					{
						posproj.Y = (player.Bottom.Floor() + player.position.Floor()).Y - posproj.Y;
					}
					//rotaproj += MathHelper.Pi;
					if (effect == SpriteEffects.None) effect = SpriteEffects.FlipHorizontally;
					else effect = SpriteEffects.None;
				}

				var color = Lighting.GetColor((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f), Color.White);
				var position = posproj - Main.screenPosition + Vector2.UnitY * player.gfxOffY;

				if (Projectile.ai[1] == 0)
				{
					rotationBonus += guardian.GuardianItemCharge * 0.0065f * player.gravDir * Projectile.spriteDirection;
				}

				if (Projectile.ai[1] < 0)
				{
					float SwingOffset = (float)Math.Sin(MathHelper.Pi / 60f * Projectile.ai[1]);
					rotationBonus += (guardian.GuardianItemCharge * 0.0065f + SwingOffset * (3.5f + guardian.GuardianItemCharge * 0.006f)) * player.gravDir * Projectile.spriteDirection;
				}

				if (BlockDuration != 0)
				{
					spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
					spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });

					for (int i = 0; i < OldPosition.Count; i++)
					{
						Vector2 drawPositionTrail = OldPosition[i] - Main.screenPosition + Vector2.UnitY * player.gfxOffY;
						spriteBatch.Draw(HammerTexture, drawPositionTrail, drawRectangle, lightColor * 0.04f * (i + 1), OldRotation[i], drawRectangle.Size() * 0.5f, Projectile.scale, effect, 0f);
					}

					spriteBatch.End();
					spriteBatch.Begin(spriteBatchSnapshot);
				}
				else if (guardian.GuardianChain > 0f && guardian.GuardianChainTexture != null)
				{ // I want to consume a shoebox
					Texture2D chainTexture = ModContent.Request<Texture2D>(guardian.GuardianChainTexture, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
					Vector2 chainDirection = Vector2.Normalize(Projectile.Center - player.Center);
					float chainOffset = guardian.GuardianChain;
					if (Projectile.ai[1] < -52) chainOffset = (chainOffset / 8f) * (Projectile.ai[1] + 60);
					if (Projectile.ai[1] > -35) chainOffset += (chainOffset / 15f) * (-Projectile.ai[1] - 35);

					while (chainOffset > 0f)
					{
						Vector2 chainPos = position - chainDirection * (chainOffset + (HammerTexture.Height / HammerItem.HammerFrames) * 0.3f);
						chainOffset -= chainTexture.Height * 0.66f;
						spriteBatch.Draw(chainTexture, chainPos, null, color, 0f, chainTexture.Size() * 0.5f, 1f, effect, 0f);
					}
				}

				if (Projectile.ai[1] != 0)
				{
					for (int i = 0; i < OldPosition.Count; i++)
					{
						color = Lighting.GetColor((int)(OldPosition[i].X / 16f), (int)(OldPosition[i].Y / 16f), Color.White) * (WeakThrow ? (0.35f * i) - 0.65f : (0.15f * i));
						position = OldPosition[i] - Main.screenPosition + Vector2.UnitY * player.gfxOffY;

						spriteBatch.Draw(HammerTexture, position, drawRectangle, color, OldRotation[i] + rotationBonus, drawRectangle.Size() * 0.5f, Projectile.scale, effect, 0f);
					}
				}

				spriteBatch.Draw(HammerTexture, position, drawRectangle, color, Projectile.rotation + rotationBonus, drawRectangle.Size() * 0.5f, Projectile.scale, effect, 0f);

				if (HammerTextureGlow != null)
				{
					Color glowColor = HammerItem.GetHammerGlowmaskColor(player, guardian, Projectile, lightColor); 
					spriteBatch.Draw(HammerTextureGlow, position, drawRectangle, glowColor, Projectile.rotation + rotationBonus, HammerTextureGlow.Size() * 0.5f, Projectile.scale, effect, 0f);
				}

				HammerItem.PostDrawHammer(player, guardian, Projectile, spriteBatch, lightColor, HammerTexture, drawRectangle);
			}

			return false;
		}
	}
}