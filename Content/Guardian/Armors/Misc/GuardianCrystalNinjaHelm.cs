using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Armors.Misc
{
	[AutoloadEquip(EquipType.Head)]
	public class GuardianCrystalNinjaHelm: OrchidModGuardianEquipable
	{
		//public static LocalizedText SetBonusText { get; private set; }

		public override void SetStaticDefaults()
		{
			//SetBonusText = this.GetLocalization("SetBonus");
			ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
		}

		public override void SafeSetDefaults()
		{
			Item.width = 30;
			Item.height = 26;
			Item.value = Item.sellPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.Pink;
			Item.defense = 21;
		}

		public override void UpdateEquip(Player player)
		{
			OrchidGuardian modPlayer = Guardian(player);
			modPlayer.GuardianCrystalNinja = true;
			modPlayer.GuardianSlamMax += 1;
			modPlayer.GuardianGuardMax += 1;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ItemID.CrystalNinjaChestplate && legs.type == ItemID.CrystalNinjaLeggings;
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = Language.GetTextValue("ArmorSetBonus.CrystalNinja");
			player.GetDamage<GenericDamageClass>() += 0.1f;
			player.GetCritChance<GenericDamageClass>() += 10;
			player.dashType = 5;
		}
	}
}
