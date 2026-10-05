using OrchidMod.Content.Guardian.Misc;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.General.Armor.Vanity
{
	[AutoloadEquip(EquipType.Legs)]
	public class EmpressPlateLegs : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 22;
			Item.height = 14;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Blue;
			Item.vanity = true;
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddIngredient<GuardianEmpressMaterial>(10);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.AddCondition(Condition.InGraveyard);
			recipe.Register();
		}
	}
}
