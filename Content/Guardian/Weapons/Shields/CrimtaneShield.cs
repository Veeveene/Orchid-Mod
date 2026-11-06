using OrchidMod.Common.ModObjects;
using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	public class CrimtaneShield : OrchidModGuardianShield
	{

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 0, 27, 0);
			Item.width = 28;
			Item.height = 38;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item1;
			Item.knockBack = 8f;
			Item.damage = 61;
			Item.rare = ItemRarityID.Blue;
			Item.useTime = 32;
			distance = 45f;
			slamDistance = 65f;
			blockDuration = 110;
		}

		public override void Protect(Player player, OrchidGuardian guardian, Projectile shield)
		{
			player.GetModPlayer<OrchidPlayer>().TryHeal(10);
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.Anvils);
			recipe.AddIngredient(ItemID.CrimtaneBar, 10);
			recipe.Register();
		}
	}
}
