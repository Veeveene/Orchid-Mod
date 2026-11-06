using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using OrchidMod.Content.Guardian.Weapons.Shields;
using System;
using OrchidMod.Utilities;

namespace OrchidMod.Content.Guardian.Buffs
{
	public class GuardianThoriumBronzeShieldBuff : ModBuff
	{
		public static LocalizedText Tooltip { get; private set; }
		public static LocalizedText AltTip { get; private set; }
		public override void SetStaticDefaults()
		{
			Tooltip = this.GetLocalization("Description");
			AltTip = this.GetLocalization("AltDescription");
			Main.buffNoTimeDisplay[Type] = false;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			OrchidGuardian guardian = player.Guardian();
			guardian.GuardianBronzeShieldBuff = true;
			if (player.HeldItem.type == ModContent.ItemType<ThoriumBronzeShield>())
				player.GetDamage<GuardianDamageClass>() *= 1 + guardian.GuardianBronzeShieldDamage;
			if (player.whoAmI == Main.myPlayer)
			{
				if (player.buffTime[buffIndex] > 58) Main.buffNoTimeDisplay[Type] = true;
				else Main.buffNoTimeDisplay[Type] = false;
			}
		}

		public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
		{
			OrchidGuardian guardian = Main.player[Main.myPlayer].GetModPlayer<OrchidGuardian>();
			tip = Tooltip.Format((int)(guardian.GuardianBronzeShieldDamage * 100f)) + (Main.buffNoTimeDisplay[Type] ? AltTip : String.Empty);
		}
	}
}