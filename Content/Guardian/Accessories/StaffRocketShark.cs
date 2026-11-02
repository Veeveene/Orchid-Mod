using System.Collections.Generic;
using Humanizer;
using OrchidMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Accessories;

/*
public abstract class StaffRocketShark<T>(int? variant = null) : StaffRocket(variant) where T : StaffRocket
{
	public override void SafeSetDefaults()
	{
		Item.width = 30;
		Item.height = 38;
		Item.value = Item.sellPrice(0, 2, 0, 0);
		Item.rare = ItemRarityID.Pink;
		Item.accessory = true;
		
	}
	
	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		base.UpdateAccessory(player, hideVisual);
		player.GetArmorPenetration(DamageClass.Generic) += 5;
	}
	
	public override void AddRecipes()
	{
		
		CreateRecipe()
			.AddIngredient<T>()
			.AddIngredient(ItemID.SharkToothNecklace)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		OrchidGuardian guardian = Guardian();
		TooltipLine tt = tooltips.Find(ttip => ttip.Mod.Equals("Terraria") && ttip.Name.Equals("Tooltip1"));
		if (tt != null) tt.Text = tt.Text.FormatWith(OrchidUtils.GetClickInfoTooltip(guardian.GauntletInputSwap, false)).FirstCharToUpper();
	}
}
*/

// public class StaffRocketSharkRed() : StaffRocketShark<StaffRocketRed>(1);
// public class StaffRocketSharkGreen() : StaffRocketShark<StaffRocketGreen>(2);
// public class StaffRocketSharkBlue() : StaffRocketShark<StaffRocketBlue>(3);
// public class StaffRocketSharkYellow() : StaffRocketShark<StaffRocketYellow>(4);