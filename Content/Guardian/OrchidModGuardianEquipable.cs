using OrchidMod.Common;
using OrchidMod.Common.Attributes;
using System.Collections.Generic;
using System.Linq;
using OrchidMod.Utilities;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian
{
	[ClassTag(ClassTags.Guardian)]
	public abstract class OrchidModGuardianEquipable : ModItem
	{
		public static OrchidGuardian Guardian(Player player = null) => player.Guardian();
		
		public virtual void SafeSetDefaults() { }

		public sealed override void SetDefaults()
		{
			Item.DamageType = ModContent.GetInstance<GuardianDamageClass>();
			SafeSetDefaults();
		}

		/// <summary> Called at the end of ModifyTooltips, allowing for further tooltip changes. </summary>
		public virtual void SafeModifyTooltips(List<TooltipLine> tooltips, OrchidGuardian guardian) {}
		
		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			TooltipLine tt = tooltips.FirstOrDefault(x => x.Name == "Damage" && x.Mod == "Terraria");
			if (tt != null)
			{
				string[] splitText = tt.Text.Split(' ');
				string damageValue = splitText.First();
				tt.Text = $"{damageValue} {Language.GetTextValue("Mods.OrchidMod.DamageClasses.GuardianDamageClass.DisplayName")}";
			}
			
			SafeModifyTooltips(tooltips, Guardian());
		}
	}
}