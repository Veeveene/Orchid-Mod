using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common;
using OrchidMod.Common.Global.Items;
using OrchidMod.Content.General.Prefixes;
using OrchidMod.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.NPC;

namespace OrchidMod.Content.Guardian
{
	public abstract class OrchidModGuardianShield : OrchidModGuardianItem
	{
		public virtual string ShieldTexture => Texture + "_Shield";
		public int ShieldFrames = 1;

		public virtual void ExtraAIShield(Projectile projectile) { }
		public virtual void PostDrawShield(SpriteBatch spriteBatch, Projectile projectile, Player player, Color lightColor) { }
		public virtual bool PreDrawShield(SpriteBatch spriteBatch, Projectile projectile, Player player, ref Color lightColor) { return true; }

		public virtual void SafeHoldItem(Player player) { }
		/// <summary>Called once per slam, when the slam first hits an enemy.</summary>
		public virtual void SlamHitFirst(Player player, Projectile shield, NPC npc, bool WeakSlam) { }
		/// <summary>Called when this shield's slam hits an enemy.</summary>
		public virtual void SlamHit(Player player, Projectile shield, NPC npc, bool WeakSlam) { }
		/// <summary>Called on the first frame of a slam.</summary>
		public virtual void Slam(Player player, Projectile shield, bool WeakSlam) { }
		/// <summary>Called on the last frame of a slam.</summary>
		public virtual void SlamEnd(Player player, Projectile shield, bool WeakSlam) { }
		/// <summary>Called when an enemy collides with the shield during a block. Will be called once per frame per enemy colliding with it.</summary>
		public virtual void Push(Player player, Projectile shield, NPC npc) { }
		/// <summary>Called once per block when the first enemy or projectile is blocked. This is called after <c>Push</c> or <c>Block</c>, but before <c>Block</c> destroys the projectile.</summary>
		public virtual void Protect(Player player, Projectile shield) { }
		/// <summary>Called when a projectile collides with the shield during a block. Return <c>true</c> to destroy the projectile. Defaults to <c>true</c>.</summary>
		/// <returns>Whether to destroy the projectile.</returns>
		public virtual bool Block(Player player, Projectile shield, Projectile projectile) { return true; }
		/// <summary>Called when a projectile collides with the shield during a block, this should be use to spawn projectiles created by reflecting projectiles.
		public virtual void Reflect(Player player, Projectile shield, Projectile projectile, ref int GuardianShieldSpikeReflect) { }
		/// <summary>Called on the first frame of a block.</summary>
		public virtual void BlockStart(Player player, Projectile shield) { }
		/// <summary>Called on the last frame of a block. Will spawn dust at the end of a block if it returns true</summary>
		public virtual bool BlockEnd(Player player, Projectile shield) => true;
		public virtual void PaviseModifyHitNPC(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, ref HitModifiers modifiers, bool firstHit) { }
		public virtual Color GetPaviseGlowmaskColor(Player player, OrchidGuardian guardian, Projectile projectile, Color lightColor) => Color.White;
		/// <summary> Responsible for playing the sound when the player begins guarding with the weapon. Default behavior is <c>SoundEngine.PlaySound(SoundID.Item37.WithPitchOffset(Main.rand.NextFloat(0.4f, 0.6f)), player.Center);</c> </summary>
		public virtual void PlayGuardSound(Player player, OrchidGuardian guardian, Projectile anchor) => SoundEngine.PlaySound(SoundID.Item37.WithPitchOffset(Main.rand.NextFloat(0.4f, 0.6f)), player.Center);

		public float distance = 100f;
		public float slamDistance = 100f;
		public int blockDuration = 60;
		/// <summary>Causes the shield's held sprite to flip when facing right.</summary>
		public bool shouldFlip = false;
		public bool slamAutoReuse = true;
		/// <summary>
		/// If true, the shield will be rotated in discrete steps, instead of a single continuous circle. Defaults to false.
		/// </summary>
		public bool useDiscreteAim = false;
		/// <summary>
		/// The amount of steps per half-rotation that the shield will snap to, if <c>useDiscreteAim</c> is true.
		/// <br/> A value of <c>0</c> results in only a single possible angle, directly in front of the player. Otherwise, there will be twice as many snap points as this value.
		/// </summary>
		public int discreteAimIncrements;
		/// <summary>
		/// The angle, in pi/2 increments, that the base angle will be rotated by.
		/// </summary>
		public int discreteAimRotation;
		/// <summary>
		/// If true, slams performed with the shield will be locked to the rotation they started with, rather than being free to rotate mid-slam.
		/// </summary>
		public bool lockSlamRotation;
		/// <summary>How fast the pavise rotates towards the player cursor while blocking. Arbitrary value, defaults to 10f.</summary>
		public float parryRotation;
		/// <summary>Charge speed multiplier for this item. Defaults to 1f.</summary>
		public float ChargeSpeedMultiplier;

		public sealed override void SetDefaults()
		{
			Item.DamageType = ModContent.GetInstance<GuardianDamageClass>();
			Item.noMelee = true;
			Item.autoReuse = true;
			Item.maxStack = 1;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item1;
			Item.useStyle = ItemUseStyleID.Thrust;
			Item.useTime = 30;
			Item.knockBack = 6f;
			useDiscreteAim = false;
			discreteAimIncrements = 2;
			discreteAimRotation = 0;
			lockSlamRotation = false;
			ShieldFrames = 1;
			parryRotation = 10f;
			ChargeSpeedMultiplier = 1f;

			OrchidGlobalItemPerEntity orchidItem = Item.GetGlobalItem<OrchidGlobalItemPerEntity>();
			orchidItem.guardianWeapon = true;

			SafeSetDefaults();
			Item.useAnimation = Item.useTime;
		}

		public override bool WeaponPrefix() => true;

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override bool CanUseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer && !player.cursed)
			{
				var projectileType = ModContent.ProjectileType<GuardianShieldAnchor>();
				if (player.ownedProjectileCounts[projectileType] > 0)
				{
					var guardian = player.GetModPlayer<OrchidGuardian>();
					var projectile = Main.projectile.First(i => i.active && i.owner == player.whoAmI && i.type == projectileType);

					bool shouldBlock = Main.mouseRight;
					bool shouldSlam = Main.mouseLeft && (Main.mouseLeftRelease || slamAutoReuse);
					if (ModContent.GetInstance<OrchidClientConfig>().GuardianSwapPaviseImputs)
					{
						shouldBlock = Main.mouseLeft;
						shouldSlam = Main.mouseRight && (Main.mouseRightRelease || slamAutoReuse);
					}

					if (projectile != null && projectile.ModProjectile is GuardianShieldAnchor shield)
					{
						if (shouldSlam)
						{ // Slam
							if (projectile.ai[1] == 0f)
							{
								if (guardian.UseSlam(1, true))
								{
									guardian.UseSlam();
									shield.WeakSlam = false;
									SoundEngine.PlaySound(Item.UseSound, player.Center);
								}
								else
								{
									shield.WeakSlam = true;
									SoundEngine.PlaySound(Item.UseSound.Value.WithPitchOffset(Main.rand.NextFloat(-0.5f, -0.25f)).WithVolumeScale(0.5f), player.Center);
								}

								shield.shieldEffectReady = true;
								projectile.ai[1] = 60f;
								if (projectile.ai[0] > 0f)
								{
									if (BlockEnd(player, projectile))
									{
										shield.spawnDusts();
									}
									resetBlockedEnemiesDuration(guardian);
								}

								projectile.ai[0] = 0f;
								projectile.ResetLocalNPCHitImmunity();
								shield.NeedNetUpdate = true;
							}
						}
						else if (shouldBlock && projectile.ai[1] == 0f)
						{ // Block
							projectile.ai[0] = -1f;
							shield.NeedNetUpdate = true;
							shield.Ding = false;
							guardian.GuardianItemCharge = 1f;
							SoundEngine.PlaySound(SoundID.Item7, player.Center);
						}
					}
				}
			}
			return false;
		}

		public GuardianShieldAnchor GetAnchor(Player player)
		{
			var projectileType = ModContent.ProjectileType<GuardianShieldAnchor>();
			if (player.ownedProjectileCounts[projectileType] > 0)
			{
				var proj = Main.projectile.First(i => i.active && i.owner == player.whoAmI && i.type == projectileType);
				if (proj != null && proj.ModProjectile is GuardianShieldAnchor shield)
				{
					return shield;
				}
			}
			return null;
		}

		public void resetBlockedEnemiesDuration(OrchidGuardian modPlayer)
		{
			for (int i = modPlayer.GuardianBlockedEnemies.Count - 1; i >= 0; i--)
			{
				BlockedEnemy blockedEnemy = modPlayer.GuardianBlockedEnemies[i];
				blockedEnemy.time = blockedEnemy.time < 60 ? blockedEnemy.time : 60;
			}
		}

		public sealed override void HoldItem(Player player)
		{
			var projectileType = ModContent.ProjectileType<GuardianShieldAnchor>();
			var guardian = player.GetModPlayer<OrchidGuardian>();
			guardian.GuardianDisplayUI = 300;

			if (player.ownedProjectileCounts[projectileType] == 0)
			{
				var index = Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center.X, player.Center.Y, 0f, 0f, projectileType, 0, 0f, player.whoAmI);

				var proj = Main.projectile[index];
				if (proj.ModProjectile is not GuardianShieldAnchor shield)
				{
					proj.Kill();
				}
				else
				{
					proj.damage = guardian.GetGuardianDamage(Item.damage);
					proj.CritChance = guardian.GetGuardianCrit(Item.crit);
					proj.knockBack = Item.knockBack;
					proj.localAI[0] = (int)(blockDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianBlockDuration); // Used for UI display
					shield.OnChangeSelectedItem(player);
				}
			}
			else
			{
				var proj = Main.projectile.First(i => i.active && i.owner == player.whoAmI && i.type == projectileType);
				if (proj != null && proj.ModProjectile is GuardianShieldAnchor shield)
				{
					if (shield.SelectedItem != player.selectedItem)
					{
						proj.localAI[0] = (int)(blockDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianBlockDuration); // Used for UI display
						shield.OnChangeSelectedItem(player);
					}
				}
			}
			this.SafeHoldItem(player);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			var guardian = Main.LocalPlayer.GetModPlayer<OrchidGuardian>();
			TooltipLine tt = tooltips.FirstOrDefault(x => x.Name == "Damage" && x.Mod == "Terraria");
			if (tt != null)
			{
				string[] splitText = tt.Text.Split(' ');
				string damageValue = splitText.First();
				tt.Text = damageValue + " " + Language.GetTextValue(ModContent.GetInstance<OrchidMod>().GetLocalizationKey("DamageClasses.GuardianDamageClass.DisplayName"));
			}

			int index = tooltips.FindIndex(ttip => ttip.Mod.Equals("Terraria") && ttip.Name.Equals("Knockback"));
			tooltips.Insert(index + 1, new TooltipLine(Mod, "BlockDuration", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.BlockDuration", OrchidUtils.FramesToSeconds((int)(blockDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianBlockDuration)))));

			string click = ModContent.GetInstance<OrchidClientConfig>().GuardianSwapPaviseImputs ? Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.LeftClick") : Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.RightClick");
			tooltips.Insert(index + 2, new TooltipLine(Mod, "ClickInfo", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.Block", click))
			{
				OverrideColor = new Color(175, 255, 175)
			});
		}

		public static float GetSnappedAngle(OrchidModGuardianShield shield, Player player, float originalAngle)
		{
			if (!shield.useDiscreteAim) return originalAngle;
			if (shield.discreteAimIncrements == 0) return -player.direction * MathHelper.PiOver2 * shield.discreteAimRotation + (player.direction == -1 ? MathHelper.Pi : 0f);
			else
			{
				float angleIncrement = MathHelper.Pi / shield.discreteAimIncrements;
				return (float)Math.Round((Vector2.Normalize(Main.MouseWorld - player.MountedCenter.Floor()).ToRotation() + MathHelper.PiOver2 * shield.discreteAimRotation) / angleIncrement) * angleIncrement - MathHelper.PiOver2 * shield.discreteAimRotation;
			}

		}
	}
}
