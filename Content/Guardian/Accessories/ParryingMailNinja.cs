using OrchidMod.Common.ModObjects;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Accessories
{
	public class ParryingMailNinja : OrchidModGuardianEquipable
	{
		public override void SafeSetDefaults()
		{
			Item.width = 24;
			Item.height = 28;
			Item.value = Item.sellPrice(0, 10, 0, 0);
			Item.rare = ItemRarityID.Yellow;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			OrchidGuardian modPlayer = Guardian(player);
			modPlayer.GuardianParryDuration += 0.25f;
			modPlayer.GuardianSharpRebuttalParry = true;
			player.GetModPlayer<OrchidPlayer>().OrchidDodgeChance *= 0.88f;
			player.GetAttackSpeed(DamageClass.Melee) += 0.12f;
			player.longInvince = true;
			player.dashType = 1;
			player.spikedBoots = 2;
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddIngredient<ParryingMailMech>();
			recipe.AddIngredient(ItemID.MasterNinjaGear);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}