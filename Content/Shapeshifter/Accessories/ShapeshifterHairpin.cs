using OrchidMod.Content.Guardian.Accessories;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Shapeshifter.Accessories
{
	public class ShapeshifterHairpin : OrchidModShapeshifterItem
	{
		public override void SafeSetDefaults()
		{
			Item.width = 28;
			Item.height = 26;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Green;
			Item.accessory = true;
			Item.vanity = true;
		}
		public override void UpdateVanity(Player player)
		{
			OrchidShapeshifter shapeshifter = Shapeshifter(player);
			shapeshifter.ShapeshifterHairpin = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			OrchidShapeshifter shapeshifter = Shapeshifter(player);
			shapeshifter.ShapeshifterHairpin = true;
		}
	}
}