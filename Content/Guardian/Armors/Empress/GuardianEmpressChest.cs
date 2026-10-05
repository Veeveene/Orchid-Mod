using OrchidMod.Content.General.Armor.Vanity;
using OrchidMod.Content.Guardian.Misc;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace OrchidMod.Content.Guardian.Armors.Empress
{
	[AutoloadEquip(EquipType.Body)]
	public class GuardianEmpressChest : OrchidModGuardianEquipable
	{
		public static LocalizedText SetBonusText { get; private set; }

		public override void SetStaticDefaults()
		{
			SetBonusText = this.GetLocalization("SetBonus");
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<EmpressPlateChest>();
		}
		
		public override void SafeSetDefaults()
		{
			Item.width = 30;
			Item.height = 18;
			Item.value = Item.sellPrice(0, 5, 10, 0);
			Item.rare = ItemRarityID.Yellow;
			Item.defense = 25;
		}

		public override void UpdateEquip(Player player)
		{
			OrchidGuardian modPlayer = player.GetModPlayer<OrchidGuardian>();
			player.GetAttackSpeed(DamageClass.Melee) += 0.25f;
			modPlayer.GuardianSlamMax += 2;
			modPlayer.GuardianSlamRecharge += 0.8f;
			player.aggro += 250;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return (head.type == ItemType<GuardianEmpressHead>()) && legs.type == ItemType<GuardianEmpressLegs>();
		}

		public override void UpdateArmorSet(Player player)
		{
			OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
			player.setBonus = SetBonusText.Value;
			guardian.GuardianBonusRune += 2;
			guardian.GuardianRuneTimer += 0.5f;
			guardian.GuardianStandardRange += 0.5f;
			guardian.GuardianStandardTimer += 0.5f;
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddIngredient<GuardianEmpressMaterial>(12);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
}
