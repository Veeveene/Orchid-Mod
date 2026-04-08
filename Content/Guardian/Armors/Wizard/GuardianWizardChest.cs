using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Armors.Wizard
{
	[AutoloadEquip(EquipType.Body)]
	public class GuardianWizardChest : OrchidModGuardianEquipable
	{
		public override void SafeSetDefaults()
		{
			Item.width = 30;
			Item.height = 24;
			Item.value = Item.buyPrice(0, 80);
			Item.rare = ItemRarityID.LightPurple;
			Item.defense = 24;
			Item.CountsAsClass(DamageClass.Magic);
		}

		public override void UpdateEquip(Player player)
		{
			OrchidGuardian modPlayer = player.GetModPlayer<OrchidGuardian>();
			modPlayer.GuardianGuardMax ++;
			modPlayer.GuardianSlamMax ++;
			player.GetDamage(DamageClass.Magic) += 0.10f;
			player.GetDamage<GuardianDamageClass>() += 0.10f;
			player.statManaMax2 += 60;
			player.aggro += 250;
		}
	}
}
