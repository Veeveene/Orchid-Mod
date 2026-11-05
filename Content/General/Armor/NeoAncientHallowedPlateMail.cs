using OrchidMod.Content.Guardian;
using OrchidMod.Content.Guardian.Armors.OreHelms;
using OrchidMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OrchidMod.Content.General.Armor
{
	[AutoloadEquip(EquipType.Body)]
	public class NeoAncientHallowedPlateMail : ModItem
	{
		public static LocalizedText[] SetBonusText { get; private set; }
		private static int GuardianHelmID;
		private static int GuardianAncientHelmID;

		private static int ThoriumGuiseID = -1;
		private static int ThoriumAncientGuiseID = -1;
		private static int ThoriumChapeauID = -1;
		private static int ThoriumAncientChapeauID = -1;
		private static int ThoriumCowlID = -1;
		private static int ThoriumAncientCowlID = -1;
		
		public override void SetStaticDefaults()
		{
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.AncientHallowedPlateMail;
			ItemID.Sets.ShimmerTransformToItem[ItemID.HallowedPlateMail] = Type;
			ArmorIDs.Body.Sets.IncludedCapeBack[Item.bodySlot] = ArmorIDs.Back.HallowedCape;
			ArmorIDs.Body.Sets.IncludedCapeBackFemale[Item.bodySlot] = ArmorIDs.Back.HallowedCape;
			SetBonusText = [this.GetLocalization("SetBonusMelee"), this.GetLocalization("SetBonusRanged"), this.GetLocalization("SetBonusMagic"), this.GetLocalization("SetBonusSummon"), this.GetLocalization("SetBonusGuardian")];
			GuardianHelmID = ModContent.ItemType<GuardianHallowedHead>();
			GuardianAncientHelmID = ModContent.ItemType<GuardianAncientHallowedHead>();

			if (OrchidMod.ThoriumMod is { } thoriumMod)
			{
				ThoriumGuiseID = thoriumMod.Find<ModItem>("HallowedGuise").Type;
				ThoriumAncientGuiseID = thoriumMod.Find<ModItem>("AncientHallowedGuise").Type;
				ThoriumChapeauID = thoriumMod.Find<ModItem>("HallowedChapeau").Type;
				ThoriumAncientChapeauID = thoriumMod.Find<ModItem>("AncientHallowedChapeau").Type;
				ThoriumCowlID = thoriumMod.Find<ModItem>("HallowedCowl").Type;
				ThoriumAncientCowlID = thoriumMod.Find<ModItem>("AncientHallowedCowl").Type;
			}
		}

		public override void SetDefaults()
		{
			Item.width = 34;
			Item.height = 24;
			Item.defense = 15;
			Item.value = Item.sellPrice(0, 4, 0, 0);
			Item.rare = ItemRarityID.Pink;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetCritChance<GenericDamageClass>() += 7;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			bool legsMatch = legs.type == ItemID.HallowedGreaves || legs.type == ItemID.AncientHallowedGreaves;
			bool headMatch = (head.type >= ItemID.HallowedHelmet && head.type <= ItemID.HallowedMask) || (head.type >= ItemID.HallowedHood && head.type <= ItemID.AncientHallowedHood) || head.type == GuardianHelmID || head.type == GuardianAncientHelmID;
			if (OrchidMod.ThoriumMod is not null && ThoriumGuiseID != -1 && ThoriumAncientGuiseID != -1 && ThoriumChapeauID != -1 && ThoriumAncientChapeauID != -1 && ThoriumCowlID != -1 && ThoriumAncientCowlID != -1)
				headMatch |= (head.type == ThoriumGuiseID || head.type == ThoriumAncientGuiseID || head.type == ThoriumChapeauID || head.type == ThoriumAncientChapeauID || head.type == ThoriumCowlID || head.type == ThoriumAncientCowlID);
			return legsMatch && headMatch;
		}

		public override void UpdateArmorSet(Player player)
		{
			player.armorEffectDrawShadow = true;
			player.armorEffectDrawOutlines = true;
			int item = player.armor[0].type;
			switch (item)
			{
				case ItemID.HallowedMask:
				case ItemID.AncientHallowedMask:
					player.setBonus = SetBonusText[0].Value;
					player.GetAttackSpeed<MeleeDamageClass>() += 0.19f;
					player.moveSpeed += 0.19f;
				break;
				case ItemID.HallowedHelmet:
				case ItemID.AncientHallowedHelmet:
					player.setBonus = SetBonusText[1].Value;
					player.ammoCost75 = true;
				break;
				case ItemID.HallowedHeadgear:
				case ItemID.AncientHallowedHeadgear:
					player.setBonus = SetBonusText[2].Value;
					player.manaCost -= 0.2f;
				break;
				case ItemID.HallowedHood:
				case ItemID.AncientHallowedHood:
					player.setBonus = SetBonusText[3].Value;
					player.maxMinions += 2;
					player.maxTurrets += 2;
				break;
				default:
					if (item == GuardianHelmID || item == GuardianAncientHelmID)
					{
						player.setBonus = SetBonusText[4].Value;
						OrchidGuardian guardian = player.Guardian();
						//this references old guardian regen formulas. before 1.5.1 guard/slam regen would apply 40% as 1 - 0.4 = 0.667 = 6.67s regen time, or 1.66 per 10 seconds, rather than 1 / (1 + 0.4) = 0.714 = 7.14s regen time, or 1.4 per 10 seconds. this makes it significantly stronger than the tooltip value implies
						guardian.GuardianGuardRecharge += 0.6667f;
						guardian.GuardianSlamRecharge += 0.6667f;
					}
					break;
			}

			if (OrchidMod.ThoriumMod is { } thoriumMod)
			{
				if (thoriumMod.Find<ModItem>("HallowedGuise") is {} guise && item == ThoriumGuiseID) guise.UpdateArmorSet(player);
				else if (thoriumMod.Find<ModItem>("AncientHallowedGuise") is {} ancientGuise && item == ThoriumAncientGuiseID) ancientGuise.UpdateArmorSet(player);
				else if (thoriumMod.Find<ModItem>("HallowedChapeau") is {} chapeau && item == ThoriumChapeauID) chapeau.UpdateArmorSet(player);
				else if (thoriumMod.Find<ModItem>("AncientHallowedChapeau") is {} ancientChapeau && item == ThoriumAncientChapeauID) ancientChapeau.UpdateArmorSet(player);
				else if (thoriumMod.Find<ModItem>("HallowedCowl") is {} cowl && item == ThoriumCowlID) cowl.UpdateArmorSet(player);
				else if (thoriumMod.Find<ModItem>("AncientHallowedCowl") is {} ancientCowl && item == ThoriumAncientCowlID) ancientCowl.UpdateArmorSet(player);
			}
		}

		public override void ArmorSetShadows(Player player)
		{
			player.armorEffectDrawShadow = true;
			player.armorEffectDrawOutlines = true;
		}
	}
}
