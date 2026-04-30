using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common;
using OrchidMod.Common.Global.Items;
using OrchidMod.Content.General.Prefixes;
using OrchidMod.Utilities;
using ReLogic.Content;
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
	public abstract class OrchidModGuardianGauntlet : OrchidModGuardianParryItem
	{
		public bool hasArm = false;
		public bool hasShoulder = false;
		public bool hasBackGauntlet = false;

		public virtual string GauntletTexture => Texture + "_Gauntlet";
		public virtual string GauntletTextureGlow => Texture + "_Gauntlet_Glow";
		public virtual string GauntletBackTexture => Texture + "_GauntletBack";
		public virtual string ArmTexture => Texture + "_Arm";
		public virtual string ShoulderTexture => Texture + "_Shoulder";
		public virtual void OnHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, HitInfo hit, bool charged) { }
		public virtual void OnHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, HitInfo hit, bool charged) { }
		public virtual void GauntletModifyHitNPC(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, ref HitModifiers modifiers, bool charged) { }
		/// <summary> Called right before a punch projectile is spawned. Return false to prevent normal punch projectiles from spawning. </summary>
		public virtual bool OnPunch(Player player, OrchidGuardian guardian, Projectile projectile, bool offHandGauntlet, bool manuallyFullyCharged, ref bool charged, ref int damage) => true;
		/// <summary> Called after the player parries damage. </summary>
		public virtual void OnParryGauntlet(Player player, OrchidGuardian guardian, Entity aggressor, Projectile anchor) { }
		/// <summary> Called when the player presses the guard button to begin guarding while at least one gauntlet can guard. Return false to prevent guarding. Defaults to <c>guardian.UseGuard(1)</c>. Returning true without calling <c>guardian.UseGuard</c> will allow the player to guard without resources. </summary>
		/// <param name="anchor"> A gauntlet currently eligible for parrying. Will be the main hand gauntlet if both can parry. </param>
		public virtual bool PreGuard(Player player, OrchidGuardian guardian, Projectile anchor) { return guardian.UseGuard(1); }
		public virtual bool ProjectileAI(Player player, Projectile projectile, bool charged) => true;
		public virtual void ExtraAIGauntlet(Player player, OrchidGuardian guardian, Projectile anchor, bool offHandGauntlet) { }
		public virtual void PostDrawGauntlet(SpriteBatch spriteBatch, Projectile projectile, Player player, bool offHandGauntlet, Color lightColor) { }
		public virtual bool PreDrawGauntlet(SpriteBatch spriteBatch, Projectile projectile, Player player, bool offHandGauntlet, ref Color lightColor) { return true; }
		public virtual void SafeModifyTooltips(List<TooltipLine> tooltips) { } // Called at the end of ModifyTooltips

		public virtual Color GetColor(bool offHand) => Color.White;
		/// <summary> Responsible for playing the sound when the player begins guarding with the weapon. Default behavior is <c>SoundEngine.PlaySound(SoundID.Item37.WithPitchOffset(Main.rand.NextFloat(0.4f, 0.6f)), player.Center);</c> </summary>
		public virtual void PlayGuardSound(Player player, OrchidGuardian guardian, Projectile anchor) => SoundEngine.PlaySound(SoundID.Item37.WithPitchOffset(Main.rand.NextFloat(0.4f, 0.6f)), player.Center);
		/// <summary> Responsible for playing the sound when the player punches with the weapon. Default behavior is <c>SoundEngine.PlaySound(charged ? SoundID.DD2_MonkStaffGroundMiss : SoundID.DD2_MonkStaffSwing, player.Center);</c> </summary>
		public virtual void PlayPunchSound(Player player, OrchidGuardian guardian, Projectile anchor, bool charged) => SoundEngine.PlaySound(charged ? SoundID.DD2_MonkStaffGroundMiss : SoundID.DD2_MonkStaffSwing, player.Center);

		public virtual void SafeHoldItem(Player player) { }
		public virtual Color GetGauntletGlowmaskColor(Player player, OrchidGuardian guardian, Projectile projectile, Color lightColor) => Color.White;

		public int GauntletFrames = 1;

		public float StrikeVelocity = 10f; // Initial speed of the punches
		/// <summary> Jab and slam animation speed multiplier. Also affected by melee speed, but not by usetime. </summary>
		public float PunchSpeed = 1f;
		public float jabDamage;
		public int ParryDuration = 60; // Duration of a right click parry in frames
		public int ParryDashDuration = 0; // Duration in frames of the parry dash
		public float ParryDashSpeed = 0f; // Velocity of the parry dash
		public float ChargeSpeedMultiplier = 1f; // Multiplies charge speed while holding left click

		public sealed override void SetDefaults()
		{
			Item.DamageType = ModContent.GetInstance<GuardianDamageClass>();
			Item.noMelee = true;
			Item.autoReuse = true;
			Item.maxStack = 1;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item7;
			Item.useStyle = ItemUseStyleID.Thrust;
			Item.useTime = 30;
			Item.knockBack = 5f;
			jabDamage = 0.25f;

			OrchidGlobalItemPerEntity orchidItem = Item.GetGlobalItem<OrchidGlobalItemPerEntity>();
			orchidItem.guardianWeapon = true;

			this.SafeSetDefaults();
			Item.useAnimation = Item.useTime;
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public sealed override void OnParry(Player player, OrchidGuardian guardian, Entity aggressor, Projectile anchor)
		{
			int[] anchors = GetAnchors(player);
			for (int i = 0; i < 2; i++)
			{
				if (Main.projectile[anchors[i]].ai[0] > 0)
				{
					Main.projectile[anchors[i]].ai[0] = 0;
					Main.projectile[anchors[i]].netUpdate = true;
				}
			}

			if (ParryDashDuration > 0 && player.whoAmI == Main.myPlayer && anchor.ModProjectile is GuardianGauntletAnchor gauntletAnchor)
			{
				// 8 dir input
				if (player.controlLeft && !player.controlRight)
				{
					gauntletAnchor.GauntletDashAngle = MathHelper.Pi * 1.5f; // Left
					if (player.controlUp && !player.controlDown)
					{
						gauntletAnchor.GauntletDashAngle += MathHelper.Pi * 0.25f; // Top Left
					}
					else if (!player.controlUp && player.controlDown)
					{
						gauntletAnchor.GauntletDashAngle -= MathHelper.Pi * 0.25f; // Bottom Left
					}
				}
				else if (!player.controlLeft && player.controlRight)
				{
					gauntletAnchor.GauntletDashAngle = MathHelper.Pi * 0.5f; // Right
					if (player.controlUp && !player.controlDown)
					{
						gauntletAnchor.GauntletDashAngle -= MathHelper.Pi * 0.25f; // Top Right
					}
					else if (!player.controlUp && player.controlDown)
					{
						gauntletAnchor.GauntletDashAngle += MathHelper.Pi * 0.25f; // Bottom Right
					}
				}
				else if (player.controlUp && !player.controlDown)
				{
					gauntletAnchor.GauntletDashAngle  = 0f; // Up
				}
				else if (!player.controlUp && player.controlDown)
				{
					gauntletAnchor.GauntletDashAngle = MathHelper.Pi; // Down
				}
				else
				{ // Projectile Direction (no input)
					gauntletAnchor.GauntletDashAngle = MathHelper.Pi * (1f + player.direction * 0.5f);
				}

				gauntletAnchor.GauntletDashTimer = ParryDashDuration;
			}

			OnParryGauntlet(player, guardian, aggressor, anchor);
		}

		public override bool WeaponPrefix() => true;

		int punchTimer = 0;
		bool shouldPunch => punchTimer > 0;
		bool shouldGuard;

		public override bool CanUseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer && !player.cursed)
			{
				OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
				var projectileType = ModContent.ProjectileType<GuardianGauntletAnchor>();
				int[] anchors = GetAnchors(player);
				if (anchors != null)
				{
					bool swap = ModContent.GetInstance<OrchidClientConfig>().GuardianSwapGauntletImputs;
					bool punchHold = swap ? Main.mouseRight : Main.mouseLeft;
					bool punchTap = swap ? Main.mouseRightRelease : Main.mouseLeftRelease;
					bool guardHold = swap ? Main.mouseLeft : Main.mouseRight;
					bool guardTap = swap ? Main.mouseLeftRelease : Main.mouseRightRelease;

					if (punchHold && punchTap && guardian.GuardianItemCharge <= 0) punchTimer = 6;
					if (guardHold && guardTap && !guardian.GuardianParry) shouldGuard = true;
				}
			}
			return false;
		}

		void DoBufferedGauntletInputs(Player player)
		{
			int[] anchors = GetAnchors(player);
			if (anchors != null)
			{
				OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
				bool swap = ModContent.GetInstance<OrchidClientConfig>().GuardianSwapGauntletImputs;
				bool punchHold = swap ? Main.mouseRight : Main.mouseLeft;
				bool punchTap = swap ? Main.mouseRightRelease : Main.mouseLeftRelease;
				bool guardHold = swap ? Main.mouseLeft : Main.mouseRight;
				bool guardTap = swap ? Main.mouseLeftRelease : Main.mouseRightRelease;

				if (guardian.GuardianItemCharge > 0) punchTimer = 0;
				if (shouldPunch && !punchHold) punchTimer--;
				if (!guardHold || guardian.GuardianParry) shouldGuard = false;
				
				if (shouldPunch || shouldGuard)
				{
					Projectile projectileMain = Main.projectile[anchors[1]];
					Projectile projectileOff = Main.projectile[anchors[0]];
					//if neither gauntlet is busy
					if (projectileMain.ai[0] == 0f || projectileOff.ai[0] == 0f || (projectileMain.ai[0] > 0f && projectileOff.ai[0] > 0f))
					{	//and trying to guard
						if (shouldGuard)
						{
							bool mainGauntletFree = projectileMain.ai[0] == 0f && projectileMain.ai[2] <= 0f;
							bool offGauntletFree = projectileOff.ai[0] == 0f && projectileOff.ai[2] <= 0f;
							if (mainGauntletFree || offGauntletFree)
							{
								if (PreGuard(player, guardian, mainGauntletFree ? projectileMain : projectileOff))
								{
									shouldGuard = false;
									player.immuneTime = 0;
									guardian.modPlayer.PlayerImmunity = 0;
									player.immune = false;
									guardian.GuardianParry = true; //remind the player that they are in fact parrying because the projectile ai runs on a slight delay
									PlayGuardSound(player, guardian, mainGauntletFree ? projectileMain : projectileOff);
									if (mainGauntletFree)
									{
										projectileMain.ai[0] = (int)(ParryDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianParryDuration);
										(projectileMain.ModProjectile as GuardianGauntletAnchor).NeedNetUpdate = true;
									}
									if (offGauntletFree)
									{
										projectileOff.ai[0] = (int)(ParryDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianParryDuration);
										(projectileOff.ModProjectile as GuardianGauntletAnchor).NeedNetUpdate = true;
									}
								}
							}
						}
						//or, if trying to punch
						else if (shouldPunch && guardian.GauntletPunchCooldown <= 0)
						{
							guardian.GauntletPunchCooldown += (int)(30f / (PunchSpeed * player.GetAttackSpeed<MeleeDamageClass>())) - 1;
							punchTimer = 0;
							//guardian.GuardianGauntletCharge++;
							SoundEngine.PlaySound(Item.UseSound, player.Center);

							if (projectileMain.ai[0] != 0f)
							{ // Main gauntlet is slamming or blocking, use offhand one
								projectileOff.ai[2] = 1f;
								(projectileOff.ModProjectile as GuardianGauntletAnchor).NeedNetUpdate = true;
							}
							else
							{ // else use main hand
								projectileMain.ai[2] = 1f; 
								(projectileMain.ModProjectile as GuardianGauntletAnchor).NeedNetUpdate = true;
							}
						}
					}
				}
			}
		}

		public int[] GetAnchors(Player player)
		{
			var projectileType = ModContent.ProjectileType<GuardianGauntletAnchor>();
			int[] anchors = [-1, -1];
			foreach (Projectile proj in Main.projectile)
			{
				if (proj.active && proj.owner == player.whoAmI && proj.type == projectileType)
				{
					if (anchors[0] == -1)
					{
						anchors[0] = proj.whoAmI;
					}
					else
					{
						anchors[1] = proj.whoAmI;
						return anchors;
					}
				}
			}

			return null;
		}

		public sealed override void HoldItem(Player player)
		{
			var projectileType = ModContent.ProjectileType<GuardianGauntletAnchor>();
			var guardian = player.GetModPlayer<OrchidGuardian>();
			guardian.GuardianDisplayUI = 300;
			int count = player.ownedProjectileCounts[projectileType];
			if (count < 2)
			{
				if (count == 1)
				{
					var proj = Main.projectile.First(i => i.active && i.owner == player.whoAmI && i.type == projectileType);
					if (proj != null && proj.ModProjectile is GuardianGauntletAnchor) proj.Kill();
				}

				int[] indexes = [-1, -1];
				for (int i = 0; i < 2; i++)
				{
					var index = Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center.X, player.Center.Y, 0f, 0f, projectileType, 0, 0f, player.whoAmI);

					var proj = Main.projectile[index];
					if (proj.ModProjectile is not GuardianGauntletAnchor gauntlet)
					{
						proj.Kill();
					}
					else
					{
						indexes[i] = proj.whoAmI;
						gauntlet.OffHandGauntlet = i == 0;
						proj.localAI[0] = (int)(ParryDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianParryDuration); // for UI display
						gauntlet.OnChangeSelectedItem(player);
						gauntlet.NeedNetUpdate = true;
					}
				}

				if (indexes[1] < indexes[0])
				{ // Swap order if necessary in Main.projectile[] so the front gauntlet is drawn first
					(Main.projectile[indexes[0]], Main.projectile[indexes[1]]) = (Main.projectile[indexes[1]], Main.projectile[indexes[0]]);
					Main.projectile[indexes[0]].whoAmI = indexes[1];
					Main.projectile[indexes[1]].whoAmI = indexes[0];
				}
			}
			else
			{
				foreach (Projectile projectile in Main.projectile)
				{
					if (projectile.type == projectileType && projectile.active && projectile.owner == player.whoAmI && projectile.ModProjectile is GuardianGauntletAnchor gauntlet)
					{
						if (gauntlet.SelectedItem != player.selectedItem)
						{
							projectile.localAI[0] = (int)(ParryDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianParryDuration); // for UI display
							gauntlet.OnChangeSelectedItem(player);
						}
					}
				}
			}
			DoBufferedGauntletInputs(player);
			SafeHoldItem(player);
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
			tooltips.Insert(index + 1, new TooltipLine(Mod, "ParryDuration", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.ParryDuration", OrchidUtils.FramesToSeconds((int)(ParryDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianParryDuration)))));


			string click = ModContent.GetInstance<OrchidClientConfig>().GuardianSwapGauntletImputs ? Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.LeftClick") : Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.RightClick");
			tooltips.Insert(index + 2, new TooltipLine(Mod, "ClickInfo", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.Parry", click))
			{
				OverrideColor = new Color(175, 255, 175)
			});

			SafeModifyTooltips(tooltips);
		}

		public virtual Texture2D GetGauntletTexture(Player player, Projectile anchor, bool OffHandGauntlet, out Rectangle? drawRectangle, int frame = 0)
		{
			drawRectangle = null;
			Texture2D texture = (hasBackGauntlet && OffHandGauntlet) ? ModContent.Request<Texture2D>(GauntletBackTexture).Value : ModContent.Request<Texture2D>(GauntletTexture).Value;
			if (GauntletFrames > 1)	drawRectangle = texture.Frame(1, GauntletFrames, 0, frame % GauntletFrames);

			return texture;
		}

		public virtual Texture2D GetArmTexture(out Rectangle? drawRectangle)
		{
			drawRectangle = null;
			return ModContent.Request<Texture2D>(ArmTexture).Value;
		}

		public virtual Texture2D GetShoulderTexture(out Rectangle? drawRectangle)
		{
			drawRectangle = null;
			return ModContent.Request<Texture2D>(ShoulderTexture).Value;
		}

		public virtual Texture2D GetGlowmaskTexture(Player player, Projectile anchor, bool OffHandGauntlet, out Rectangle? drawRectangle)
		{
			drawRectangle = null;

			if (ModContent.RequestIfExists<Texture2D>(GauntletTextureGlow, out Asset<Texture2D> assetglow))
			{
				return assetglow.Value;
			}
			else
			{
				return null;
			}
		}
	}
}
