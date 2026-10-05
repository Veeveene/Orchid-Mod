using OrchidMod.Content.General.Armor.Vanity;
using OrchidMod.Content.Guardian.Misc;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Armors.Empress
{
	[AutoloadEquip(EquipType.Legs)]
	public class GuardianEmpressLegs : OrchidModGuardianEquipable
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<EmpressPlateLegs>();
		}

		public override void SafeSetDefaults()
		{
			Item.width = 24;
			Item.height = 16;
			Item.value = Item.sellPrice(0, 4, 80, 0);
			Item.rare = ItemRarityID.Yellow;
			Item.defense = 19;
		}

		public override void UpdateEquip(Player player)
		{
			OrchidGuardian modPlayer = player.GetModPlayer<OrchidGuardian>();
			player.GetDamage<GuardianDamageClass>() += 0.1f;
			player.moveSpeed += 0.15f;
			player.aggro += 250;
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddIngredient<GuardianEmpressMaterial>(10);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
}
