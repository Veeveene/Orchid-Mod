using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common.Global.Items;
using OrchidMod.Utilities;
using System.Collections.Generic;
using System.Linq;
using OrchidMod.Common;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace OrchidMod.Content.Guardian
{
	public abstract class OrchidModGuardianStandard : OrchidModGuardianItem
	{
		public int SlamStacks; // Stam Stacks given by the item
		public int GuardStacks; // Block Stacks given by the item
		public int FlagOffset; // Number of diagonal pixels from the top-right of the sprite to the base of the flag
		public float AuraRange; // Flag effect range in tiles
		public float BaseSyncedValue; // Sometimes used to sync some behaviours in multiplayer;
		public bool AffectNearbyPlayers; // Flag has an effect on nearby players
		public bool AffectNearbyNPCs; // Flag has an effect on nearby npcs
		public int StandardDuration; // Effect duration in ticks

		public virtual string ShaftTexture => Texture + "_Shaft";
		public virtual string FlagUpTexture => Texture + "_FlagUp";
		public virtual string FlagQuarterTexture => Texture + "_FlagQuarter";
		public virtual string FlagTwoQuarterTexture => Texture + "_FlagTwoQuarter";
		public virtual string FlagEndTexture => Texture + "_FlagEnd";
		public virtual bool NearbyPlayerEffect(GuardianStandardStats standardStats, Player affectedPlayer, OrchidGuardian guardian, bool isLocalPlayer, bool reinforced) => false; // isLocalPlayer is true when this is ran on the client being affected. Do not change stats on the affectedPlayer it won't work, use standardStats - Should return true if the player was affected
		public virtual bool NearbyNPCEffect(Player player, OrchidGuardian guardian, NPC npc, bool isLocalPlayer, bool reinforced) => false; // isLocalPlayer is true when this is ran by the guardian with the flag active - Should return true if the npc was affected
		public virtual void OnCharge(Player player, OrchidGuardian guardian) { }
		public virtual void EffectSimple(Player player, OrchidGuardian guardian) { }
		public virtual void EffectUpgrade(Player player, OrchidGuardian guardian) { }
		public virtual void ExtraAIStandardHeld(GuardianStandardAnchor anchor, Projectile projectile, Player player, OrchidGuardian guardian) { }
		public virtual void ExtraAIStandardWorn(GuardianStandardAnchor anchor, Projectile projectile, Player player, OrchidGuardian guardian) { }
		public virtual void PostDrawStandard(SpriteBatch spriteBatch, Projectile projectile, Player player, Color lightColor) { }
		public virtual bool PreDrawStandard(SpriteBatch spriteBatch, Projectile projectile, Player player, ref Color lightColor) { return true; }
		/// <summary>Overrides the default flag drawing, but still renders the shaft. Return true to prevent the default flag from being drawn.</summary>
		public virtual bool DrawCustomFlag(SpriteBatch spriteBatch, Projectile projectile, Player player, Color lightColor, Vector2 drawPosition, float drawRotation) { return false; }
		public virtual Color GetColor() => Color.White;
		public virtual bool DrawAura(bool isPlayer, bool PlayerisOwner, bool isNPC, bool isOwner, bool isReinforced) => (isPlayer && !PlayerisOwner) || (isNPC && isOwner); // Whether or not the aura should be drawn. This should cover most cases.

		public virtual void SafeHoldItem(Player player) { }

		
		/// <summary>Draws extra UI elements on the GuardianUIState while held.</summary>
		public virtual void StandardPostDrawUI(SpriteBatch spriteBatch, Player player, ref Color lightColor, Projectile projectile) { }
		
		public override int AnchorType => ModContent.ProjectileType<GuardianStandardAnchor>();
		public sealed override void SetDefaults()
		{
			Item.DamageType = ModContent.GetInstance<GuardianDamageClass>();
			Item.noMelee = true;
			Item.autoReuse = true;
			Item.maxStack = 1;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item1;
			Item.useStyle = ItemUseStyleID.Thrust;
			Item.useTime = 30;
			Item.knockBack = 0f;
			Item.damage = 1;
			SlamStacks = 0;
			GuardStacks = 0;
			FlagOffset = 0;
			AuraRange = 0;
			StandardDuration = 1800; // 30 sec
			AffectNearbyPlayers = false;
			AffectNearbyNPCs = false;
			BaseSyncedValue = 0f;

			OrchidGlobalItemPerEntity orchidItem = Item.GetGlobalItem<OrchidGlobalItemPerEntity>();
			orchidItem.guardianWeapon = true;

			SafeSetDefaults();

			AuraRange *= 16f;
			Item.useAnimation = Item.useTime;
		}

		public override bool? PrefixChance(int pre, UnifiedRandom rand) => false;

		public override bool CanUseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer && !player.cursed)
			{
				if (player.ownedProjectileCounts[AnchorType] > 0)
				{
					var guardian = Guardian(player);
					var proj = Main.projectile.FirstOrDefault(i => i.active && i.owner == player.whoAmI && i.type == AnchorType);
					if (proj != null && proj.ModProjectile is GuardianStandardAnchor anchor && guardian.GuardianItemCharge == 0f)
					{
						proj.ai[0] = 1f;
						anchor.NeedNetUpdate = true;
						guardian.GuardianItemCharge++;
						SoundEngine.PlaySound(SoundID.Item7, player.Center);
					}
				}
			}
			return false;
		}

		public sealed override void HoldItem(Player player)
		{
			var guardian = Guardian(player);
			guardian.GuardianDisplayUI = 300;

			if (player.ownedProjectileCounts[AnchorType] != 1)
			{
				foreach (Projectile projectile in Main.projectile)
				{
					if (projectile.active && projectile.owner == player.whoAmI && projectile.type == AnchorType)
					{
						projectile.Kill();
					}
				}

				var proj = SpawnAnchor(player, out GuardianStandardAnchor standard);
				if (standard is not null)
					standard.OnChangeSelectedItem(player);
				else proj.Kill();
			}
			else
			{
				var proj = Main.projectile.FirstOrDefault(i => i.active && i.owner == player.whoAmI && i.type == AnchorType);
				if (proj != null && proj.ModProjectile is GuardianStandardAnchor standard)
				{
					if (standard.SelectedItem != player.selectedItem)
					{
						standard.OnChangeSelectedItem(player);
					}
				}
			}
			SafeHoldItem(player);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			int index = tooltips.FindIndex(ttip => ttip.Mod.Equals("Terraria") && ttip.Name.Equals("Knockback"));

			if (GuardStacks > 0 || SlamStacks > 0)
			{
				string TooltipToGet = Mod.GetLocalizationKey("Misc.GuardianGrants");
				if (GuardStacks> 0) TooltipToGet += "Guard";
				if (SlamStacks > 0) TooltipToGet += "Slam";
				if (GuardStacks == SlamStacks) TooltipToGet += "Same";

				tooltips.Insert(index + 1, new TooltipLine(Mod, "GuardianGrants", Language.GetTextValue(TooltipToGet, GuardStacks, SlamStacks))
				{
					OverrideColor = OrchidColors.GuardianTooltip
				});
			}

			/*if (SlamStacks > 0)
			{
				tooltips.Insert(index + 1, new TooltipLine(Mod, "ShieldSlams", "Grants " + this.SlamStacks + " slam" + (this.SlamStacks > 1 ? "s" : "") + " when fully charged")
				{
					OverrideColor = OrchidColors.GuardianTooltip
				});
			}*/

			index = tooltips.FindIndex(ttip => ttip.Mod.Equals("Terraria") && ttip.Name.Equals("Knockback"));
			tooltips.Insert(index + 1, new TooltipLine(Mod, "RuneDuration", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.BuffDuration", OrchidUtils.FramesToSeconds((int)(StandardDuration * Guardian().GuardianRuneTimer)))));

			tooltips.RemoveAll(x => x.Name == "Damage" && x.Mod == "Terraria");
			tooltips.RemoveAll(x => x.Name == "Knockback" && x.Mod == "Terraria");
			tooltips.RemoveAll(x => x.Name == "CritChance" && x.Mod == "Terraria");
		}
	}

	public class GuardianStandardStats()
	{
		public int lifeMax = 0;
		public int lifeRegen = 0;
		public int defense;
		public float moveSpeed = 0f;
		public float guardianDamage = 0f;
		public float allDamage = 0f;

		public void ApplyStats(Player player)
		{
			player.statLifeMax2 += lifeMax;
			player.statDefense += defense;
			player.moveSpeed += moveSpeed;
			player.GetDamage<GuardianDamageClass>() += guardianDamage;
			player.GetDamage(DamageClass.Generic) += allDamage;

			lifeMax = 0;
			defense = 0;
			moveSpeed = 0f;
			guardianDamage = 0f;
			allDamage = 0f;
		}

		public void ApplyLifeRegen(Player player)
		{
			player.lifeRegen += lifeRegen;

			lifeRegen = 0;
		}
	}
}
