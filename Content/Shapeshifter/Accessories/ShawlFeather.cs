using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Shapeshifter.Accessories
{
	public class ShawlFeather : OrchidModShapeshifterItem
	{
		public override void SafeSetDefaults()
		{
			Item.width = 28;
			Item.height = 24;
			Item.value = Item.sellPrice(0, 0, 70, 0);
			Item.rare = ItemRarityID.Green;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			OrchidShapeshifter shapeshifter = Shapeshifter(player);
			shapeshifter.ShapeshifterShawlFeather = true;
			if (shapeshifter.ShapeshifterHookDash < 10f)
			{
				shapeshifter.ShapeshifterHookDash = 10f;
			}
		}
	}
}