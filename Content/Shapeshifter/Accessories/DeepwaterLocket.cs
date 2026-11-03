using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Shapeshifter.Accessories
{
	public class DeepwaterLocket : OrchidModShapeshifterItem
	{
		public override void SafeSetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.value = Item.sellPrice(0, 0, 45, 0);
			Item.rare = ItemRarityID.Green;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			OrchidShapeshifter shapeshifter = Shapeshifter(player);
			shapeshifter.ShapeshifterSageDamageOnHit = true;
		}
	}
}