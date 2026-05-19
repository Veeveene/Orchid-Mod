using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using OrchidMod.Common.Attributes;

namespace OrchidMod.Content.Guardian.Armors.Void
{
	[CrossmodContent("ThoriumMod")]
	[AutoloadEquip(EquipType.Head)]
	public class ThoriumGuardianVoidHead : OrchidModGuardianEquipable
	{
		public static LocalizedText SetBonusText { get; private set; }

		public override void SetStaticDefaults()
		{
			SetBonusText = this.GetLocalization("SetBonus");
		}

		public override void SafeSetDefaults()
		{
			Item.width = 26;
			Item.height = 20;
			Item.value = Item.sellPrice(0, 7, 50);
			Item.rare = ItemRarityID.Pink;
			Item.defense = 5;
		}

		public override void UpdateEquip(Player player)
		{
			OrchidGuardian modPlayer = player.GetModPlayer<OrchidGuardian>();
			modPlayer.GuardianGuardRecharge += 0.75f;
			modPlayer.GuardianGuardMax++;
			player.GetDamage<GuardianDamageClass>() += 0.10f;
			player.aggro += 600;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ItemType<ThoriumGuardianVoidChest>() && legs.type == ItemType<ThoriumGuardianVoidLegs>();
		}

		public override void UpdateArmorSet(Player player)
		{
			OrchidGuardian modPlayer = player.GetModPlayer<OrchidGuardian>();
			player.setBonus = SetBonusText.Value;
			player.statLifeMax2 -= 250;
		}

		public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (OrchidMod.ThoriumMod != null)
			{
				CreateRecipe()
				.AddTile(TileID.MythrilAnvil)
				.AddIngredient(thoriumMod, "VoidHeart")
				.AddIngredient(ItemID.HallowedBar, 7)
				.AddIngredient(ItemID.SoulofNight, 8)
				.Register();
			}
		}
	}
}
