using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OrchidMod.Content.Shapeshifter
{
	public abstract class OrchidModShapeshifterShapeshift : OrchidModShapeshifterItem
	{
		/// <summary>XXXXX</summary>
		private static int AllowGFXOffY; // used to prevent visual issues with slopes

		/// <summary>Wildshape hitbox Width. Defaults to 10.</summary>
		public int ShapeshiftWidth;
		/// <summary>Wildshape hitbox Height. Defaults to 10.</summary>
		public int ShapeshiftHeight;
		/// <summary>Whether the left click ability click casts repeatedly hy holding the input. Defaults to true.</summary>
		public bool AutoReuseLeft;
		/// <summary>Whether the right click ability click casts repeatedly hy holding the input. Defaults to false.</summary>
		public bool AutoReuseRight;
		/// <summary>Whether melee speed makes the left click ability recover faster. Defaults to true.</summary>
		public bool MeleeSpeedLeft;
		/// <summary>Whether melee speed makes the right click ability recover faster. Defaults to true.</summary>
		public bool MeleeSpeedRight;
		/// <summary>Is the shapeshift supposedly a grounded creature? (affects the movement speed given by some items like magiluminescense). Defaults to false.</summary>
		public bool GroundedWildshape;
		/// <summary>If true, movement is not affected by webs. Defaults to false.</summary>
		public bool WebImmunity;
		/// <summary>Fall speed multiplier for the wildshape. defaults to 1f.</summary>
		public float GravityMult;
		/// <summary>Which type is this wildshape (Sage, Predator, Warden, Symbiote)? Defaults to ShapeshifterShapeshiftType.None (don't use that)!</summary>
		public ShapeshifterShapeshiftType ShapeshiftType;
		/// <summary>Will attempt to draw a simple UI for the wildshape if this is != None. Defaults to ShapeshifterShapeshiftTypeUI.None. (None, Count, Fill).</summary>
		public ShapeshifterShapeshiftTypeUI ShapeshiftTypeUI;
		/// <summary>A list of Buff IDs this specific wildshape is immune to.</summary>
		public List<int> ShapeshiftImmunities;

		/// <summary>Can be overriden to replace the text on the left click tooltip line. Probably shouldn't be.</summary>
		public virtual string LeftClickTooltip => Language.GetTextValue(Mod.GetLocalizationKey("Misc.ShapeshifterLeftClick")) + Language.GetTextValue(Mod.GetLocalizationKey("Items." + GetType().Name + ".LeftClick"));
		/// <summary>Can be overriden to replace the text on the right click tooltip line. Probably shouldn't be.</summary>
		public virtual string RightClickTooltip => Language.GetTextValue(Mod.GetLocalizationKey("Misc.ShapeshifterRightClick")) + Language.GetTextValue(Mod.GetLocalizationKey("Items." + GetType().Name + ".RightClick"));
		/// <summary>Can be overriden to replace the text on the jump tooltip line. Probably shouldn't be.</summary>
		public virtual string JumpTooltip => Language.GetTextValue(Mod.GetLocalizationKey("Misc.ShapeshifterJump")) + Language.GetTextValue(Mod.GetLocalizationKey("Items." + GetType().Name + ".Jump"));
		/// <summary>Can be overriden to replace the text on the passive tooltip line. Probably shouldn't be.</summary>
		public virtual string PassiveTooltip => Language.GetTextValue(Mod.GetLocalizationKey("Misc.ShapeshifterPassive")) + Language.GetTextValue(Mod.GetLocalizationKey("Items." + GetType().Name + ".Passive"));
		/// <summary>Can be overriden to replace the text on wildshape type tooltip line. Probably shouldn't be.</summary>
		public virtual string TypeTooltip => Language.GetTextValue(Mod.GetLocalizationKey("Misc.Shapeshifter" + ShapeshiftType.ToString()));
		/// <summary>Path to this wildshape's textures. Probably shouldn't be overriden.</summary>
		public virtual string TextureShapeshift => Texture + "_Shapeshift";
		/// <summary>Path to this wildshape's icon texture. Probably shouldn't be overriden.</summary>
		public virtual string TextureIcon => Texture + "_Icon";
		/// <summary>Path to this wildshape's UI front texture. Probably shouldn't be overriden.</summary>
		public virtual string TextureUI => Texture + "_UI";
		/// <summary>Path to this wildshape's UI back texture. Probably shouldn't be overriden.</summary>
		public virtual string TextureUIBack => Texture + "_UI_Back";
		/// <summary>Called before drawing the shapeshift anchor if it is going to be drawn.</summary>
		public virtual void ShapeshiftPreDraw(SpriteBatch spriteBatch, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Vector2 drawPosition, Rectangle drawRectangle, SpriteEffects effect, Player player, Color lightColor) { }
		/// <summary>Called after drawing the shapeshift anchor</summary>
		public virtual void ShapeshiftPostDraw(SpriteBatch spriteBatch, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Vector2 drawPosition, Rectangle drawRectangle, SpriteEffects effect, Player player, Color lightColor) { }
		/// <summary>Called before drawing the shapeshift anchor, return false to prevent it. Defaults to true;</summary>
		public virtual bool ShapeshiftShouldDraw(Projectile projectile, Player player, ref Color lightColor) { return true; }
		/// <summary>Called every frame as the main AI for this wildshape's Anchor projectile.</summary>
		public virtual void ShapeshiftAnchorAI(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) { }
		/// <summary>Called after PostUpdateEquips, change player stats here if necessary.</summary>
		public virtual void ShapeshiftBuffs(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) { }
		/// <summary>What happens after a successful left click input (only called on the owner's client).</summary>
		public virtual void ShapeshiftOnLeftClick(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) { }
		/// <summary>What happens after a successful right click input (only called on the owner's client)</summary>
		public virtual void ShapeshiftOnRightClick(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) { }
		/// <summary>What happens after a successful jump input (only called on the owner's client).</summary>
		public virtual void ShapeshiftOnJump(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) { }
		/// <summary>Called right after the player transforms.</summary>
		public virtual void ShapeshiftAnchorOnShapeshift(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) { }
		/// <summary>Called when the player "fast transforms", which happens if they haven't changed shapes in a few seconds. Override fields here for faster gameplay (eg: make the player starts with a double jump ready).</summary>
		public virtual void ShapeshiftAnchorOnShapeshiftFast(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) { }
		/// <summary>Makes stuff happen when the anchor disappears (player leaves shapeshift form).</summary>
		public virtual void OnKillAnchor(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) { }
		/// <summary>If true, will use this wildshape's left click ability (calls ShapeshiftOnRightClick).</summary>
		public virtual bool ShapeshiftCanLeftClick(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) => anchor.IsLeftClick && (Main.mouseLeftRelease || AutoReuseLeft) && anchor.CanLeftClick;
		/// <summary>If true, will use this wildshape's right click ability (calls ShapeshiftOnRightClick).</summary>
		public virtual bool ShapeshiftCanRightClick(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) => anchor.IsRightClick && (Main.mouseRightRelease || AutoReuseRight) && anchor.CanRightClick && anchor.CanLeftClick && !Main.mouseLeft;
		/// <summary>If true, will use this wildshape's jump ability (calls ShapeshiftOnJump). Defaults to a simple player.controlJump.</summary>
		public virtual bool ShapeshiftCanJump(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) => player.controlJump;
		/// <summary>Called when the player takes damage, return true to not take the hit (used for some effects like the tortoise block).</summary>
		public virtual bool ShapeshiftFreeDodge(Player.HurtInfo info, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) => false;
		/// <summary>Allows modifying damage taken while shapeshifted.</summary>
		public virtual void ShapeshiftModifyHurt(ref Player.HurtModifiers modifiers, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) { }
		/// <summary>Called after taking a hit from a Projectile.</summary>
		public virtual void ShapeshiftOnHitByProjectile(Projectile damagingProjectile, Player.HurtInfo hurtInfo, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) { }
		/// <summary>Called after taking a hit from a NPC.</summary>
		public virtual void ShapeshiftOnHitByNPC(NPC damagingNPC, Player.HurtInfo hurtInfo, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) { }
		/// <summary>Called after taking a hit from a Anything.</summary>
		public virtual void ShapeshiftOnHitByAnything(Player.HurtInfo hurtInfo, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) { }
		/// <summary>Called upon dealing "contact" damage to a NPC with the shapeshift anchor.</summary>
		public virtual void ShapeshiftOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) { }
		/// <summary>Called after a bleed has been applied to a NPC.</summary>
		public virtual void ShapeshiftOnApplyBleed(NPC target, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter, ShapeshifterBleed bleed) { }
		/// <summary>Called locally when drawing the UI. Will draw uiCount "clear" symbols on top of uiCountMax "background" symbols.</summary>
		public virtual void ShapeshiftGetUIInfo(Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter, ref int uiCount, ref int uiCountMax) { }
		/// <summary>ModItem.HoldItem(Player player).</summary>
		public virtual void SafeHoldItem(Player player) { }
		/// <summary>Color used to draw the shapeshift anchor. Defaults to player.GetImmuneAlphaPure(inputColor, 0f);</summary>
		public virtual Color GetColor(Color inputColor, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter, bool isHairColor = false) => player.GetImmuneAlphaPure(inputColor, 0f);
		/// <summary>Color used to draw the shapeshift anchor glowmask. Defaults to player.GetImmuneAlphaPure(Color.White, 0f);</summary>
		public virtual Color GetColorGlow(Color lightColor, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter) => player.GetImmuneAlphaPure(Color.White, 0f);

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => false;

		public sealed override void SetDefaults()
		{
			Item.DamageType = ModContent.GetInstance<ShapeshifterDamageClass>();
			Item.noMelee = true;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = false;
			Item.maxStack = 1;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.shootSpeed = 0f;
			ShapeshiftWidth = 10;
			ShapeshiftHeight = 10;
			AutoReuseLeft = true;
			AutoReuseRight = false;
			MeleeSpeedLeft = true;
			MeleeSpeedRight = false;
			GravityMult = 1f;
			GroundedWildshape = false;
			ShapeshiftType = ShapeshifterShapeshiftType.None;
			ShapeshiftTypeUI = ShapeshifterShapeshiftTypeUI.None;
			ShapeshiftImmunities = new List<int>();
			SafeSetDefaults();
			Item.useAnimation = Item.useTime;
		}

		public override bool WeaponPrefix() => true;

		public sealed override void HoldItem(Player player)
		{
			OrchidShapeshifter shapeshifter = player.GetModPlayer<OrchidShapeshifter>();
			if (PlayerInput.ScrollWheelDelta != 0)
			{
				shapeshifter.ShapeshifterScrollTransformationBuffer = ModContent.GetInstance<OrchidClientConfig>().ShapeshifterScrollDelay;
			}

			if (IsLocalPlayer(player) && shapeshifter.ShapeshifterScrollTransformationBuffer <= 0)
			{
				var projectileType = ModContent.ProjectileType<ShapeshifterShapeshiftAnchor>();

				if (player.ownedProjectileCounts[projectileType] != 0)
				{
					Projectile proj = Main.projectile.First(i => i.active && i.owner == player.whoAmI && i.type == projectileType);
					if (proj != null && proj.ModProjectile is ShapeshifterShapeshiftAnchor anchor)
					{
						if (anchor.ShapeshifterItem != Item)
						{
							proj.Kill();
							CreateNewAnchor(player);
						}
					}
				}
			}
			SafeHoldItem(player);
		}

		public override bool CanUseItem(Player player)
		{
			int projectileType = ModContent.ProjectileType<ShapeshifterShapeshiftAnchor>();
			if (player.ownedProjectileCounts[projectileType] != 1)
			{
				if (IsLocalPlayer(player))
				{
					foreach (Projectile projectile in Main.projectile)
					{
						if (projectile.active && projectile.owner == player.whoAmI && projectile.type == projectileType)
						{
							projectile.Kill();
						}
					}

					player.GetModPlayer<OrchidShapeshifter>().ShapeshifterScrollTransformationBuffer = 0;
					CreateNewAnchor(player);
				}
			}
			return false;
		}

		public void CreateNewAnchor(Player player)
		{
			OrchidShapeshifter shapeshifter = player.GetModPlayer<OrchidShapeshifter>();
			if (shapeshifter.ShapeshifterFastShapeshiftTimer >= 300)
			{
				player.itemTime = 10;
				player.itemAnimation = 10;
			}
			else
			{
				player.itemTime = 30;
				player.itemAnimation = 30;
			}

			int projectileType = ModContent.ProjectileType<ShapeshifterShapeshiftAnchor>();
			Projectile proj = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), player.Center, player.velocity, projectileType, 0, 0f, player.whoAmI);
			if (proj.ModProjectile is not ShapeshifterShapeshiftAnchor anchor)
			{
				proj.Kill();
			}
			else
			{
				if (player.mount.Active)
				{
					player.mount.Dismount(player);
				}

				player.RemoveAllGrapplingHooks();
				anchor.OnChangeSelectedItem(player);
				anchor.NeedNetUpdate = true;
				anchor.ShapeshifterShapeshiftType = ShapeshiftType;
			}
		}


		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			TooltipLine tt = tooltips.FirstOrDefault(x => x.Name == "Damage" && x.Mod == "Terraria");
			if (tt != null)
			{
				string[] splitText = tt.Text.Split(' ');
				string damageValue = splitText.First();
				tt.Text = damageValue + " " + Language.GetTextValue(ModContent.GetInstance<OrchidMod>().GetLocalizationKey("DamageClasses.ShapeshifterDamageClass.DisplayName"));
			}

			int index = tooltips.FindIndex(ttip => ttip.Mod.Equals("Terraria") && ttip.Name.Equals("Knockback"));
			tooltips.Insert(index + 1, new TooltipLine(Mod, "ToolTipType", TypeTooltip)
			{
				OverrideColor = OrchidColors.GetClassTagColor(ClassTags.Shapeshifter)
			});
			tooltips.Insert(index + 2, new TooltipLine(Mod, "ToolTipLeftClick", LeftClickTooltip));
			tooltips.Insert(index + 3, new TooltipLine(Mod, "ToolTipRightClick", RightClickTooltip));
			tooltips.Insert(index + 4, new TooltipLine(Mod, "ToolTipJump", JumpTooltip));
			tooltips.Insert(index + 5, new TooltipLine(Mod, "ToolTipPassive", PassiveTooltip));
		}

		// Virtual Methods

		public virtual void ShapeshiftTeleport(Vector2 position, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter, bool updateFallStart = true)
		{ // Call this to teleport a wildshape, should be overriden on wildshapes that "do not like" being teleported, like the man eater
			player.Center = position;
			projectile.Center = position;
			anchor.NeedNetUpdate = true;

			if (updateFallStart)
			{
				player.fallStart = (int)(player.position.Y / 16f);
				player.fallStart2 = (int)(player.position.Y / 16f);
			}
		}

		// Custom methods

		public void ShapeshiftApplyBleed(NPC target, Projectile projectile, ShapeshifterShapeshiftAnchor anchor, Player player, OrchidShapeshifter shapeshifter, int timer, int potency, int maxCount, bool isGeneral = false)
		{ // Applies the bleed while in singleplayer, sends a packet for it while on a server
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				ShapeshifterGlobalNPC globalNPC = target.GetGlobalNPC<ShapeshifterGlobalNPC>();
				ShapeshifterBleed bleed = globalNPC.ApplyBleed(player.whoAmI, timer, potency, maxCount, isGeneral);
				ShapeshiftOnApplyBleed(target, projectile, anchor, player, shapeshifter, bleed);
			}
			else
			{
				var packet = OrchidMod.Instance.GetPacket();
				packet.Write((byte)OrchidModMessageType.SHAPESHIFTERAPPLYBLEEDTONPC);
				packet.Write(target.whoAmI);
				packet.Write(player.whoAmI);
				packet.Write(potency);
				packet.Write(maxCount);
				packet.Write(timer);
				packet.Write(isGeneral);
				packet.Send();
			}
		}

		public float GetFallSpeed(Player player, OrchidShapeshifter shapeshifter)
		{
			return player.gravity * GravityMult * shapeshifter.ShapeshifterGravity;
		}

		public void ResetFallHeight(Player player)
		{
			player.fallStart = (int)(player.position.Y / 16f);
			player.fallStart2 = (int)(player.position.Y / 16f);
		}

		public void SetCameraLerp(Player player, float lerp, int time)
		{
			if (IsLocalPlayer(player))
			{
				Main.SetCameraLerp(lerp, time);
			}
		}

		public Projectile ShapeshifterNewProjectile(OrchidShapeshifter shapeshifter, Vector2 position, Vector2 velocity, int type, float damage, int critChance, float knockback, int owner = -1, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f, IEntitySource spawnSource = null)
		{
			if (damage > 0)
			{
				damage = shapeshifter.GetShapeshifterDamage(damage);
			}

			if (spawnSource == null)
			{
				spawnSource = Item.GetSource_FromAI();
			}

			Projectile newProjectile = Projectile.NewProjectileDirect(spawnSource, position, velocity, type, (int)damage, knockback, owner, ai0, ai1, ai2);

			if (critChance > 0)
			{
				newProjectile.CritChance = shapeshifter.GetShapeshifterCrit(critChance);
			}

			if (newProjectile.ModProjectile is OrchidModShapeshifterProjectile shapeshifterProjectile)
			{
				shapeshifterProjectile.ShapeshifterShapeshiftType = ShapeshiftType;
			}

			return newProjectile;
		}

		public void GravityCalculations(ref Vector2 intendedVelocity, Player player, OrchidShapeshifter shapeshifter, float maxFallSpeedOverride = 0f, bool updateFallStart = true)
		{
			if (maxFallSpeedOverride == 0f)
			{
				maxFallSpeedOverride = player.maxFallSpeed;
			}

			maxFallSpeedOverride *= player.gravity * GravityMult * shapeshifter.ShapeshifterGravity / Player.defaultGravity;

			if ((intendedVelocity.Y < 0f && GroundedWildshape) || !GroundedWildshape && updateFallStart)
			{
				ResetFallHeight(player);
			}

			maxFallSpeedOverride *= shapeshifter.ShapeshifterMaxFallSpeed;

			if (intendedVelocity.Y <= maxFallSpeedOverride)
			{ // gravity
				intendedVelocity.Y += GetFallSpeed(player, shapeshifter);
				if (intendedVelocity.Y > maxFallSpeedOverride)
				{
					intendedVelocity.Y = maxFallSpeedOverride;
				}
			}
			else
			{ // slow down players if they are going too fast (eg: entering water)
				intendedVelocity.Y *= 0.9f;
				if (intendedVelocity.Y < maxFallSpeedOverride)
				{
					intendedVelocity.Y = maxFallSpeedOverride;
				}
			}
		}

		public void TryAccelerate(ref Vector2 intendedVelocity, OrchidShapeshifter shapeshifter, float maxSpeed, float speedmult, float amount, float acceleration = 0f, bool Yaxis = false)
		{
			float accelerationmult = speedmult;
			if (acceleration != 0f)
			{
				accelerationmult = acceleration;
			}

			accelerationmult *= shapeshifter.ShapeshifterMoveSpeedAccelerate;

			if (!Decelerate(ref intendedVelocity, maxSpeed, speedmult, amount * 0.75f, Yaxis))
			{
				if (Yaxis)
				{
					if (Math.Sign(intendedVelocity.Y) != 0 && Math.Sign(intendedVelocity.Y) != amount)
					{
						amount *= 2f;
					}

					intendedVelocity.Y += amount * accelerationmult * Math.Sign(maxSpeed);
				}
				else
				{
					if (Math.Sign(intendedVelocity.X) != 0 && Math.Sign(intendedVelocity.X) != amount)
					{
						amount *= 2f;
					}

					intendedVelocity.X += amount * accelerationmult * Math.Sign(maxSpeed);
				}

			}
		}

		public void TrySlowDown(ref Vector2 intendedVelocity, float factor, Player player, OrchidShapeshifter shapeshifter, Projectile anchor, bool ignoreWildshapeBehaviour = false)
		{
			factor = GetSlowDownFactor(factor, shapeshifter);
			if (!IsGrounded(anchor, player, 2f) && intendedVelocity.Y > 0f && !ignoreWildshapeBehaviour && shapeshifter.Shapeshift.GroundedWildshape)
			{ // grounded wildshapes slow down less while airborne
				factor = 1f - (1f - factor) * 0.5f;
			}

			intendedVelocity.X *= factor;
		}

		public float GetSlowDownFactor(float factor, OrchidShapeshifter shapeshifter)
		{
			return 1f - (1f - factor) * shapeshifter.ShapeshifterMoveSpeedDecelerate;
		}

		public bool Decelerate(ref Vector2 intendedVelocity, float maxSpeed, float speedmult, float amount = 0.5f, bool Yaxis = false)
		{
			float axisVelocity = Yaxis ? intendedVelocity.Y : intendedVelocity.X;

			if (maxSpeed > 0)
			{
				if (axisVelocity > maxSpeed * speedmult)
				{
					axisVelocity -= amount * speedmult;
					if (axisVelocity < maxSpeed * speedmult)
					{
						axisVelocity = maxSpeed * speedmult;
					}

					if (Yaxis) intendedVelocity.Y = axisVelocity;
					else intendedVelocity.X = axisVelocity;
					return true;
				}
			}
			else
			{
				if (axisVelocity < maxSpeed * speedmult)
				{
					axisVelocity += amount * speedmult;
					if (axisVelocity > maxSpeed * speedmult)
					{
						axisVelocity = maxSpeed * speedmult;
					}
					
					if (Yaxis) intendedVelocity.Y = axisVelocity;
					else intendedVelocity.X = axisVelocity;
					return true;
				}
			}
			return false;
		}

		public void TryJump(ref Vector2 intendedVelocity, float intendedJumpSpeed, Player player, OrchidShapeshifter shapeshifter, ShapeshifterShapeshiftAnchor anchor, bool mustBeGrounded = false, float speedEfficiency = 0.33f)
		{ // set groundedCheck to a value above 0 to make sure the player is touching the ground
			if (mustBeGrounded)
			{
				if(IsGrounded(anchor.Projectile, player, 8f) && anchor.Projectile.velocity.Y == 0f)
				{
					intendedVelocity.Y = -intendedJumpSpeed * GetJumpMult(player, shapeshifter, anchor, speedEfficiency);
					anchor.NeedNetUpdate = true;
				}
			}
			else 
			{
				intendedVelocity.Y = -intendedJumpSpeed * GetJumpMult(player, shapeshifter, anchor, speedEfficiency);
				anchor.NeedNetUpdate = true;
			}
		}

		public float GetJumpMult(Player player, OrchidShapeshifter shapeshifter, ShapeshifterShapeshiftAnchor anchor, float speedEfficiency)
		{ // When jumping, a wildshape benefits from 100% of its jump speed mult, and 33% of its movement speed mult
			float jumpSpeed = shapeshifter.ShapeshifterJumpSpeed + (GetSpeedMult(player, shapeshifter, anchor) - 1f) * speedEfficiency;

			if (jumpSpeed < 0.5f)
			{ // Lower jump speeds make shapeshifter unplayable
				jumpSpeed = 0.5f;
			}

			return jumpSpeed;
		}

		public float GetSpeedMult(Player player, OrchidShapeshifter shapeshifter, ShapeshifterShapeshiftAnchor anchor, bool GroundedSpeedBonus = false, bool ignoreBonuses = false)
		{
			float speed = 1f;

			if ((player.moveSpeed + shapeshifter.ShapeshifterMoveSpeedBonus) > 0.75f)
			{
				//Main.NewText(1f + (float)Math.Log10(player.moveSpeed + shapeshifter.ShapeshifterMoveSpeedBonus) * 2f + " --- " + (player.moveSpeed + shapeshifter.ShapeshifterMoveSpeedBonus));
				speed += (float)Math.Log10(player.moveSpeed + shapeshifter.ShapeshifterMoveSpeedBonus) * 2f;
			}
			else
			{
				speed = 0.75f;
			}

			if (!ignoreBonuses)
			{
				if (anchor.ShapeshifterItem.ModItem is OrchidModShapeshifterShapeshift shapeshifterItem)
				{
					if (!shapeshifterItem.GroundedWildshape)
					{ // Speed bonus for non-grounded shapeshifts applies at all times
						speed *= shapeshifter.ShapeshifterMoveSpeedBonusNotGrounded;
					}
					else if (GroundedSpeedBonus)
					{ // Speed bonus for grounded shapeshifts only applies when grounded, use GroundedSpeedBonus accordingly when calling the method
						speed *= shapeshifter.ShapeshifterMoveSpeedBonusGrounded;
					}
				}

				speed *= shapeshifter.ShapeshifterMoveSpeedBonusFinal;
				speed += shapeshifter.ShapeshifterMoveSpeedBonusFlat;
			}

			return speed;
		} 

		public void FinalVelocityCalculations(ref Vector2 intendedVelocity, Projectile projectile, Player player, bool stepUpDown = false, bool cancelSlopeOffet = true, bool preventDownInput = false, bool forceFallThrough = false)
		{ // Prevents the player from phashing through tiles after all other velocity calculations
			if (stepUpDown)
			{ // Allows the projectiles to "step" up and down tiles like a walking player would
				if (StepUpTiles(intendedVelocity, projectile, player))
				{
					intendedVelocity.Y = 0;
				}
			}

			if (cancelSlopeOffet)
			{ // Prevents issues with player phasing through slopes
				if (projectile.gfxOffY != 0)
				{ // fuck slopes all my homies hate slopes
					if (intendedVelocity.Y > -0.5f)
					{
						intendedVelocity.Y = -0.5f;
					}
					projectile.gfxOffY = 0;
					//player.gfxOffY = 0;
				}

				if (player.gfxOffY != 0 && AllowGFXOffY <= 0)
				{ // I hate slopes
					player.gfxOffY = 0;
				}

				if (AllowGFXOffY > 0)
				{
					AllowGFXOffY--;
				}
			}

			bool phaseThroughTiles = false;
			HandleSpecificInteractionsFinalVelocity(ref intendedVelocity, ref phaseThroughTiles, projectile, player, stepUpDown, cancelSlopeOffet, preventDownInput, forceFallThrough);
			Vector2 finalVelocity = Vector2.Zero;

			if (!phaseThroughTiles)
			{
				bool goThroughPlatforms = player.controlDown && !preventDownInput && intendedVelocity.Y < 0.5f;
				intendedVelocity /= 10f;
				for (int i = 0; i < 10; i++)
				{
					//finalVelocity += Collision.TileCollision(projectile.position + finalVelocity, intendedVelocity, projectile.width, projectile.height, (goThroughPlatforms || forceFallThrough), forceFallThrough, (int)player.gravDir);
					bool isSlope = false;
					finalVelocity += TileCollideShapeshifter(projectile.position + finalVelocity, intendedVelocity, projectile.width, projectile.height, ref isSlope, (goThroughPlatforms || forceFallThrough), forceFallThrough, (int)player.gravDir);
				}
			}
			else
			{
				finalVelocity += intendedVelocity;
			}

			if (player.waterWalk || player.waterWalk2)
			{
				finalVelocity = Collision.WaterCollision(projectile.position, finalVelocity, projectile.width, projectile.height, false, false, player.waterWalk);
			}

			if (player.stickyBreak > 0 && !WebImmunity)
			{ // cobwebs
				finalVelocity *= 0.5f;
			}

			if (player.HasBuff(BuffID.Webbed))
			{ // webbed stun
				finalVelocity *= 0f;
			}

			projectile.velocity = finalVelocity;
		}

		public bool CanGoUp(Vector2 intendedVelocity, Projectile projectile, Player player)
		{ // false if the player is unable to go up because of a tile above their hitbox
			if (intendedVelocity.Y > -1f)
			{
				intendedVelocity.Y = -1f;
			}

			//return (Collision.TileCollision(projectile.position, Vector2.UnitY * intendedVelocity.Y, projectile.width, projectile.height, false, false, (int)player.gravDir) == Vector2.UnitY * intendedVelocity.Y);
			bool isSlope = false;
			return (TileCollideShapeshifter(projectile.position, Vector2.UnitY * intendedVelocity.Y, projectile.width, projectile.height, ref isSlope, false, false, (int)player.gravDir) == Vector2.UnitY * intendedVelocity.Y);
		}

		public bool StepUpTiles(Vector2 intendedVelocity, Projectile projectile, Player player)
		{
			if (CanGoUp(intendedVelocity, projectile, player) && projectile.velocity.Y >= 0f)
			{
				bool isSlope = false;
				Vector2 finalVelocity = Vector2.Zero;
				intendedVelocity.Y = 0;
				intendedVelocity /= 10f;
				for (int i = 0; i < 10; i++)
				{
					//finalVelocity += Collision.TileCollision(projectile.position + finalVelocity, intendedVelocity, projectile.width, projectile.height, false, false, (int)player.gravDir);
					finalVelocity += TileCollideShapeshifter(projectile.position + finalVelocity, intendedVelocity, projectile.width, projectile.height, ref isSlope, false, false, (int)player.gravDir);

					if (Math.Abs(finalVelocity.X - intendedVelocity.X * (i + 1)) > 0.001f)
					{ // A tile was hit on the X axis
						Vector2 newPosition = projectile.position;
						newPosition.Y -= 16;
						Vector2 finalVelocity2 = Vector2.Zero;
						float offY;
						for (int j = 0; j < 10; j++)
						{
							//finalVelocity2 += Collision.TileCollision(newPosition + finalVelocity2, intendedVelocity, projectile.width, projectile.height, false, false, (int)player.gravDir);
							finalVelocity2 += TileCollideShapeshifter(newPosition + finalVelocity2, intendedVelocity, projectile.width, projectile.height, ref isSlope, false, false, (int)player.gravDir);
							if (Math.Abs(finalVelocity2.X - intendedVelocity.X * (j + 1)) > 0.001f)
							{ // A tile was hit on the X axis above the first tested tile
								if (finalVelocity2.X > finalVelocity.X)
								{ // Player will hit a tile from the new position, but could go further on the X axis than previously. Go up a tile
									newPosition = projectile.position + finalVelocity2;
									newPosition.Y -= 16f;
									//offY = 16f - Collision.TileCollision(newPosition, Vector2.UnitY * 16f, projectile.width, projectile.height, false, false, (int)player.gravDir).Y;
									offY = 16f - TileCollideShapeshifter(newPosition, Vector2.UnitY * 16f, projectile.width, projectile.height, ref isSlope, false, false, (int)player.gravDir).Y;
									projectile.position.Y -= offY;
									AllowGFXOffY = 5;
									return true;
								}
								return false;
							}
						}

						// The tile above is empty and the player can go up the tile
						newPosition = projectile.position + finalVelocity2;
						newPosition.Y -= 16f;
						//offY = 16f - Collision.TileCollision(newPosition, Vector2.UnitY * 16f, projectile.width, projectile.height, false, false, (int)player.gravDir).Y;
						offY = 16f - TileCollideShapeshifter(newPosition, Vector2.UnitY * 16f, projectile.width, projectile.height, ref isSlope, false, false, (int)player.gravDir).Y;
						projectile.position.Y -= offY;
						player.gfxOffY = offY;
						AllowGFXOffY = 5;
						return true;
					}
				}
			}
			return false;
		}

		public bool IsGrounded(Projectile projectile, Player player, float distanceToGround = 32f, bool fall1 = false, bool fall2 = false)
		{ // Returns true if the player is below "distanceToGround" from a solid tile
			Vector2 intendedVelocity = Vector2.UnitY * distanceToGround * 0.1f;
			Vector2 finalVelocity = Vector2.Zero;

			for (int j = 0; j < 10; j++)
			{
				//finalVelocity += Collision.TileCollision(projectile.position + finalVelocity, intendedVelocity, projectile.width, projectile.height, fall1, fall2, (int)player.gravDir);
				bool isSlope = false;
				finalVelocity += TileCollideShapeshifter(projectile.position + finalVelocity, intendedVelocity, projectile.width, projectile.height, ref isSlope, fall1, fall2, (int)player.gravDir);
				if (Math.Abs(finalVelocity.Y - intendedVelocity.Y * (j + 1)) > 0.001f)
				{ // A tile was hit on the Y axis
					return true;
				}
			}


			if (player.waterWalk || player.waterWalk2)
			{
				finalVelocity = Collision.WaterCollision(projectile.position, intendedVelocity, projectile.width, projectile.height, false, false, player.waterWalk);
				if (Math.Abs(finalVelocity.Y - intendedVelocity.Y) > 0.001f)
				{ // A tile was hit on the Y axis
					return true;
				}
			}

			return false;
		}

		public bool AnyTileCollideShapeshifter(Vector2 Position, Vector2 Velocity, int Width, int Height, bool fallThrough = false, bool fall2 = false, int gravDir = 1)
		{
			int count = 1;
			while ((Velocity.Length() / count) > 0.5f)
			{
				count++;
			}

			Vector2 segment = Velocity / count;

			for (int i = 0; i < count; i++)
			{
				Vector2 oldPosition = Position;
				Position += TileCollideShapeshifter(Position, segment, Width, Height, fallThrough, fall2, gravDir);

				if ((oldPosition + segment).Distance(Position) > 0.01f)
				{
					return true;
				}
			}

			return false;
		}

		public Vector2 TileCollideShapeshifter(Vector2 Position, Vector2 Velocity, int Width, int Height, bool fallThrough = false, bool fall2 = false, int gravDir = 1)
		{
			bool isSlope = false;
			return TileCollideShapeshifter(Position, Velocity, Width, Height, ref isSlope, fallThrough, fall2, gravDir);
		}

		public static Vector2 TileCollideShapeshifter(Vector2 Position, Vector2 Velocity, int Width, int Height, ref bool isSlope, bool fallThrough = false, bool fall2 = false, int gravDir = 1)
		{ // Custom copy of Collision.TileCollide that considers slopes as full tiles
			isSlope = false;
			Collision.up = false;
			Collision.down = false;
			Vector2 result = Velocity;
			Vector2 vector = Velocity;
			Vector2 vector2 = Position + Velocity;
			Vector2 vector3 = Position;
			int value = (int)(Position.X / 16f) - 1;
			int value2 = (int)((Position.X + (float)Width) / 16f) + 2;
			int value3 = (int)(Position.Y / 16f) - 1;
			int value4 = (int)((Position.Y + (float)Height) / 16f) + 2;
			int num = -1;
			int num2 = -1;
			int num3 = -1;
			int num4 = -1;
			int num5 = Utils.Clamp(value, 0, Main.maxTilesX - 1);
			value2 = Utils.Clamp(value2, 0, Main.maxTilesX - 1);
			value3 = Utils.Clamp(value3, 0, Main.maxTilesY - 1);
			value4 = Utils.Clamp(value4, 0, Main.maxTilesY - 1);
			float num6 = (value4 + 3) * 16;
			Vector2 vector4 = default(Vector2);
			for (int i = num5; i < value2; i++)
			{
				for (int j = value3; j < value4; j++)
				{
					if (Main.tile[i, j] == null || !Main.tile[i, j].HasTile || Main.tile[i, j].IsActuated || (!Main.tileSolid[Main.tile[i, j].TileType] && (!Main.tileSolidTop[Main.tile[i, j].TileType] || Main.tile[i, j].TileFrameY != 0)))
						continue;

					vector4.X = i * 16;
					vector4.Y = j * 16;
					int num7 = 16;
					if (Main.tile[i, j].IsHalfBlock)
					{
						vector4.Y += 8f;
						num7 -= 8;
					}

					if (!(vector2.X + (float)Width > vector4.X) || !(vector2.X < vector4.X + 16f) || !(vector2.Y + (float)Height > vector4.Y) || !(vector2.Y < vector4.Y + (float)num7))
						continue;

					bool flag = false;
					
					// player is standing on a slope
					if (Main.tile[i, j].Slope > (SlopeType)0 && Main.tile[i, j].Slope < (SlopeType)3)
					{
						if (Main.tile[i, j].Slope == (SlopeType)1 && vector3.Y + (float)Height - Math.Abs(Velocity.X) <= vector4.Y + (float)num7 && vector3.X >= vector4.X)
							isSlope = true;

						if (Main.tile[i, j].Slope == (SlopeType)2 && vector3.Y + (float)Height - Math.Abs(Velocity.X) <= vector4.Y + (float)num7 && vector3.X + (float)Width <= vector4.X + 16f)
							isSlope = true;
					}

					if (vector3.Y + (float)Height <= vector4.Y)
					{
						Collision.down = true;
						if ((!(Main.tileSolidTop[Main.tile[i, j].TileType] && fallThrough) || !(Velocity.Y <= 1f || fall2)) && num6 > vector4.Y)
						{
							num3 = i;
							num4 = j;
							if (num7 < 16)
								num4++;

							if (num3 != num && !flag)
							{
								result.Y = vector4.Y - (vector3.Y + (float)Height) + ((gravDir == -1) ? (-0.01f) : 0f);
								num6 = vector4.Y;
							}
						}
					}
					else if (vector3.X + (float)Width <= vector4.X && !Main.tileSolidTop[Main.tile[i, j].TileType])
					{
						/*
						if (i >= 1 && Main.tile[i - 1, j] == null)
							Main.tile[i - 1, j] = new Tile();
						*/

						if (i < 1 || (Main.tile[i - 1, j].Slope != (SlopeType)2 && Main.tile[i - 1, j].Slope != (SlopeType)4))
						{
							num = i;
							num2 = j;
							if (num2 != num4)
								result.X = vector4.X - (vector3.X + (float)Width);

							if (num3 == num)
								result.Y = vector.Y;
						}
					}
					else if (vector3.X >= vector4.X + 16f && !Main.tileSolidTop[Main.tile[i, j].TileType])
					{
						/*
						if (Main.tile[i + 1, j] == null)
							Main.tile[i + 1, j] = new Tile();
						*/

						if (Main.tile[i + 1, j].Slope != (SlopeType)1 && Main.tile[i + 1, j].Slope != (SlopeType)3)
						{
							num = i;
							num2 = j;
							if (num2 != num4)
								result.X = vector4.X + 16f - vector3.X;

							if (num3 == num)
								result.Y = vector.Y;
						}
					}
					else if (vector3.Y >= vector4.Y + (float)num7 && !Main.tileSolidTop[Main.tile[i, j].TileType])
					{
						Collision.up = true;
						num3 = i;
						num4 = j;
						result.Y = vector4.Y + (float)num7 - vector3.Y + ((gravDir == 1) ? 0.01f : 0f);
						if (num4 == num2)
							result.X = vector.X;
					}
				}
			}

			return result;
		}

		public void HandleSpecificInteractionsPostUpdate(Projectile projectile, Player player)
		{ // Cross mod interactions, or buff specific interactions. Handled on a case-by-case basis
			OrchidShapeshifter shapeshifter = player.GetModPlayer<OrchidShapeshifter>();
			if (shapeshifter.Player.windPushed)
			{ // mighty winds (sandstorm)
				ShapeshifterShapeshiftAnchor anchor = shapeshifter.ShapeshiftAnchor;

				float windSpeed = Main.windSpeedCurrent * 0.5f;
				if (Math.Abs(Main.windSpeedCurrent) < 0.5f)
				{
					windSpeed = Math.Sign(Main.windSpeedCurrent) * 0.0125f;
				}

				if (Math.Abs(Main.windSpeedCurrent) > 0.5)
				{
					windSpeed *= 1.37f;
				}

				if (!IsGrounded(anchor.Projectile, shapeshifter.Player, 2f))
				{
					windSpeed *= 1.5f;
				}

				if (anchor.IsInputLeft || anchor.IsInputRight)
				{
					windSpeed *= 0.8f;
				}

				if (anchor.Projectile.direction != Math.Sign(Main.windSpeedCurrent))
				{
					windSpeed *= -1f;
				}

				if (Main.windSpeedCurrent < 0f)
				{
					windSpeed *= -1f;
				}

				shapeshifter.ShapeshifterMoveSpeedBonusFinal *= 1f + (windSpeed / 1f) * 0.2f;
				shapeshifter.ShapeshifterMoveSpeedAccelerate *= 1f + (windSpeed / 1f);
			}

			if (OrchidMod.ThoriumMod != null)
			{
				foreach (Projectile crossProjectile in Main.projectile)
				{
					if (crossProjectile.type == OrchidMod.ThoriumMod.Find<ModProjectile>("QueenTorrent").Type && crossProjectile.active)
					{ // queen jellyfish whirlpool
						float distance = crossProjectile.DistanceSQ(player.Center);
						if (distance < 250000)
						{
							shapeshifter.ShapeshifterMoveSpeedBonusFinal *= 0.9f;
						}
					}
				}
			}
		}

		public void HandleSpecificInteractionsFinalVelocity(ref Vector2 intendedVelocity, ref bool phaseThroughTiles, Projectile projectile, Player player, bool stepUpDown = false, bool cancelSlopeOffet = true, bool preventDownInput = false, bool forceFallThrough = false)
		{ // Cross mod interactions, or buff specific interactions. Handled on a case-by-case basis
			OrchidShapeshifter shapeshifter = player.GetModPlayer<OrchidShapeshifter>();
			if (player.windPushed && projectile.ModProjectile is ShapeshifterShapeshiftAnchor anchor)
			{ // mighty winds (sandstorm)
				float windSpeed = Main.windSpeedCurrent * 0.025f;
				if (Math.Abs(Main.windSpeedCurrent) < 0.5f)
				{
					windSpeed = Math.Sign(Main.windSpeedCurrent) * 0.0125f;
				}

				if (Math.Abs(Main.windSpeedCurrent) > 0.5)
				{
					windSpeed *= 1.37f;
				}

				if (!IsGrounded(projectile, player, 2f))
				{
					windSpeed *= 1.5f;
				}

				if (anchor.IsInputLeft || anchor.IsInputRight)
				{
					windSpeed *= 0.8f;
				}

				intendedVelocity.X += windSpeed;
			}

			if (player.tongued)
			{
				foreach(NPC npc in Main.npc)
				{
					if (npc.type == NPCID.WallofFlesh)
					{
						float pull = 11f;
						if (Main.expertMode)
						{
							pull = MathHelper.Lerp(11, 22, Math.Min(1f, npc.velocity.Length()) / 5f);
						}

						Vector2 targetPosition = npc.Center + Vector2.UnitX * 200 * npc.direction;
						intendedVelocity = Vector2.Normalize(targetPosition - projectile.Center) * pull;
						phaseThroughTiles = true;
						break;
					}
				}
			}

			if (OrchidMod.ThoriumMod != null)
			{
				if (player.HasBuff(OrchidMod.ThoriumMod.Find<ModBuff>("Bubbled").Type))
				{ // Quenn jellyfish bubble buff
					intendedVelocity.Y -= 0.8f;
				}

				foreach (Projectile crossProjectile in Main.projectile)
				{
					if (crossProjectile.active)
					{
						if (crossProjectile.type == OrchidMod.ThoriumMod.Find<ModProjectile>("QueenTorrent").Type)
						{ // queen jellyfish whirlpool
							float distance = crossProjectile.DistanceSQ(player.Center);
							if (distance < 250000)
							{
								float pull = 0.02f;
								if (distance < 40000)
									pull = 0.03f;
								if (distance < 2500)
									pull = 0.04f;
								if (player.Center.X > crossProjectile.Center.X)
								{
									if (intendedVelocity.X > -3.0)
										intendedVelocity.X -= pull;
								}
								else if (intendedVelocity.X < 3.0)
								{
									intendedVelocity.X += pull;
								}
							}
						}

						if (crossProjectile.type == OrchidMod.ThoriumMod.Find<ModProjectile>("CursedHammerPro1").Type)
						{ // cursed hammer
							if (player.whoAmI != crossProjectile.owner && projectile.DistanceSQ(player.Center) < 900.0) // && !player2.GetThoriumPlayer().accOvercomingGrief)
							{ // waiting fo a mod call to get the accOvercomingGrief value
								intendedVelocity = crossProjectile.velocity * 1.25f;
								shapeshifter.ShapeshifterNoDecelerationTimer = 10;
							}
						}

						if (crossProjectile.type == OrchidMod.ThoriumMod.Find<ModProjectile>("DrumMalletPro").Type)
						{ // drum mallet bounce
							if (crossProjectile.timeLeft < 1190 && crossProjectile.velocity.X < 1.0 && crossProjectile.velocity.X > -1.0)
							{
								if (intendedVelocity.Y > 3.0 && player.DistanceSQ(crossProjectile.Center) < 784.0)// && (!player.GetThoriumPlayer().accOvercomingGrief || this.Projectile.owner == ((Entity)player).whoAmI))
								{ // waiting fo a mod call to get the accOvercomingGrief value
									if (intendedVelocity.Y < 15.0)
										intendedVelocity.Y *= 1.5f;
									intendedVelocity.Y = -intendedVelocity.Y;
									player.fallStart = (int)(player.position.Y / 16f);
									crossProjectile.frameCounter = 0;
									crossProjectile.frame = 0;
									crossProjectile.ai[0] = 1f;
									crossProjectile.ai[1] = 0.0f;
									SoundEngine.PlaySound(SoundID.Item56, crossProjectile.position);
								}
							}
						}
					}
				}
			}

			if (OrchidMod.BetterCaves != null)
			{ // Better caves gravity and pressure traps
				foreach (Projectile crossProjectile in Main.projectile)
				{
					if (crossProjectile.type == OrchidMod.BetterCaves.Find<ModProjectile>("Pressure").Type && crossProjectile.active)
					{
						if (crossProjectile.timeLeft == 180 && projectile.Hitbox.Intersects(crossProjectile.Hitbox) && player.active)
						{
							switch (crossProjectile.ai[0])
							{
								case 1:
									intendedVelocity.X = 22.5f;
									break;
								case 2:
									intendedVelocity.X = -22.5f;
									break;
								case 3:
									intendedVelocity.Y = 22.5f;
									break;
								default:
									intendedVelocity.Y = -22.5f;
									break;
							}
						}
					}
				}

				if (player.HasBuff(OrchidMod.BetterCaves.Find<ModBuff>("GravityBuff").Type) && shapeshifter.IsShapeshifted)
				{
					shapeshifter.ShapeshiftAnchor.NeedKill = true;
				}
			}
		}
	}
}
