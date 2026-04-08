using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Armors.Wizard
{
	[AutoloadEquip(EquipType.Head)]
	public class GuardianWizardHead : OrchidModGuardianEquipable
	{
		public static LocalizedText SetBonusText { get; private set; }

		public override void SetStaticDefaults()
		{
			SetBonusText = this.GetLocalization("SetBonus");
		}

		public override void SafeSetDefaults()
		{
			Item.width = 30;
			Item.height = 28;
			Item.value = Item.buyPrice(0, 60);
			Item.rare = ItemRarityID.LightPurple;
			Item.defense = 14;
			Item.CountsAsClass(DamageClass.Magic);
		}

		public override void UpdateEquip(Player player)
		{
			OrchidGuardian modPlayer = player.GetModPlayer<OrchidGuardian>();
			modPlayer.GuardianGuardMax++;
			modPlayer.GuardianSlamMax++;
			modPlayer.GuardianGuardRecharge += 0.5f;
			modPlayer.GuardianSlamRecharge += 0.5f;
			player.statManaMax2 += 40;
			player.aggro += 250;
		}

		
		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ModContent.ItemType<GuardianWizardChest>() && legs.type == ModContent.ItemType<GuardianWizardLegs>();
		}

		public override void UpdateArmorSet(Player player)
		{
			OrchidGuardian modPlayer = player.GetModPlayer<OrchidGuardian>();
			player.setBonus = SetBonusText.Value;
			player.manaCost -= 0.1f;
			modPlayer.GuardianMeteorite = true;
		}
	}
}
