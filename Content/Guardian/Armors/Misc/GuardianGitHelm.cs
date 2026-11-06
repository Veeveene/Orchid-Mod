using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Armors.Misc
{
	[AutoloadEquip(EquipType.Head)]
	public class GuardianGitHelm: OrchidModGuardianEquipable
	{
		public static LocalizedText SetBonusText { get; private set; }

		public override void SetStaticDefaults()
		{
			SetBonusText = this.GetLocalization("SetBonus");
		}

		public override void SafeSetDefaults()
		{
			Item.width = 24;
			Item.height = 22;
			Item.value = Item.sellPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.Green;
			Item.defense = 8;
		}

		public override void UpdateEquip(Player player)
		{
			OrchidGuardian modPlayer = Guardian(player);
			modPlayer.GuardianSlamMax += 1;
			modPlayer.GuardianGuardMax += 1;
			player.aggro += 250;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			Mod ThoriumMod = OrchidMod.ThoriumMod;
			if (ThoriumMod != null)
			{
				if (body.type == ThoriumMod.Find<ModItem>("ThoriumMail").Type && legs.type == ThoriumMod.Find<ModItem>("ThoriumGreaves").Type)
				{
					return true;
				}
			}

			return (body.type == ItemID.CopperChainmail && legs.type == ItemID.CopperGreaves)
				|| (body.type == ItemID.TinChainmail && legs.type == ItemID.TinGreaves)
				|| (body.type == ItemID.IronChainmail && legs.type == ItemID.IronGreaves)
				|| (body.type == ItemID.LeadChainmail && legs.type == ItemID.LeadGreaves)
				|| (body.type == ItemID.SilverChainmail && legs.type == ItemID.SilverGreaves)
				|| (body.type == ItemID.TungstenChainmail && legs.type == ItemID.TungstenGreaves)
				|| (body.type == ItemID.GoldChainmail && legs.type == ItemID.GoldGreaves)
				|| (body.type == ItemID.PlatinumChainmail && legs.type == ItemID.PlatinumGreaves);
		}

		public override void UpdateArmorSet(Player player)
		{
			OrchidGuardian modPlayer = Guardian(player);
			player.setBonus = SetBonusText.Value;
			modPlayer.GuardianGit = true;
		}
	}
}
