using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Shapeshifter.Accessories
{
	public class ShawlPhoenix : OrchidModShapeshifterItem
	{
		public override void SafeSetDefaults()
		{
			Item.width = 28;
			Item.height = 26;
			Item.value = Item.sellPrice(0, 5, 30, 0);
			Item.rare = ItemRarityID.LightPurple;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.lavaMax += 420; // Lava charm ADDS 7 seconds of lava immunity, it doesn't override it 
			player.lavaRose = true;
			player.fireWalk = true; // obsi skull effect
			player.moveSpeed += 0.12f;
			OrchidShapeshifter shapeshifter = Shapeshifter(player);
			shapeshifter.ShapeshifterShawlPhoenix = true;
			if (shapeshifter.ShapeshifterHookDash < 12f)
			{
				shapeshifter.ShapeshifterHookDash = 12f;
			}
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddIngredient<ShawlOfTheWind>();
			recipe.AddIngredient(ItemID.MoltenSkullRose);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}