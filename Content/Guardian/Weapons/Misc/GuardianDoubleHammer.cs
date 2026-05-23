using System;
using System.Collections.Generic;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Localization;
using System.Linq;
using OrchidMod.Utilities;
using OrchidMod.Content.Guardian;
using OrchidMod.Content.General.Prefixes;
using OrchidMod.Content.Guardian.Projectiles.Misc;
using OrchidMod.Common.Global.Items;

namespace OrchidMod.Content.Guardian.Weapons.Misc
{

    public abstract class GuardianDoubleHammer : OrchidModGuardianItem
    {
        public bool hasSpecialHammerTexture = false;
        /// <summary>If true, the offhand hammer will be drawn using a separate texture</summary>
		public bool hasBackHammer = false;

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

        public virtual string HammerTexture => Texture + "_Hammer";
        public virtual string HammerBackTexture => Texture + "_HammerBack";
        public virtual string HammerTextureGlow => HammerTexture + "_Glow";
        

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

		/// <summary>
		/// Called upon landing a hit on an enemy.
		/// </summary>
		public virtual void WarhammerOnHitNPC(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, DoubleHammerHitContext context, bool Charged, bool firstHit) { } // Called upon landing any melee swing hit
		public virtual void WarhammerModifyHitNPC(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, ref NPC.HitModifiers modifiers, bool FullyCharged, bool Melee, bool Block, bool firstHit) { }

		public virtual void OnThrowTileCollide(Player player, OrchidGuardian guardian, Projectile projectile, Vector2 oldVelocity) { }
		public virtual void OnSwing(Player player, OrchidGuardian guardian, Projectile projectile, bool offHandHammer, bool Charged) { } // Called on the first frame of a throw, FullyCharged is true if the guardian's hammer charge is full, FullyCharged is true if the guardian's hammer charge is full
		public virtual void OnThrow(Player player, OrchidGuardian guardian, Projectile projectile, bool offHandHammer, bool Charged) { } // Called on the first frame of a swing
		public virtual void OnBlockThrow(Player player, OrchidGuardian guardian, Projectile projectile) { } // Called on the first frame of a block
		public virtual void ExtraAI(Player player, OrchidGuardian guardian, Projectile anchor, bool offHandHammer) { } // Called at the end of the anchors Projectile AI()
		/// <summary>Called before default throw AI. Return false to prevent the default AI from running.</summary>
		/// <remarks>Remember to set <c>Projectile.friendly</c> and <c>OrchidModGuardianProjectile.ResetHitStatus()</c> if overriding default behavior.</remarks>
		public virtual bool PreThrowAI(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak) => true;

		/// <summary>Called before drawing the hammer. Return false to prevent the default draw code from running.</summary>
		/// <remarks>The default draw code will use hammerTexture and drawRectangle (which defaults to null)</remarks>
		public virtual bool PreDrawHammer(SpriteBatch spriteBatch, OrchidGuardian guardian, Player player, Projectile projectile, bool offHandHammer, ref Color lightColor, ref Texture2D hammerTexture, ref Rectangle drawRectangle) => true;
		/// <summary>Called after drawing the hammer.</summary>
		public virtual void PostDrawHammer(SpriteBatch spriteBatch, OrchidGuardian guardian, Player player, Projectile projectile, bool offHandHammer, Color lightColor, Texture2D hammerTexture, Rectangle drawRectangle) { }
		public virtual Color GetHammerGlowmaskColor(Player player, OrchidGuardian guardian, Projectile projectile, Color lightColor) => Color.White;
		
		public virtual void SafeHoldItem(Player player) { }
        
        public virtual void SafeModifyTooltips(List<TooltipLine> tooltips) { } // Called at the end of ModifyTooltips
        
        public virtual void WarhammerPostDrawUI(SpriteBatch spriteBatch, Player player, ref Color lightColor, Projectile main, Projectile alt) { }

        public override int? AnchorType => ModContent.ProjectileType<GuardianDoubleHammerAnchor>();
		public sealed override void SetDefaults()
		{
			Item.DamageType = ModContent.GetInstance<GuardianDamageClass>();
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
			SwingDamage = 0.25f;
			SwingChargeGain = 1f;
			BlockDamage = 0.1667f;
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

		public override bool WeaponPrefix() => true;

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}


		public int[] DoubleClicking = new int[2];
		public int[] DoubleClicked = new int[2];
		public bool DoubleClickLeft => DoubleClicked[0] == 15;
		public bool DoubleClickRight => DoubleClicked[1] == 15;
		public ref int DoubleClickedLeft => ref DoubleClicked[0];
		public ref int DoubleClickedRight => ref DoubleClicked[1];


        public sealed override void HoldItem(Player player)
		{
			var projectileType = ModContent.ProjectileType<GuardianDoubleHammerAnchor>();
			var guardian = player.GetModPlayer<OrchidGuardian>();
			guardian.GuardianDisplayUI = 300;



			int count = player.ownedProjectileCounts[projectileType];
			if (count < 2)
			{
				if (count == 1)
				{
					var proj = Main.projectile.First(i => i.active && i.owner == player.whoAmI && i.type == projectileType);
					if (proj != null && proj.ModProjectile is GuardianDoubleHammerAnchor) proj.Kill();
				}

				int[] indexes = [-1, -1];
				for (int i = 0; i < 2; i++)
				{
					var index = Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center.X, player.Center.Y, 0f, 0f, projectileType, 0, 0f, player.whoAmI);

					var proj = Main.projectile[index];
					if (proj.ModProjectile is not GuardianDoubleHammerAnchor hammer)
					{
						proj.Kill();
					}
					else
					{
						indexes[i] = proj.whoAmI;
						hammer.OffHandHammer = i == 0;
						hammer.OnChangeSelectedItem(player);
						hammer.NeedNetUpdate = true;
					}
				}

				if (indexes[1] < indexes[0])
				{ // Swap order if necessary in Main.projectile[] so the front hammer is drawn first
					(Main.projectile[indexes[0]], Main.projectile[indexes[1]]) = (Main.projectile[indexes[1]], Main.projectile[indexes[0]]);
					Main.projectile[indexes[0]].whoAmI = indexes[1];
					Main.projectile[indexes[1]].whoAmI = indexes[0];
				}
			}
			else
			{
				foreach (Projectile projectile in Main.projectile)
				{
					if (projectile.type == projectileType && projectile.active && projectile.owner == player.whoAmI && projectile.ModProjectile is GuardianDoubleHammerAnchor hammer)
					{
						if (hammer.SelectedItem != player.selectedItem)
							hammer.OnChangeSelectedItem(player);
					}
				}
			}
			DoBufferedHammerInputs(player);
			SafeHoldItem(player);
		}

        public override bool CanUseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer && !player.cursed)
			{
				OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
				var projectileType = ModContent.ProjectileType<GuardianDoubleHammerAnchor>();
				int[] anchors = GetAnchors(player);
				if (anchors != null)
				{
                    bool punchHold = Main.mouseLeft;
                    bool punchTap = Main.mouseLeftRelease;
                    bool guardHold = Main.mouseRight;
                    bool guardTap = Main.mouseRightRelease;

					if (punchHold && punchTap && guardian.GuardianItemCharge <= 0) swingTimer = 6;
					if (guardHold && guardTap && !guardian.GuardianParry) shouldGuard = true;
				}
			}
			return false;
		}

		int swingTimer = 0;
		bool shouldSwing => swingTimer > 0;
		bool shouldGuard;

        void DoBufferedHammerInputs(Player player)
		{
			int[] anchors = GetAnchors(player);
			if (anchors != null)
			{
				OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
				bool punchHold = Main.mouseLeft;
				bool punchTap = Main.mouseLeftRelease;
				bool guardHold = Main.mouseRight;
				bool guardTap = Main.mouseRightRelease;

				Projectile projectileMain = Main.projectile[anchors[1]];
				Projectile projectileOff = Main.projectile[anchors[0]];

				if (guardian.GuardianItemCharge > 0) swingTimer = 0;
				if (shouldSwing && !punchHold) swingTimer--;
				if (!guardHold || guardian.GuardianParry) shouldGuard = false;
				
				if (projectileMain.ModProjectile is GuardianDoubleHammerAnchor mainAnchor && projectileOff.ModProjectile is GuardianDoubleHammerAnchor offAnchor)
				{
					bool? swingButton = null;
					if (mainAnchor.Thrown || mainAnchor.BlockDuration != 0)
						swingButton = Main.mouseRight;
					else if (offAnchor.Thrown || offAnchor.BlockDuration != 0)
						swingButton = Main.mouseLeft;

                    if (projectileMain.ai[1] == 0f || projectileOff.ai[1] == 0f || (projectileMain.ai[1] > 0f && projectileOff.ai[1] > 0f))
                    {

					}
                }	
			}

		}

        public static int[] GetAnchors(Player player)
		{
			var projectileType = ModContent.ProjectileType<GuardianDoubleHammerAnchor>();
			int[] anchors = [-1, -1];
			foreach (Projectile proj in Main.projectile)
			{
				if (proj.active && proj.owner == player.whoAmI && proj.type == projectileType)
				{
					if (anchors[0] == -1)
						anchors[0] = proj.whoAmI;
					else
					{
						anchors[1] = proj.whoAmI;
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
				string TooltipToGet = Mod.GetLocalizationKey("Misc.GuardianGrants");
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
            SafeModifyTooltips(tooltips);
		}

        public virtual Texture2D GetHammerTexture(Player player, Projectile anchor, bool OffHandHammer, out Rectangle? drawRectangle, int frame = 0)
        {
            drawRectangle = null;

            Texture2D texture;
            if (hasSpecialHammerTexture) {
                if (OffHandHammer && hasBackHammer) texture = ModContent.Request<Texture2D>(HammerBackTexture).Value;
                else texture = ModContent.Request<Texture2D>(HammerTexture).Value;
            }
            else texture = ModContent.Request<Texture2D>(Texture).Value;

            if (HammerFrames > 1) drawRectangle = texture.Frame(1, HammerFrames, 0, frame % HammerFrames);

            return texture;
        }

        public virtual Texture2D GetGlowmaskTexture(Player player, Projectile anchor, bool OffHandHammer, out Rectangle? drawRectangle)
        {
            drawRectangle = null;
            return ModContent.RequestIfExists<Texture2D>(HammerTextureGlow, out Asset<Texture2D> assetGlow) ? assetGlow.Value : null;
        }
    }
}