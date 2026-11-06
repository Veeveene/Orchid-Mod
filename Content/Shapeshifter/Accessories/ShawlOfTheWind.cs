using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Shapeshifter.Accessories
{
	public class ShawlOfTheWind : OrchidModShapeshifterItem
	{
		public override void SafeSetDefaults()
		{
			Item.width = 28;
			Item.height = 26;
			Item.value = Item.sellPrice(0, 1, 22, 50);
			Item.rare = ItemRarityID.Orange;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.moveSpeed += 0.12f;
			OrchidShapeshifter shapeshifter = Shapeshifter(player);
			shapeshifter.ShapeshifterShawlWind = true;
			if (shapeshifter.ShapeshifterHookDash < 11f)
			{
				shapeshifter.ShapeshifterHookDash = 11f;
			}
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddIngredient<ShawlFeather>();
			recipe.AddIngredient(ItemID.AnkletoftheWind);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}