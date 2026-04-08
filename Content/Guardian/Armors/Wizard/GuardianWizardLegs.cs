using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Armors.Wizard
{
	[AutoloadEquip(EquipType.Legs)]
	public class GuardianWizardLegs : OrchidModGuardianEquipable
	{
		public override void SafeSetDefaults()
		{
			Item.width = 26;
			Item.height = 16;
			Item.value = Item.buyPrice(0, 40);
			Item.rare = ItemRarityID.LightPurple;
			Item.defense = 18;
			Item.CountsAsClass(DamageClass.Magic);
		}

		public override void UpdateEquip(Player player)
		{
			OrchidGuardian modPlayer = player.GetModPlayer<OrchidGuardian>();
			modPlayer.GuardianGuardMax++;
			modPlayer.GuardianSlamMax++;
			player.manaRegenBonus += 10;
			player.manaRegenDelayBonus += 0.5f;
			player.statManaMax2 += 40;
			player.aggro += 250;
		}
	}
}
