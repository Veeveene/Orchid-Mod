using System;
using Microsoft.Xna.Framework;
using OrchidMod.Common;
using OrchidMod.Common.ModObjects;
using OrchidMod.Content.Guardian;
using OrchidMod.Content.Guardian.Armors.OreHelms;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace OrchidMod.Content.General.Armor
{
	[AutoloadEquip(EquipType.Body)]
	public class NeoAncientHallowedPlateMail : ModItem
	{
		public static LocalizedText[] SetBonusText { get; private set; }
		static int GuardianHelmID;
		static int GuardianAncientHelmID;
		public override void SetStaticDefaults()
		{
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.AncientHallowedPlateMail;
			ItemID.Sets.ShimmerTransformToItem[ItemID.HallowedPlateMail] = Type;
			ArmorIDs.Body.Sets.IncludedCapeBack[Item.bodySlot] = ArmorIDs.Back.HallowedCape;
			ArmorIDs.Body.Sets.IncludedCapeBackFemale[Item.bodySlot] = ArmorIDs.Back.HallowedCape;
			SetBonusText = [this.GetLocalization("SetBonusMelee"), this.GetLocalization("SetBonusRanged"), this.GetLocalization("SetBonusMagic"), this.GetLocalization("SetBonusSummon"), this.GetLocalization("SetBonusGuardian")];
			GuardianHelmID = ModContent.ItemType<GuardianHallowedHead>();
			GuardianAncientHelmID = ModContent.ItemType<GuardianAncientHallowedHead>();
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
			return (legs.type == ItemID.HallowedGreaves || legs.type == ItemID.AncientHallowedGreaves) && ((head.type >= ItemID.HallowedHelmet && head.type <= ItemID.HallowedMask) || (head.type >= ItemID.HallowedHood && head.type <= ItemID.AncientHallowedHood) || head.type == GuardianHelmID || head.type == GuardianAncientHelmID);
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
						OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
						//this references old guardian regen formulas. before 1.5.1 guard/slam regen would apply 40% as 1 - 0.4 = 0.667 = 6.67s regen time, or 1.66 per 10 seconds, rather than 1 / (1 + 0.4) = 0.714 = 7.14s regen time, or 1.4 per 10 seconds. this makes it significantly stronger than the tooltip value implies
						guardian.GuardianGuardRecharge += 0.6667f;
						guardian.GuardianSlamRecharge += 0.6667f;
					}
				break;
			}
		}

		public override void ArmorSetShadows(Player player)
		{
			player.armorEffectDrawShadow = true;
			player.armorEffectDrawOutlines = true;
		}
	}
}
