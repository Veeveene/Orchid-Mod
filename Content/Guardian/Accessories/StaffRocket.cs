using System.Collections.Generic;
using Humanizer;
using OrchidMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Accessories;

public abstract class StaffRocket(int? variant = null) : OrchidModGuardianEquipable
{
	/// <summary>Determines the type of rocket </summary>
	protected readonly int Variant = variant ?? 1;
	
	public override void SafeSetDefaults()
	{
		Item.width = 28;
		Item.height = 32;
		Item.value = Item.sellPrice(0, 1, 0, 0);
		Item.rare = ItemRarityID.LightRed;
		Item.accessory = true;
	}
	
	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		OrchidGuardian modPlayer = Guardian(player);
		modPlayer.GuardianStaffRocket = Variant;
	}
	
	public override void AddRecipes()
	{
		int rocketType = Variant switch
		{
			2 => ItemID.GreenRocket,
			3 => ItemID.BlueRocket,
			4 => ItemID.YellowRocket,
			_ => ItemID.RedRocket
		};
		CreateRecipe()
		.AddIngredient(rocketType, 10)
		.AddRecipeGroup(RecipeGroupID.IronBar, 5)
		.AddTile(TileID.Anvils)
		.Register();
	}

	public override void SafeModifyTooltips(List<TooltipLine> tooltips, OrchidGuardian guardian)
	{
		TooltipLine tt = tooltips.Find(ttip => ttip.Mod.Equals("Terraria") && ttip.Name.Equals("Tooltip0"));
		if (tt != null) tt.Text = tt.Text.FormatWith(OrchidUtils.GetClickInfoTooltip(guardian.GauntletInputSwap, false)).FirstCharToUpper();
	}
}
	public class StaffRocketRed() : StaffRocket(1);
	public class StaffRocketGreen() : StaffRocket(2);
	public class StaffRocketBlue() : StaffRocket(3);
	public class StaffRocketYellow() : StaffRocket(4);
