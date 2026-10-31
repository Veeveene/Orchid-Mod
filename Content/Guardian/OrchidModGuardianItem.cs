using System;
using OrchidMod.Common;
using OrchidMod.Common.Attributes;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian
{
	[ClassTag(ClassTags.Guardian)]
	public abstract class OrchidModGuardianItem : ModItem
	{
		protected Projectile SpawnAnchor<T>(Player player, out T anchor, int damage = 0, float knockback = 0) where T : OrchidModGuardianAnchor
		{
			Projectile projectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<T>(), damage, knockback, player.whoAmI);
			anchor = projectile.ModProjectile as T;
			return projectile;
		}
		
		public bool IsLocalPlayer(Player player) => player.whoAmI == Main.myPlayer;

		/// <summary>Determines the multiplier that applies to <see cref="OrchidGuardian.GuardianChargeSpeed"/>) for this item.</summary>
		public virtual float BonusChargeRateMultiplier => 1f;
		
		/// <summary>The relevant anchor type this item spawns, if not -1.</summary>
		public virtual int AnchorType => -1;
		/// <summary> Called when an anchor spawned by this item calls <c>OrchidGuardian.OnAttack</c> to trigger attack-related effects like set bonuses or accessories. Modify the <c>GuardianAttackInfo</c> reference to change what attack-related effects it triggers. <c>GuardianAttackInfo.Counter</c> will automatically be set to <c>true</c> if <c>GuardianAttackInfo.Slam</c> is <c>true</c> and the player meets the conditions for a counterattack, and if <c>GuardianAttackInfo.Counter</c> is <c>true</c> after this function, <c>OrchidGuardian.GuardianCounterTime</c> will be reset. Return <c>false</c> to prevent triggering any effects. Returns <c>true</c> by default. </summary>
		public virtual bool ModifyAttackInfo(ref GuardianAttackInfo info) => true;

		public override void SetStaticDefaults() => SafeSetStaticDefaults();

		public virtual void SafeSetStaticDefaults() { }
		
		public virtual void SafeSetDefaults() { }

		public override void SetDefaults()
		{
			Item.DamageType = ModContent.GetInstance<GuardianDamageClass>();
			Item.noMelee = true;
			Item.maxStack = 1;
			SafeSetDefaults();
		}

		protected override bool CloneNewInstances => true;

		public override bool CanUseItem(Player player)
		{
			//OrchidPlayer modPlayer = player.GetModPlayer<OrchidPlayer>();
			return base.CanUseItem(player);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			// TooltipLine tt = tooltips.FirstOrDefault(x => x.Name == "Damage" && x.Mod == "Terraria");
			// if (tt != null)
			// {
			// 	string[] splitText = tt.Text.Split(' ');
			// 	string damageValue = splitText.First();
			// 	tt.Text = damageValue + " " + Language.GetTextValue(Mod.GetLocalizationKey("DamageClasses.GuardianDamageClass.DisplayName"));
			// }

			int index = tooltips.FindIndex(x => x.Name == "Speed" && x.Mod == "Terraria");
			if (index != -1 && Math.Abs(1 - BonusChargeRateMultiplier) > 0.01f)
			{
				string key = Mod.GetLocalizationKey("UI.GuardianItem.") + (BonusChargeRateMultiplier <= 0 ? "No" : "Special") + "ChargeScaling";
				tooltips.Insert(index + 1, new TooltipLine(Mod, "ChargeSpeedScaling", Language.GetTextValue(key, 100 *  BonusChargeRateMultiplier)));
			}
		}
	}
}
