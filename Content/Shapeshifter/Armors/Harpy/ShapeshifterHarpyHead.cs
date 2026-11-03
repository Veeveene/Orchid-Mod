using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OrchidMod.Content.Shapeshifter.Armors.Harpy
{
	[AutoloadEquip(EquipType.Head)]
	public class ShapeshifterHarpyHead : OrchidModShapeshifterItem
	{
		public static LocalizedText SetBonusText { get; private set; }

		public override void SetStaticDefaults()
		{
			SetBonusText = this.GetLocalization("SetBonus");
		}

		public override void SafeSetDefaults()
		{
			Item.width = 26;
			Item.height = 16;
			Item.value = Item.sellPrice(0, 0, 15, 50);
			Item.rare = ItemRarityID.Green;
			Item.defense = 5;
			ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = true;
			ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetCritChance<ShapeshifterDamageClass>() += 5;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ModContent.ItemType<ShapeshifterHarpyChest>() && legs.type == ModContent.ItemType<ShapeshifterHarpyLegs>();
		}

		public override void UpdateArmorSet(Player player)
		{
			OrchidShapeshifter shapeshifter = Shapeshifter(player);
			shapeshifter.ShapeshifterSetHarpy = true;
			player.setBonus = SetBonusText.Value;
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Feather, 4);
			recipe.AddIngredient(ItemID.DemoniteBar, 6);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
			
			recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Feather, 4);
			recipe.AddIngredient(ItemID.CrimtaneBar, 6);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}
