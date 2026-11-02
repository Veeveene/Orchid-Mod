using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Armors.Meteorite
{
	[AutoloadEquip(EquipType.Body)]
	public class GuardianMeteoriteChest : OrchidModGuardianEquipable
	{
		public override void SafeSetDefaults()
		{
			Item.width = 30;
			Item.height = 20;
			Item.value = Item.sellPrice(0, 0, 80, 0);
			Item.rare = ItemRarityID.Blue;
			Item.defense = 10;
		}

		public override void UpdateEquip(Player player)
		{
			OrchidGuardian modPlayer = Guardian(player);
			modPlayer.GuardianSlamMax ++;
			player.GetDamage<GuardianDamageClass>() += 0.05f;
			player.aggro += 250;
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.MeteoriteBar, 20);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
