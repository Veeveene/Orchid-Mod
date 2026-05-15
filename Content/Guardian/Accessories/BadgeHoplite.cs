using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Accessories
{
	public class BadgeHoplite : OrchidModGuardianEquipable
	{
		public override void SafeSetDefaults()
		{
			Item.width = 34;
			Item.height = 28;
			Item.value = Item.sellPrice(0, 0, 30, 0);
			Item.rare = ItemRarityID.Blue;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			OrchidGuardian modPlayer = player.GetModPlayer<OrchidGuardian>();
			modPlayer.GuardianBadgeHoplite = true;
		}
	}
}