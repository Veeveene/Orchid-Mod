using OrchidMod.Common.ModSystems;
using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Accessories
{
	public class ThoriumLifeQuartzCenser : OrchidModGuardianEquipable
	{
		public override void SafeSetDefaults()
		{
			Item.width = 26;
			Item.height = 30;
			Item.value = Item.sellPrice(0, 0, 15, 0);
			Item.rare = ItemRarityID.Blue;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			OrchidGuardian modPlayer = Guardian(player);
			modPlayer.GuardianThoriumCenser = true;
		}

		public override void AddRecipes()
		{
			if (OrchidMod.ThoriumMod != null)
			{
				var recipe = CreateRecipe();
				recipe.AddTile(TileID.Anvils);
				recipe.AddRecipeGroup(OrchidRecipes.AnySilverBarGroup, 6);
				recipe.AddIngredient(OrchidMod.ThoriumMod, "LifeQuartz", 4);
				recipe.AddIngredient(OrchidMod.ThoriumMod, "SmoothCoal", 4);
				recipe.Register();
			}
		}
	}
}