using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	public class CrimsonQuarterstaff : OrchidModGuardianQuarterstaff
	{
		public override void SafeSetDefaults()
		{
			Item.width = 48;
			Item.height = 48;
			Item.value = Item.sellPrice(0, 1, 50, 0);
			Item.rare = ItemRarityID.Blue;
			Item.useTime = 30;
			ParryDuration = 60;
			Item.knockBack = 10f;
			Item.damage = 58;
			JabSpeed = 1.25f;
			JabKnockback = 1.25f;
			SwingStyle = 2;
			SwingSpeed = 1.25f;
			ChargedDamage = 2f;
			ChargedKnockback = 2f;
		}
	}
}
