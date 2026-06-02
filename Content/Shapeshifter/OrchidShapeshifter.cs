using Microsoft.Xna.Framework;
using OrchidMod.Common;
using OrchidMod.Common.ModObjects;
using OrchidMod.Content.Shapeshifter;
using OrchidMod.Content.Shapeshifter.Dusts;
using OrchidMod.Content.Shapeshifter.Projectiles.Misc;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod
{
	public class OrchidShapeshifter : ModPlayer
	{
		public OrchidPlayer modPlayer;
		public ShapeshifterShapeshiftAnchor ShapeshiftAnchor;
		public OrchidModShapeshifterShapeshift Shapeshift;
		public bool IsShapeshifted => ShapeshiftAnchor != null && ShapeshiftAnchor.Projectile.active;
		public int GetShapeshifterDamage(float damage) => (int)(Player.GetDamage<ShapeshifterDamageClass>().ApplyTo(damage) + Player.GetDamage(DamageClass.Generic).ApplyTo(damage) - damage);
		public int GetShapeshifterCrit(int additionalCritChance = 0) => (int)(Player.GetCritChance<ShapeshifterDamageClass>() + Player.GetCritChance<GenericDamageClass>() + additionalCritChance);
		public float GetShapeshifterMeleeSpeed(float additionalMeleeSpeed = 0) => Player.GetTotalAttackSpeed(DamageClass.Melee) + ShapeshifterMeleeSpeedBonus + additionalMeleeSpeed;
		public int GetShapeshifterHealing(float healing) => (int)Math.Ceiling(healing * ShapeshifterHealingBonus);
		/// <summary>These projectile IDs will kill the wildshape anchor if they are owned by the local player </summary>
		public static List<int> ShapeshifterIncompatibleProjectiles;
		/// <summary>These buff IDs will kill the wildshape anchor if they affect the local player </summary>
		public static List<int> ShapeshifterIncompatibleBuffs;
		/// <summary>Internal Names of DrawLayers that remain visible when shapeshifted. Should be considered when handling cross-mod compatiblity.</summary>
		public static List<string> ShapeshifterAuthorizedDrawLayers;

		// Can be edited by gear (Set effects, accessories, misc)

		/// <summary>Damage per stack of the predator attack bleed (applied by all attacks while shapeshifted into a predator). Be careful not to override this with a lower value. Defaults to 0.</summary>
		public int ShapeshifterPredatorBleedPotency = 0;
		/// <summary>Maximum stacks of the predator attack bleed (applied by all attacks while shapeshifted into a predator). Be careful not to override this with a lower value. Defaults to 0.</summary>
		public int ShapeshifterPredatorBleedMaxStacks = 0;
		/// <summary>Added to shapeshifter melee speed multipliers. Defaults to 0f.</summary>
		public float ShapeshifterMeleeSpeedBonus = 0f;
		/// <summary>Additive, scales logarithmically. Can be used as shapeshifter-only alternative to player.movespeed, should be preferred. Defaults to 0f.</summary>
		public float ShapeshifterMoveSpeedBonus = 0f;
		/// <summary>Additive, added to the final shapeshifter movespeed. Defaults to 0f.</summary>
		public float ShapeshifterMoveSpeedBonusFlat = 0f;
		/// <summary>Multiplicative, used before adding ShapeshifterMoveSpeedBonusFlat. Defaults to 1f.</summary>
		public float ShapeshifterMoveSpeedBonusFinal = 1f;
		/// <summary>Multiplicative, used for effects that increase grounded speed like magiluminescence. Defaults to 1f.</summary>
		public float ShapeshifterMoveSpeedBonusGrounded = 1f;
		/// <summary>Multiplicative, used for effects that increase the movespeed of "flying" wildshapes, at all times. Defaults to 1f.</summary>
		public float ShapeshifterMoveSpeedBonusNotGrounded = 1f;
		/// <summary>Multiplies the speed of other means of movement, like dashes. Should generally not be edited as dashes that should be affected by movespeed already are. Defaults to 1f.</summary>
		public float ShapeshifterMoveSpeedMiscOverride = 1f;
		/// <summary>Deceleration multiplier for shapeshifter movement. Allows making slippery surfaces, or dashes. Defaults to 1f.</summary>
		public float ShapeshifterMoveSpeedDecelerate = 1f;
		/// <summary>Acceleration multiplier for shapeshifter movement, does not modify top speed and should be used for stuff like slippery surfaces. Defaults to 1f.</summary>
		public float ShapeshifterMoveSpeedAccelerate = 1f;
		/// <summary>Multiplicative, affects the direct healing provided by shapeshifter effects (generally possesed by Warden wildshapes). Defaults to 1f.</summary>
		public float ShapeshifterHealingBonus = 1f;
		/// <summary>Multiplicative, affects most jumps for various wildshapes. Defaults to 1f.</summary>
		public float ShapeshifterJumpSpeed = 1f;
		/// <summary>Multiplicative, mostly used for movement in liquids. Defaults to 1f.</summary>
		public float ShapeshifterGravity = 1f;
		/// <summary>Multiplicative, mostly used for movement in liquids. Defaults to 1f.</summary>
		public float ShapeshifterMaxFallSpeed = 1f;

		/// <summary>Harpy armor set bonus (causes feathers to fall when attacking from above)</summary>
		public bool ShapeshifterSetHarpy = false;
		/// <summary>Pyre armor set bonus (creates flames around thep layer when dealing damage)</summary>
		public bool ShapeshifterSetPyre = false;
		/// <summary>If true, hitting new targets while shapeshifted into a Sage increase feral damage (Deepwater Locket locket effect).</summary>
		public bool ShapeshifterSageDamageOnHit = false;
		/// <summary>Used by the survival potion.</summary>
		public bool ShapeshifterSurvival = false;
		/// <summary>If true, uses the player hair color on compatible wildshapes.</summary>
		public bool ShapeshifterHairpin = false;

		/// <summary>Used only for dash visuals</summary>
		public bool ShapeshifterShawlFeather = false;
		/// <summary>Used only for dash visuals</summary>
		public bool ShapeshifterShawlWind = false;
		/// <summary>Used only for dash visuals</summary>
		public bool ShapeshifterShawlPhoenix = false;

		/// <summary>Shawl accessories tree effect. If >0f, the player gains the ability to dash while shapeshifted.
		/// This is the base speed at which the player will be launched. Be careful not to override this with a lower value. Defaults to 0f.</summary>
		public float ShapeshifterHookDash = 0f;
		/// <summary>Youxia Harness effect, this is the base damage of the projectile fired.</summary>
		public int ShapeshifterHarness = 0;

		// Dynamic gameplay and UI fields

		/// <summary>Goes up by 1 every frame, up to 300. Shapeshifting removes 300 from this value. If equal to 300 when shapeshifter, reduces cooldowns for a more smooth gameplay.
		/// If below 0 while shapeshifted, applied the Shapeshifting Sickness debuff, reducing various stats.</summary>
		public int ShapeshifterFastShapeshiftTimer = 300;
		/// <summary>Used by the Ice Fox wildshape to increase speed upon transforming.</summary>
		public int ShapeshifterPredatorFoxSpeed = 0;
		/// <summary>Used by the Deepwater Locket effect (ShapeshifterSageDamageOnHit)</summary>
		public int ShapeshifterSageDamageOnHitCount = 0;
		/// <summary>Used by the Deepwater Locket effect (ShapeshifterSageDamageOnHit)</summary>
		public int ShapeshifterSageDamageOnHitTimer = 0;
		/// <summary>Used by the Deepwater Locket effect (ShapeshifterSageDamageOnHit)</summary>
		public int[] ShapeshifterSageDamageOnHitTargets;
		/// <summary>Used to spawn the Harpy set feathers (ShapeshifterShawlFeather)</summary>
		public int ShapeshifterSetHarpyDamagePool = 0;
		/// <summary>Used as a time by various shapeshifter armor sets.</summary>
		public int ShapeshifterSetTimer = 0;
		/// <summary>Used to spawn the Pyre set flames (ShapeshifterSetPyre)</summary>
		public int ShapeshifterSetPyreDamagePool = 0;
		/// <summary>Cooldown for the shawl accessory dashes</summary>
		public int ShapeshifterShawlCooldown = 0;
		/// <summary>How long has the player been holding the dash key?</summary>
		public int ShapeshifterHookInputTimer = 0;
		/// <summary>Lowers deceleration while >0. Used by dashes.</summary>
		public int ShapeshifterNoDecelerationTimer = 0;
		/// <summary>Synced so other clients can display what happens at the start of a hook dash</summary>
		public bool ShapeshifterHookDashSync = false;
		/// <summary>Should be set to 30, used to display an arrow when the dash is available</summary>
		public int ShapeshifterUIDashTimer = 0;
		/// <summary>Should be set to 30, used to display a fox icon when a transformation is ready or the player transforms too much</summary>
		public int ShapeshifterUITransformationTimer = 0;
		/// <summary>Prevents transforming while the player is scrolling</summary>
		public int ShapeshifterScrollTransformationBuffer = 0;
		/// <summary>Used to quickly transform while no mount is equipped. Resets to false every frame is Player.controlMount is not true.</summary>
		public bool ShapeshifterControlMountRelease = false;

		public override void HideDrawLayers(PlayerDrawSet drawInfo)
		{
			if (ShapeshiftAnchor != null)
			{
				foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.DrawOrder)
				{
					if (!ShapeshifterAuthorizedDrawLayers.Contains(layer.Name))
					{
						layer.Hide();
					}
				}
			}
		}

		/*
		public override void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, Position> positions)
		{
			if (OrchidMod.ThoriumMod != null)
			{ // places the shifter drawlayer between thorium orbitals if thorium is enabled
				PlayerDrawLayer thoriumOrbitalsLayer = null;
				foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.DrawOrder)
				{
					if (layer.Name == "OrbitalLayerFront")
					{
						thoriumOrbitalsLayer = layer;
						break;
					}
				}

				foreach (var position in positions)
				{
					if (position.Key is ShapeshifterHairDrawLayer)
					{
						positions.Add(position.Key, new BeforeParent(thoriumOrbitalsLayer));
						positions.Remove(position);
						break;
					}
				}
			}
		}
		*/

		public override void Initialize()
		{
			modPlayer = Player.GetModPlayer<OrchidPlayer>();
			ShapeshifterSageDamageOnHitTargets = [-1, -1, -1, -1, -1, -1, -1, -1, -1, -1];
			ShapeshifterIncompatibleProjectiles = new List<int>();
			ShapeshifterIncompatibleBuffs = [BuffID.Stoned];
			ShapeshifterAuthorizedDrawLayers = new List<string>() { 
				"SolarShield", "FrozenOrWebbedDebuff", "ElectrifiedDebuffBack", "IceBarrier", "CaptureTheGem", // vanilla
				"BeetleBuff", "EyebrellaCloud", "ElectrifiedDebuffFront", "ForbiddenSetRing", "SafemanSun", "WebbedDebuffBack", // vanilla
				"OrbitalLayerBack", "OrbitalLayerFront", // thorium
				"ShapeshifterDrawLayer" // ...
			};

			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				ShapeshifterIncompatibleProjectiles.Add(thoriumMod.Find<ModProjectile>("AmmutsebaSashPro").Type);
				ShapeshifterIncompatibleProjectiles.Add(thoriumMod.Find<ModProjectile>("ZephyrsGripPro").Type);
			}
		}

		public override void ResetEffects()
		{
			ShapeshifterScrollTransformationBuffer--;

			if (IsShapeshifted)
			{
				if (Player.Center.Distance(ShapeshiftAnchor.Projectile.Center) > 96f && ShapeshiftAnchor.Projectile.velocity.Length() < 32f)
				{ // the player is far away from the projectile center, which is abnormal -> they likely teleported
					Shapeshift.ShapeshiftTeleport(Player.Center, ShapeshiftAnchor.Projectile, ShapeshiftAnchor, Player, this);
				}

				Player.width = Shapeshift.ShapeshiftWidth;
				Player.height = Shapeshift.ShapeshiftHeight;
				Player.Center = ShapeshiftAnchor.Projectile.Center;
			}

			if (ShapeshiftAnchor != null && ShapeshiftAnchor.Projectile.active)
			{
				if (ShapeshiftAnchor.NeedKill && Player.whoAmI == Main.myPlayer)
				{ // Kills the anchor properly if NeedKill was set to true, avoiding issues caused by killing it randomly
					ShapeshiftAnchor.NeedKill = false;
					ShapeshiftAnchor.Projectile.Kill();
					ShapeshiftAnchor = null;
					Shapeshift = null;
				}

				if (Player.mount.Active || Player.grappling[0] >= 0 || Player.timeShimmering > 0 && ShapeshiftAnchor != null)
				{ // Disable the shapeshift if the player is mounted, shimmered or uses a hook
					ShapeshiftAnchor.Projectile.Kill();
					ShapeshiftAnchor = null;
					Shapeshift = null;
				}

				foreach (Projectile projectile in Main.projectile)
				{// Disable the shapeshift if the player owns an incompatible projectile (special thorium mod hooks for example)
					if (ShapeshifterIncompatibleProjectiles.Contains(projectile.type) && projectile.owner == Player.whoAmI && projectile.active && ShapeshiftAnchor != null)
					{
						ShapeshiftAnchor.Projectile.Kill();
						ShapeshiftAnchor = null;
						Shapeshift = null;
					}
				}

				foreach (int buffType in ShapeshifterIncompatibleBuffs)
				{ // Disable the shapeshift if the player has an incompatible buff
					if (Player.HasBuff(buffType) && ShapeshiftAnchor != null)
					{
						ShapeshiftAnchor.Projectile.Kill();
						ShapeshiftAnchor = null;
						Shapeshift = null;
					}
				}
			}

			ShapeshifterSetTimer--;

			if (!ShapeshifterSetHarpy)
			{
				ShapeshifterSetHarpyDamagePool = 0;
			}

			if (!ShapeshifterSetPyre)
			{
				ShapeshifterSetPyreDamagePool = 0;
			}

			if (ShapeshifterUIDashTimer > 0)
			{
				ShapeshifterUIDashTimer--;
			}

			if (ShapeshifterUITransformationTimer > 0)
			{
				ShapeshifterUITransformationTimer--;
			}

			if (ShapeshifterFastShapeshiftTimer < 300f)
			{
				ShapeshifterFastShapeshiftTimer++;
				if (ShapeshifterFastShapeshiftTimer >= 300f && Player.whoAmI == Main.myPlayer)
				{
					ShapeshifterUITransformationTimer = 30;
					SoundStyle soundStyle = SoundID.Item35;
					soundStyle.Volume -= 0.5f;
					soundStyle.Pitch -= 1f;
					SoundEngine.PlaySound(soundStyle, Player.Center);
				}
			}

			if (ShapeshifterShawlCooldown > 0f)
			{
				ShapeshifterShawlCooldown--;
				if (ShapeshifterShawlCooldown <= 0f && Player.whoAmI == Main.myPlayer)
				{
					ShapeshifterUIDashTimer = 30;
					SoundStyle soundStyle = SoundID.Grass;
					soundStyle.Volume -= 0.5f;
					soundStyle.Pitch += 1f;
					SoundEngine.PlaySound(soundStyle, Player.Center);
				}
			}

			if (!Player.controlMount)
			{
				ShapeshifterControlMountRelease = true;
			}

			// Reset gameplay fields

			ShapeshifterPredatorBleedPotency = 0;
			ShapeshifterPredatorBleedMaxStacks = 0;
			ShapeshifterMeleeSpeedBonus = 0f;
			ShapeshifterMoveSpeedBonus = 0f;
			ShapeshifterMoveSpeedBonusFlat = 0f;
			ShapeshifterMoveSpeedBonusFinal = 1f;
			ShapeshifterMoveSpeedBonusGrounded = 1f;
			ShapeshifterMoveSpeedBonusNotGrounded = 1f;
			ShapeshifterMoveSpeedMiscOverride = 1f;
			ShapeshifterMoveSpeedDecelerate = 1f;
			ShapeshifterMoveSpeedAccelerate = 1f;
			ShapeshifterHealingBonus = 1f;
			ShapeshifterJumpSpeed = 1f;
			ShapeshifterGravity = 1f;
			ShapeshifterMaxFallSpeed = 1f;

			ShapeshifterHookDash = 0f;
			ShapeshifterHarness = 0;
			ShapeshifterSetHarpy = false;
			ShapeshifterSetPyre = false;
			ShapeshifterSageDamageOnHit = false;
			ShapeshifterSurvival = false;
			ShapeshifterHairpin = false;
			ShapeshifterShawlFeather = false;
			ShapeshifterShawlWind = false;
			ShapeshifterShawlPhoenix = false;
		}

		public override void PostUpdateEquips()
		{
			if (IsShapeshifted)
			{
				Shapeshift.ShapeshiftBuffs(ShapeshiftAnchor.Projectile, ShapeshiftAnchor, Player, this);

				// Cancels some equipment effects to prevent visual & audio issues

				Player.rocketBoots = 0;
				Player.vanityRocketBoots = 0;
				Player.accRunSpeed = 3f; // clears hermes boots smoke
				Player.ExtraJumps.Clear(); // clears double jump visuals
				Player.dashDelay = 30; // clears dash visuals
				Player.spikedBoots = 0;

				if (Player.wingTime > 0)
				{
					Player.wingTime = 0;
				}

				// Grants stats to make some equipment compatible with shapeshifter

				if (Player.hasMagiluminescence)
				{
					ShapeshifterMoveSpeedBonusGrounded += 0.15f;
				}

				if (Player.shadowArmor)
				{
					ShapeshifterMoveSpeedBonusFinal += 0.15f;
				}

				foreach (int buffType in Shapeshift.ShapeshiftImmunities)
				{
					Player.buffImmune[buffType] = true;
				}

				// misc stat changes

				if (ShapeshifterSurvival)
				{
					int count = 0;
					float segment = Player.statLifeMax2 * 0.167f;

					while (Player.statLife - count * segment > segment)
					{
						count++;
					}

					modPlayer.OrchidDamageResistance += 0.15f - count * 0.03f;
					Player.GetDamage<ShapeshifterDamageClass>() += count * 0.03f;
				}
			}

			// Misc Effects that should be called before Shapeshifter Core mechanics (eg : stat changes that should affect the shapeshifted player)

			if (ShapeshifterPredatorFoxSpeed > 0)
			{
				ShapeshifterPredatorFoxSpeed--;
				Player.moveSpeed += ShapeshifterPredatorFoxSpeed * 0.003f;
			}

			if (ShapeshifterSageDamageOnHitTimer > 0)
			{
				ShapeshifterSageDamageOnHitTimer--;

				if (ShapeshifterSageDamageOnHitTimer <= 0)
				{
					ShapeshifterSageDamageOnHitCount = 0;
					ShapeshifterSageDamageOnHitTargets = [-1, -1, -1, -1, -1, -1, -1, -1, -1, -1];
				}
			}

			if (ShapeshifterSetPyre)
			{
				int projectileType = ModContent.ProjectileType<ShapeshifterAshwoodFlame>();
				foreach (Projectile projectile in Main.projectile)
				{
					if (projectile.active && projectile.type == projectileType && Player.whoAmI == projectile.ai[0])
					{
						Player.GetDamage<ShapeshifterDamageClass>() += 0.02f;
					}
				}
			}
		}

		public override void PostUpdate()
		{
			if (IsShapeshifted)
			{
				// SHAPESHIFTER GENERAL STATS CHANGES

				// SHAPESHIFTER MOVEMENT STATS CHANGES

				// Jump speed has to be edited here, because its fields are updated after PostUpdateEquips()
				// This makes it so a wildshape gets slightly worse benefits as a normal player from the Shiny Red Balloon and Frog Leg accessories
				// However, because wildshapes jumps are also enhanced by their movement speed, this is fine, it simply avoids movement getting out of hand
				ShapeshifterJumpSpeed += (Player.jumpHeight - 15f) * 0.025f; // the base Player.jumpHeight value is 15
				ShapeshifterJumpSpeed += (Player.jumpSpeed - 5.01f) * 0.1f; // the base Player.jumpSpeed value is 5.01

				if (Player.gravity > Player.defaultGravity)
				{ // compensates gravity efects to "buff" jump height in low gravity
					ShapeshifterJumpSpeed += (Player.gravity - Player.defaultGravity) * 1.5f;
				}

				if (Player.gravity < Player.defaultGravity)
				{ // compensates gravity efects to "nerf" jump height in high gravity
					ShapeshifterJumpSpeed -= (Player.defaultGravity - Player.gravity) * 1.5f;
				}

				// Environmental movement speed changes (honey, liquids, etc)

				if (Player.sticky)
				{ // honey
					ShapeshifterMoveSpeedBonusFinal *= 0.3f;
					ShapeshifterMoveSpeedBonusFlat *= 0.3f;
				}

				if ((Player.wet && !Player.ignoreWater) || Player.lavaWet)
				{ // in lava on water
					ShapeshifterGravity *= 0.5f;
					ShapeshifterJumpSpeed *= 0.5f;
					ShapeshifterMoveSpeedBonusFinal *= 0.5f;
					ShapeshifterMoveSpeedBonusFlat *= 0.5f;
				}

				if (Player.wet && Player.ignoreWater)
				{ // I don't know why either, but this is needed to replicate normal movement
					ShapeshifterGravity *= 2f;
				}

				if (Player.honeyWet)
				{ // in honey (not vanilla accurate but good enough)
					ShapeshifterGravity *= 5f;
					ShapeshifterJumpSpeed *= 0.5f;
					ShapeshifterMoveSpeedBonusFinal *= 0.25f;
					ShapeshifterMoveSpeedBonusFlat *= 0.25f;
					ShapeshifterMoveSpeedMiscOverride *= 0.5f;
				}

				if (Player.shimmerWet)
				{ // in shimmer
					ShapeshifterGravity *= 0.375f;
					ShapeshifterJumpSpeed *= 0.375f;
					ShapeshifterMoveSpeedBonusFinal *= 0.375f;
					ShapeshifterMoveSpeedBonusFlat *= 0.375f;
					ShapeshifterMoveSpeedMiscOverride *= 0.375f;
				}

				if (Player.powerrun && Shapeshift.GroundedWildshape)
				{ // asphalt
					ShapeshifterMoveSpeedBonusGrounded *= 3f;
				}

				if (Player.slippy)
				{ // Ice
					if (Player.iceSkate)
					{
						ShapeshifterMoveSpeedDecelerate *= 0.25f;
						ShapeshifterMoveSpeedBonusGrounded += 0.15f;
					}
					else
					{
						ShapeshifterMoveSpeedDecelerate *= 0.05f;
						ShapeshifterMoveSpeedAccelerate *= 0.25f;
					}
				}
				
				if (Player.slippy2)
				{ // Frozen Slime (no deceleration)
					ShapeshifterMoveSpeedDecelerate *= 0f;
					ShapeshifterMoveSpeedAccelerate *= 0.1f;
				}

				if (ShapeshifterNoDecelerationTimer > 0)
				{
					ShapeshifterNoDecelerationTimer--;
					ShapeshifterMoveSpeedDecelerate *= 0f;
				}

				Shapeshift.HandleSpecificInteractionsPostUpdate(ShapeshiftAnchor.Projectile, Player);

				// SHAPESHIFTER CORE BEHAVIOUR

				// Runs the shapeshift AI and adjust player position accordingly
				Player.width = Shapeshift.ShapeshiftWidth;
				Player.height = Shapeshift.ShapeshiftHeight;

				Projectile projectile = ShapeshiftAnchor.Projectile;

				if (Player.whoAmI == Main.myPlayer)
				{ // Shapeshift inputs
					ShapeshiftAnchor.CheckInputs(Player); // mostly used to sync inputs in mp

					if (Shapeshift.ShapeshiftCanLeftClick(projectile, ShapeshiftAnchor, Player, this) && !Player.cursed)
					{
						Shapeshift.ShapeshiftOnLeftClick(projectile, ShapeshiftAnchor, Player, this);

						if (Player.boneGloveItem != null && !Player.boneGloveItem.IsAir && Player.boneGloveTimer == 0)
						{ // Bone glove compatibility, from vanilla code
							Player.boneGloveTimer = 60;
							Vector2 center = Player.Center;
							Vector2 vector = Player.DirectionTo(Player.ApplyRangeCompensation(0.2f, center, Main.MouseWorld)) * 10f;
							Projectile.NewProjectile(Player.GetSource_ItemUse(Player.boneGloveItem), center.X, center.Y, vector.X, vector.Y, 532, 25, 5f, Player.whoAmI);
						}
					}

					if (Shapeshift.ShapeshiftCanRightClick(projectile, ShapeshiftAnchor, Player, this) && !Player.cursed)
					{
						Shapeshift.ShapeshiftOnRightClick(projectile, ShapeshiftAnchor, Player, this);
					}

					if (Shapeshift.ShapeshiftCanJump(projectile, ShapeshiftAnchor, Player, this))
					{
						Shapeshift.ShapeshiftOnJump(projectile, ShapeshiftAnchor, Player, this);
					}

					if (Player.Center.Distance(projectile.Center) > 96f && projectile.velocity.Length() < 32f)
					{ // the player is far away from the projectile center, which is abnormal -> they likely teleported
						Shapeshift.ShapeshiftTeleport(Player.Center, projectile, ShapeshiftAnchor, Player, this);
					}

					if (Player.controlHook && ShapeshifterHookDash > 0f)
					{
						ShapeshifterHookInputTimer++;

						if (!ModContent.GetInstance<OrchidClientConfig>().ShapeshifterHookDashRelease && ShapeshifterHookInputTimer == 1 && ShapeshifterShawlCooldown <= 0)
						{
							ShapeshifterHookDashSync = true;

							if (Main.netMode == NetmodeID.MultiplayerClient)
							{
								var packet = OrchidMod.Instance.GetPacket();
								packet.Write((byte)OrchidModMessageType.SHAPESHIFTERHOOKDASH);
								packet.Write(Player.whoAmI);
								packet.Send();
							}
						}

						if (ShapeshifterHookInputTimer == ModContent.GetInstance<OrchidClientConfig>().ShapeshifterHookDelay + 1 && Player.miscEquips[4].type != ItemID.None)
						{ // uses the player hook
							Item item = Player.miscEquips[4];
							Vector2 velocity = Vector2.Normalize(Main.MouseWorld - Player.Center) * item.shootSpeed;
							Projectile.NewProjectile(Player.GetSource_ItemUse(item), Player.Center + velocity * 3, velocity, item.shoot, 0, 0, Player.whoAmI);
							SoundEngine.PlaySound(item.UseSound, Player.Center);
						}
					}
					else
					{
						if (ShapeshifterHookDash > 0f && ShapeshifterHookInputTimer <= ModContent.GetInstance<OrchidClientConfig>().ShapeshifterHookDelay && ShapeshifterHookInputTimer > 0 && ModContent.GetInstance<OrchidClientConfig>().ShapeshifterHookDashRelease && ShapeshifterShawlCooldown <= 0)
						{
							ShapeshifterHookDashSync = true;

							if (Main.netMode == NetmodeID.MultiplayerClient)
							{
								var packet = OrchidMod.Instance.GetPacket();
								packet.Write((byte)OrchidModMessageType.SHAPESHIFTERHOOKDASH);
								packet.Write(Player.whoAmI);
								packet.Send();
							}
						}

						ShapeshifterHookInputTimer = 0;
					}
				}

				if (ShapeshifterHookDashSync)
				{
					ShapeshifterHookDashSync = false;
					OnShapeshiftHookDash();
				}

				Shapeshift.ShapeshiftAnchorAI(projectile, ShapeshiftAnchor, Player, this);
				ShapeshiftAnchor.ExtraAI(Player, this);

				// Rounds up the player X and Y velocity to 0 when they are neglibily small
				if (Math.Abs(projectile.velocity.X) < 0.001f)
				{
					projectile.velocity.X = 0f;
				}

				if (Math.Abs(projectile.velocity.Y) < 0.001f)
				{
					projectile.velocity.Y = 0f;
				}

				Player.velocity = projectile.velocity;
				Player.Center = projectile.Center + projectile.velocity;
			}
			else if (ShapeshiftAnchor != null || Shapeshift != null)
			{ // Failsafe in case the anchor isn't properly killed
				Player.width = Player.defaultWidth;
				Player.height = Player.defaultHeight;
				Shapeshift = null;
				ShapeshiftAnchor = null;
			}

			if (Player.controlMount && ShapeshifterControlMountRelease && Player.QuickMount_GetItemToUse() == null)
			{ // attempts to shapeshift the player into the first wildshape in their hotbar (by using it normally) if they press the mount key while not having a mount equipped.
				if (IsShapeshifted)
				{ // unshifts the player
					ShapeshifterControlMountRelease = false;
					ShapeshiftAnchor.NeedKill = true;
				}
				else if (Player.ItemTimeIsZero)
				{
					for (int i = 0 ; i < 9; i++)
					{
						Item item = Player.inventory[i];
						if (item.type != ItemID.None && item.ModItem != null && item.ModItem is OrchidModShapeshifterShapeshift)
						{
							ShapeshifterControlMountRelease = false;
							Player.selectedItem = i;
							Player.controlUseItem = true;
							Player.ItemCheck();
						}
					}
				}
			}

			// Misc Effects that should be called after shapeshifter core mechanics (eg: that depend of the player width and height to be correct)

			if (ShapeshifterPredatorFoxSpeed > 0)
			{
				if (Main.rand.NextBool((int)(30 - ShapeshifterPredatorFoxSpeed / 6f) + 1))
				{
					Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.IceTorch, Scale: Main.rand.NextFloat(1f, 1.4f));
					dust.noGravity = true;
					dust.noLight = true;
				}
			}

			if (ShapeshifterSageDamageOnHitCount > 0)
			{ // increases damage for each individual enemy hit by a sage attack
				Player.GetDamage<ShapeshifterDamageClass>() += ShapeshifterSageDamageOnHitCount * 0.02f;
			}

			if (ShapeshifterSetHarpyDamagePool > 0)
			{ // if ShapeshifterSetHarpyDamagePool is above 20, empty it gradually, spawning damaging feathers
				if (ShapeshifterSetHarpyDamagePool >= 20 && ShapeshifterSetTimer <= 0 && modPlayer.LastHitNPC != null)
				{
					ShapeshifterSetHarpyDamagePool -= 20;
					ShapeshifterSetTimer = 10;
					int projectileType = ModContent.ProjectileType<ShapeshifterHarpyProj>();
					NPC target = modPlayer.LastHitNPC;
					int damage = GetShapeshifterDamage(15);
					Vector2 velocity = Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(20)) * 8f;
					Vector2 position = target.Center + target.velocity * 20f - velocity * 90f;
					Projectile newProjectile = Projectile.NewProjectileDirect(Player.GetSource_FromAI(), position, velocity, projectileType, damage, 0f, Player.whoAmI, ai2: target.Center.Y);
					newProjectile.CritChance = GetShapeshifterCrit();
				}
			}

			if (ShapeshifterSetPyre)
			{ // if ShapeshifterSetPyreDamagePool is above 300, empty it gradually, spawning pyre flames
				int projectileType = ModContent.ProjectileType<ShapeshifterAshwoodFlame>();
				int count = 0;
				foreach (Projectile projectile in Main.projectile)
				{
					if (projectile.active && projectile.type == projectileType && Player.whoAmI == projectile.ai[0])
					{
						count++;

						if (ShapeshifterSetTimer <= -600)
						{
							projectile.ai[0] = 1f; // kill
							projectile.netUpdate = true;
						}
					}
				}

				if (ShapeshifterSetPyreDamagePool >= 300 && ShapeshifterSetTimer <= 0 && modPlayer.LastHitNPC != null)
				{
					ShapeshifterSetPyreDamagePool -= 300;
					ShapeshifterSetTimer = 30;

					if (count < 5)
					{
						int damage = GetShapeshifterDamage(45);
						Projectile newProjectile = Projectile.NewProjectileDirect(Player.GetSource_FromAI(), Player.Center, Vector2.Zero, projectileType, damage, 0f, Player.whoAmI, Player.whoAmI);
						newProjectile.CritChance = GetShapeshifterCrit();
					}
				}
			}
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!target.CountsAsACritter && !target.friendly && Player.whoAmI == Main.myPlayer && IsShapeshifted)
			{
				if (ShapeshifterSetHarpy)
				{
					int projectileType = ModContent.ProjectileType<ShapeshifterHarpyProj>();
					if (target.Center.Y /* - target.width * 0.5f */ > Player.Center.Y + Player.height * 0.5f && proj.type != projectileType)
					{ // attacks from above store 50% of the damage dealt in ShapeshifterSetHarpyDamagePool
						ShapeshifterSetHarpyDamagePool += (int)(damageDone * 0.5f);
						if (ShapeshifterSetHarpyDamagePool > 100)
						{
							ShapeshifterSetHarpyDamagePool = 100;
						}
					}
				}

				if (ShapeshifterSetPyre)
				{
					int projectileType = ModContent.ProjectileType<ShapeshifterAshwoodProj>();
					if (proj.type != projectileType)
					{
						ShapeshifterSetPyreDamagePool += (int)(damageDone);
						if (ShapeshifterSetPyreDamagePool > 1000)
						{
							ShapeshifterSetPyreDamagePool = 1000;
						}
					}
				}
			}
		}

		public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
		{
			if (IsShapeshifted && !Player.noKnockback)
			{
				Shapeshift.ShapeshiftOnHitByAnything(hurtInfo, ShapeshiftAnchor.Projectile, ShapeshiftAnchor, Player, this);
				Shapeshift.ShapeshiftOnHitByProjectile(proj, hurtInfo, ShapeshiftAnchor.Projectile, ShapeshiftAnchor, Player, this);

				if (!Player.noKnockback)
				{ // Player knockback on hit
					ShapeshiftAnchor.Projectile.velocity = new Vector2(3f * hurtInfo.HitDirection, -3f);
				}
			}
		}

		public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
		{
			if (IsShapeshifted)
			{
				Shapeshift.ShapeshiftOnHitByAnything(hurtInfo, ShapeshiftAnchor.Projectile, ShapeshiftAnchor, Player, this);
				Shapeshift.ShapeshiftOnHitByNPC(npc, hurtInfo, ShapeshiftAnchor.Projectile, ShapeshiftAnchor, Player, this);

				if (!Player.noKnockback)
				{ // Player knockback on hit
					ShapeshiftAnchor.Projectile.velocity = new Vector2(3f * hurtInfo.HitDirection, -3f);
				}
			}
		}

		public override void OnHurt(Player.HurtInfo info)
		{
			if (ShapeshifterSetPyre)
			{
				int projectileType = ModContent.ProjectileType<ShapeshifterAshwoodFlame>();
				int count = 0;
				foreach (Projectile projectile in Main.projectile)
				{
					if (projectile.active && projectile.type == projectileType && Player.whoAmI == projectile.ai[0])
					{
						count++;
						projectile.ai[1] = count * 5f; // kill (explode)
						projectile.netUpdate = true;
					}
				}
			}
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			if (IsShapeshifted)
			{
				Shapeshift.ShapeshiftModifyHurt(ref modifiers, ShapeshiftAnchor.Projectile, ShapeshiftAnchor, Player, this);
			}
		}

		public override bool FreeDodge(Player.HurtInfo info)
		{
			if (IsShapeshifted)
			{
				return Shapeshift.ShapeshiftFreeDodge(info, ShapeshiftAnchor.Projectile, ShapeshiftAnchor, Player, this);
			}
			return false;
		}

		public void OnShapeshift(Projectile anchorProjectile, ShapeshifterShapeshiftAnchor anchor, Player owner, OrchidShapeshifter shapeshifter)
		{
		}

		public void OnShapeshiftHookDash()
		{
			Projectile projectile = ShapeshiftAnchor.Projectile;

			// General dash visuals and effects

			SoundStyle dashSound = SoundID.DoubleJump;
			dashSound.Pitch *= Main.rand.NextFloat(1.25f, 1.75f);
			SoundEngine.PlaySound(dashSound, projectile.Center);

			for (int i = 0; i < 5; i++)
			{
				Dust dust = Dust.NewDustDirect(projectile.Center, 0, 0, DustID.Smoke);
				dust.scale *= Main.rand.NextFloat(1f, 1.5f);
				dust.velocity *= Main.rand.NextFloat(0.5f, 0.75f);
			}

			for (int i = 0; i < 3; i++)
			{
				Gore gore = Gore.NewGoreDirect(projectile.GetSource_FromAI(), projectile.Center + new Vector2(Main.rand.NextFloat(-24f, 0f), Main.rand.NextFloat(-24f, 0f)), Vector2.UnitY.RotatedByRandom(MathHelper.Pi), 61 + Main.rand.Next(3));
				gore.rotation = Main.rand.NextFloat(MathHelper.Pi);
			}

			// Specific dash effects

			if (ShapeshifterShawlPhoenix)
			{
				SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFurySwing, projectile.Center);
				for (int i = 0; i < 5; i++)
				{
					Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, ModContent.DustType<ShapeshifterDustPhoenix>(), Scale: Main.rand.NextFloat(1.2f, 1.4f));
					dust.velocity *= 0.5f;
					dust.velocity.Y = 2f;
					dust.customData = Main.rand.Next(314);
					dust.fadeIn -= 60;
				}

				for (int i = 0; i < 8; i++)
				{
					Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Torch, Scale: Main.rand.NextFloat(1.2f, 1.4f));
					dust.velocity *= Main.rand.NextFloat(0.5f, 1.5f);
					dust.noGravity = true;
				}
			}
			else if (ShapeshifterShawlWind)
			{
				SoundEngine.PlaySound(SoundID.Grass, projectile.Center);
				for (int i = 0; i < 5; i++)
				{
					Gore.NewGoreDirect(Player.GetSource_ItemUse(Shapeshift.Item), projectile.Center + new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f)), Vector2.UnitY.RotatedByRandom(MathHelper.Pi), GoreID.TreeLeaf_Jungle);
				}
			}
			else if (ShapeshifterShawlFeather)
			{ // dash visuals for the feather shawl
				SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFurySwing, projectile.Center);
				for (int i = 0; i < 5; i++)
				{
					Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, ModContent.DustType<PredatorHarpyDust>(), Scale: Main.rand.NextFloat(1.2f, 1.4f));
					dust.velocity *= 0.5f;
					dust.velocity.Y = 2f;
					dust.customData = Main.rand.Next(314);
					dust.fadeIn -= 60;
				}
			}

			// base dash behaviour
			if (Player.whoAmI == Main.myPlayer)
			{
				Vector2 offSet = Vector2.Normalize(Main.MouseWorld - projectile.Center) * ShapeshifterHookDash * Shapeshift.GetSpeedMult(Player, this, ShapeshiftAnchor);
				projectile.velocity = offSet;
				ShapeshiftAnchor.NeedNetUpdate = true;
			}

			Shapeshift.ResetFallHeight(Player);
			ShapeshifterMoveSpeedDecelerate = 0;
			ShapeshifterNoDecelerationTimer = 13;
			ShapeshifterShawlCooldown = 300;
		}

		public void OnShapeshiftFast(Projectile anchorProjectile, ShapeshifterShapeshiftAnchor anchor, Player owner, OrchidShapeshifter shapeshifter)
		{
			if (ShapeshifterHarness > 0)
			{ // Youxia Harness daggers
				int nbDaggers = 5; // maybe this could be edited with gear by upgrading the harness
				int[] shotenemies = new int[nbDaggers];

				for (int j = 0; j < shotenemies.Length; j++)
				{
					shotenemies[j] = -1;
				}

				for (int i = 0; i < nbDaggers; i++)
				{
					int hitCountMaximum = 0;
					float closestDistance = 400f; // 25 tiles
					float closestDistanceBase = closestDistance;
					NPC closestTarget = null;
					foreach (NPC npc in Main.npc)
					{
						if (OrchidModProjectile.IsValidTarget(npc))
						{
							float distance = anchorProjectile.Center.Distance(npc.Center);
							int hitCount = 0;

							for (int j = 0; j < shotenemies.Length; j ++)
							{
								if (shotenemies[j] == npc.whoAmI)
								{
									hitCount++;
								}
							}

							if (distance < closestDistanceBase && (closestDistance == closestDistanceBase || hitCount < hitCountMaximum))
							{
								closestTarget = npc;
								closestDistance = distance;

								if (hitCountMaximum < hitCount)
								{
									hitCountMaximum = hitCount;
								}
							}
						}
					}

					int projectileType = ModContent.ProjectileType<ShapeshifterHarnessProj>();
					int damage = GetShapeshifterDamage(ShapeshifterHarness);
					Vector2 position = anchorProjectile.Center;
					Vector2 velocity;

					if (closestTarget != null)
					{

						for (int j = 0; j < shotenemies.Length; j++)
						{
							if (shotenemies[j] == -1)
							{
								shotenemies[j] = closestTarget.whoAmI;
								break;
							}
						}

						velocity = Vector2.Normalize(closestTarget.Center + closestTarget.velocity * 20f - anchorProjectile.Center).RotatedByRandom(MathHelper.ToRadians(2f)) * 8f;
					}
					else
					{
						float logDaggers = (float)Math.Log10(nbDaggers) * 20f;
						float angle = (-logDaggers * nbDaggers * 0.5f + (nbDaggers % 2 == 0 ? 0f : logDaggers * 0.5f) + logDaggers * i) * -owner.direction;
						velocity = Vector2.Normalize(Main.MouseWorld - anchorProjectile.Center).RotatedBy(MathHelper.ToRadians(angle)) * 8f;
					}

					Projectile newProjectile = Projectile.NewProjectileDirect(Player.GetSource_FromAI(), position, velocity, projectileType, damage, 0f, Player.whoAmI, ai0: i * 20);
					newProjectile.CritChance = GetShapeshifterCrit();
				}
			}
		}
	}
}
