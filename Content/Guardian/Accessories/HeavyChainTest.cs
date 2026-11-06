using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Accessories
{
	public class HeavyChainTest : OrchidModGuardianEquipable
	{
		public override void SafeSetDefaults()
		{
			Item.width = 28;
			Item.height = 30;
			Item.value = Item.sellPrice(0, 0, 2, 0);
			Item.rare = ItemRarityID.White;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			OrchidGuardian modPlayer = Guardian(player);
			if (modPlayer.GuardianChain < 64f)
			{ // Overrides any "worse" chain with this one, and uses its texture
				modPlayer.GuardianChain = 64f;
				modPlayer.GuardianChainTexture = Texture + "_Chain";
			}
		}
	}
}