using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Katars
{
	public class PalmKatar : OrchidModGuardianKatar
	{
		public override void SafeSetDefaults()
		{
			Item.width = 30;
			Item.height = 30;
			Item.knockBack = 5f;
			Item.damage = 31;
			Item.value = Item.sellPrice(0, 0, 0, 20);
			Item.rare = ItemRarityID.White;
			Item.useTime = 25;
			JabVelocity = 15f;
			ParryDuration = 10;
			ParryDashSpeed = 16f;
			NoUpwardsParryDash = true;
		}

		public override Color GetColor()
		{
			return new Color(198, 170, 104);
		}

		public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				var recipe = CreateRecipe();
				recipe.AddTile(TileID.WorkBenches);
				recipe.AddIngredient(ItemID.PalmWood, 10);
				recipe.Register();
			}
		}
	}
}
