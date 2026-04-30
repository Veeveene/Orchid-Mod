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
	public abstract class OrchidModGuardianKatar : OrchidModGuardianParryItem
	{
		/// <summary> Path to the texture of the held katar. </summary>
		public virtual string KatarTexture => Texture + "_Katar";
		/// <summary> Path to the texture of held katar glowmasks, if any. </summary>
		public virtual string KatarTextureGlow => Texture + "_Katar_Glow";
		/// <summary> Path to the texture of the offhand katar held texture, if any. </summary>
		public virtual string KatarBackTexture => Texture + "_KatarBack";
		/// <summary> Called upon hitting an enemy with a slam or a charged attack. </summary>
		public virtual void OnHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, HitInfo hit, bool fullyCharged) { }
		/// <summary> Called upon hitting the first enemy of a given slam or a charged attack. Use this to trigger effects that should not happen multiple times when hitting multiple targets with one attack. </summary>
		public virtual void OnHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, HitInfo hit, bool fullyCharged) { }
		/// <summary> Allows access to Projectile.ModifyHitNPC for katar jabs. </summary>
		public virtual void KatarModifyHitNPC(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, ref HitModifiers modifiers, bool fullyCharged) { }
		/// <summary> Called right before a jab projectile is spawned. Return false to prevent normal jab projectiles from spawning. </summary>
		public virtual bool OnJab(Player player, OrchidGuardian guardian, Projectile projectile, bool offHandKatar, bool manuallyFullyCharged, ref bool charged, ref int damage) => true;
		/// <summary> Called after the player parries damage. </summary>
		public virtual void OnParryKatar(Player player, OrchidGuardian guardian, Entity aggressor, Projectile anchor) { }
		/// <summary> Called before the players begins a parry (which is a dash when using katars). Return false to prevent parrying. </summary>
		public virtual bool PreGuard(Player player, OrchidGuardian guardian, Projectile anchor) { return guardian.UseGuard(1); }
		/// <summary> Called every frame by Katar jab projectiles. Return false to prevent normal projectile AI from running (which simply slows down the projectile). </summary>
		public virtual bool ProjectileAI(Player player, Projectile projectile, bool charged) => true;
		/// <summary> Called at the end of the Anchor Projectile AI update. </summary>
		public virtual void ExtraAIKatar(Player player, OrchidGuardian guardian, Projectile anchor, bool offHandKatar) { }
		/// <summary> Called after drawing the katar anchor projectile. </summary>
		public virtual void PostDrawKatar(SpriteBatch spriteBatch, Projectile projectile, Player player, bool offHandKatar, Color lightColor) { }
		/// <summary> Called before drawing the katar anchor projectile. Return false to prevent normal draw code from running. </summary>
		public virtual bool PreDrawKatar(SpriteBatch spriteBatch, Projectile projectile, Player player, bool offHandKatar, ref Color lightColor) { return true; }
		/// <summary> Called at the end of ModifyTooltips, allowing for further tooltip changes. </summary>
		public virtual void SafeModifyTooltips(List<TooltipLine> tooltips) { }
		/// <summary> Called when drawing the katar jab projectiles color. </summary>
		public virtual Color GetColor(bool offHand) => Color.White;
		/// <summary> Responsible for playing the sound when the player begins guarding with the weapon. Default behavior is <c>SoundEngine.PlaySound(SoundID.DD2_JavelinThrowersAttack.WithPitchOffset(Main.rand.NextFloat(0.4f, 0.6f)), player.Center);</c> </summary>
		public virtual void PlayGuardSound(Player player, OrchidGuardian guardian, Projectile anchor) => SoundEngine.PlaySound(SoundID.DD2_JavelinThrowersAttack.WithPitchOffset(Main.rand.NextFloat(0.4f, 0.6f)), player.Center);
		/// <summary> Responsible for playing the sound when the player punches with the weapon. Default behavior is <c>SoundEngine.PlaySound(charged ? SoundID.DD2_MonkStaffGroundMiss : SoundID.DD2_MonkStaffSwing, player.Center);</c> </summary>
		public virtual void PlayPunchSound(Player player, OrchidGuardian guardian, Projectile anchor, bool charged) => SoundEngine.PlaySound(charged ? SoundID.DD2_MonkStaffGroundMiss : SoundID.DD2_MonkStaffSwing, player.Center);
		/// <summary> Called at the end of OrchidModGuardianKatar.HoldItem. </summary>
		public virtual void SafeHoldItem(Player player) { }
		/// <summary> Color used to draw potential glowmasks. Defaults to Color.White. </summary>
		public virtual Color GetKatarGlowmaskColor(Player player, OrchidGuardian guardian, Projectile projectile, Color lightColor) => Color.White;

		/// <summary> Return true if the weapon should use a different texture for the offhand katar. Remember to add a corresponding XXX_KatarBack.png texture. </summary>
		public bool hasBackKatar = false;
		/// <summary> How many frames does the spritesheet uses? </summary>
		public int Frames = 1;
		/// <summary> Velocity of jab projectiles. </summary>
		public float JabVelocity = 10f;
		/// <summary> Jab and slam animation speed multiplier. Also affected by melee speed, but not by usetime. Defaults to 1f.</summary>
		public float JabSpeed = 1f;
		/// <summary> Multiplier applied to the item damage to get a Slam damage. Defaults to 1f. </summary>
		public float SlamDamage = 1f;
		/// <summary> Multiplier applied to the item damage to get a Slam damage. Defaults to 2f. </summary>
		public float ChargedAttackDamage = 1f;
		/// <summary> Multiplier for how much of a charged attack damage should be dealt as a DoT. Defaults to 0.5f. </summary>
		public float ChargedAttackDoT = 0.5f;
		/// <summary> Duration (in frames) of a right click parry (also the duration of the parry dash). Defaults to 10. </summary>
		public int ParryDuration = 10;
		/// <summary> Velocity of the Parry dash. Defaults to 20f. </summary>
		public float ParryDashSpeed = 20f;
		/// <summary> Multiplier to the weapon charge speed when holding left click. Defaults to 1f. </summary>
		public float ChargeSpeedMultiplier = 1f;

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

			OnParryKatar(player, guardian, aggressor, anchor);
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
				var projectileType = ModContent.ProjectileType<GuardianKatarAnchor>();
				int[] anchors = GetAnchors(player);
				if (anchors != null)
				{
					bool swap = ModContent.GetInstance<OrchidClientConfig>().GuardianSwapGauntletImputs;
					bool punchHold = swap ? Main.mouseRight : Main.mouseLeft;
					bool punchTap = swap ? Main.mouseRightRelease : Main.mouseLeftRelease;
					bool guardHold = swap ? Main.mouseLeft : Main.mouseRight;
					bool guardTap = swap ? Main.mouseLeftRelease : Main.mouseRightRelease;

					if (punchHold && punchTap && guardian.GuardianItemCharge <= 0) punchTimer = 6;
					if (guardHold && guardTap && !guardian.GuardianGauntletParry) shouldGuard = true;
				}
			}
			return false;
		}

		void DoBufferedKatarInputs(Player player)
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
				if (!guardHold || guardian.GuardianGauntletParry) shouldGuard = false;
				
				if (shouldPunch || shouldGuard)
				{
					Projectile projectileMain = Main.projectile[anchors[1]];
					if (projectileMain.ai[0] == 0f || projectileMain.ai[0] > 0f)
					{
						if (shouldGuard)
						{
							bool mainGauntletFree = projectileMain.ai[0] == 0f && projectileMain.ai[2] <= 0f;
							if (mainGauntletFree)
							{
								if (PreGuard(player, guardian, projectileMain) && projectileMain.ModProjectile is GuardianKatarAnchor anchor)
								{
									// 8 dir input
									if (player.controlLeft && !player.controlRight)
									{
										anchor.KatarDashAngle = MathHelper.Pi * 1.5f; // Left
										if (player.controlUp && !player.controlDown)
										{
											anchor.KatarDashAngle += MathHelper.Pi * 0.25f; // Top Left
										}
										else if (!player.controlUp && player.controlDown)
										{
											anchor.KatarDashAngle -= MathHelper.Pi * 0.25f; // Bottom Left
										}
									}
									else if (!player.controlLeft && player.controlRight)
									{
										anchor.KatarDashAngle = MathHelper.Pi * 0.5f; // Right
										if (player.controlUp && !player.controlDown)
										{
											anchor.KatarDashAngle -= MathHelper.Pi * 0.25f; // Top Right
										}
										else if (!player.controlUp && player.controlDown)
										{
											anchor.KatarDashAngle += MathHelper.Pi * 0.25f; // Bottom Right
										}
									}
									else if (player.controlUp && !player.controlDown)
									{
										anchor.KatarDashAngle = 0f; // Up
									}
									else if (!player.controlUp && player.controlDown)
									{
										anchor.KatarDashAngle = MathHelper.Pi; // Down
									}
									else
									{ // Projectile Direction (no input)
										anchor.KatarDashAngle = MathHelper.Pi * (1f + player.direction * 0.5f);
									}

									anchor.KatarDashTimer = ParryDuration + 1;

									shouldGuard = false;
									player.immuneTime = 0;
									guardian.modPlayer.PlayerImmunity = 0;
									player.immune = false;
									guardian.GuardianGauntletParry = true; //remind the player that they are in fact parrying because the projectile ai runs on a slight delay
									projectileMain.ai[0] = (int)(ParryDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianParryDuration);
									(projectileMain.ModProjectile as GuardianKatarAnchor).NeedNetUpdate = true;
								}
							}
						}
						//or, if trying to punch
						else if (shouldPunch && guardian.GauntletPunchCooldown <= 0)
						{
							guardian.GauntletPunchCooldown += (int)(30f / (JabSpeed * player.GetAttackSpeed<MeleeDamageClass>())) - 1;
							punchTimer = 0;
							SoundEngine.PlaySound(Item.UseSound, player.Center);

							projectileMain.ai[2] = 1f;
							(projectileMain.ModProjectile as GuardianKatarAnchor).NeedNetUpdate = true;
						}
					}
				}
			}
		}

		public int[] GetAnchors(Player player)
		{
			var projectileType = ModContent.ProjectileType<GuardianKatarAnchor>();
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
			var projectileType = ModContent.ProjectileType<GuardianKatarAnchor>();
			var guardian = player.GetModPlayer<OrchidGuardian>();
			guardian.GuardianDisplayUI = 300;
			int count = player.ownedProjectileCounts[projectileType];
			if (count < 2)
			{
				if (count == 1)
				{
					var proj = Main.projectile.First(i => i.active && i.owner == player.whoAmI && i.type == projectileType);
					if (proj != null && proj.ModProjectile is GuardianKatarAnchor) proj.Kill();
				}

				int[] indexes = [-1, -1];
				for (int i = 0; i < 2; i++)
				{
					var index = Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center.X, player.Center.Y, 0f, 0f, projectileType, 0, 0f, player.whoAmI);

					var proj = Main.projectile[index];
					if (proj.ModProjectile is not GuardianKatarAnchor katar)
					{
						proj.Kill();
					}
					else
					{
						indexes[i] = proj.whoAmI;
						katar.OffHandKatar = i == 0;
						proj.localAI[0] = (int)(ParryDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianParryDuration); // for UI display
						katar.OnChangeSelectedItem(player);
						katar.NeedNetUpdate = true;
					}
				}

				if (indexes[1] < indexes[0])
				{ // Swap order if necessary in Main.projectile[] so the front katar is drawn first
					(Main.projectile[indexes[0]], Main.projectile[indexes[1]]) = (Main.projectile[indexes[1]], Main.projectile[indexes[0]]);
					Main.projectile[indexes[0]].whoAmI = indexes[1];
					Main.projectile[indexes[1]].whoAmI = indexes[0];
				}

				// Katars need a pointer to their linked weapon, they aren't asynchronous like gauntlets and use this to sync animations
				(Main.projectile[indexes[0]].ModProjectile as GuardianKatarAnchor).LinkedKatarAnchor = (Main.projectile[indexes[1]].ModProjectile as GuardianKatarAnchor);
				(Main.projectile[indexes[1]].ModProjectile as GuardianKatarAnchor).LinkedKatarAnchor = (Main.projectile[indexes[0]].ModProjectile as GuardianKatarAnchor);
			}
			else
			{
				foreach (Projectile projectile in Main.projectile)
				{
					if (projectile.type == projectileType && projectile.active && projectile.owner == player.whoAmI && projectile.ModProjectile is GuardianKatarAnchor katar)
					{
						if (katar.SelectedItem != player.selectedItem)
						{
							projectile.localAI[0] = (int)(ParryDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianParryDuration); // for UI display
							katar.OnChangeSelectedItem(player);
						}
					}
				}
			}
			DoBufferedKatarInputs(player);
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

		public virtual Texture2D GetKatarTexture(Player player, Projectile anchor, bool OffHandGauntlet, out Rectangle? drawRectangle, int frame = 0)
		{
			drawRectangle = null;
			Texture2D texture = (hasBackKatar && OffHandGauntlet) ? ModContent.Request<Texture2D>(KatarBackTexture).Value : ModContent.Request<Texture2D>(KatarTexture).Value;
			if (Frames > 1) drawRectangle = texture.Frame(1, Frames, 0, frame % Frames);
			return texture;
		}

		public virtual Texture2D GetGlowmaskTexture(Player player, Projectile anchor, bool OffHandGauntlet, out Rectangle? drawRectangle)
		{
			drawRectangle = null;

			if (ModContent.RequestIfExists<Texture2D>(KatarTextureGlow, out Asset<Texture2D> assetglow))
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
