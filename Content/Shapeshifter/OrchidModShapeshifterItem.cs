using OrchidMod.Common;
using OrchidMod.Common.Attributes;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OrchidMod.Content.Shapeshifter
{
	[ClassTag(ClassTags.Shapeshifter)]
	public abstract class OrchidModShapeshifterItem : ModItem
	{
		/// <seealso cref="OrchidUtils.Shapeshifter(Player)"/>
		public static OrchidShapeshifter Shapeshifter(Player player = null) => player.Shapeshifter();
		
		public bool IsLocalPlayer(Player player) => player.whoAmI == Main.myPlayer;

		public virtual void SafeSetDefaults() { }

		public override void SetDefaults()
		{
			Item.DamageType = ModContent.GetInstance<ShapeshifterDamageClass>();
			Item.noMelee = true;
			Item.maxStack = 1;
			SafeSetDefaults();
		}

		protected override bool CloneNewInstances => true;

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			// TooltipLine tt = tooltips.FirstOrDefault(x => x.Name == "Damage" && x.Mod == "Terraria");
			// if (tt != null)
			// {
			// 	string[] splitText = tt.Text.Split(' ');
			// 	string damageValue = splitText.First();
			// 	tt.Text = damageValue + " " + Language.GetTextValue(Mod.GetLocalizationKey("DamageClasses.ShapeshifterDamageClass.DisplayName"));
			// }
		}
	}
}
