using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common.Global.Items;
using OrchidMod.Content.General.Prefixes;
using OrchidMod.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Terraria.NPC;

namespace OrchidMod.Content.Guardian
{
	public abstract class OrchidModGuardianHammer : OrchidModGuardianItem
	{
		public int Range;
		/// <summary>Amount of slams (resource) to grand the owner on a fully charged throw hit. SlamStacks or GuardStacks should always be >0. Defaults to 0</summary>
		public int SlamStacks;
		/// <summary>Amount of guards (resource) to grand the owner on a fully charged throw hit. GuardStacks or SlamStacks should always be >0. Defaults to 0</summary>
		public int GuardStacks;
		/// <summary>Time in frame the warhammer will remain in the air before returning to the player when blocking. Defaults to 180, as warhammers tend to have long block durations.</summary>
		public int BlockDuration;
		/// <summary>Will the warhammer pierce enemies when thrown? Defaults to false.</summary>
		public bool Penetrate;
		/// <summary>Will the warhammer phase through tiles when thrown? Defaults to true.</summary>
		public bool TileCollide;
		/// <summary>Will the warhammer bounce off tiles when thrown? Defaults to false.</summary>
		public bool TileBounce;
		/// <summary>Multiplier for the speed of the warhammer when returning after a throw. Defaults to 1f.</summary>
		public float ReturnSpeed;
		/// <summary>Multiplier for the speed of a melee swing animation. Defaults to 1f.</summary>
		public float SwingSpeed;
		/// <summary>Multiplier for the amount of bonus charge gained from hitting with a melee swing. Defaults to 1f.</summary>
		public float SwingChargeGain;
		/// <summary>Multiplier for the baseline charge speed of the weapon. Defaults to 1f.</summary>
		public float WaitChargeGain;
		/// <summary>Multiplier for the item damage dealt to enemies hit by a swing. Defaults to 0.5f</summary>
		public float SwingDamage;
		/// <summary>Multiplier for the item damage dealt to enemies hit by a throw. Defaults to 1f</summary>
		public float ThrowDamage;
		/// <summary>Multiplier for the item damage dealt to enemies hit by a block. Defaults to 0.33f</summary>
		public float BlockDamage;
		/// <summary>Projectile.localNPCHitCooldown for the hammer anchor. Defaults to 30.</summary>
		public int HitCooldown;
		/// <summary>If true, the warhammer cannot block. Defaults to false.</summary>
		public bool CannotBlock;
		/// <summary>If true, the warhammer will never be swung.</summary>
		public bool CannotSwing;
		/// <summary>offsets the drawing of the warhammer while being held (in pixels). Negative values draw it closer, while positive further.</summary>
		public float HoldOffset;
		/// <summary>Multiplier for the initial velocity of the hammer when blocking.</summary>
		public float BlockVelocityMult;
		/// <summary>If true, the anchor will load and use ItemName_Hammer.png as its texture.</summary>
		public bool hasSpecialHammerTexture;
		/// <summary>Amount of slams (resource) required to initiate a block. Defaults to 0.</summary>
		public int SlamBlockCost;
		/// <summary>Amount of guards (resource) required to initiate a block. Defaults to 1.</summary>
		public int GuardBlockCost;
		/// <summary>Amount of frames drawn on the HammerTexture. Used occasionally for special hammers. See PumpkingWarhammer for an example. Defaults to 1.</summary>
		public int HammerFrames = 1;
		/// <summary>If true, the warhammer will be unaffected by the Hammer Magnet accessory.</summary>
		public bool CannotMagnet;
		/// <summary>If true, the warhammer will be unaffected by the Remote Detonator accessory.</summary>
		public bool CannotExplode;
		/// <summary>If true, the OrchidGuardian.GuardianHammerThrowVelocity multiplier will not affect throw velocity. Defaults to false.</summary>
		public bool IgnoreHammerThrowVelocity;
		/// <summary>If true, this item will have the player dual wield warhammers. Defaults to false.</summary>
		public bool DualWarhammers;
		public virtual string HammerTexture => Texture + "_Hammer";

		/// <summary>Called upon pushing an enemy with a throw (can happen repeatedly).</summary>
		public virtual void OnBlockContact(Player player, OrchidGuardian guardian, NPC target, Projectile projectile) { }
		/// <summary>Called upon blocking an enemy (1 time per throw per enemy).</summary>
		public virtual void OnBlockNPC(Player player, OrchidGuardian guardian, NPC target, Projectile projectile) { }
		/// <summary>Called upon blocking the first enemy of a blocking throw</summary>
		public virtual void OnBlockFirstNPC(Player player, OrchidGuardian guardian, NPC target, Projectile projectile) { }
		/// <summary>Called upon blocking a proejctile, return false to prevent the projectile from being destroyed</summary>
		public virtual bool OnBlockProjectile(Player player, OrchidGuardian guardian, Projectile projectileHammer, Projectile projectileBlocked) { return true; }
		/// <summary>Called upon blocking the first projectile of a blocking throw</summary>
		public virtual void OnBlockFirstProjectile(Player player, OrchidGuardian guardian, Projectile projectileHammer, Projectile projectileBlocked) { }
		/// <summary>Called upon landing any melee swing hit</summary>
		public virtual void OnMeleeHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged, bool OffHand) { }
		/// <summary>Called upon landing the first hit of a melee swing</summary>
		public virtual void OnMeleeHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged, bool OffHand) { }
		/// <summary>Called upon landing any throw hit</summary>
		public virtual void OnThrowHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak, bool OffHand) { }
		/// <summary>Called upon landing the first hit of a throw</summary>
		public virtual void OnThrowHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak, bool OffHand) { }
		/// <summary>Called upon landing any block hit</summary>
		public virtual void OnBlockHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit) { }
		/// <summary>Called upon landing the first hit of a block</summary>
		public virtual void OnBlockHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit) { }
		/// <summary>Called at the end of the anchor's WarhammerModifyHitNPC() whenever it hits a target, after applying normal warhammer damage multipliers.</summary>
		public virtual void WarhammerModifyHitNPC(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, ref HitModifiers modifiers, bool FullyCharged, bool Melee, bool Block, bool firstHit, bool OffHand) { }
		/// <summary>Called at the end of the anchor's TileCollide(), after applying normal warhammer tilecollide behavior.</summary>
		public virtual void OnThrowTileCollide(Player player, OrchidGuardian guardian, Projectile projectile, Vector2 oldVelocity, bool OffHand) { }
		/// <summary>Called on the first frame of a swing, FullyCharged is true if the guardian's hammer charge is full, FullyCharged is true if the guardian's hammer charge is full</summary>
		public virtual void OnSwing(Player player, OrchidGuardian guardian, Projectile projectile, bool FullyCharged, bool OffHand) { }
		/// <summary>Called on the first frame of a throw</summary>
		public virtual void OnThrow(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak, bool OffHand) { }
		/// <summary>Called on the first frame of a block</summary>
		public virtual void OnBlockThrow(Player player, OrchidGuardian guardian, Projectile projectile) { }
		/// <summary>Called at the end of the anchor's Projectile AI()</summary>
		public virtual void ExtraAI(Player player, OrchidGuardian guardian, Projectile projectile, bool OffHand) { }
		/// <summary>Called before default throw AI. Return false to prevent the default AI from running.</summary>
		/// <remarks>Remember to set <c>Projectile.friendly</c> and <c>OrchidModGuardianProjectile.ResetHitStatus()</c> if overriding default behavior.</remarks>
		public virtual bool ThrowAI(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak, bool OffHand) => true;
		/// <summary>Called before drawing the hammer. Return false to prevent the default draw code from running.</summary>
		/// <remarks>The default draw code will use hammerTexture and drawRectangle (which defaults to null)</remarks>
		public virtual bool PreDrawHammer(Player player, OrchidGuardian guardian, Projectile projectile, SpriteBatch spriteBatch, ref Color lightColor, ref Texture2D hammerTexture, ref Rectangle drawRectangle, bool OffHand) => true;
		/// <summary>Called after drawing the hammer.</summary>
		public virtual void PostDrawHammer(Player player, OrchidGuardian guardian, Projectile projectile, SpriteBatch spriteBatch, Color lightColor, Texture2D hammerTexture, Rectangle drawRectangle, bool OffHand) { }
		/// <summary>Color applied to the hammer's glowmask when drawn. To automatically draw a glowmask for the hammer, add a HammerName_Glow texture file where the hammer texture is located.</summary>
		public virtual Color GetHammerGlowmaskColor(Player player, OrchidGuardian guardian, Projectile projectile, Color lightColor, bool OffHand) => Color.White;
		
		/// <summary>Draws extra UI elements on the GuardianUIState while held.</summary>
		public virtual void WarhammerPostDrawUI(SpriteBatch spriteBatch, Player player, ref Color lightColor, Projectile projectile) { }

		public override int? AnchorType => ModContent.ProjectileType<GuardianHammerAnchor>();
		public sealed override void SetDefaults()
		{
			Item.DamageType = GetInstance<GuardianDamageClass>();
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = false;
			Item.maxStack = 1;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 30;
			Item.knockBack = 10f;
			Item.shootSpeed = 10f;
			Range = 0;
			HitCooldown = 30;
			Penetrate = false;
			TileBounce = false;
			TileCollide = true;
			SlamStacks = 0;
			ThrowDamage = 1f;
			ReturnSpeed = 1f;
			SwingSpeed = 1f;
			SwingDamage = 0.5f;
			SwingChargeGain = 1f;
			WaitChargeGain = 1f;
			BlockDamage = 0.33f;
			BlockDuration = 180;
			BlockVelocityMult = 1f;
			CannotBlock = false;
			hasSpecialHammerTexture = false;
			DualWarhammers = false;
			IgnoreHammerThrowVelocity = false;

			SlamBlockCost = 0;
			GuardBlockCost = 1;
			
			HammerFrames = 1;

			OrchidGlobalItemPerEntity orchidItem = Item.GetGlobalItem<OrchidGlobalItemPerEntity>();
			orchidItem.guardianWeapon = true;

			SafeSetDefaults();

			Item.useAnimation = Item.useTime;
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override bool WeaponPrefix() => true;

		public sealed override void HoldItem(Player player)
		{
			var guardian = player.GetModPlayer<OrchidGuardian>();
			guardian.GuardianDisplayUI = 300;
		}

		public override bool? UseItem(Player player)
		{
			var guardian = player.GetModPlayer<OrchidGuardian>();
			bool validRMBInput = !CannotBlock && Main.mouseRight && Main.mouseRightRelease && ((SlamBlockCost > 0 && guardian.UseSlam(SlamBlockCost, true)) || (GuardBlockCost > 0 && guardian.UseGuard(GuardBlockCost, true)));

			if (validRMBInput || Main.mouseLeft)
			{ // If the player does a valid RMB input (the weapon can block) or a mouseleft input - so we don't create a proejctile for nothing in edge cases
				int projType = ProjectileType<GuardianHammerAnchor>();
				int damage = guardian.GetGuardianDamage(Item.damage);
				Projectile projectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), player.Center, Vector2.Zero, projType, damage, Item.knockBack, player.whoAmI);
				projectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);

				if (DualWarhammers && !validRMBInput)
				{ // Creates another Anchor on use, and flags it as the offhand warhammer, before making sure they are drawn in the correct order
					Projectile projectileOffHand = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), player.Center, Vector2.Zero, projType, damage, Item.knockBack, player.whoAmI);
					projectileOffHand.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);

					if (projectile.whoAmI < projectileOffHand.whoAmI)
					{ // Swap order if necessary in Main.projectile[] so the front hammer is drawn first
						(Main.projectile[projectile.whoAmI], Main.projectile[projectileOffHand.whoAmI]) = (Main.projectile[projectileOffHand.whoAmI], Main.projectile[projectile.whoAmI]);
						projectile.whoAmI = projectileOffHand.whoAmI;
						projectileOffHand.whoAmI = projectile.whoAmI;
					}

					if (projectileOffHand.ModProjectile is GuardianHammerAnchor anchorOffHand)
					{
						anchorOffHand.OffHand = true;
					}
				}

				if (projectile.ModProjectile is GuardianHammerAnchor anchor && validRMBInput && GetAnchors(player)[1] == -1)
				{ // starts a block
					if (SlamBlockCost > 0) guardian.UseSlam(SlamBlockCost);
					if (GuardBlockCost > 0) guardian.UseGuard(GuardBlockCost);
					projectile.velocity = Vector2.Normalize(Main.MouseWorld - player.Center) * (10f + (Item.shootSpeed - 10f) * 0.35f * BlockVelocityMult);
					projectile.friendly = true;
					projectile.knockBack = 0f;
					projectile.tileCollide = true;

					anchor.BlockDuration = (int)(BlockDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianBlockDuration + 10);
					anchor.NeedNetUpdate = true;
				}

				guardian.GuardianItemCharge = 0f;
				return true;
			}

			return false;
		}
		
		public override bool CanUseItem(Player player)
		{
			int projType = ProjectileType<GuardianHammerAnchor>();

			var proj = Main.projectile.FirstOrDefault(i => i.active && i.owner == player.whoAmI && i.type == projType && i.ModProjectile is GuardianHammerAnchor warhammer && warhammer.BlockDuration > 0);
			if (Main.mouseRight && Main.mouseRightRelease && proj != null && proj.ModProjectile is GuardianHammerAnchor warhammer)
			{ // recalls existing blocking warhammers when right clicking
				warhammer.BlockDuration = -30; // -30 instead of -1 so they return faster
				proj.netUpdate = true;
				return false;
			}

			bool offHandCheck = true; // set to false if a one of the 2 warhammers is ready (when dual wielding)
			int[] anchors = GetAnchors(player);
			if (DualWarhammers && anchors != null)
			{
				Projectile mainHand = Main.projectile[anchors[0]];
				Projectile offHand = Main.projectile[anchors[1]];

				if (mainHand.ai[1] == 0f || offHand.ai[1] == 0f)
				{ // at least one of the warhammers is idle
					offHandCheck = false;
				}
			}

			var guardian = player.GetModPlayer<OrchidGuardian>();
			bool validRMBInput = anchors == null && !CannotBlock && Main.mouseRight && Main.mouseRightRelease && ((SlamBlockCost > 0 && guardian.UseSlam(SlamBlockCost, true)) || (GuardBlockCost > 0 && guardian.UseGuard(GuardBlockCost, true)));

			if ((player.ownedProjectileCounts[projType] > 0 && (offHandCheck || !validRMBInput)) || (!(Main.mouseRight && Main.mouseRightRelease && player.GetModPlayer<OrchidGuardian>().UseGuard(1, true)) && !Main.mouseLeft)) return false;

			if (validRMBInput || Main.mouseLeft)
			{
				return base.CanUseItem(player);
			}

			return false;
		}


		/// <summary>Returns the whoamI of the warhammer anchors used by this item. When dual wielding, the 2nd value of the array will the the whoam of the offhand warhammer, else, it will be -1. Returns null if no hammers exist.</summary>
		public int[] GetAnchors(Player player)
		{
			var projectileType = ModContent.ProjectileType<GuardianHammerAnchor>();
			int[] anchors = [-1, -1];
			foreach (Projectile proj in Main.projectile)
			{
				if (proj.active && proj.owner == player.whoAmI && proj.type == projectileType)
				{
					if (anchors[0] == -1)
					{
						anchors[0] = proj.whoAmI;
						if (!DualWarhammers)
						{
							return anchors;
						}
					}
					else
					{
						anchors[1] = anchors[0];
						anchors[0] = proj.whoAmI;

						return anchors;
					}
				}
			}

			return null;
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

			if (!CannotBlock)
			{
				tooltips.Insert(index + 1, new TooltipLine(Mod, "BlockDuration", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.BlockDuration", OrchidUtils.FramesToSeconds((int)(BlockDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianBlockDuration)))));

				string click = Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.RightClick");
				string block = "Mods.OrchidMod.UI.GuardianItem.Block";
				if (!(GuardBlockCost == 1 && SlamBlockCost == 0))
				{
					if (GuardBlockCost > 0) block += "Guard";
					if (SlamBlockCost > 0) block += "Slam";
					if (GuardBlockCost == SlamBlockCost) block += "Same";
				}
				tooltips.Insert(index + 2, new TooltipLine(Mod, "ClickInfo", Language.GetText(block).Format(click, GuardBlockCost, SlamBlockCost))
				{
					OverrideColor = new Color(175, 255, 175)
				});
			}

			string ChargeToThrow = Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.ChargeToThrow");
			if (!CannotSwing) ChargeToThrow = Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.SwingWhileCharging");
			tooltips.Insert(index + (!CannotBlock ? 3 : 1), new TooltipLine(Mod, "Swing", ChargeToThrow)
			{
				OverrideColor = new Color(175, 255, 175)
			});

			if (GuardStacks > 0 || SlamStacks > 0)
			{
				string TooltipToGet = GetInstance<OrchidMod>().GetLocalizationKey("Misc.GuardianGrants");
				switch(GuardStacks)
				{
					case > 0: TooltipToGet += "Guard"; break;
				}
				switch (SlamStacks)
				{
					case > 0: TooltipToGet += "Slam"; break;
				}
				if (GuardStacks == SlamStacks) TooltipToGet += "Same";

				tooltips.Insert(index + 1, new TooltipLine(Mod, "GuardianGrants", Language.GetText(TooltipToGet).Format(GuardStacks, SlamStacks))
				{
					OverrideColor = new Color(175, 255, 175)
				});
			}
		}
	}
}
