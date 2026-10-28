using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	public class CorruptionQuarterstaff : OrchidModGuardianQuarterstaff
	{
		public override void SafeSetDefaults()
		{
			Item.width = 46;
			Item.height = 48;
			Item.value = Item.sellPrice(0, 1, 50, 0);
			Item.rare = ItemRarityID.Blue;
			Item.useTime = 40;
			ParryDuration = 90;
			Item.knockBack = 8f;
			Item.damage = 55;
			CounterSpeed = 0.75f;
			CounterKnockback = 0.75f;
			CounterHits = 4;
			InvincibilityDuration = 60;
		}
	}
}
