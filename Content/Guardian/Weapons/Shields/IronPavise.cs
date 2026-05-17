using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	public class IronPavise : OrchidModGuardianShield
	{

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 0, 3, 40);
			Item.width = 28;
			Item.height = 34;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item1;
			Item.knockBack = 9f;
			Item.damage = 52;
			Item.rare = ItemRarityID.White;
			Item.useTime = 24;
			distance = 30f;
			slamDistance = 65f;
			blockDuration = 90;
			parryRotation = 50f;
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.Anvils);
			recipe.AddIngredient(ItemID.IronBar, 8);
			recipe.AddRecipeGroup(RecipeGroupID.Wood, 6);
			recipe.Register();
		}
	}
}
