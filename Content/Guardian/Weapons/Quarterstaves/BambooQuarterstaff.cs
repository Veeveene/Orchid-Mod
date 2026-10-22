using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	public class BambooQuarterstaff : OrchidModGuardianQuarterstaff
	{
		public override void SafeSetDefaults()
		{
			Item.width = 42;
			Item.height = 42;
			Item.value = Item.sellPrice(0, 0, 3, 50);
			Item.rare = ItemRarityID.White;
			Item.useTime = 20;
			ParryDuration = 40;
			Item.knockBack = 6f;
			Item.damage = 29;
			SwingSpeed = 1.3f;
			CounterSpeed = 1.3f;
			JabSpeed = 1.3f;
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.Sawmill);
			recipe.AddIngredient(ItemID.BambooBlock, 20);
			recipe.Register();
		}
	}
}
