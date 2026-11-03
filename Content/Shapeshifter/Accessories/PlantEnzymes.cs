using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Shapeshifter.Accessories
{
	public class PlantEnzymes : OrchidModShapeshifterItem
	{
		public override void SafeSetDefaults()
		{
			Item.width = 30;
			Item.height = 22;
			Item.value = Item.sellPrice(0, 0, 35, 0);
			Item.rare = ItemRarityID.Green;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			OrchidShapeshifter shapeshifter = Shapeshifter(player);
			shapeshifter.ShapeshifterHealingBonus += 0.5f;
			player.aggro += 250;
		}
	}
}