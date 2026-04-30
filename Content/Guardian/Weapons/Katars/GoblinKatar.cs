using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Katars
{
	public class GoblinKatar : OrchidModGuardianKatar
	{
		public override void SafeSetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.knockBack = 3f;
			Item.damage = 58;
			Item.value = Item.sellPrice(0, 0, 8, 40);
			Item.rare = ItemRarityID.White;
			Item.useTime = 31;
			JabVelocity = 15f;
			ParryDuration = 10;
		}

		public override Color GetColor(bool offHand)
		{
			return new Color(209, 216, 217);
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.Anvils);
			recipe.AddIngredient(ItemID.SilverBar, 8);
			recipe.Register();
		}
	}
}
