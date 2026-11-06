using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Accessories
{
	public class HeavyChain : OrchidModGuardianEquipable
	{
		public override void SafeSetDefaults()
		{
			Item.width = 28;
			Item.height = 30;
			Item.value = Item.sellPrice(0, 0, 2, 0);
			Item.rare = ItemRarityID.Blue;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			OrchidGuardian modPlayer = Guardian(player);
			if (modPlayer.GuardianChain < 32f)
			{ // Overrides any "worse" chain with this one, and uses its texture
				modPlayer.GuardianChain = 32f;
				modPlayer.GuardianChainTexture = Texture + "_Chain";
			}
		}


		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Chain, 12);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}