using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Accessories
{
	public class TempleSpike : OrchidModGuardianEquipable
	{
		public override void SafeSetDefaults()
		{
			Item.width = 30;
			Item.height = 30;
			Item.value = Item.sellPrice(0, 6, 0, 0);
			Item.rare = ItemRarityID.Lime;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			OrchidGuardian modPlayer = Guardian(player);
			if (modPlayer.GuardianSpikeDamage < 2f)
				modPlayer.GuardianSpikeDamage = 2f;
			modPlayer.GuardianSpikeTemple = true;
			modPlayer.GuardianSharpRebuttalBlock = true;
			player.GetCritChance<GuardianDamageClass>() += 10;
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddIngredient<DungeonSpike>();
			recipe.AddIngredient(ItemID.DestroyerEmblem);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
			
			recipe = CreateRecipe();
			recipe.AddIngredient<MechanicalSpike>();
			recipe.AddIngredient(ItemID.EyeoftheGolem);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}