using OrchidMod.Content.Gambler.Misc;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Accessories
{
	public class ParryingMailMech : OrchidModGuardianEquipable
	{
		public override void SafeSetDefaults()
		{
			Item.width = 24;
			Item.height = 28;
			Item.value = Item.sellPrice(0, 4, 50, 0);
			Item.rare = ItemRarityID.LightPurple;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			OrchidGuardian modPlayer = Guardian(player);
			modPlayer.GuardianParryDuration += 0.25f;
			modPlayer.GuardianSharpRebuttalParry = true;
			player.GetAttackSpeed(DamageClass.Melee) += 0.12f;
			player.longInvince = true;
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddIngredient<ParryingMailFeral>();
			recipe.AddIngredient(ItemID.CrossNecklace);
			recipe.AddIngredient(ItemID.AvengerEmblem);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
			recipe = CreateRecipe();
			recipe.AddIngredient<ParryingMailHoly>();
			recipe.AddIngredient(ItemID.FeralClaws);
			recipe.AddIngredient(ItemID.AvengerEmblem);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}