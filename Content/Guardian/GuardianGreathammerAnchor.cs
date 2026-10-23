using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian
{
	public class GuardianGreathammerAnchor : OrchidModGuardianAnchor
	{
		public List<Vector2> OldPosition;
		public List<float> OldRotation;

		public OrchidModGuardianGreathammer HammerItem;
		public Texture2D HammerTexture;
		public Texture2D HammerAuraTexture;
		public Texture2D HammerGlowTexture;
		public int HitCount = 0;
		public float AuraIntensity = 0;
		public bool AuraFlip = false;
		// int trailing = 0; // commented because unused

		public bool Ding = false;
		public bool NeedNetUpdate = false;
		public bool WeakSlam = false;

		public const int IdleAnim = -1;
		public const int Idle = 0;
		public const int Charging = 1;
		public const int Slamming = 2;
		public const int StrongDeflecting = 3;
		public const int WeakDeflecting = 4;
		public const int Thrown = 5;

		public ref float Mode => ref Projectile.ai[0];
		public ref float Anim => ref Projectile.ai[1];
		public ref float Extra => ref Projectile.ai[2];

		public bool IsInIdleAnim => Mode == IdleAnim;
		public bool IsIdle => Mode <= Idle;
		public bool IsCharging => Mode == Charging;
		public bool IsSlamming => Mode == Slamming;
		public bool IsDeflecting => Mode == StrongDeflecting || Projectile.ai[0] == WeakDeflecting;
		public bool IsStrongDeflecting => Mode == StrongDeflecting;
		public bool IsWeakDeflecting => Mode == WeakDeflecting;
		public bool IsThrown => Mode == Thrown;

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overPlayers.Add(index);

		public override void SafeSetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.netImportant = true;
			OldPosition = new();
			OldRotation = new();
		}

		public void ResetHammerItem()
		{
			Player player = Main.player[Projectile.owner];
			Item item = player.inventory[player.selectedItem];
			if (item.ModItem is OrchidModGuardianGreathammer)
			{
				HammerItem = item.ModItem as OrchidModGuardianGreathammer;
				OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
				HammerTexture = ModContent.Request<Texture2D>(HammerItem.HammerTexture, AssetRequestMode.ImmediateLoad).Value;
				Projectile.netUpdate = true;
				Projectile.friendly = false;
				guardian.GuardianItemCharge = 0;

				if (ModContent.RequestIfExists<Texture2D>(HammerItem.HammerTexture + "_Aura", out Asset<Texture2D> aura, AssetRequestMode.ImmediateLoad))
					HammerAuraTexture = aura.Value;
				else HammerAuraTexture = ModContent.Request<Texture2D>(Texture).Value;
				if (ModContent.RequestIfExists<Texture2D>(HammerItem.HammerTexture + "_Glow", out Asset<Texture2D> glow, AssetRequestMode.ImmediateLoad))
					HammerGlowTexture = glow.Value;
				else HammerGlowTexture = null;
			}
			else if (IsLocalOwner)
			{
				Projectile.Kill();
			}
		}

		/// <summary>Sets the Greathammer's <c>ai[]</c> states to match the specified mode and animation time.</summary>
		/// <remarks>Sets <c>Mode</c>, <c>Anim</c>, <c>Extra</c> and <c>velocity</c>. <c>Anim</c> can only be reduced if <c>timeUpperLimit</c> is true, and only be increased if <c>timeLowerLimit</c> is true. If <c>quiet</c> is false, then this will only run on local and not remote and cause <c>netUpdate</c> to be flagged, with a delay of one frame if <c>updateDelay</c> is true.</remarks>
		public void ChangeState(float mode, float anim, float extra = 0, bool timeLowerLimit = true, bool timeUpperLimit = true, float velocityX = 0, float velocityY = 0, bool updateDelay = false, bool quiet = false)
		{
			if (IsLocalOwner || quiet)
			{
				Mode = mode;
				if ((timeLowerLimit || Anim < anim) && (timeUpperLimit || Anim > anim))
					Anim = anim;
				Extra = extra;
				Projectile.velocity = new Vector2(velocityX, velocityY);
				Projectile.friendly = false;
				if (!quiet)
				{
					Projectile.netUpdate = !updateDelay;
					NeedNetUpdate = updateDelay;
				}
			}
		}

		public void ReturnFromThrow()
		{
			Player player = Main.player[Projectile.owner];
			ChangeState(IdleAnim, 25, Projectile.velocity.ToRotation());
			DoAnim(2, 0, player.MountedCenter.Floor(), Extra);
			Ding = false;
			Projectile.localNPCHitCooldown = -1;
			if (player.HeldItem.ModItem is OrchidModGuardianGreathammer)
			{
				if (HammerItem.PreSound(player, player.GetModPlayer<OrchidGuardian>(), Projectile, false)) SoundEngine.PlaySound(SoundID.Grab.WithPitchOffset(-0.5f), Projectile.Center);
				player.SetDummyItemTime(0);
			}
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
			if (NeedNetUpdate)
			{
				Projectile.netUpdate = NeedNetUpdate;
				NeedNetUpdate = false;
			}
			if (IsThrown)
			{
				if (HammerItem?.ThrowAI(player, guardian, Projectile, this) ?? true)
				{
					if (Anim > 0)
					{
						Projectile.timeLeft = 2;
						//projectile will lose speed linearly
						Anim--;
						Projectile.velocity *= Anim / (Anim + 1f);
						if (Anim <= 0)
						{
							Extra *= HammerItem.ReturnSpeed;
							if (Extra == 0) Projectile.timeLeft = 15;
							else
							{
								Projectile.timeLeft = 180;
							}
						}
					}
					else //returning
					{
						if (Anim > -80)
						{
							Projectile.velocity += new Vector2(Extra, 0).RotatedBy(Projectile.Center.AngleTo(player.Center));
							if (Projectile.velocity.Length() > Extra / 80 * -Anim) Projectile.velocity /= Projectile.velocity.Length() / (Extra / 40 * -Anim);
						}
						else
						{
							Projectile.velocity = new Vector2(Extra, 0).RotatedBy(Projectile.Center.AngleTo(player.Center));
						}
						if (IsLocalOwner && Projectile.velocity.Length() > Projectile.Center.Distance(player.Center))
						{
							ReturnFromThrow();
						}
						else
						{
							if (Anim > -79f)
								Anim--;
							else if (Anim > -80f)
							{
								Anim = -80f;
								Projectile.friendly = false;
							}
						}
					}
					if (IsThrown) //generic thrown ai. it may have just been caught in the returning ai above, so we only want to run this if it hasn't been caught yet
					{
						Projectile.extraUpdates = 1;
						Projectile.rotation += (0.325f + Anim * 0.005f) * Projectile.spriteDirection;
						AuraIntensity = 0.4f + Anim * 0.005f;
						AuraFlip = false;
						int sound = (int)Anim;
						if (sound > 20)
						{
							if (sound % 20 == 0 && HammerItem.PreSound(player, player.GetModPlayer<OrchidGuardian>(), Projectile, false)) SoundEngine.PlaySound(SoundID.Item18.WithPitchOffset(-0.9f), Projectile.Center);
						}
						else if ((sound == 14 || sound == -20 || sound == -60) && HammerItem.PreSound(player, player.GetModPlayer<OrchidGuardian>(), Projectile, false))  SoundEngine.PlaySound(SoundID.Item18.WithPitchOffset(-1f), Projectile.Center);
					}
				}
			}
			else
			{
				if (player.HeldItem.ModItem is not OrchidModGuardianGreathammer || !player.active || player.dead)
				{
					Projectile.Kill();
					return;
				}
				if (IsLocalOwner)
				{
					if (IsIdle || IsCharging)
					{
						if (Main.MouseWorld.X > player.Center.X && player.direction != 1) player.ChangeDir(1);
						else if (Main.MouseWorld.X < player.Center.X && player.direction != -1) player.ChangeDir(-1);
					}
					else player.ChangeDir(Projectile.spriteDirection);
				}
				Projectile.position += player.position - player.oldPosition; //align for tweening
				if (Projectile.spriteDirection != player.direction)
				{
					Projectile.spriteDirection = player.direction;
					Projectile.rotation = MathHelper.Pi - Projectile.rotation;
				}
				Projectile.timeLeft = 2;
				Projectile.extraUpdates = 0;
				AuraIntensity = 0f;
				if (player.heldProj != Projectile.whoAmI)
				{
					if (player.heldProj != -1 && Main.projectile[player.heldProj].ModProjectile is GuardianGreathammerAnchor)
					{
						Projectile.Kill();
						return;
					}
					player.heldProj = Projectile.whoAmI;
				}
				if (IsIdle)
				{
					if (IsInIdleAnim)
					{
						if (Anim <= 5)
						{
							Mode = 0;
							Extra = 0;
						}
						else
						{
							DoAnim(2, Progress(Anim, 25, 5), player.MountedCenter.Floor(), Extra);
							Anim--;
						}
					}
					if (Projectile.ai[0] == 0)
					{
						if (Anim > 0) //tween from previous position if another animation just finished
						{
							TweenAnim(player.velocity.X == 0 ? 0 : 1, 0, 1 / (Anim + 1), player.MountedCenter.Floor());
							Anim--;
						}
						else 
							DoAnim(player.velocity.X == 0 ? 0 : 1, 0, player.MountedCenter.Floor());
					}
				}
				else
				{
					//reset the player's item animation
					//initially my intuition was to set this up to have the player be considered in an animation so they can't switch, same as most vanilla channeled weapons
					//other guardian weapons just don't do this though which i didn't even think about, so i'll leave this at 0 for now?
					player.SetDummyItemTime(0);
				}
				if (IsCharging)
				{
					if (IsLocalOwner)
					{
						bool swingInput = Main.mouseRight;
						bool chargeInput = Main.mouseLeft;
						if (ModContent.GetInstance<OrchidClientConfig>().GuardianSwapGauntletImputs)
						{
							swingInput = Main.mouseLeft;
							chargeInput = Main.mouseRight;
						}
						if (guardian.GuardianItemCharge < 210f)
						{
							guardian.GuardianItemCharge += 30f / HammerItem.Item.useTime * HammerItem.WaitChargeGain * player.GetTotalAttackSpeed(DamageClass.Melee);
							if (guardian.GuardianItemCharge > 210) guardian.GuardianItemCharge = 210;
						}
						if (!Ding && guardian.GuardianItemCharge >= 180)
						{
							Ding = true;
							if (ModContent.GetInstance<OrchidClientConfig>().GuardianAltChargeSounds) SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, player.Center);
							else SoundEngine.PlaySound(SoundID.MaxMana, player.Center);
						}
						if (!chargeInput)
						{
							Projectile.netUpdate = true;
							if (guardian.GuardianItemCharge >= 180f)
							{
								Projectile.ai[0] = 5;
								Anim = HammerItem.Range * 2f;
								Projectile.Center = player.Center;
								Item item = HammerItem.Item;
								Extra = item.shootSpeed * player.GetTotalAttackSpeed(DamageClass.Melee) * 0.5f;
								Projectile.velocity = new Vector2(Extra, 0).RotatedBy(player.Center.AngleTo(Main.MouseWorld));
								Projectile.damage = guardian.GetGuardianDamage(item.damage * HammerItem.ThrowDamage);
								Projectile.CritChance = guardian.GetGuardianCrit(item.crit);
								Projectile.knockBack = item.knockBack;
								Projectile.friendly = true;
								Projectile.ResetLocalNPCHitImmunity();
								ResetHitStatus(true);
								HitCount = 0;
								if (HammerItem.ThrowHitCooldown > 0)
									Projectile.localNPCHitCooldown = HammerItem.ThrowHitCooldown * 2; //localnpchitcooldown is affected by extraupdates... idk if this is a tmodloader bug or something
								player.SetDummyItemTime(item.useTime);
								if (HammerItem.PreSound(player, guardian, Projectile, true)) SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown.WithPitchOffset(-0.2f), player.Center);
								guardian.OnAttack(AttackID.GreathammerCharge);
							}
							else
							{
								Projectile.ai[0] = 0;
								Anim = 20;
							}
							guardian.GuardianItemCharge = 0;
						}
						else if (swingInput && guardian.UseSlam(checkOnly: true, warn: true))
						{
							if (guardian.UseSlam())
							{
								DoSlam(guardian);
							}
						}
						else
						{
							Extra = guardian.GuardianItemCharge;
						}
					}
					else
					{
						//do fake charging for remote clients
						if (Extra < 210f)
						{
							Extra += 30f / HammerItem.Item.useTime * HammerItem.WaitChargeGain * player.GetTotalAttackSpeed(DamageClass.Melee);
							if (Extra > 210) Extra = 210;
						}
					}
					if (IsCharging)
					{
						float chargeProgress = Progress(Extra, 0, 180);
						if (chargeProgress < 1f)
						{
							if (Anim > 0) TweenAnim(10, chargeProgress, 1 / (Anim + 2), player.MountedCenter.Floor());
							else DoAnim(10, chargeProgress, player.MountedCenter.Floor());
						}
						else
						{
							chargeProgress = Progress(Extra, 180, 210);
							if (chargeProgress < 1f)
							{
								if (Anim > 0) TweenAnim(11, chargeProgress, 1 / (Anim + 1), player.MountedCenter.Floor());
								else DoAnim(11, chargeProgress, player.MountedCenter.Floor());
							}
							else
							{
								if (Anim > 0) TweenAnim(12, 0, Anim > 0 ? 1 / (Anim + 1) : 1, player.MountedCenter.Floor());
								else DoAnim(12, 0, player.MountedCenter.Floor());
							}
						}
						if (Anim > 0) Anim--;
					}
				}
				if (IsSlamming)
				{
					if (guardian.GuardianItemCharge < 210f)
					{
						guardian.GuardianItemCharge += 30f / HammerItem.Item.useTime * HammerItem.WaitChargeGain * player.GetTotalAttackSpeed(DamageClass.Melee);
						if (guardian.GuardianItemCharge > 210) guardian.GuardianItemCharge = 210;
					}
					if (!Ding && guardian.GuardianItemCharge >= 180)
					{
						Ding = true;
						if (ModContent.GetInstance<OrchidClientConfig>().GuardianAltChargeSounds) SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, player.Center);
						else SoundEngine.PlaySound(SoundID.MaxMana, player.Center);
					}

					Anim -= HammerItem.SlamSpeed * player.GetTotalAttackSpeed(DamageClass.Melee) * (guardian.GuardianArmMotorCount > 0 ? 2 : 1);
					switch (Anim)
					{
						case > 35:
							//tween into animation start
							TweenAnim(20, 0, Progress(Anim, 40, 35), player.MountedCenter.Floor());
						break;
						case > 25:
							//attack
							DoAnim(21, Progress(Anim, 35, 25), player.MountedCenter.Floor());
							if (!Projectile.friendly)
							{
								if (HammerItem.PreSound(player, guardian, Projectile, false)) SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing.WithPitchOffset(-0.5f), player.Center);
								Projectile.friendly = true;
							}
						break;
						case > 0:
							//recovery
							DoAnim(22, Progress(Anim, 25, 0), player.MountedCenter.Floor());
							Projectile.friendly = false;
						break;
						case <= 0:
							ChangeState(Charging, guardian.GuardianItemCharge < 210 ? 20 : 5, guardian.GuardianItemCharge, quiet: true);
							HammerItem.OnSlamEnd(player, guardian, Projectile, this, guardian.GuardianItemCharge >= 180f, WeakSlam);

							if (guardian.GuardianArmMotorCount > 0)
							{ // Arm Motor accessory
								guardian.GuardianArmMotorCount --;
								if (guardian.GuardianArmMotorCount > 0 && IsCharging && guardian.GuardianArmMotor)
								{
									DoSlam(guardian, true);
								}
							}
						break;
					}
				}
				if (IsStrongDeflecting)
				{
					//animation wip
					DoAnim(30, Progress(Anim, 30, 0), player.MountedCenter.Floor());
					Anim--;
					if (Anim <= 0)
					{
						Projectile.ai[0] = 0;
						Projectile.friendly = false;
					}
				}
				if (IsWeakDeflecting)
				{
					//animation wip
					DoAnim(40, Progress(Anim, 30, 0), player.MountedCenter.Floor());
					Anim--;
					if (Anim <= 0)
					{
						Projectile.ai[0] = 0;
						Projectile.friendly = false;
					}
				}

				//handle arms
				float offset = new Vector2(4 * Projectile.spriteDirection, 0).RotatedBy(Projectile.rotation).X;
				if (Math.Abs(offset) > 1) offset /= Math.Abs(offset);
				player.compositeFrontArm = GetArmData(player.MountedCenter.Floor() + new Vector2(player.direction * -8, 0), -10 * offset, true);
				if (Mode != 0 || player.bodyFrame.Y != 0) //looks better with back arm in the default position if idle
					player.compositeBackArm = GetArmData(player.MountedCenter.Floor() + new Vector2(player.direction * 8, 0), 10 * offset);
			}
			HammerItem?.ExtraAI(player, guardian, Projectile, this);
		}

		/// <summary>The Greathammer will initiate a slam. If "weak" is set to true, the slam will deal half damage and grant half charge.</summary>
		public void DoSlam(OrchidGuardian guardian, bool weak = false)
		{
			WeakSlam = weak;
			Item item = HammerItem.Item;
			Projectile.damage = guardian.GetGuardianDamage(item.damage * HammerItem.SlamDamage);
			Projectile.CritChance = guardian.GetGuardianCrit(item.crit);
			Projectile.knockBack = item.knockBack;
			//not set to friendly until animation reaches the attack
			ChangeState(Slamming, 40);
			Projectile.ResetLocalNPCHitImmunity();
			ResetHitStatus(true);
			guardian.OnAttack(WeakSlam ? AttackID.GreathammerSlamWeak : AttackID.GreathammerSlam);
			HammerItem.OnSlam(guardian.Player, guardian, Projectile, this, guardian.GuardianItemCharge >= 180f, ref WeakSlam);
		}

		static float Progress(float value, float start, float end) => (value - start) / (end - start);

		void TweenAnim(int animID, float progress, float tweenStrength, Vector2 center)
		{
			Vector2 oldP = Projectile.position;
			float oldR = Projectile.rotation;
			DoAnim(animID, progress, center);
			Projectile.position = Vector2.Lerp(oldP, Projectile.position, tweenStrength);
			Projectile.rotation = oldR + MathHelper.WrapAngle(Projectile.rotation - oldR) * tweenStrength;
		}

		void DoAnim(int animID, float progress, Vector2 center, float extra = 0)
		{
			float sin;
			float cos;
			switch(animID)
			{
				case 0: //idle pose
					Projectile.rotation = Projectile.spriteDirection == 1 ? 0.2f : 2.9416f;
					Projectile.Center = center + new Vector2(0, 10);
				break;
				case 1: //idle moving pose
					Projectile.rotation = Projectile.spriteDirection == 1 ? 0.1f : 3.0416f;
					Projectile.Center = center + new Vector2(0, 8);
				break;
				case 2: //throw catch recovery
					sin = (float)Math.Sin(progress * 2.14 + 1);
					cos = (float)Math.Cos(progress * 5.65 / (progress + 0.2));
					Projectile.rotation = (Projectile.spriteDirection == 1 ? 0 : 3.1416f) - sin * 2 * Projectile.spriteDirection;
					Projectile.Center = center + new Vector2(0, 6 * (1 - sin)) + new Vector2(cos * -10, 0).RotatedBy(extra);
				break;
				
				case 10: //charge
					Projectile.rotation = (Projectile.spriteDirection == 1 ? 0 : MathHelper.Pi) - progress * Projectile.spriteDirection;
					Projectile.Center = center + new Vector2(Projectile.spriteDirection * (2 + 10 * progress), 10 * (1 - progress));
				break;
				case 11: //full charge
					Projectile.rotation = (Projectile.spriteDirection == 1 ? 0 : MathHelper.Pi) - Projectile.spriteDirection * 1.7f + (float)Math.Cos(progress * MathHelper.Pi) * Projectile.spriteDirection * 0.7f;
					Projectile.Center = center + new Vector2(Projectile.spriteDirection * 12 * (1 - progress), 0);
				break;
				case 12: //max charge pose
					Projectile.rotation = (Projectile.spriteDirection == 1 ? 0 : MathHelper.Pi) - Projectile.spriteDirection * 2.4f;
					Projectile.Center = center;
				break;

				case 20: //slam start pose
					Projectile.rotation = -MathHelper.PiOver2 + 0.37f * Projectile.spriteDirection;
					Projectile.Center = center + new Vector2(3.6f * Projectile.spriteDirection, -10);
				break;
				case 21: //slam attack
					sin = (float)Math.Sin(progress * MathHelper.Pi);
					cos = (float)Math.Cos(progress * 2.63 + 0.21);
					Projectile.rotation = (Projectile.spriteDirection == 1 ? 0 : MathHelper.Pi) - cos * 1.3f * Projectile.spriteDirection;
					Projectile.Center = center + new Vector2(10, 0).RotatedBy(Projectile.rotation) + new Vector2(sin * 2 * Projectile.spriteDirection, sin * 2);
					AuraIntensity = sin * 0.6f;
					AuraFlip = false;
				break;
				case 22: //slam recovery
					sin = (float)Math.Sin(progress * 2.5 - 0.93);
					Projectile.rotation = MathHelper.PiOver2 + MathHelper.PiOver4 * Projectile.spriteDirection + sin * 1.4f * Projectile.spriteDirection;
					Projectile.Center = center + new Vector2(10 - progress * 10, 0).RotatedBy(Projectile.rotation);
				break;
				
				case 30: //deflect attack
					sin = (float)Math.Sin(progress * 2 - 0.4);
					cos = (float)Math.Cos(progress * 3);
					Projectile.rotation = -MathHelper.PiOver2 + (1.5f - sin) * Projectile.spriteDirection;
					Projectile.Center = center + new Vector2(cos * -10 * Projectile.spriteDirection, sin * -6);
					AuraIntensity = cos * 0.6f;
					AuraFlip = true;
				break;
				case 31: //deflect drop
				break;
				case 32: //deflect recovery
				break;

				case 40: //failed deflect
					sin = (float)Math.Sin(progress * 2 - 0.4);
					cos = (float)Math.Cos(progress * 3);
					Projectile.rotation = -MathHelper.PiOver2 + (1.5f - sin) * Projectile.spriteDirection;
					Projectile.Center = center + new Vector2(cos * -10 * Projectile.spriteDirection, sin * -6);
				break;
			}
		}

		public Player.CompositeArmData GetArmData(Vector2 shoulder, float offset, bool fix = false)
		{
			//finding closest point on a line: subtract line position from point to get offset, rotate offset by the inverse of the line's rotation so that line is parallel to the x axis
			Vector2 origin = (shoulder - Projectile.Center).RotatedBy(-Projectile.rotation);
			//shortest distance point from (x,y) on y=0 is (x,0)
			Vector2 hand = new(origin.X, 0);
			if (Math.Abs(origin.Y) <= 10)
			{
				//apply hand spacing along weapon shaft if the distance is short enough so the player holds it more naturally
				if (offset > 0)
					hand.X += Math.Min(10 - Math.Abs(origin.Y), offset);
				else
					hand.X += Math.Max(-10 + Math.Abs(origin.Y), offset);
			}
			//re-apply line's rotation to offset and add to line position to get world coordinates
			hand = Projectile.Center + hand.RotatedBy(Projectile.rotation);

			bool debug = Main.player[Projectile.owner].GetModPlayer<OrchidGuardian>().GuardianDebugVisuals > 0;
			if (debug)
			{
				Dust dust = Dust.NewDustPerfect(shoulder, DustID.TheDestroyer);
				dust.velocity = Vector2.Zero;
				dust.scale = 0.5f;
				dust.noGravity = true;
				dust = Dust.NewDustPerfect(hand, DustID.PurpleCrystalShard);
				dust.velocity = Vector2.Zero;
				dust.scale = 0.5f;
				dust.noGravity = true;
			}

			float stretch = shoulder.Distance(hand);
			float angle = shoulder.AngleTo(hand) - MathHelper.PiOver2;
			if (fix)//terraria sucks and the player's front arm doesn't pivot correctly. offset slightly based on rotation to try to fix this
			{
				hand -= (Vector2.UnitX * Projectile.spriteDirection - Vector2.UnitX.RotatedBy(angle) * Projectile.spriteDirection) * 5f;
				angle = shoulder.AngleTo(hand) - MathHelper.PiOver2;
			
				if (debug)
				{
					Dust dust = Dust.NewDustPerfect(hand, DustID.GemSapphire);
					dust.velocity = Vector2.Zero;
					dust.scale = 0.5f;
					dust.noGravity = true;
				}
			}

			return stretch switch
			{
				<= 6 => new(true, Player.CompositeArmStretchAmount.None, angle),
				<= 8 => new(true, Player.CompositeArmStretchAmount.Quarter, angle),
				<= 10 => new(true, Player.CompositeArmStretchAmount.ThreeQuarters, angle),
				_ => new(true, Player.CompositeArmStretchAmount.Full, angle)
			};
		}

		public override void ModifyDamageHitbox(ref Rectangle hitbox)
		{
			if (Projectile.ai[0] < 2) return;
			int size = (int)(10 * HammerItem?.AuraScale ?? 1f);
			switch(Projectile.ai[0])
			{
				case 2: //slam
					size *= 4;
					Player player = Main.player[Projectile.owner];
					Point hitboxToAdd = new Vector2(size, 0).RotatedBy(Projectile.rotation).ToPoint();
					hitbox.X = (int)player.Center.X;
					if (Projectile.spriteDirection < 0) hitbox.X -= size;
					if (hitboxToAdd.X < 0) hitbox.X += hitboxToAdd.X;
					hitbox.Y = (int)player.Center.Y - size;
					if (hitboxToAdd.Y < 0) hitbox.Y += hitboxToAdd.Y;
					hitbox.Width = size + Math.Abs(hitboxToAdd.X);
					hitbox.Height = size * 2 + Math.Abs(hitboxToAdd.Y);
				break;
				case 3: //deflect
				break;
				case 4: //weak deflect
				break;
				case 5: //throw
					size *= 5;
					hitbox.X = (int)Projectile.Center.X - size;
					hitbox.Y = (int)Projectile.Center.Y - size;
					hitbox.Width = size * 2;
					hitbox.Height = size * 2;
				break;
			}
		}

		public override void SafeModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers, Player player, OrchidGuardian guardian)
		{
			if (HammerItem?.ModifyHit(player, guardian, Projectile, this, target, ref modifiers) ?? true)
			{
				if (IsThrown)
				{
					if (HitCount > 0)
					{
						modifiers.FinalDamage *= 1 - HitCount / 10f;
					}
				}
				else if (FirstHit && IsSlamming)
				{
					if (WeakSlam)
					{
						modifiers.SourceDamage /= 2f;
					}

					float charge = guardian.GuardianItemCharge / 180f;
					float chargeToGain = 0.5f * HammerItem.SlamChargeGain;
					if (charge + chargeToGain > 1)
						modifiers.SourceDamage *= charge + chargeToGain * HammerItem.ThrowDamage;
				}
			}
		}

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidGuardian guardian)
		{
			if (FirstHit)
			{
				if (IsSlamming && guardian.GuardianItemCharge < 210f)
				{
					guardian.GuardianItemCharge += 60f * HammerItem.SlamChargeGain * (WeakSlam ? 0.5f : 1f);

					if (guardian.GuardianItemCharge > 180f) guardian.GuardianItemCharge = 210f;
					guardian.SyncGuardianItemCharge();
				}
				else if (IsThrown)
				{
					guardian.AddGuard(HammerItem.GuardStacks);
					guardian.AddSlam(HammerItem.SlamStacks);

					if (guardian.GuardianArmMotor && guardian.GuardianArmMotorCount < 2)
					{
						guardian.GuardianArmMotorCount = 2;
					}
				}
			}
			if (IsThrown && HitCount < 8)
			{
				HitCount++;
			}
		}

		public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
		{
			Player player = Main.player[Projectile.owner];
			SpriteEffects effect = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			float rot = Projectile.rotation + MathHelper.PiOver4 * 3 * Projectile.spriteDirection - MathHelper.PiOver2;
			Vector2 pos = Projectile.Center - Main.screenPosition + new Vector2(0, player.gfxOffY);
			if (AuraIntensity > 0)
			{
				SpriteEffects auraEffect = Projectile.spriteDirection == 1 ^ AuraFlip ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
				float auraRot = rot;
				if (AuraFlip) auraRot += MathHelper.PiOver2 * Projectile.spriteDirection; 
				int w = HammerAuraTexture.Width;
				float scale = HammerItem?.AuraScale ?? 1f;
				spriteBatch.Draw(HammerAuraTexture, pos, new Rectangle(0, w + 2, w, w), HammerItem.AuraSecondary * AuraIntensity, auraRot, new Vector2(w * 0.5f), scale, auraEffect, 0);
				spriteBatch.Draw(HammerAuraTexture, pos, new Rectangle(0, 0, w, w), HammerItem.AuraPrimary * AuraIntensity, auraRot, new Vector2(w * 0.5f), scale, auraEffect, 0);
			}

			Rectangle? frame = null;
			if (HammerItem.HammerFrames > 1)
			{
				int height = HammerTexture.Height / HammerItem.HammerFrames - 2;
				frame = new Rectangle(0, height * Projectile.frame, HammerTexture.Width, height);
			}

			Vector2 drawOffs = HammerTexture.Size() * 0.5f + new Vector2(HammerItem.DrawOffset.X * Projectile.spriteDirection, HammerItem.DrawOffset.Y);
			spriteBatch.Draw(HammerTexture, pos, frame, lightColor, rot, drawOffs, 1, effect, 0);

			if (HammerGlowTexture != null)
			{
				spriteBatch.Draw(HammerTexture, pos, frame, HammerItem.GetHammerGlowmaskColor(player, Projectile, lightColor), rot, drawOffs, 1, effect, 0);
			}

			return false;
		}
	}
}