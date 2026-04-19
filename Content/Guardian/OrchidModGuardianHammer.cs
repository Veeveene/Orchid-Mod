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
		public int SlamStacks;
		public int GuardStacks;
		public int BlockDuration;
		public bool Penetrate;
		public bool TileCollide;
		public bool TileBounce;
		public float ReturnSpeed;
		public float SwingSpeed;
		/// <summary>Multiplier for the amount of bonus charge gained from hitting with a melee swing.</summary>
		public float SwingChargeGain;
		public float SwingDamage;
		public float ThrowDamage;
		public float BlockDamage;
		public int HitCooldown;
		/// <summary>If true, the warhammer will never be swung.</summary>
		public bool CannotSwing;
		/// <summary>offsets the drawing of the warhammer while being held (in pixels). Negative values draw it closer, while positive further.</summary>
		public float HoldOffset;
		/// <summary>Multiplier for the initial velocity of the hammer when blocking.</summary>
		public float BlockVelocityMult;
		/// <summary>If true, the anchor will load and use ItemName_Hammer.png as its texture.</summary>
		public bool hasSpecialHammerTexture = false;
		public virtual string HammerTexture => Texture + "_Hammer";
		public int SlamBlockCost;
		public int GuardBlockCost;
		public int HammerFrames = 1;

		/// <summary>If true, the warhammer will be unaffected by the Hammer Magnet accessory.</summary>
		public bool CannotMagnet;
		/// <summary>If true, the warhammer will be unaffected by the Remote Detonator accessory.</summary>
		public bool CannotExplode;

		public virtual void OnBlockContact(Player player, OrchidGuardian guardian, NPC target, Projectile projectile) { } // Called upon pushing an enemy with a throw (can happen repeatedly)
		public virtual void OnBlockNPC(Player player, OrchidGuardian guardian, NPC target, Projectile projectile) { } // Called upon blocking an enemy (1 time per throw per enemy)
		public virtual void OnBlockFirstNPC(Player player, OrchidGuardian guardian, NPC target, Projectile projectile) { } // Called upon blocking the first enemy of a blocking throw
		public virtual bool OnBlockProjectile(Player player, OrchidGuardian guardian, Projectile projectileHammer, Projectile projectileBlocked) { return true; } // Called upon blocking a proejctile, return false to prevent the projectile from being destroyed
		public virtual void OnBlockFirstProjectile(Player player, OrchidGuardian guardian, Projectile projectileHammer, Projectile projectileBlocked) { } // Called upon blocking the first projectile of a blocking throw
		public virtual void OnMeleeHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged) { } // Called upon landing any melee swing hit
		public virtual void OnMeleeHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged) { } // Called upon landing the first hit of a melee swing
		public virtual void OnThrowHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak) { } // Called upon landing any throw hit
		public virtual void OnThrowHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak) { } // Called upon landing the first hit of a throw
		public virtual void OnBlockHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit) { } // Called upon landing any block hit
		public virtual void OnBlockHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit) { } // Called upon landing the first hit of a block
		public virtual void WarhammerModifyHitNPC(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, ref HitModifiers modifiers, bool FullyCharged, bool Melee, bool Block, bool firstHit) { }
		public virtual void OnThrowTileCollide(Player player, OrchidGuardian guardian, Projectile projectile, Vector2 oldVelocity) { }
		public virtual void OnSwing(Player player, OrchidGuardian guardian, Projectile projectile, bool FullyCharged) { } // Called on the first frame of a throw, FullyCharged is true if the guardian's hammer charge is full, FullyCharged is true if the guardian's hammer charge is full
		public virtual void OnThrow(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak) { } // Called on the first frame of a swing
		public virtual void OnBlockThrow(Player player, OrchidGuardian guardian, Projectile projectile) { } // Called on the first frame of a block
		public virtual void ExtraAI(Player player, OrchidGuardian guardian, Projectile projectile) { } // Called at the end of the anchors Projectile AI()
		/// <summary>Called before default throw AI. Return false to prevent the default AI from running.</summary>
		/// <remarks>Remember to set <c>Projectile.friendly</c> and <c>OrchidModGuardianProjectile.ResetHitStatus()</c> if overriding default behavior.</remarks>
		public virtual bool ThrowAI(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak) => true;
		/// <summary>Called before drawing the hammer. Return false to prevent the default draw code from running.</summary>
		/// <remarks>The default draw code will use hammerTexture and drawRectangle (which defaults to null)</remarks>
		public virtual bool PreDrawHammer(Player player, OrchidGuardian guardian, Projectile projectile, SpriteBatch spriteBatch, ref Color lightColor, ref Texture2D hammerTexture, ref Rectangle drawRectangle) => true;
		/// <summary>Called after drawing the hammer.</summary>
		public virtual void PostDrawHammer(Player player, OrchidGuardian guardian, Projectile projectile, SpriteBatch spriteBatch, Color lightColor, Texture2D hammerTexture, Rectangle drawRectangle) { }
		public virtual Color GetHammerGlowmaskColor(Player player, OrchidGuardian guardian, Projectile projectile, Color lightColor) => Color.White;

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
			BlockDamage = 0.33f;
			BlockDuration = 180;
			BlockVelocityMult = 1f;

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
			int projType = ProjectileType<GuardianHammerAnchor>();
			int damage = guardian.GetGuardianDamage(Item.damage);
			Projectile projectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), player.Center, Vector2.Zero, projType, damage, Item.knockBack, player.whoAmI);
			projectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);

			if (Main.mouseRight && Main.mouseRightRelease && projectile.ModProjectile is GuardianHammerAnchor anchor && ((SlamBlockCost > 0 && guardian.UseSlam(SlamBlockCost, true)) || (GuardBlockCost > 0 && guardian.UseGuard(GuardBlockCost, true))))
			{
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
		
		public override bool CanUseItem(Player player)
		{
			int projType = ProjectileType<GuardianHammerAnchor>();

			if (Main.mouseRight && Main.mouseRightRelease)
			{
				var proj = Main.projectile.FirstOrDefault(i => i.active && i.owner == player.whoAmI && i.type == projType && i.ModProjectile is GuardianHammerAnchor warhammer && warhammer.BlockDuration > 0);
				if (proj != null && proj.ModProjectile is GuardianHammerAnchor warhammer)
				{ // recalls existing blocking warhammers when right clicking
					warhammer.BlockDuration = -30; // -30 instead of -1 so they return faster
					proj.netUpdate = true;
				}
			}

			if (player.ownedProjectileCounts[projType] > 0 || (!(Main.mouseRight && Main.mouseRightRelease && player.GetModPlayer<OrchidGuardian>().UseGuard(1, true)) && !Main.mouseLeft)) return false;
			return base.CanUseItem(player);
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

			tooltips.Insert(index + 1, new TooltipLine(Mod, "BlockDuration", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.BlockDuration", OrchidUtils.FramesToSeconds((int)(BlockDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianBlockDuration)))));

			string click = Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.RightClick");
			string block = "Mods.OrchidMod.UI.GuardianItem.Block";
			if (!(GuardBlockCost == 1 && SlamBlockCost == 0)) {
				if (GuardBlockCost > 0) block += "Guard";
				if (SlamBlockCost > 0) block += "Slam";
				if (GuardBlockCost == SlamBlockCost) block += "Same";
			}
			tooltips.Insert(index + 2, new TooltipLine(Mod, "ClickInfo", Language.GetText(block).Format(click, GuardBlockCost, SlamBlockCost))
			{
				OverrideColor = new Color(175, 255, 175)
			});

			string ChargeToThrow = Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.ChargeToThrow");
			if (!CannotSwing) ChargeToThrow = Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.SwingWhileCharging");
			tooltips.Insert(index + 3, new TooltipLine(Mod, "Swing", ChargeToThrow)
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
