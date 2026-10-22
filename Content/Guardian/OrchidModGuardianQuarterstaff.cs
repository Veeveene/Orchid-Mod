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

		/// <summary>Item parry duration. Defaults to 60.</summary>
		public int ParryDuration;
		/// <summary>Slams granted by a Charged Attack hit. Should generally not be modified unless part of a specific weapon gimmick. Defaults to 0.</summary>
		public int SlamStacks;
		/// <summary>Guards granted by a Charged Attack hit. Should generally not be modified unless part of a specific weapon gimmick. Defaults to 2.</summary>
		public int GuardStacks;
		/// <summary>Jab speed multiplier. Defaults to 1f.</summary>
		public float JabSpeed;
		/// <summary>Charged attack speed multiplier. Defaults to 1f.</summary>
		public float SwingSpeed;
		/// <summary>Counter speed multiplier. Defaults to 1f.</summary>
		public float CounterSpeed;
		/// <summary>Jab damage multiplier. Defaults to 0.5f.</summary>
		public float JabDamage;
		/// <summary>Charged attack damage multiplier. Defaults to 1f.</summary>
		public float ChargedDamage;
		/// <summary>Counter damage multiplier. Defaults to 0.67f.</summary>
		public float CounterDamage;
		/// <summary>Number of local iframe resets during a counterattack, use scarsely. Defaults to 3.</summary>
		public float CounterHits;
		/// <summary>Charged attack knockback multiplier. Defaults to 1.5f.</summary>
		public float ChargedKnockback;
		/// <summary>Jab knockback multiplier. Defaults to 1f.</summary>
		public float JabKnockback;
		/// <summary>Counter knockback multiplier. Defaults to 1f.</summary>
		public float CounterKnockback;
		/// <summary>Indexes into <c>GuardianQuarterstaffAnchor.DoAnimStyle</c> to get jab animation AI. Defaults to 0.</summary>
		/// <inheritdoc cref="GuardianQuarterstaffAnchor.DoAnimStyle(int, float, SoundStyle, bool)" select="remarks" />
		public int JabStyle;
		/// <summary>Indexes into <c>GuardianQuarterstaffAnchor.DoAnimStyle</c> to get swing animation AI. Defaults to 1.</summary>
		/// <inheritdoc cref="GuardianQuarterstaffAnchor.DoAnimStyle(int, float, SoundStyle, bool)" select="remarks" />
		public int SwingStyle;
		/// <summary>Indexes into <c>GuardianQuarterstaffAnchor.DoAnimStyle</c> to get counterattack animation AI. Defaults to 4.</summary>
		/// <inheritdoc cref="GuardianQuarterstaffAnchor.DoAnimStyle(int, float, SoundStyle, bool)" select="remarks" />
		public int CounterStyle;
		//public bool SingleSwing = false; // allows a special swing behaviour
		/// <summary>Multiplier for the amount of bonus charge gained from hitting with a jab.</summary>
		public float JabChargeGain;
		/// <summary>
		/// The amount of animation frames of the quarterstaff texture.
		/// <br/> The animation frame must be set manually using <c>GuardianQuarterstaffAnchor.QuarterstaffAnimFrame</c>.
		/// </summary>
		public int QuarterstaffFrames;
		
		/// <summary>Draws extra UI elements on the GuardianUIState while held.</summary>
		public virtual void QuarterstaffPostDrawUI(SpriteBatch spriteBatch, Player player, ref Color lightColor, Projectile projectile) { }

		public override int? AnchorType => ModContent.ProjectileType<GuardianQuarterstaffAnchor>();
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
			CounterKnockback = 1f;
			JabKnockback = 1f;
			ChargedKnockback = 1.5f;
			CounterHits = 3;
			CounterDamage = 0.67f;
			ChargedDamage = 1f;
			JabDamage = 0.5f;
			CounterSpeed = 1f;
			SwingSpeed = 1f;
			JabSpeed = 1f;
			ParryDuration = 60;
			SlamStacks = 0;
			GuardStacks = 2;

			JabStyle = 0;
			SwingStyle = 1;
			CounterStyle = 4;
			JabChargeGain = 1;

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

						if (shouldBlock && !shouldCharge && guardian.UseGuard(1, true, warn: true) && proj.ai[0] <= 0f && proj.ai[2] == 0f)
						{
							player.immuneTime = 0;
							player.immune = false;
							guardian.modPlayer.PlayerImmunity = 0;
							guardian.GuardianItemCharge = 0f;
							guardian.UseGuard(1);
							proj.ai[0] = 0f;
							proj.ai[2] = ParryDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianParryDuration;
							anchor.NeedNetUpdate = true;
							SoundEngine.PlaySound(SoundID.Item37.WithPitchOffset(Main.rand.NextFloat(0.4f, 0.6f)), player.Center);
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

		/// <remarks> Quarterstaves use this by default to set their parry action as <c>Offense</c> and <c>Counter</c>. When overriding this, set <c>Offense</c> and <c>Counter</c> if <c>Defense</c> is set unless intending to replace default quarterstaff counterattack functionality. </remarks>
		/// <inheritdoc cref="OrchidModGuardianItem.ModifyAttackInfo(ref GuardianAttackInfo)"/>
		public override bool ModifyAttackInfo(ref GuardianAttackInfo info)
		{
			if (info.Defense) info = AttackID.QuarterstaffCounter;
			return true;
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
