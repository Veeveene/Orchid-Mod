using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Accessories
{
	public class SturdySlab : OrchidModGuardianEquipable
	{
		public override void SafeSetDefaults()
		{
			Item.width = 24;
			Item.height = 28;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Blue;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			OrchidGuardian modPlayer = Guardian(player);
			modPlayer.GuardianBlockDuration += 0.25f;
			modPlayer.GuardianParryDuration += 0.25f;
		}

		public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				CreateRecipe()
				.AddTile(TileID.Anvils)
				.AddIngredient(thoriumMod, "GraniteEnergyCore", 10)
				.AddDecraftCondition(Condition.DownedSkeletron)
				.Register();
			}
		}
	}
}