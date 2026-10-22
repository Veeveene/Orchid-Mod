using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	public class BorealQuarterstaff : OrchidModGuardianQuarterstaff
	{
		public override void SafeSetDefaults()
		{
			Item.width = 38;
			Item.height = 38;
			Item.value = Item.sellPrice(0, 0, 0, 20);
			Item.rare = ItemRarityID.White;
			Item.useTime = 35;
			ParryDuration = 48;
			Item.knockBack = 8f;
			Item.damage = 26;
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.WorkBenches);
			recipe.AddIngredient(ItemID.BorealWood, 10);
			recipe.Register();
		}
	}
}
