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

		public virtual void ExtraAIShield(Player player, OrchidGuardian guardian, Projectile shield) {}

		public virtual void PostDrawShield(SpriteBatch spriteBatch, Projectile projectile, Player player, Color lightColor) {}

		public virtual bool PreDrawShield(SpriteBatch spriteBatch, Projectile projectile, Player player, ref Color lightColor)
		{
			return true;
		}

		public virtual void SafeHoldItem(Player player) {}

		/// <summary>Called once per slam, when the slam first hits an enemy.</summary>
		public virtual void SlamHitFirst(Player player, OrchidGuardian guardian, Projectile shield, NPC npc, bool WeakSlam) {}

		/// <summary>Called when this shield's slam hits an enemy.</summary>
		public virtual void SlamHit(Player player, OrchidGuardian guardian, Projectile shield, NPC npc, bool WeakSlam) {}

		/// <summary>Called on the first frame of a slam.</summary>
		public virtual void Slam(Player player, OrchidGuardian guardian, Projectile shield, bool WeakSlam) {}

		/// <summary>Called on the last frame of a slam.</summary>
		public virtual void SlamEnd(Player player, OrchidGuardian guardian, Projectile shield, bool WeakSlam) {}

		/// <summary>Called when an enemy collides with the shield during a block. Will be called once per frame per enemy colliding with it.</summary>
		public virtual void Push(Player player, OrchidGuardian guardian, Projectile shield, NPC npc) {}

		/// <summary>Called once per block when the first enemy or projectile is blocked. This is called after <c>Push</c> or <c>Block</c>, but before <c>Block</c> destroys the projectile.</summary>
		public virtual void Protect(Player player, OrchidGuardian guardian, Projectile shield) {}

		/// <summary>Called when a projectile collides with the shield during a block. Return <c>true</c> to destroy the projectile. Defaults to <c>true</c>.</summary>
		/// <returns>Whether to destroy the projectile.</returns>
		public virtual bool Block(Player player, OrchidGuardian guardian, Projectile shield, Projectile projectile)
		{
			return true;
		}

		/// <summary>Called when a projectile collides with the shield during a block, this should be used to spawn projectiles created by reflecting projectiles.</summary>
		public virtual void Reflect(Player player, OrchidGuardian guardian, Projectile shield, Projectile projectile, ref int GuardianShieldSpikeReflect) {}

		/// <summary>Called on the first frame of a block.</summary>
		public virtual void BlockStart(Player player, OrchidGuardian guardian, Projectile shield, bool manuallyFullyCharged) {}

		/// <summary>Called on the last frame of a block. Will spawn dust at the end of a block if it returns true</summary>
		public virtual bool BlockEnd(Player player, OrchidGuardian guardian, Projectile shield) => true;

		public virtual void PaviseModifyHitNPC(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, ref HitModifiers modifiers, bool firstHit) {}

		public virtual Color GetPaviseGlowmaskColor(Player player, OrchidGuardian guardian, Projectile projectile, Color lightColor) => Color.White;

		/// <summary> Responsible for playing the sound when the player begins guarding with the weapon. Default behavior is <c>SoundEngine.PlaySound(SoundID.Item37.WithPitchOffset(Main.rand.NextFloat(0.4f, 0.6f)), player.Center);</c> </summary>
		public virtual void PlayGuardSound(Player player, OrchidGuardian guardian, Projectile anchor) => SoundEngine.PlaySound(SoundID.Item37.WithPitchOffset(Main.rand.NextFloat(0.4f, 0.6f)), player.Center);

		/// <summary>Called while blocking to modify the angle the shield blocks toward. Return <c>false</c> to prevent default shield rotation behavior. Defaults to <c>true</c>.</summary>
		public virtual bool ModifyBlockingAngle(Player player, OrchidGuardian guardian, Projectile shield, ref float initialAngle) => true;

	public float distance = 100f;
		public float slamDistance = 100f;
		public int blockDuration = 60;
		/// <summary>Causes the shield's held sprite to flip when facing right.</summary>
		public bool shouldFlip = false;
		public bool slamAutoReuse = true;
		/// <summary>
		/// If true, slams performed with the shield will be locked to the rotation they started with, rather than being free to rotate mid-slam.
		/// </summary>
		public bool lockSlamRotation;
		/// <summary>How fast the pavise rotates towards the player cursor while blocking. Arbitrary value, defaults to 10f.</summary>
		public float blockRotation;
		/// <summary>Charge speed multiplier for this item. Defaults to 1f.</summary>
		public float ChargeSpeedMultiplier;
		
		/// <summary>Draws extra UI elements on the GuardianUIState while held.</summary>
		public virtual void PavisePostDrawUI(SpriteBatch spriteBatch, Player player, ref Color lightColor, Projectile projectile) { }

		public override int AnchorType => ModContent.ProjectileType<GuardianShieldAnchor>();
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
			lockSlamRotation = false;
			ShieldFrames = 1;
			blockRotation = 10f;
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
				if (player.ownedProjectileCounts[AnchorType] > 0)
				{
					OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
					Projectile projectile = Main.projectile.First(i => i.active && i.owner == player.whoAmI && i.type == AnchorType);

					bool shouldBlock = Main.mouseRight;
					bool shouldSlam = Main.mouseLeft && (Main.mouseLeftRelease || slamAutoReuse);
					if (ModContent.GetInstance<OrchidClientConfig>().GuardianSwapPaviseInputs)
					{
						shouldBlock = Main.mouseLeft;
						shouldSlam = Main.mouseRight && (Main.mouseRightRelease || slamAutoReuse);
					}

					if (projectile.ModProjectile is GuardianShieldAnchor shield)
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
									guardian.WarnMissingResources(0, 1, playSound: false);
									shield.WeakSlam = true;
									SoundEngine.PlaySound(Item.UseSound.Value.WithPitchOffset(Main.rand.NextFloat(-0.5f, -0.25f)).WithVolumeScale(0.5f), player.Center);
								}

								shield.shieldEffectReady = true;
								projectile.ai[1] = 60f;
								if (projectile.ai[0] > 0f)
								{
									if (BlockEnd(player, guardian, projectile))
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
			if (player.ownedProjectileCounts[AnchorType] > 0)
			{
				Projectile proj = Main.projectile.FirstOrDefault(i => i.active && i.owner == player.whoAmI && i.type == AnchorType);
				if (proj?.ModProjectile is GuardianShieldAnchor shield)
					return shield;
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
			OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
			guardian.GuardianDisplayUI = 300;

			if (player.ownedProjectileCounts[AnchorType] == 0)
			{
				Projectile proj = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), player.Center, Vector2.Zero, AnchorType, 0, 0f, player.whoAmI);
				
				if (proj.ModProjectile is GuardianShieldAnchor shield)
				{
					proj.damage = guardian.GetGuardianDamage(Item.damage);
					proj.CritChance = guardian.GetGuardianCrit(Item.crit);
					proj.knockBack = Item.knockBack;
					proj.localAI[0] = (int)guardian.GetBlockDuration(Item, blockDuration);; // Used for UI display
					shield.OnChangeSelectedItem(player);
				}
				else proj.Kill();
			}
			else
			{
				Projectile proj = Main.projectile.FirstOrDefault(i => i.active && i.owner == player.whoAmI && i.type == AnchorType);
				if (proj.ModProjectile is GuardianShieldAnchor shield)
				{
					if (shield.SelectedItem != player.selectedItem)
					{
						proj.localAI[0] = (int)guardian.GetBlockDuration(Item, blockDuration);; // Used for UI display
						shield.OnChangeSelectedItem(player);
					}
				}
			}
			SafeHoldItem(player);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			OrchidGuardian guardian = Main.LocalPlayer.GetModPlayer<OrchidGuardian>();
			TooltipLine tt = tooltips.FirstOrDefault(x => x.Name == "Damage" && x.Mod == "Terraria");
			if (tt != null)
			{
				string[] splitText = tt.Text.Split(' ');
				string damageValue = splitText.First();
				tt.Text = damageValue + " " + Language.GetTextValue(ModContent.GetInstance<OrchidMod>().GetLocalizationKey("DamageClasses.GuardianDamageClass.DisplayName"));
			}

			int index = tooltips.FindIndex(ttip => ttip.Mod.Equals("Terraria") && ttip.Name.Equals("Knockback"));
			tooltips.Insert(index + 1, new TooltipLine(Mod, "BlockDuration", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.BlockDuration", OrchidUtils.FramesToSeconds((int)(blockDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianBlockDuration)))));

			string click = OrchidUtils.GetClickInfoTooltip(guardian.PaviseInputSwap);
			tooltips.Insert(index + 2, new TooltipLine(Mod, "ClickInfo", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.Block", click))
			{
				OverrideColor = new Color(175, 255, 175)
			});
		}

		public static float GetSnappedAngle(Player player, float originalAngle, int increments, int offset)
		{
			if (increments == 0) return -player.direction * MathHelper.PiOver2 * offset + (player.direction == -1 ? MathHelper.Pi : 0f);
			else
			{
				float angleIncrement = MathHelper.Pi / increments;
				return (float)Math.Round((Vector2.Normalize(Main.MouseWorld - player.MountedCenter.Floor()).ToRotation() + MathHelper.PiOver2 * offset) / angleIncrement) * angleIncrement - MathHelper.PiOver2 * offset;
			}

		}
	}
}
