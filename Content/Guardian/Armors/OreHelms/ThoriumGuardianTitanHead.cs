using OrchidMod;
using OrchidMod.Common.Attributes;
using OrchidMod.Content.Guardian;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;


namespace OrchidMod.Content.Guardian.Armors.OreHelms
{
	[AutoloadEquip(EquipType.Head)]
	[CrossmodContent("ThoriumMod")]
	public class ThoriumGuardianTitanHead : OrchidModGuardianEquipable
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
			Item.value = Item.sellPrice(0, 2, 28);
			Item.rare = ItemRarityID.LightPurple;
			Item.defense = 22;
		}

		public override void UpdateEquip(Player player)
		{	
			OrchidGuardian modPlayer = player.GetModPlayer<OrchidGuardian>();
			player.GetAttackSpeed<MeleeDamageClass>() += 0.08f;
			modPlayer.GuardianGuardMax++;
			modPlayer.GuardianSlamMax++;
			modPlayer.GuardianGuardRecharge += 0.5f;
			modPlayer.GuardianSlamRecharge += 0.5f;
			player.aggro += 600;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null) 
				return body.type == thoriumMod.Find<ModItem>("TitanBreastplate").Type && legs.type == thoriumMod.Find<ModItem>("TitanGreaves").Type;
			return false;
		}

		public override void UpdateArmorSet(Player player)
		{			
			player.setBonus = SetBonusText.Value;
			player.GetDamage<GenericDamageClass>() += 0.18f;
		}

		public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null) {
				CreateRecipe()
				.AddIngredient(thoriumMod, "TitanicBar", 12)
				.AddTile(thoriumMod, "SoulForgeNew")
				.Register();
			}
		}
	}
}
