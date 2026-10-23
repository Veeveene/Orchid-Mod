using Microsoft.Xna.Framework;
using OrchidMod.Common.ModObjects;
using OrchidMod.Content.Guardian;
using OrchidMod.Content.Guardian.Buffs;
using OrchidMod.Content.Guardian.Projectiles.Misc;
using OrchidMod.Content.Guardian.Projectiles.Standards;
using OrchidMod.Content.Guardian.Weapons.Gauntlets;
using System;
using System.Reflection;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using OrchidMod.Content.Guardian.Accessories;
using OrchidMod.Common;

namespace OrchidMod
{
	public class OrchidGuardian : ModPlayer
	{


		public bool CrossModGodMode;

		// Misc & Static fields

		/// <summary> Current timer for slam stack regen or degen. Increments slams at 1 or higher, decrements at -1 or lower.</summary>
		public static List<int> ProjectilesBlockBlacklist;
		public OrchidPlayer modPlayer;

		// Can be edited by gear

		/// <summary> Natural guard generation multiplier. The player will naturally generate this many guards every 10 seconds.</summary>
		public float GuardianGuardRecharge = 1f;
		/// <summary> Natural slam generation multiplier. The player will naturally generate this many slams every 10 seconds.</summary>
		public float GuardianSlamRecharge = 1f;
		/// <summary> Max guard charges. Defaults to 3.</summary>
		public int GuardianGuardMax = 3;
		/// <summary> Max slam charges. Defaults to 3.</summary>
		public int GuardianSlamMax = 3;
		/// <summary> The ratio of guards and slams out of the player's maximum that can naturally be regenerated before the regeneration penalty takes effect. Default is 0.5.</summary>
		public float GuardianRegenThreshold = 0.5f;
		public int GuardianBonusRune = 0; // Bonus projectiles spawned by runes
		/// <summary> Bonus in frames added to the length of parry iframes. Defaults to 0.</summary>
		public int ParryInvincibilityBonus = 0;
		public float GuardianRuneTimer = 1f; // Rune duration multiplier
		public float GuardianStandardTimer = 1f; // Standard duration multiplier
		public float GuardianStandardRange = 1f;
		/// <summary> Slam Distance multiplier. Defaults to 1f.</summary>
		public float GuardianSlamDistance = 1f;
		/// <summary> Block Duration multiplier (pavises). Defaults to 1f.</summary>
		public float GuardianBlockDuration = 1f;
		/// <summary> Parry Duration multiplier (gauntlets, misc). Defaults to 1f.</summary>
		public float GuardianParryDuration = 1f;
		/// <summary> Deflect Stun Duration multiplier (greathammers). Defaults to 1f.</summary>
		public float GuardianDeflectStunDuration = 1f;
		public float GuardianMeleeSpeed = 1f; // Edited via reforges. It multiplies the player MeleeSpeed in postupdate
		public float GuardianWeaponScale = 1f; // Multiplies pavise scale

		// Set effects, accessories, misc

		public bool GuardianMeteorite = false; // Armor Sets
		public bool GuardianBamboo = false;
		public bool GuardianGit = false;
		public bool GuardianHorizon = false;
		public bool GuardianCrystalNinja = false;
		public float GuardianSpikeDamage = 0; // Accessories
		public bool GuardianSpikeDungeon = false;
		public bool GuardianSpikeMech = false;
		public bool GuardianSpikeTemple = false;
		public bool GuardianSharpRebuttalBlock = false;
		public bool GuardianSharpRebuttalParry = false;
		public bool GuardianWormTooth = false;
		public bool GuardianMonsterFang = false;
		public bool GuardianBadgeHoplite = false;
		public bool GuardianStandardDesert = false; // Standards
		public int GuardianStandardStarScouter = -1; //Points to current StarScouterStandard holder
		public bool GuardianStandardStarScouterWarp = false;
		public int GuardianStandardStarScouterWarpCD = 0; //Holds cooldown and animation for warp effect
		public bool GuardianHoneyPotion = false; // Misc
		public bool GuardianInfiniteResources = false;
		public int GuardianDebugVisuals = 0;
		public int OldGuardianDebugVisuals = 0;
		public byte GuardianJewelerGauntlet = 0;
		public bool GuardianBronzeShieldBuff = false;
		public float GuardianBronzeShieldDamage = 0;
		public bool GuardianBronzeShieldProtection = false;
		public float GuardianChain = 0f; // Increases the swing range on Warhammers (additive, 16f = 1 tile)
		public string GuardianChainTexture = null; // Used to draw the warhammer chain
		public int GuardianStaffRocket = 0; // If > 0, the player can dash by spending slams with a quarterstaff (1,2,3,4 = red,green,blue,yellow)
		/// <summary> Throw warhammers will bend in the direction of the cursor when thrown if this is >0f. For reference, the basic Hammer magnet sets this value to 50f. Defaults to 0f.</summary>
		public float GuardianHammerMagnet = 0f;
		public bool GuardianHammerDetonator = false;
		/// <summary> Multiplier applied to the velocity of all warhammer throws. Defaults to 1f.</summary>
		public float GuardianHammerThrowVelocity = 1f;
		public bool GuardianThoriumCenser = false; // Life Quartz Censer
		public bool GuardianArmMotor = false; // Greathammer Arm Motor

		// Debug bonus resources that do not get updated (for use with DragonLens)
		
		public int GuardianDebugBonusGuards = 0;
		public int GuardianDebugBonusSlams = 0;
		public int GuardianDebugBonusRunes = 0;

		// Dynamic gameplay and UI fields

		public GuardianStandardStats GuardianStandardStats = new GuardianStandardStats(); // Used to receive stats from standards
		public int GuardianGuard = 0; // Current Guard stacks
		public int GuardianSlam = 0; // Current Slam Stacks
		/// <summary> Current timer for guard stack regen or degen. Increments guards at 1 or higher, decrements at -1 or lower.</summary>
		public float GuardianGuardRecharging = 0;
		/// <summary> Current timer for slam stack regen or degen. Increments slams at 1 or higher, decrements at -1 or lower.</summary>
		public float GuardianSlamRecharging = 0;
		public bool OverThresholdGuards => GuardianGuard + GuardianGuardRecharging > GuardianGuardMax * GuardianRegenThreshold;
		public bool OverThresholdSlams => GuardianSlam + GuardianSlamRecharging > GuardianSlamMax * GuardianRegenThreshold;
		public int GuardianDisplayUI = 0; // Guardian UI is displayed if > 0
		public float GuardianItemCharge = 0f; // Player Warhammer Throw Charge, max is 180f
		/// <summary> Player is currently parrying.</summary>
		public bool GuardianParry = false; // Player is currently parrying
		public bool GuardianParryBuffer = false; // Player is currently parrying (1 frame buffer)
		/// <summary> Cooldown in frames between starting a new punch charge since starting the last one. Can begin a punch when 0 or lower, goes down to -10. Half of the gauntlet's punch animation time is added when a charge is started. </summary>
		public int GauntletPunchCooldown = 0;
		public bool GuardianStandardBuffer = false; // used to delay the deactivation of various standards effects by 1 frame
		/// <summary> Displays an outline around guards in the UI if > 0.</summary>
		public int SlamCostUI = 0; // 
		/// <summary> Displays an outline around slams in the UI if > 0.</summary>
		public int GuardCostUI = 0; // Displays an outline around slams in the UI if > 0
		/// <summary>Causes this number amount of guards to flash red on the UI.</summary>
		public int MissingGuardsUI = 0;
		/// <summary>Causes this number amount of slams to flash red on the UI.</summary>
		public int MissingSlamsUI = 0;
		/// <summary>How long until MissingGuardsUI and MissingSlamsUI is cleared.</summary>
		public int MissingResourcesTimer = 0;
		public int ChargeHoldTimer; // Timer (in frames) since GuardianItemCharge has been >0 
		/// <summary> Allows the player to trigger counterattack effects. Set when able to use an item that has counterattack effects. Use GuardianCounterTime to check for counterattack eligibility. </summary>
		public bool GuardianCounter;
		/// <summary> Timer for performing a counterattack if GuardianCounter is true. Set to 60 after guarding with a gauntlet, or to the block or counter duration after guarding with a pavise or quarterstaff. Will always be 0 if GuardianCounter is false. </summary>
		public int GuardianCounterTime = 0;
		/// <summary> Makes the GuardianCounter indicator flash Horizon colors instead of red. </summary>
		public bool GuardianCounterHorizon;
		public int GuardianShieldSpikeReflect = 5; // Shgield spikes only fire a projectile if this is >0
		public List<BlockedEnemy> GuardianBlockedEnemies = new List<BlockedEnemy>();
		/// <summary>Enemies deflected by the player's last <c>DoDeflect</c>. Cleared and repopulated when <c>DoDeflect</c> is called.</summary>
		public List<NPC> DeflectedEnemies = new List<NPC>();
		public List<Projectile> RuneProjectiles = new List<Projectile>();
		public Projectile GuardianCurrentStandardAnchor;
		public float GauntletSlamPool = 0f; // How much slam charge will be granted by hitting the next punch
		public int GuardianStaffRocketCooldown = 0; // Cooldown between rocket dashes
		public int GuardianBadgeHopliteLevel = 0; // goes up to 2 for bonus katar charge speed
		public int GuardianArmMotorCount = 0; // set to 2 after a charged attack hit, greathammers do additional slams if this is >1.

		public const int GuardianRechargeTime = 600;

		public int GetGuardianDamage(float damage) => (int)(Player.GetDamage<GuardianDamageClass>().ApplyTo(damage) + Player.GetDamage(DamageClass.Generic).ApplyTo(damage) - damage);
		public int GetGuardianCrit(int addedCrit = 0) => (int)(Player.GetCritChance<GuardianDamageClass>() + Player.GetCritChance<GenericDamageClass>() + addedCrit);

		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
			Item item = Player.HeldItem;
			if (item.ModItem is OrchidModGuardianGauntlet gauntlet)
			{
				drawInfo.compBackArmFrame = new Rectangle(1, 1, 1, 1); // Makes the back arm disappear when holding a gauntlet

				if (gauntlet.hasArm)
				{ // Makes the front arm disappear if the gauntlet has its own arm texture.
					drawInfo.compFrontArmFrame = new Rectangle(1, 1, 1 ,1); // An empty rectangle crashes the game
				}

				if (gauntlet.hasShoulder)
				{
					drawInfo.hideCompositeShoulders = true; // Makes the shoulders disappear if the gauntlet has its own shoulder texture
				}
			}

			if (item.ModItem is OrchidModGuardianKatar)
			{
				drawInfo.compBackArmFrame = new Rectangle(1, 1, 1, 1); // Makes the back arm disappear when holding a katar
			}

			if (item.ModItem is OrchidModGuardianItem && Player.compositeFrontArm.enabled)
			{
				drawInfo.compShoulderOverFrontArm = true; // Why is this not on by default
				drawInfo.hideCompositeShoulders = false;

				if (Player.bodyFrame == new Rectangle(0, 56 * 5, 40, 56))
				{
					if (Player.Male)
					{
						drawInfo.compTorsoFrame = new(0, 0, 40, 56); //Jumping
					}
					else
					{
						drawInfo.compTorsoFrame = new(0, 112, 40, 56); //Jumping
					}
				}
			}
		}
		
		public Mod CheatSheet;
		public Mod HerosMod;
		public Mod DragonLens;
		public FieldInfo CSGodMode;
		public FieldInfo HMGodMode;
		public FieldInfo DLGodMode;
		
		public override void SetStaticDefaults()
		{
			ProjectilesBlockBlacklist = new List<int>
			{
				ProjectileID.PhantasmalDeathray,
				ProjectileID.FairyQueenSunDance,
				ProjectileID.SandBallFalling,
				ProjectileID.SiltBall,
				ProjectileID.SlushBall,
				ProjectileID.EbonsandBallFalling,
				ProjectileID.CrimsandBallFalling,
				ProjectileID.PearlSandBallFalling,
				ProjectileID.AshBallFalling
			};

			Mod thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				ProjectilesBlockBlacklist.Add(thoriumMod.Find<ModProjectile>("OctopusArm").Type);
				ProjectilesBlockBlacklist.Add(thoriumMod.Find<ModProjectile>("GraniteEradicatorArm").Type);
				ProjectilesBlockBlacklist.Add(thoriumMod.Find<ModProjectile>("KrakenArm").Type);
			}
			
			if (ModLoader.TryGetMod("CheatSheet", out Mod cheatSheet))
			{
				CheatSheet = cheatSheet;
				// var godModeService = CheatSheet.Code.GetType("CheatSheet.Menus.GodMode");
				// if (godModeService != null) CSGodMode ??= godModeService.GetField("Enabled", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				CSGodMode = CheatSheet.Code.GetType("CheatSheet.Menus.GodMode")?.GetField("Enabled", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (CSGodMode != null)
				{
					Console.Out.WriteLine("OrchidGuardian: CheatSheet detected");
					Mod.Logger.Debug("OrchidGuardian: CheatSheet detected");
				}
			}
			if (ModLoader.TryGetMod("HEROsMod", out Mod herosMod))
			{
				HerosMod = herosMod;
				// var godModeService = HerosMod.Code.GetType("HEROsMod.HEROsModServices.GodModeService");
				// if (godModeService != null) HMGodMode ??= godModeService.GetField("Enabled", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				HMGodMode = HerosMod.Code.GetType("HEROsMod.HEROsModServices.GodModeService")?.GetField("Enabled", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (HMGodMode != null)
				{
					Console.Out.WriteLine("OrchidGuardian: HERO's Mod detected");
					Mod.Logger.Debug("OrchidGuardian: HERO's Mod detected");
				}

			}
			if (ModLoader.TryGetMod("DragonLens", out Mod dragonLens))
			{
				DragonLens = dragonLens;
				// var godModeService = DragonLens.Code.GetType("DragonLens.Content.Tools.Gameplay.Godmode");
				// if (godModeService != null) DLGodMode ??= godModeService.GetField("godMode", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				DLGodMode = DragonLens.Code.GetType("DragonLens.Content.Tools.Gameplay.Godmode")?.GetField("godMode", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (DLGodMode != null)
				{
					Console.Out.WriteLine("OrchidGuardian: DragonLens detected");
					Mod.Logger.Debug("OrchidGuardian: DragonLens detected");
				}
			}
		}


		public override void Unload()
		{
			CheatSheet = null;
			HerosMod = null;
			DragonLens = null;
			CSGodMode = null;
			HMGodMode = null;
			DLGodMode = null;
		}

		public override void Initialize()
		{
			modPlayer = Player.GetModPlayer<OrchidPlayer>();
		}

		public override void OnRespawn()
		{
			GuardianGuard = GuardianGuardMax;
			GuardianSlam = 1;
		}

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
		{
			GuardianItemCharge = 0;
		}

		public override void PostUpdate()
		{
			// This should multiply the player melee speed AFTER all other modifications, making reforge melee speed multiplicative instead of additive
			Player.GetAttackSpeed(DamageClass.Melee) *= GuardianMeleeSpeed;
			Player.ApplyMeleeScale(ref GuardianWeaponScale); // melee scale affects guardian weapon scale
		}

		public override void PostUpdateMiscEffects()
		{
			GuardianStandardStats.ApplyStats(Player); // Standards apply their stats here

			if (GuardianJewelerGauntlet != 0)
			{ // Player.HoldItem being called too late in the frame, these fields need to be buffered and handled here
				if (GuardianJewelerGauntlet == (byte)JewelerGauntletGem.EMERALD) modPlayer.OrchidDoubleDash = true;
				if (GuardianJewelerGauntlet == (byte)JewelerGauntletGem.SAPPHIRE) Player.GetJumpState(ExtraJump.CloudInABottle).Enable();
				if (GuardianJewelerGauntlet == (byte)JewelerGauntletGem.AQUAMARINE) Player.trident = true;
				if (GuardianJewelerGauntlet == (byte)JewelerGauntletGem.TOPAZ) ParryInvincibilityBonus += 40;
				GuardianJewelerGauntlet = 0;
			}
			
			if (GuardianParry) {
				if (GuardianCrystalNinja && Player.dashDelay < 0) DoParryItemParry(null);

				// Condition for when the player is in God Mode (intangible otherwise)
				if (Player.creativeGodMode || CrossModGodMode) {
				
					Entity entity = (Entity)Main.projectile?.FirstOrDefault(proj => proj.active && proj.hostile && proj.damage > 0 && Collision.CheckAABBvAABBCollision(Player.Center, Player.Hitbox.Size(), proj.Center, proj.Hitbox.Size()), null)
					?? (Entity)Main.npc?.FirstOrDefault(npc => npc.active && (!npc.friendly && npc.damage > 0) && Collision.CheckAABBvAABBCollision(Player.Center, Player.Hitbox.Size(), npc.Center, npc.Hitbox.Size()));

					if (entity != null) DoParryItemParry(entity);
				}

				// Condition for parrying an Aggro Dummy or Boss Dummy from Thorium 
				// (only if the player is either in godmode, or if there aren't any enemies/bosses within 45 tiles)
				if (OrchidMod.ThoriumMod != null && Main.npc.Any(npc => {
					int aggroDummy = OrchidMod.ThoriumMod.Find<ModNPC>("AggroDummy").Type;
					int bossDummy = OrchidMod.ThoriumMod.Find<ModNPC>("BossDummy").Type;
					if ((npc.type == aggroDummy || npc.type == bossDummy) && Collision.CheckAABBvAABBCollision(Player.Center, Player.Hitbox.Size(), npc.Center, npc.Hitbox.Size())) 
					{
						if (Player.creativeGodMode || CrossModGodMode) return true;
						else return !Main.npc.Any(n => n.active && (!n.friendly || n.boss) && !(npc.type == aggroDummy || npc.type == bossDummy) && n.Center.DistanceSQ(Player.Center) <= 1638400f);
					}
					return false;
				})
				) DoParryItemParry(null);
			}

			if (GauntletPunchCooldown > -10) GauntletPunchCooldown--;
		}

		public override void UpdateLifeRegen()
		{
			GuardianStandardStats.ApplyLifeRegen(Player); // Standards apply their stats here
		}

		public override void ResetEffects()
		{
		
			bool csGodMode = CheatSheet != null && CSGodMode != null && CSGodMode?.GetValue(null) is true;
			bool hmGodMode = HerosMod != null && HMGodMode != null && HMGodMode?.GetValue(null) is true;
			bool dlGodMode = DragonLens != null && DLGodMode != null && DLGodMode?.GetValue(null) is true;
			CrossModGodMode = csGodMode || hmGodMode || dlGodMode;

			// Resetting Core guardian fields
			if (Player.itemTime > 0 && Player.HeldItem.damage > 0 && Player.HeldItem.ModItem is not OrchidModGuardianItem && Player.HeldItem.pick + Player.HeldItem.hammer + Player.HeldItem.axe == 0)
				GuardianRegenThreshold = 0;
			else if (GuardianRegenThreshold > 1) GuardianRegenThreshold = 1;

			if (!OverThresholdGuards)
				GuardianGuardRecharging += GuardianGuardRecharge / GuardianRechargeTime;
			else GuardianGuardRecharging += (-2 + GuardianGuardRecharge) / GuardianRechargeTime;
			
			if (GuardianGuard == GuardianGuardMax && GuardianGuardRecharging > 0) GuardianGuardRecharging = 0;

			if (GuardianGuardRecharging >= 1f)
			{
				GuardianGuard++;
				GuardianGuardRecharging--;
			}
			else if (GuardianGuardRecharging <= -1f)
			{
				GuardianGuard--;
				GuardianGuardRecharging++;
			}

			if (!OverThresholdSlams)
				GuardianSlamRecharging += GuardianSlamRecharge / GuardianRechargeTime;
			else GuardianSlamRecharging += (-2 + GuardianSlamRecharge) / GuardianRechargeTime;
			
			if (GuardianSlam == GuardianSlamMax && GuardianSlamRecharging > 0) GuardianSlamRecharging = 0;

			if (GuardianSlamRecharging >= 1f)
			{
				GuardianSlam++;
				GuardianSlamRecharging--;
			}
			else if (GuardianSlamRecharging <= -1f)
			{
				GuardianSlam--;
				GuardianSlamRecharging++;
			}

			if (GuardianStaffRocketCooldown > 0)
			{
				GuardianStaffRocketCooldown--;
			}

			if (GuardianItemCharge > 0)
			{
				ChargeHoldTimer++;
			}
			else
			{
				ChargeHoldTimer = 0;
			}

			if (Player.HeldItem.ModItem is OrchidModGuardianGauntlet)
			{
				GauntletSlamPool += 0.0017f; // charges in 5 seconds

				if (GauntletSlamPool > 0.5f)
				{
					GauntletSlamPool = 0.5f;
				}
			}
			else
			{
				GauntletSlamPool = 0f;
			}

			if (Player.HeldItem.ModItem is not OrchidModGuardianKatar)
			{
				GuardianBadgeHopliteLevel = 0;
			}

			if (GuardianCounter)
			{
				if (GuardianCounterTime > 0) GuardianCounterTime--;
				GuardianCounter = false;
				GuardianCounterHorizon = false;
			}
			else GuardianCounterTime = 0;

			if (MissingResourcesTimer > 0)
			{
				MissingResourcesTimer--;
				if (MissingResourcesTimer <= 0)
				{
					MissingGuardsUI = 0;
					MissingSlamsUI = 0;
				}
			}

			GuardianDisplayUI--;

			for (int i = GuardianBlockedEnemies.Count - 1; i >= 0; i--)
			{
				BlockedEnemy blockedEnemy = GuardianBlockedEnemies[i];
				blockedEnemy.time--;
				if (blockedEnemy.time < 0)
				{
					GuardianBlockedEnemies.Remove(blockedEnemy);
				}
			}

			if (modPlayer.Timer % 30 == 0 && GuardianShieldSpikeReflect < 5)
			{
				GuardianShieldSpikeReflect++;
			}

			if (Player.HeldItem.ModItem is not OrchidModGuardianItem) GuardianItemCharge = 0f;

			if (GuardianParryBuffer) GuardianParryBuffer = false;
			else GuardianParry = false;

			SlamCostUI = 0;
			GuardCostUI = 0;

			if (GuardianGuard > GuardianGuardMax) GuardianGuard = GuardianGuardMax;
			if (GuardianSlam > GuardianSlamMax) GuardianSlam = GuardianSlamMax;

			RuneProjectiles.Clear();

			// Resetting standards effects

			ResetStandards();

			// Resetting equipment variables

			GuardianGuardRecharge = 1f;
			GuardianSlamRecharge = 1f;
			GuardianGuardMax = 3;
			GuardianSlamMax = 3;
			GuardianRegenThreshold = 0.5f;
			GuardianBonusRune = 0;
			GuardianRuneTimer = 1f;
			GuardianStandardTimer = 1f;
			GuardianStandardRange = 1f;
			GuardianSlamDistance = 1f;
			GuardianBlockDuration = 1f;
			GuardianParryDuration = 1f;
			GuardianDeflectStunDuration = 1f;
			GuardianMeleeSpeed = 1f;
			GuardianWeaponScale = 1f;
			ParryInvincibilityBonus = 0;
			GuardianChain = 0f;
			GuardianStaffRocket = 0;
			GuardianChainTexture = null;
			GuardianHammerMagnet = 0f;
			GuardianHammerThrowVelocity = 1f;
			GuardianThoriumCenser = false;
			GuardianArmMotor = false;
			if (!GuardianBronzeShieldBuff) GuardianBronzeShieldDamage = 0;

			GuardianMeteorite = false;
			GuardianSpikeDamage = 0;
			GuardianSpikeDungeon = false;
			GuardianSpikeMech = false;
			GuardianSpikeTemple = false;
			GuardianSharpRebuttalBlock = false;
			GuardianSharpRebuttalParry = false;
			GuardianBamboo = false;
			GuardianGit = false;
			GuardianHorizon = false;
			GuardianCrystalNinja = false;
			GuardianHoneyPotion = false;
			GuardianWormTooth = false;
			GuardianMonsterFang = false;
			// GuardianInfiniteResources = false;
			GuardianInfiniteResources = (Player.creativeGodMode || CrossModGodMode);
			if (OldGuardianDebugVisuals != GuardianDebugVisuals)
			{
				if (Main.myPlayer == Player.whoAmI)
				{
					/*
					if (GuardianDebugVisuals == 1) CombatText.NewText(Player.Hitbox, new Color(1f, 0f, 1f), "Enabled shield and deflect check", true);
					if (GuardianDebugVisuals == 1) CombatText.NewText(Player.Hitbox, new Color(1f, 0f, 1f), "Enabled attackinfo display", true);
					if (GuardianDebugVisuals == 1) CombatText.NewText(Player.Hitbox, new Color(1f, 0f, 1f), "Enabled greathammer hand placement visual", true);
					*/
					if (GuardianDebugVisuals == 1)
					{
						Main.NewText("Enabled shield and deflect check", 175, 255, 175);
						Main.NewText("Enabled attackinfo display", 175, 255, 175);
						Main.NewText("Enabled greathammer hand placement visual", 175, 255, 175);
						Main.NewText("Hide the Guardian Test accessory to disable test visuals", 255, 128, 255);
					}
					else
					{
						Main.NewText("Disabled guardian test visuals", 255, 92, 92);
					}

				}
				OldGuardianDebugVisuals = GuardianDebugVisuals;
			}
			GuardianDebugVisuals = 0;
			GuardianBronzeShieldBuff = false;
			GuardianBronzeShieldProtection = false;
			GuardianBadgeHoplite = false;
			GuardianHammerDetonator = false;

			GuardianGuardMax += GuardianDebugBonusGuards;
			GuardianSlamMax += GuardianDebugBonusSlams;
			GuardianBonusRune += GuardianDebugBonusRunes;
		}

		public override void PreUpdateMovement()
		{
			if (GuardianStandardStarScouterWarp)
			{
				OrchidPlayer input = Player.GetModPlayer<OrchidPlayer>();
				if (GuardianStandardStarScouterWarpCD == 0)
				{
					if (input.DoubleTapAny && UseGuard())
					{
						GuardianStandardStarScouterWarpCD = 30;
						SoundEngine.PlaySound(SoundID.Item162.WithVolumeScale(0.5f).WithPitchOffset(1.5f), Player.position);
						Player.immune = true;
						Player.immuneTime = 60;
						Player.oldVelocity = Player.velocity;
					}
				}
				else GuardianStandardStarScouterWarpCD--;
				if (GuardianStandardStarScouterWarpCD > 20)
				{
					Player.velocity = Player.oldVelocity * (-2 + GuardianStandardStarScouterWarpCD * 0.1f);
					for (int i = 0; i < 4; i++)
					{
						Dust dust = Dust.NewDustDirect(Player.Center - new Vector2(4, 4), 0, 0, DustID.ShadowbeamStaff);
						dust.velocity.X *= 0.6f;
						dust.position -= dust.velocity * 20f;
						dust.velocity = dust.velocity.RotatedBy(Player.direction * 0.3f) * 6f;
					}
				}
				else if (GuardianStandardStarScouterWarpCD == 20)
				{
					Vector2 warp = Vector2.Zero;
					if (input.DoubleTappedUp > 0 || Player.controlUp) warp.Y -= 1;
					if (input.DoubleTappedDown > 0 || Player.controlDown) warp.Y += 1;
					if (input.DoubleTappedRight > 0 || Player.controlRight) warp.X += 1;
					if (input.DoubleTappedLeft > 0 || Player.controlLeft) warp.X -= 1;

					/*
					if (GuardianCurrentStandardAnchor != null)
					{
						if (GuardianCurrentStandardAnchor.ModProjectile is GuardianStandardAnchor standardAnchor)
						{
							standardAnchor.UpdateAndSyncValue(warp.ToRotation());
						}
					}
					*/

					if (warp != Vector2.Zero)
					{
						warp.Normalize();
						int clipPrevention = 3 + (warp.X != 0 ? 7 : 0) + (warp.Y != 0 ? 20 : 0);
						int dist = clipPrevention;
						for (int i = 160; i >= 1; i /= 2)
						{
							if (Collision.CanHit(Player.Center, 0, 0, Player.Center + warp * (dist + i), 0, 0))
								dist += i;
						}
						Player.velocity = warp * 8f;
						warp *= dist - clipPrevention;
						dist = (dist - clipPrevention) / 4;
						Player.position += warp;
						Main.SetCameraLerp(0.1f, 10);
						SoundEngine.PlaySound(SoundID.Item163.WithVolumeScale(0.3f).WithPitchOffset(1), Player.position);
						for (int i = 0; i < dist; i++)
						{
							float currPos = i * 1f / dist;
							Dust dust = Dust.NewDustDirect(Player.position - warp * currPos - new Vector2(4, 4), Player.width, Player.height, DustID.ShadowbeamStaff);
							dust.noGravity = true;
							dust.scale *= 2.5f - 1 * currPos;
							dust.velocity += Player.velocity * 2;
						}
					}

					AddSlam();
				}
				else if (GuardianStandardStarScouterWarpCD > 0)
				{
					Dust dust = Dust.NewDustDirect(Player.Center - new Vector2(4, 4), 0, 0, DustID.ShadowbeamStaff);
					dust.velocity.X *= 0.6f;
					dust.position += dust.velocity * 6f;
					dust.velocity *= 3f;
				}
			}

			if (Player.HeldItem != null)
			{ // Ucaps players Y velocity while dashing downwards with a katar
				if (Player.HeldItem.ModItem != null)
				{
					if (Player.HeldItem.ModItem is OrchidModGuardianKatar katar && katar.GetAnchors(Player) != null)
					{
						if (Main.projectile[katar.GetAnchors(Player)[1]].ModProjectile is GuardianKatarAnchor anchor && anchor.KatarDashTimer > 1)
						{
							modPlayer.ForcedVelocityIgnoresPlatforms = true;
							modPlayer.ForcedVelocityTimer = 2;
							modPlayer.ForcedVelocityUpkeep = katar.ParryDashMomentum;
							modPlayer.ForcedVelocityVector = Vector2.UnitY.RotatedBy(anchor.KatarDashAngle) * -katar.ParryDashSpeed;
						}
					}
				}
			}
		}

		public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
		{
			if (GuardianStandardStarScouter >= 0)
			{
				if (Player.whoAmI == Main.myPlayer)
				{
					Player standardHolder = Main.player[GuardianStandardStarScouter];
					if (standardHolder.active && standardHolder.GetModPlayer<OrchidGuardian>().GuardianStandardStarScouter != -1)
					{
						Vector2 offsFromGuardStandard = standardHolder.Center + new Vector2(-14 * standardHolder.direction * (standardHolder.HeldItem.ModItem is OrchidModGuardianStandard ? -1 : 1), -28 * standardHolder.gravDir) - npc.Center;
						SoundEngine.PlaySound(SoundID.Item91.WithVolumeScale(0.5f), npc.position);
						Player.ApplyDamageToNPC(npc, (int)(npc.damage * 0.33f), 10, offsFromGuardStandard.X < 0 ? 1 : -1, crit: false);
						int dist = (int)offsFromGuardStandard.Length() / 4;
						if (dist < 100)
							for (int i = 0; i < dist; i++)
							{
								float currPos = i * 1f / dist;
								Dust dust = Dust.NewDustDirect(npc.Center + offsFromGuardStandard * currPos - new Vector2(4, 4), 0, 0, DustID.ShadowbeamStaff);
								dust.noGravity = true;
							}
					}
					else GuardianStandardStarScouter = -1; //reset in the event of a stale reference
				}
			}
		}

		public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
		{
			foreach (BlockedEnemy blockedEnemy in GuardianBlockedEnemies)
			{
				if (blockedEnemy.npc.whoAmI == npc.whoAmI && !GuardianParry)
				{
					return false;
				}
			}
			if (GuardianBronzeShieldProtection)
			{
				foreach (Player ally in Main.player)
				{
					if (ally.GetModPlayer<OrchidGuardian>().GuardianBronzeShieldBuff)
					{
						foreach (BlockedEnemy blockedEnemy in ally.GetModPlayer<OrchidGuardian>().GuardianBlockedEnemies)
						{
							if (blockedEnemy.npc.whoAmI == npc.whoAmI) return false;
						}
					}
				}
			}

			return true;
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (proj.ModProjectile is OrchidModGuardianProjectile orchidProj)
			{
				if (GuardianCurrentStandardAnchor != null)
				{
					if (GuardianStandardDesert && GuardianCurrentStandardAnchor.ModProjectile is GuardianStandardAnchor standardAnchor && orchidProj.FirstHit && orchidProj.Strong)
					{
						int type = ModContent.ProjectileType<DesertStandardProj>();
						float range = (standardAnchor.BuffItem.ModItem as OrchidModGuardianStandard).AuraRange * GuardianStandardRange + target.width * 0.5f;
						if (proj.type != type && target.Center.Distance(Player.Center) < range)
						{
							float damage = damageDone;
							if (hit.Crit) damage *= 0.5f;
							Projectile projectile = Projectile.NewProjectileDirect(Player.GetSource_FromThis(), target.Center, Vector2.Zero, type, 0, 1f, Player.whoAmI, target.whoAmI, damage);
							projectile.CritChance = GetGuardianCrit();
						}
					}
				}

				//..
			}
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			if (GuardianParry)
			{
				modifiers.DamageSource.TryGetCausingEntity(out Entity entity);
				DoParryItemParry(entity);
				modifiers.Cancel();
			}
		}

		// Below are custom Guardian Methods

		public void OnUseSlam()
		{
			if (GuardianThoriumCenser && OrchidMod.ThoriumMod != null)
			{
				Player lowestHealthPlayer = Player;
				foreach (Player player in Main.player)
				{ // targets the lowest heath nearby player
					if (player.DistanceSQ(Player.Center) < 800f && player.active && !player.dead && player.statLife < lowestHealthPlayer.statLife)
					{ // 16 * 50 = 800f for a 50 tiles range
						lowestHealthPlayer = player;
					}
				}

				/* Ended up being unused, but this is how I would check for the players shield health, potentially to target unshielded players.
				foreach (ModPlayer thoriumPlayer in Player.ModPlayers)
				{
					if (thoriumPlayer.Name == "ThoriumPlayer" && thoriumPlayer.Mod == OrchidMod.ThoriumMod)
					{
						FieldInfo field = thoriumPlayer.GetType().GetField("shieldHealth", BindingFlags.Public | BindingFlags.Instance);
						int shieldHealth = (int)field.GetValue(thoriumPlayer);
						break;
					}
				}
				*/

				// This is how the War Forger applies its shield, where 5f is the shield amount, and 10f is the maximum shield amount that can be applied
				int projectileType = OrchidMod.ThoriumMod.Find<ModProjectile>("HealerShield").Type;
				Projectile.NewProjectile(Player.GetSource_FromThis(), lowestHealthPlayer.Center, Vector2.Zero, projectileType, 0, 0.0f, Player.whoAmI, 5f, 10f, 0.0f);
			}
			if (MissingResourcesTimer > 0)
			{
				MissingResourcesTimer = 0;
				MissingGuardsUI = 0;
				MissingSlamsUI = 0;
			}
		}

		public void OnUseGuard()
		{
			if (MissingResourcesTimer > 0)
			{
				MissingResourcesTimer = 0;
				MissingGuardsUI = 0;
				MissingSlamsUI = 0;
			}
		}

		public void AddSlam(int nb = 1)
		{
			if (nb > 0 && GuardianSlamRecharging < 0) GuardianSlamRecharging = 0;
			if (GuardianSlam + nb > GuardianSlamMax) nb = GuardianSlamMax - GuardianSlam;
			if (nb > 0)
			{
				Rectangle rect = Player.Hitbox;
				rect.Y -= 64;
				CombatText.NewText(rect, Color.LightCyan, "+" + Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.Slam", nb), false, true);
				GuardianSlam += nb;
			}
		}

		public void AddGuard(int nb = 1)
		{
			if (nb > 0 && GuardianGuardRecharging < 0) GuardianGuardRecharging = 0;
			if (GuardianGuard + nb > GuardianGuardMax) nb = GuardianGuardMax - GuardianGuard;
			if (nb > 0)
			{
				Rectangle rect = Player.Hitbox;
				rect.Y -= 64;
				CombatText.NewText(rect, Color.LightSkyBlue, "+" + Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.Guard", nb), false, true);
				GuardianGuard += nb;
			}
		}

		public bool UseSlam(int nb = 1, bool checkOnly = false, bool showUICost = false, bool warn = false)
		{
			if (nb <= 0) return true;

			if (showUICost)
			{
				SlamCostUI = nb;
			}

			if (GuardianHorizon && Player.statLife > Player.statLifeMax2 * 0.5f && Player.statLife > 20)
			{ // Horizon armor set consumes health instead of guardian charges
				if (!checkOnly)
				{
					Player.statLife -= 20;
					CombatText.NewText(Player.Hitbox, CombatText.DamagedFriendly, 20, false, true);
					SoundEngine.PlaySound(SoundID.DD2_DarkMageAttack, Player.Center);
					
					while (nb > 0)
					{
						nb--;
						OnUseSlam();
					}
				}
				return true;
			}

			if (GuardianInfiniteResources && GuardianSlam < nb)
			{
				if (!checkOnly)
				{
					int nb2 = nb - GuardianSlam;
					Rectangle rect = Player.Hitbox;
					rect.Y -= 64;
					CombatText.NewText(rect, new Color(1f, 0f, 1f), "+" + Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.Slam", nb2), false, true);
					GuardianSlam += nb2;
					GuardianSlamRecharging = 0;
					SoundEngine.PlaySound(SoundID.Item56, Player.Center);
				}
				else return true;
			}

			if (GuardianSlam >= nb)
			{
				if (!checkOnly)
				{
					GuardianSlam -= nb;
					if (GuardianBamboo) Player.AddBuff(ModContent.BuffType<BambooBuff>(), 300);
					if (GuardianSlamRecharging < 0) GuardianSlamRecharging = 0;

					while (nb > 0)
					{
						nb--;
						OnUseSlam();
					}
				}
				return true;
			}
			else if (warn)
			{
				WarnMissingResources(0, nb, playSound: false);
			}

			return false;
		}

		public bool UseGuard(int nb = 1, bool checkOnly = false, bool showUICost = false, bool warn = false)
		{
			if (nb <= 0) return true;

			if (showUICost)
			{
				GuardCostUI = nb;
			}

			if (GuardianHorizon && Player.statLife > Player.statLifeMax2 * 0.5f && Player.statLife > 20)
			{ // Horizon armor set consumes health instead of guardian charges
				if (!checkOnly)
				{
					Player.statLife -= 20;
					CombatText.NewText(Player.Hitbox, CombatText.DamagedFriendly, 20, false, true);
					SoundEngine.PlaySound(SoundID.DD2_DarkMageAttack, Player.Center);
					
					while (nb > 0)
					{
						nb--;
						OnUseGuard();
					}
				}
				return true;
			}
			
			if (GuardianInfiniteResources && GuardianGuard < nb)
			{
				if (!checkOnly)
				{
					int nb2 = nb - GuardianGuard;
					Rectangle rect = Player.Hitbox;
					rect.Y -= 64;
					CombatText.NewText(rect, new Color(1f, 0f, 1f), "+" + Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.Guard", nb2), false, true);
					GuardianGuard += nb2;
					GuardianGuardRecharging = 0;
					SoundEngine.PlaySound(SoundID.Item56, Player.Center);
				}
				else return true;
			}

			if (GuardianGuard >= nb)
			{
				if (!checkOnly)
				{
					GuardianGuard -= nb;
					if (GuardianBamboo) Player.AddBuff(ModContent.BuffType<BambooBuff>(), 300);
					if (GuardianGuardRecharging < 0) GuardianGuardRecharging = 0;

					while (nb > 0)
					{
						nb--;
						OnUseGuard();
					}
				}
				return true;
			}
			else if (warn)
			{
				WarnMissingResources(nb, 0, playSound: false);
			}
			return false;
		}

		public void StandardNearbyPlayerEffect(GuardianStandardStats standardStats, Player affectedPlayer, OrchidGuardian guardian, bool isLocalPlayer, bool reinforced)
		{ // Called after affectedPlayer has been affected by a standard held by guardian. isLocalPlayer is true when this is ran by the client holding the standard. Do not change stats on the affectedPlayer it won't work, use standardStats
			if (GuardianWormTooth)
			{
				standardStats.allDamage += 0.05f;
			}

			if (GuardianMonsterFang)
			{
				standardStats.lifeRegen += 2;
			}
		}

		public void StandardNearbyNPCEffect(Player player, OrchidGuardian guardian, NPC npc, bool isLocalPlayer, bool reinforced)
		{ // isLocalPlayer is true when this is ran by the client holding the standard - Should return true if the npc was affected
			// ..
		}

		/// <summary>Resets all boolean-controlled Standard effects. Called in OrchidGuardian.ResetEffects and GuardianStandardAnchor when a standard is switched. forceReset controls whether to respect OrchidGuardian.GuardianStandardBuffer to give one frame  of leeway.</summary>
		public void ResetStandards(bool forceReset = false)
		{
			if (forceReset || !GuardianStandardBuffer)
			{
				GuardianCurrentStandardAnchor = null;
				GuardianStandardDesert = false;
				GuardianStandardStarScouter = -1;
				GuardianStandardStarScouterWarp = false;
			}
			GuardianStandardBuffer = false;
		}

		public void OnBlockAnyFirst(Projectile anchor, ref int toAdd, bool parry)
		{ // Called by both FirstBlockEffect methods to do universal on-first-block effect
			if (GuardianMeteorite && Main.rand.NextBool(2))
			{
				toAdd++;
			}

			if (GuardianHoneyPotion)
			{ // Heal the player if they have the honey potion effect
				modPlayer.TryHeal((int)(Player.statLifeMax2 * 0.01f));
			}

			//set counterattack time. unsure if this is the best place to put this?
			if (GuardianCounter)
			{
				if (anchor.ModProjectile is GuardianShieldAnchor)
					GuardianCounterTime = (int)anchor.ai[0];
				else if (anchor.ModProjectile is GuardianGauntletAnchor)
					GuardianCounterTime = 60;
				else if (anchor.ModProjectile is GuardianQuarterstaffAnchor && Player.inventory[Player.selectedItem].ModItem is OrchidModGuardianQuarterstaff qs)
					GuardianCounterTime = (int)(40 / (qs.CounterSpeed * Player.GetTotalAttackSpeed<MeleeDamageClass>()));
			}
		}
		public void OnBlockNPCNew(Projectile anchor, NPC target, int toAdd = 1, bool parry = false)
		{ // Called anytime the player blocks/parries a NPC for the first time (NPC not contained in GuardianBlockedEnemies)
			if (Player.whoAmI == Main.myPlayer)
			{
				if (GuardianSpikeDamage > 0)
				{
					float damage = Player.statDefense * GuardianSpikeDamage;

					damage = GetGuardianDamage(damage);
					if (damage < 1) damage = 1;
					if (damage > 500) damage = 500;
					bool crit = Main.rand.NextFloat(100) < anchor.CritChance;
					Player.ApplyDamageToNPC(target, (int)damage, 0f, Player.direction, crit, ModContent.GetInstance<GuardianDamageClass>());
				}
			}
		}

		public void OnBlockNPCFirst(Projectile anchor, NPC target, int toAdd = 1, bool parry = false)
		{ // Called anytime the player blocks/parries their first NPC
			OnBlockAnyFirst(anchor, ref toAdd, parry);

			if (anchor.ModProjectile is not GuardianHammerAnchor)
			{
				AddSlam(toAdd);
			}
		}

		public void OnBlockProjectileFirst(Projectile anchor, Projectile blockedProjectil0e, int toAdd = 1, bool parry = false)
		{ // Called anytime the player blocks/parries their first projectile
			OnBlockAnyFirst(anchor, ref toAdd, parry);
			AddSlam(toAdd);
		}

		public void OnBlockAny(Projectile anchor, bool parry)
		{ // Called by both BlockEffect methods to do universal block effects
			if (GuardianGit)
			{
				Player.AddBuff(ModContent.BuffType<GuardianGitBuff>(), 600);
			}
			if ((GuardianSharpRebuttalBlock && !parry) || (GuardianSharpRebuttalParry && parry))
			{
				Player.AddBuff(ModContent.BuffType<GuardianSpikeBuff>(), 600);
			}
		}

		public void OnBlockNPC(Projectile anchor, NPC target, bool parry = false)
		{ // Called anytime the player blocks/parries a NPC
			OnBlockAny(anchor, parry);
		}

		public void OnBlockProjectile(Projectile anchor, Projectile blockedProjectile, bool parry = false)
		{ // Called anytime the player blocks/parries a projectile
			OnBlockAny(anchor, parry);

			if (anchor.ModProjectile is GuardianShieldAnchor shieldAnchor)
			{
				if (shieldAnchor.ShieldItem.ModItem is OrchidModGuardianShield shield)
				{
					shield.Reflect(Player, anchor, blockedProjectile, ref GuardianShieldSpikeReflect);
				}

				if (GuardianShieldSpikeReflect > 0)
				{
					GuardianShieldSpikeReflect--;

					if (GuardianSpikeDamage > 0)
					{
						int damage = Math.Max(GetGuardianDamage(Player.statDefense * GuardianSpikeDamage), 1);
						if (GuardianSpikeTemple)
						{
							int type = ModContent.ProjectileType<TempleSpikeProj>();
							Vector2 dir = Vector2.Normalize(anchor.Center - Player.Center);
							Projectile projectile = Projectile.NewProjectileDirect(anchor.GetSource_FromAI(), anchor.Center + dir * 2f, dir, type, damage, 1f, Player.whoAmI);
							projectile.CritChance = (int)(Player.GetCritChance<GuardianDamageClass>() + Player.GetCritChance<GenericDamageClass>());
							SoundEngine.PlaySound(SoundID.Item91.WithPitchOffset(0.2f).WithVolumeScale(0.6f), anchor.Center);
							SoundEngine.PlaySound(SoundID.Item68.WithPitchOffset(0.6f).WithVolumeScale(0.5f), anchor.Center);
						}
						else if (GuardianSpikeMech)
						{
							int type = ModContent.ProjectileType<MechSpikeProj>();
							Vector2 dir = Vector2.Normalize(anchor.Center - Player.Center);
							NPC target = null;
							float distanceClosest = 400f;
							foreach (NPC npc in Main.npc)
							{
								float distance = npc.Center.Distance(anchor.Center);
								if (OrchidModProjectile.IsValidTarget(npc) && distance < distanceClosest)
								{
									float targetRotOffs = (npc.Center - anchor.Center).ToRotation() - dir.ToRotation();
									if (targetRotOffs > MathHelper.Pi) targetRotOffs -= MathHelper.TwoPi;
									if (Math.Abs(targetRotOffs) < MathHelper.PiOver4)
									{
										target = npc;
										distanceClosest = distance;
									}
								}
							}
							if (target != null) dir = new Vector2(1, 0).RotatedBy((target.Center - anchor.Center).ToRotation());
							dir = dir.RotatedByRandom(0.1f);
							Projectile projectile = Projectile.NewProjectileDirect(anchor.GetSource_FromAI(), anchor.Center, dir * 12f, type, damage, 1f, Player.whoAmI);
							projectile.CritChance = (int)(Player.GetCritChance<GuardianDamageClass>() + Player.GetCritChance<GenericDamageClass>());
							projectile.rotation = dir.ToRotation() - MathHelper.PiOver2;
							SoundEngine.PlaySound(SoundID.Item12.WithVolumeScale(0.6f), anchor.Center);
						}
						else if (GuardianSpikeDungeon)
						{ // For some god forsaken reason, any projectile spawned here is destroyed on frame 1
							int type = ModContent.ProjectileType<WaterSpikeProjAlt>();
							Vector2 dir = Vector2.Normalize(anchor.Center - Player.Center) * 10f;
							Projectile projectile = Projectile.NewProjectileDirect(anchor.GetSource_FromAI(), anchor.Center, dir, type, damage, 1f, Player.whoAmI);
							projectile.CritChance = (int)(Player.GetCritChance<GuardianDamageClass>() + Player.GetCritChance<GenericDamageClass>());
							SoundEngine.PlaySound(SoundID.Item21.WithPitchOffset(0.5f).WithVolumeScale(0.7f), anchor.Center);
						}
					}
				}
			}
		}

		/// <remarks> Calls <c>OnAttack</c> with <c>AttackID.Parry</c>. Use overload to override. </remarks>
		/// <inheritdoc cref="DoParryItemParry(Entity, GuardianAttackInfo)"/>
		public void DoParryItemParry(Entity aggressor)
		{
			DoParryItemParry(aggressor, AttackID.Parry);
		}

		/// <summary> Performs parrying actions if the player is holding a parryItem, including giving the player invulnerability, triggering the parryItem's parrying related functions, and the player's responses to guarding enemies or projectiles, depending on the aggressor. </summary>
		/// <remarks> <c>attack</c> is the GuardianAttackInfo that will be supplied for <c>OnAttack</c>, and defaults to <c>AttackID.Parry</c> in simplified overload. </remarks>
		public void DoParryItemParry(Entity aggressor, GuardianAttackInfo attack)
		{
			GuardianParryBuffer = false;

			if (Player.HeldItem.ModItem is OrchidModGuardianParryItem parryItem)
			{
				int intendedImmunityLength = parryItem.InvincibilityDuration + ParryInvincibilityBonus;
				if (Player.longInvince) intendedImmunityLength += 20;
				if (parryItem is OrchidModGuardianKatar katar && Main.projectile[katar.GetAnchors(Player)[1]].ModProjectile is GuardianKatarAnchor katarAnchor && katarAnchor.KatarDashTimer > 0) intendedImmunityLength += katarAnchor.KatarDashTimer;
				modPlayer.PlayerImmunity = intendedImmunityLength;
				Player.immuneTime = intendedImmunityLength;
				Player.immune = true;

				Projectile anchor = null;

				/*
				int[] anchorTypes = [ModContent.ProjectileType<GuardianQuarterstaffAnchor>(), ModContent.ProjectileType<GuardianGauntletAnchor>(), ModContent.ProjectileType<GuardianHorizonLanceAnchor>()];

				foreach (int type in anchorTypes)
				{
					if (Player.ownedProjectileCounts[type] > 0)
					{
						anchor = Main.projectile.First(i => i.active && i.owner == Player.whoAmI && (i.type == type));
						break;
					}
				}
				*/

				foreach (Projectile anchorProjectile in Main.projectile)
				{
					if (anchorProjectile.ModProjectile is OrchidModGuardianParryAnchor && anchorProjectile.owner == Player.whoAmI && anchorProjectile.active)
					{
						anchor = anchorProjectile;
						break;
					}
				}

				if (anchor != null)
				{
					parryItem.OnParry(Player, this, aggressor, anchor);
					DoParryItemParrySpecialCases(anchor);
					//int toAdd = (anchor.type == anchorTypes[0]) ? 0 : 1;
					int toAdd = (anchor.ModProjectile is GuardianQuarterstaffAnchor) ? 0 : 1; // Gives 0 slam for a quarterstaff parry, 1 for other items

					if (aggressor is NPC npc)
					{
						parryItem.OnParryNPC(Player, this, npc, anchor);
						OnBlockNPC(anchor, npc, true);
						OnBlockNPCFirst(anchor, npc, toAdd, true);
					}
					else if (aggressor is Projectile projectile)
					{
						parryItem.OnParryProjectile(Player, this, projectile, anchor);
						OnBlockProjectile(anchor, projectile, true);
						OnBlockProjectileFirst(anchor, projectile, toAdd, true);
					}
					else
					{
						parryItem.OnParryOther(Player, this, anchor);
						OnBlockAny(anchor, true);
						OnBlockAnyFirst(anchor, ref toAdd, true);
						AddSlam(toAdd);
					}
				}
				parryItem.PlayParrySound(Player, this, anchor);

				OnAttack(attack, parryItem);
			}
		}

		public void DoParryItemParrySpecialCases(Projectile anchor)
		{ // I kinda hate this, so I'll leave it as a separate method in case we want to delete it.
			if (Player.whoAmI == Main.myPlayer)
			{
				if (Player.panic)
				{
					Player.AddBuff(BuffID.Panic, 480, false);
				}

				if (OrchidMod.ThoriumMod != null)
				{
					int beeDamage = 30; // public static readonly int BeeDamage = 30;
					for (int i = 3; i < 10; i ++)
					{
						Item item = Player.armor[i];
						if (item.type == OrchidMod.ThoriumMod.Find<ModItem>("SweetVengeance").Type)
						{ // Adapted from ThoriumPlayer.cs (even though it doesn't work with Hive Pack and World Difficulty)
							for (int index = 0; index < Main.rand.Next(2, 5); ++index)
								Projectile.NewProjectile(Player.GetSource_Accessory(item), Player.Center.X, Player.Center.Y, Main.rand.Next(-2, 2), Main.rand.Next(-2, 2), ProjectileID.Bee, beeDamage, 1f, Main.myPlayer, 0.0f, 0.0f, 0.0f);
							Player.AddBuff(BuffID.Honey, 300, false);
							break;
						}
					}
				}

				if (Player.honeyCombItem != null && !Player.honeyCombItem.IsAir)
				{ // adapted from Player.cs
					int nbBees = 1;
					if (Main.rand.NextBool(3))
						nbBees++;

					if (Main.rand.NextBool(3))
						nbBees++;

					if (Player.strongBees && Main.rand.NextBool(3))
						nbBees++;

					float beeDamage = 13f;
					if (Player.strongBees)
						beeDamage = 18f;

					if (Main.masterMode)
						beeDamage *= 2f;

					else if (Main.expertMode)
						beeDamage *= 1.5f;

					for (int count = 0; count < nbBees; count++)
					{
						float speedX = (float)Main.rand.Next(-35, 36) * 0.02f;
						float speedY = (float)Main.rand.Next(-35, 36) * 0.02f;
						Projectile.NewProjectile(Player.GetSource_Accessory(Player.honeyCombItem), Player.Center.X, Player.Center.Y, speedX, speedY, Player.beeType(), Player.beeDamage((int)beeDamage), Player.beeKB(0f), Main.myPlayer);
					}

					Player.AddBuff(BuffID.Honey, 300, false);
				}

				if (Player.starCloakItem != null && !Player.starCloakItem.IsAir)
				{ // Adapted from player.cs
					for (int count = 0; count < 3; count++)
					{
						float x = Player.position.X + (float)Main.rand.Next(-400, 400);
						float y = Player.position.Y - (float)Main.rand.Next(500, 800);
						Vector2 vector = new Vector2(x, y);
						float speedX = Player.position.X + (float)(Player.width / 2) - vector.X;
						float speedY = Player.position.Y + (float)(Player.height / 2) - vector.Y;
						speedX += (float)Main.rand.Next(-100, 101);
						float num18 = (float)Math.Sqrt(speedX * speedX + speedY * speedY);
						num18 = 23f / num18;
						speedX *= num18;
						speedY *= num18;
						int type = 726;

						Item item = Player.starCloakItem;
						if (Player.starCloakItem_starVeilOverrideItem != null)
						{
							item = Player.starCloakItem_starVeilOverrideItem;
							type = 725;
						}

						if (Player.starCloakItem_beeCloakOverrideItem != null)
						{
							item = Player.starCloakItem_beeCloakOverrideItem;
							type = 724;
						}

						if (Player.starCloakItem_manaCloakOverrideItem != null)
						{
							item = Player.starCloakItem_manaCloakOverrideItem;
							type = 723;
						}

						int starDamage = 75;
						if (Main.masterMode)
							starDamage *= 3;
						else if (Main.expertMode)
							starDamage *= 2;

						Projectile.NewProjectile(Player.GetSource_Accessory(item), x, y, speedX, speedY, type, starDamage, 5f, Player.whoAmI, 0f, Player.position.Y);
					}
				}
			}
		}

		/// <summary> Should be called when the player performs any action with a guardian weapon. Uses <c>GuardianAttackInfo</c> to determine attack context. See <c>AttackID</c> for a set of predefined attacks. </summary>
		public void OnAttack(GuardianAttackInfo info, OrchidModGuardianItem guardianItem = null)
		{
			if (GuardianCounter && info.Slam && GuardianCounterTime > 0)
			{
				info.Counter = true;
			}

			if (guardianItem != null)
			{
				if (!guardianItem.ModifyAttackInfo(ref info)) return;
			}

			if (info.Counter) GuardianCounterTime = 0;
			
			if (info.Offense && Player.boneGloveItem != null && !Player.boneGloveItem.IsAir && Player.boneGloveTimer == 0)
			{ // Bone glove compatibility, from vanilla code
				Player.boneGloveTimer = 60;
				Vector2 center = Player.Center;
				Vector2 vector = Player.DirectionTo(Player.ApplyRangeCompensation(0.2f, center, Main.MouseWorld)) * 10f;
				Projectile.NewProjectile(Player.GetSource_ItemUse(Player.boneGloveItem), center.X, center.Y, vector.X, vector.Y, ProjectileID.BoneGloveProj, 25, 5f, Player.whoAmI);
			}

			//DEBUG
			if (GuardianDebugVisuals == 1)
			{
				CombatText.NewText(new Rectangle((int)Player.Center.X, Player.Hitbox.Y, 0, 0), Color.White, GuardianTest.AttackTypeString(info) + " (" + (byte)info + ")");
			}
		}

		/// <summary>If <c>item</c> is null or <c>item.PreDeflect</c> returns true, performs a deflect. Any enemies or blockable projectiles that would intersect a point in <c>targetArea</c> within <c>range</c> distance of <c>origin</c> in the next <c>lenience</c> frames and match <c>EnemyPredicate</c> or <c>ProjPredicate</c> respectively will be deflected, caching enemies in the player's <c>DeflectedEnemies</c> and deleting projectiles. Returns <c>true</c> if a deflect occurred, and flags <c>caughtProjectile</c> if at least one projectile was deflected and/or <c>killedNPC</c> if at least one NPC was killed by the damage.</summary>
		/// <remarks>Fields left default will be treated with default behavior. <c>item</c> functions will not be called if <c>null</c>. The default <c>Predicate</c>s will check only for <c>!NPC.friendly</c> or <c>Projectile.hostile</c>. <c>range</c> defaults to -1 and will be ignored if less than 0. <c>lenience</c> defaults to 0, so the target needs to already be inside of the hitbox. <c>origin</c> defaults to the <c>Center</c> of this player.</remarks>
		public bool DoDeflect(Rectangle targetArea, out bool caughtProjectile, out bool killedNPC, OrchidModGuardianDeflectItem item = null, ModProjectile anchor = null, Predicate<NPC> EnemyPredicate = null, Predicate<Projectile> ProjPredicate = null, float range = -1, float lenience = 0, Vector2? origin = null, Vector2? originVelocity = null, int? damage = null)
		{
			Vector2 from = origin ?? Player.Center; 
			Vector2 fromVel;
			if (origin == null) fromVel = originVelocity ?? Player.velocity;
			else fromVel = originVelocity ?? Vector2.Zero;
			caughtProjectile = false;
			killedNPC = false;
			bool success = false;
			int dmg = GetGuardianDamage(damage ?? item?.Item.damage * item?.DeflectDamage ?? 0);
			int crit = GetGuardianCrit(item?.Item.crit ?? 0);
			DeflectedEnemies.Clear();
			EnemyPredicate ??= new Predicate<NPC>(T => !T.friendly && !T.CountsAsACritter);
			ProjPredicate ??= new Predicate<Projectile>(T => T.hostile);
			if (item != null && !item.PreDeflect(ref targetArea, ref success, ref caughtProjectile, ref killedNPC, anchor, ref EnemyPredicate, ref ProjPredicate, ref range, ref lenience, ref from, ref dmg))
				return false;

			if (GuardianDebugVisuals == 1)
			{
				Dust dust = Dust.NewDustPerfect(targetArea.TopLeft(), DustID.MagicMirror);
				dust.velocity = Vector2.Zero;
				dust.scale = 2f;
				dust = Dust.NewDustPerfect(targetArea.TopRight(), DustID.MagicMirror);
				dust.velocity = Vector2.Zero;
				dust.scale = 2f;
				dust = Dust.NewDustPerfect(targetArea.BottomLeft(), DustID.MagicMirror);
				dust.velocity = Vector2.Zero;
				dust.scale = 2f;
				dust = Dust.NewDustPerfect(targetArea.BottomRight(), DustID.MagicMirror);
				dust.velocity = Vector2.Zero;
				dust.scale = 2f;
			}
			
			Predicate<NPC> NPCInRange;
			if (range < 0) NPCInRange = new Predicate<NPC>(T => T.Hitbox.Intersects(targetArea) || (lenience > 0 && Collision.CheckAABBvLineCollision(targetArea.TopLeft(), targetArea.Size(), T.Hitbox.ClosestPointInRect(from), T.Hitbox.ClosestPointInRect(from) + (T.velocity - fromVel) * lenience)));
			else NPCInRange = new Predicate<NPC>(T => (T.Hitbox.Intersects(targetArea) || (lenience > 0 && Collision.CheckAABBvLineCollision(targetArea.TopLeft(), targetArea.Size(), T.Hitbox.ClosestPointInRect(from), T.Hitbox.ClosestPointInRect(from) + (T.velocity - fromVel) * lenience))) && (T.Hitbox.ClosestPointInRect(from).Distance(from) <= range || (lenience > 0 && from.ClosestPointOnLine(T.Hitbox.ClosestPointInRect(from), T.Hitbox.ClosestPointInRect(from) + (T.velocity - fromVel) * lenience).Distance(from) <= range)));
			foreach (NPC npc in Main.npc.Where(T => T.active && NPCInRange(T) && EnemyPredicate(T)))
			{
				if (item != null && !item.OnDeflectNPC(Player, this, npc, anchor)) continue;
				if (dmg > 0 && !npc.dontTakeDamage)
				{
					NPCKillAttempt isKill = new(npc);
					Player.ApplyDamageToNPC(npc, dmg, 0, npc.Center.X > from.X ? 1 : -1, Main.rand.Next(100) < crit, ModContent.GetInstance<GuardianDamageClass>(), true);
					if (isKill.DidNPCDie()) killedNPC = true;
					else DeflectedEnemies.Add(npc);
				}
				else DeflectedEnemies.Add(npc);
				success = true;
			}
			Predicate<Projectile> ProjInRange;
			if (range < 0) ProjInRange = new Predicate<Projectile>(T => T.Hitbox.Intersects(targetArea) || (lenience > 0 && Collision.CheckAABBvLineCollision(targetArea.TopLeft(), targetArea.Size(), T.Hitbox.ClosestPointInRect(from), T.Hitbox.ClosestPointInRect(from) + (T.velocity - fromVel) * lenience)));
			else ProjInRange = new Predicate<Projectile>(T => (T.Hitbox.Intersects(targetArea) || (lenience > 0 && Collision.CheckAABBvLineCollision(targetArea.TopLeft(), targetArea.Size(), T.Hitbox.ClosestPointInRect(from), T.Hitbox.ClosestPointInRect(from) + (T.velocity - fromVel) * lenience))) && (T.Hitbox.ClosestPointInRect(from).Distance(from) <= range || (lenience > 0 && from.ClosestPointOnLine(T.Hitbox.ClosestPointInRect(from), T.Hitbox.ClosestPointInRect(from) + (T.velocity - fromVel) * lenience).Distance(from) <= range)));
			foreach (Projectile proj in Main.projectile.Where(T => T.active && !ProjectilesBlockBlacklist.Contains(T.type) && ProjInRange(T) && ProjPredicate(T)))
			{
				if (anchor != null && proj.ModProjectile == anchor)
				{
					Main.NewText("A deflect weapon's anchor just attempted to deflect itself. Please report which weapon caused this and when!");
					continue;
				}
				if (item == null || item.OnDeflectProjectile(Player, this, proj, anchor))
				{
					proj.Kill();
					caughtProjectile = true;
					success = true;
				}
			}
			if (success) item?.PostDeflect(Player, this, anchor, caughtProjectile, killedNPC);
			return success;
		}

		/// <summary>Highlights that the player is missing resources. The number of guards or slams specified will flash red for the time specified, and text will optionally be displayed for which one the player doesn't have enough of.</summary>
		public void WarnMissingResources(int guards, int slams, int time = 45, bool playSound = true, bool spawnText = true, bool allowSpam = false)
		{
			if (MissingResourcesTimer > 0 && !allowSpam)
			{ // can't spam WarnMissingResources unless allowSpam is set to true, allowing for an easy way to have the resource flash on input release
				if (guards > 0 && MissingGuardsUI > 0 && slams > 0 && MissingSlamsUI > 0) return;
				else if (guards > 0 && MissingGuardsUI > 0) return;
				else if (slams > 0 && MissingSlamsUI > 0) return;
			}

			if (MissingGuardsUI != guards || MissingSlamsUI != slams)
			{
				if (playSound && MissingResourcesTimer <= 0) SoundEngine.PlaySound(SoundID.NPCHit42.WithVolumeScale(0.4f).WithPitchOffset(Main.rand.NextFloat(-0.3f, 0.3f))); // MissingResourcesTimer <= 0 prevents audio spam
				if (spawnText)
				{
					Rectangle rectangle = Player.Hitbox;
					rectangle.Y -= 32;
					int textWhoAmI = -1;
					Color color = new Color(255, 92, 92);
					if (guards > GuardianGuard && slams > GuardianSlam)
						textWhoAmI = CombatText.NewText(rectangle, color, Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.MissingGuardsAndSlams"), false, false);
					else if (guards > GuardianGuard)
						textWhoAmI = CombatText.NewText(rectangle, color, Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.MissingGuards"), false, false);
					else if (slams > GuardianSlam)
						textWhoAmI = CombatText.NewText(rectangle, color, Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.MissingSlams"), false, false);
					else //what is the player even being warned for
					{
						SoundEngine.PlaySound(SoundID.Item150);
						textWhoAmI = CombatText.NewText(Player.Hitbox, new Color(1f, 0f, 1f), "you're playing it wrong give me the controller");
					}

					if (textWhoAmI >= 0 && textWhoAmI < 100)
					{
						CombatText combattext = Main.combatText[textWhoAmI];
						combattext.dot = true;
						combattext.velocity.Y = -2.5f;
						combattext.lifeTime = 25;
					}
				}
			}
			else if (playSound) SoundEngine.PlaySound(SoundID.NPCHit42.WithVolumeScale(0.5f));
			MissingGuardsUI = guards;
			MissingSlamsUI = slams;
			MissingResourcesTimer = time;
		}

		public int ThrowLevel()
		{
			if (GuardianItemCharge < 45f) return 0;
			if (GuardianItemCharge < 90f) return 1;
			if (GuardianItemCharge < 135f) return 2;
			if (GuardianItemCharge < 180f) return 3;
			return 4;
		}

		/// <summary>Attempts to send a packet syncing GuardianItemCharge on the server and other clients. Does nothing unless called by the local client as a multiplayer client.</summary>
		public void SyncGuardianItemCharge()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer)
			{
				var packet = OrchidMod.Instance.GetPacket();
				packet.Write((byte)OrchidModMessageType.GUARDIANSYNCCHARGE);
				packet.Write(Player.whoAmI);
				packet.Write(GuardianItemCharge);
				packet.Send();
			}
		}
	}
}
