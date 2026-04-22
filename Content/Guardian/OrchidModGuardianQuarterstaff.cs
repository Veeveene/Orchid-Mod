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
	public abstract class OrchidModGuardianQuarterstaff : OrchidModGuardianParryItem
	{
		public virtual string QuarterstaffTexture => Texture + "_Staff";
		public virtual void OnHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, HitInfo hit, bool jabAttack, bool counterAttack) { } // Called when hitting a target during an attack
		public virtual void OnHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, HitInfo hit, bool jabAttack, bool counterAttack) { } // Called when hitting the first target for the first time during an attack
		public virtual void QuarterstaffModifyHitNPC(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, ref NPC.HitModifiers modifiers, bool jabAttack, bool counterAttack, bool firstHit) { } // anchor's modifyhitNPC
		public virtual void OnAttack(Player player, OrchidGuardian guardian, Projectile projectile, bool jabAttack, bool counterAttack) { } // Called on the first frame of an attack
		/// <summary>Called before uncharged jab or mid-charging jab AI is executed, including repositioning the quarterstaff and the player's arms for its attack animation. Return false to prevent normal AI from running, effectively overriding <c>JabStyle</c>.</summary>
		/// <remarks>During a jab, <c>Projectile.ai[0]</c> is set to -40 as an animation timer and incremented by <c>JabSpeed</c> every frame until it reaches 0. Use <c>Projectile.ResetLocalNPCHitImmunity()</c> for multi-swing animations to allow them to hit multiple times. See <c>GuardianQuarterstaffAnchor</c> for examples of default <c>JabStyle</c> behavior.</remarks>
		public virtual bool PreJabAI(Player player, OrchidGuardian guardian, Projectile anchor) { return true; }
		/// <summary>Called before fully charged swing AI is executed, including repositioning the quarterstaff and the player's arms for its attack animation. Return false to prevent normal AI from running, effectively overriding <c>SwingStyle</c>.</summary>
		/// <remarks>During a swing, <c>Projectile.ai[0]</c> is set to 41 as an animation timer and decremented by <c>SwingSpeed</c> every frame until it reaches 1. Use <c>Projectile.ResetLocalNPCHitImmunity()</c> for multi-swing animations to allow them to hit multiple times. See <c>GuardianQuarterstaffAnchor</c> for examples of default <c>SwingStyle</c> behavior.</remarks>
		public virtual bool PreSwingAI(Player player, OrchidGuardian guardian, Projectile anchor) { return true; }
		public virtual bool PreCounterAI(Player player, OrchidGuardian guardian, Projectile anchor) { return true; }
		public virtual void OnParryQuarterstaff(Player player, OrchidGuardian guardian, Entity aggressor, Projectile anchor) { } // Called on parrying anything
		public virtual void ExtraAIQuarterstaff(Player player, OrchidGuardian guardian, Projectile projectile) { } // Called at the end of the Anchor Projectile AI
		public virtual void ExtraAIQuarterstaffJabbing(Player player, OrchidGuardian guardian, Projectile projectile) { } // Called while jabbing
		public virtual void ExtraAIQuarterstaffBlocking(Player player, OrchidGuardian guardian, Projectile projectile) { } // Called while blocking
		public virtual void ExtraAIQuarterstaffSwinging(Player player, OrchidGuardian guardian, Projectile projectile) { } // Called while swinging
		public virtual void ExtraAIQuarterstaffParrying(Player player, OrchidGuardian guardian, Projectile projectile) { } // Called while parrying
		public virtual void ExtraAIQuarterstaffCharging(Player player, OrchidGuardian guardian, Projectile projectile) { } // Called while chargins
		public virtual void ExtraAIQuarterstaffCounterattacking(Player player, OrchidGuardian guardian, Projectile projectile) { } // Called while counterattacking
		public virtual void ExtraAIQuarterstaffIdle(Player player, OrchidGuardian guardian, Projectile projectile) { } // Called while idle
		public virtual void PostDrawQuarterstaff(SpriteBatch spriteBatch, Projectile projectile, Player player, Color lightColor) { } // Called after the item is done being drawn
		public virtual bool PreDrawQuarterstaff(SpriteBatch spriteBatch, Projectile projectile, Player player, ref Color lightColor) { return true; } // Return false to prevent normal draw code
		public virtual Color GetQuarterstaffGlowmaskColor(Player player, OrchidGuardian guardian, Projectile projectile, Color lightColor) => Color.White;
		public virtual Color GetColor(bool offHand) => Color.White;

		public virtual void SafeHoldItem(Player player) { }

		public int ParryDuration = 60; // Parry duration in ticks
		public int SlamStacks; // Stam Stacks given by the item
		public int GuardStacks; // Block Stacks given by the item
		public float JabSpeed = 1f; // Jab speed multiplier
		public float SwingSpeed = 1f; // swing speed multiplier
		public float CounterSpeed = 1f; // spin speed multiplier
		public float JabDamage = 0.5f; // Jab damage multiplier
		public float SwingDamage = 1.5f; // swing damage multiplier
		public float CounterDamage = 1f; // spin damage multiplier
		public float CounterHits = 3; // number of local iframe resets during a counterattack - use scarsely
		public float SwingKnockback = 1.5f; // swing knockback multiplier
		public float JabKnockback = 1f; // spin knockback multiplier
		public float CounterKnockback = 1f; // counter knockback multiplier
		/// <summary>Indexes into <c>GuardianQuarterstaffAnchor.DoAnimStyle</c> to get jab animation AI.</summary>
		/// <inheritdoc cref="GuardianQuarterstaffAnchor.DoAnimStyle(int, float, SoundStyle, bool)" select="remarks" />
		public int JabStyle = 0;
		/// <summary>Indexes into <c>GuardianQuarterstaffAnchor.DoAnimStyle</c> to get swing animation AI.</summary>
		/// <inheritdoc cref="GuardianQuarterstaffAnchor.DoAnimStyle(int, float, SoundStyle, bool)" select="remarks" />
		public int SwingStyle = 1;
		/// <summary>Indexes into <c>GuardianQuarterstaffAnchor.DoAnimStyle</c> to get counterattack animation AI.</summary>
		/// <inheritdoc cref="GuardianQuarterstaffAnchor.DoAnimStyle(int, float, SoundStyle, bool)" select="remarks" />
		public int CounterStyle = 4;
		//public bool SingleSwing = false; // allows a special swing behaviour
		/// <summary>Multiplier for the amount of bonus charge gained from hitting with a jab.</summary>
		public float JabChargeGain = 1;
		/// <summary>
		/// The amount of animation frames of the quarterstaff texture.
		/// <br/> The animation frame must be set manually using <c>GuardianQuarterstaffAnchor.QuarterstaffAnimFrame</c>.
		/// </summary>
		public int QuarterstaffFrames = 1;

		public sealed override void SetDefaults()
		{
			Item.DamageType = ModContent.GetInstance<GuardianDamageClass>();
			Item.noMelee = true;
			Item.autoReuse = true;
			Item.maxStack = 1;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.DD2_MonkStaffGroundMiss;
			Item.useStyle = ItemUseStyleID.Thrust;
			Item.useTime = 30;
			Item.knockBack = 5f;
			
			QuarterstaffFrames = 1;

			OrchidGlobalItemPerEntity orchidItem = Item.GetGlobalItem<OrchidGlobalItemPerEntity>();
			orchidItem.guardianWeapon = true;

			SafeSetDefaults();
			Item.useAnimation = Item.useTime;
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public sealed override void OnParry(Player player, OrchidGuardian guardian, Entity aggressor, Projectile anchor)
		{
			anchor.ai[2] = -40f;
			anchor.ai[1] = Vector2.Normalize(Main.MouseWorld - player.MountedCenter).ToRotation() - MathHelper.PiOver2;			
			(anchor.ModProjectile as GuardianQuarterstaffAnchor).NeedNetUpdate = true;
			OnParryQuarterstaff(player, guardian, aggressor, anchor);
		}

		public override bool WeaponPrefix() => true;

		public override bool CanUseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer && !player.cursed)
			{
				var projectileType = ModContent.ProjectileType<GuardianQuarterstaffAnchor>();
				if (player.ownedProjectileCounts[projectileType] > 0)
				{

					var guardian = player.GetModPlayer<OrchidGuardian>();
					var proj = Main.projectile.First(i => i.active && i.owner == player.whoAmI && i.type == projectileType);
					if (proj != null && proj.ModProjectile is GuardianQuarterstaffAnchor anchor)
					{
						bool shouldBlock = Main.mouseRight && Main.mouseRightRelease;
						bool shouldCharge = Main.mouseLeft;
						
						if (ModContent.GetInstance<OrchidClientConfig>().GuardianSwapGauntletImputs)
						{
							shouldBlock = Main.mouseLeft && Main.mouseLeftRelease;
							shouldCharge = Main.mouseRight;
						}

						if (shouldBlock && !shouldCharge && guardian.UseGuard(1, true) && proj.ai[0] <= 0f && proj.ai[2] == 0f)
						{
							player.immuneTime = 0;
							player.immune = false;
							guardian.modPlayer.PlayerImmunity = 0;
							guardian.GuardianItemCharge = 0f;
							guardian.UseGuard(1);
							proj.ai[0] = 0f;
							proj.ai[2] = ParryDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianParryDuration;
							anchor.NeedNetUpdate = true;
							SoundEngine.PlaySound(SoundID.Item37, player.Center);
						}

						if (shouldCharge && guardian.GuardianItemCharge == 0f && proj.ai[0] == 0f && proj.ai[2] >= -10f && (proj.ai[2] <= 0 || guardian.GuardianStaffRocket == 0))
						{
							proj.ai[0] = 1f;
							proj.ai[2] = 0f;
							anchor.NeedNetUpdate = true;
							guardian.GuardianItemCharge++;
							SoundEngine.PlaySound(SoundID.Item7, player.Center);
						}
					}
				}
			}
			return false;
		}

		public sealed override void HoldItem(Player player)
		{
			var projectileType = ModContent.ProjectileType<GuardianQuarterstaffAnchor>();
			var guardian = player.GetModPlayer<OrchidGuardian>();
			guardian.GuardianDisplayUI = 300;

			if (player.ownedProjectileCounts[projectileType] != 1)
			{
				foreach (Projectile projectile in Main.projectile)
				{
					if (projectile.active && projectile.owner == player.whoAmI && projectile.type == projectileType)
					{
						projectile.Kill();
					}
				}

				var index = Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center.X, player.Center.Y, 0f, 0f, projectileType, 0, 0f, player.whoAmI);

				var proj = Main.projectile[index];
				if (proj.ModProjectile is not GuardianQuarterstaffAnchor quarterstaff)
				{
					proj.Kill();
				}
				else
				{
					quarterstaff.OnChangeSelectedItem(player);
				}
			}
			else
			{
				var proj = Main.projectile.First(i => i.active && i.owner == player.whoAmI && i.type == projectileType);
				if (proj != null && proj.ModProjectile is GuardianQuarterstaffAnchor quarterstaff)
				{
					if (quarterstaff.SelectedItem != player.selectedItem)
					{
						quarterstaff.OnChangeSelectedItem(player);
					}
				}
			}
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

			tooltips.Insert(index + 3, new TooltipLine(Mod, "Swing", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.ChargeToSwing", click))
			{
				OverrideColor = new Color(175, 255, 175)
			});

			if (GuardStacks > 0 || SlamStacks > 0)
			{
				string TooltipToGet = ModContent.GetInstance<OrchidMod>().GetLocalizationKey("Misc.GuardianGrants");
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
