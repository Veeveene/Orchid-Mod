using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Katars
{
	public class GoldChestKatar : OrchidModGuardianKatar
	{
		public override void SafeSetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.knockBack = 6f;
			Item.damage = 57;
			Item.value = Item.sellPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.Blue;
			Item.useTime = 30;
			JabVelocity = 16f;
			ParryDuration = 10;
			ChargedAttackDoT = 0.66666f;
			ChargedAttackDamage = 3f; // This makes the charged attack deal normal damage, but doubles the bleed damage
		}

		public override Color GetColor()
		{
			return new Color(186, 86, 86);
		}
	}
}
