using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Shapeshifter.Accessories
{
	public class GoblinDagger : OrchidModShapeshifterItem
	{
		public override void SafeSetDefaults()
		{
			Item.width = 30;
			Item.height = 34;
			Item.value = Item.sellPrice(0, 0, 15, 0);
			Item.rare = ItemRarityID.Blue;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			OrchidShapeshifter shapeshifter = Shapeshifter(player);

			if (shapeshifter.ShapeshifterPredatorBleedPotency < 2)
			{
				shapeshifter.ShapeshifterPredatorBleedMaxStacks = 15;
				shapeshifter.ShapeshifterPredatorBleedPotency = 2;
			}
		}
	}
}