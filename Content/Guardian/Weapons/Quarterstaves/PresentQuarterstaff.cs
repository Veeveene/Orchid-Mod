using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	public class PresentQuarterstaff : OrchidModGuardianQuarterstaff
	{
		public override void SafeSetDefaults()
		{
			Item.width = 44;
			Item.height = 36;
			Item.value = Item.sellPrice(0, 0, 27, 0);
			Item.rare = ItemRarityID.Blue;
			Item.useTime = 37;
			ParryDuration = 70;
			Item.knockBack = 11f;
			Item.damage = 43;
		}
	}
}
