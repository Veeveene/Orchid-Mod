using OrchidMod.Common;
using OrchidMod.Content.Gambler.Misc;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Accessories
{
	public class DungeonSpike : OrchidModGuardianEquipable
	{
		public override void SafeSetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.value = Item.sellPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.Green;
			Item.accessory = true;
			Item.damage = 30; // duplicate modifications in GuardianShieldAnchor for the projectile spawn
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			OrchidGuardian modPlayer = Guardian(player);
			if (modPlayer.GuardianSpikeDamage < 1.5f)
				modPlayer.GuardianSpikeDamage = 1.5f;
			modPlayer.GuardianSpikeDungeon = true;
		}

		public override void AddRecipes()
		{
			if (ModContent.GetInstance<OrchidServerConfig>().EnableContentGambler)
			{
				var recipe = CreateRecipe();
				recipe.AddIngredient<GoblinSpike>();
				recipe.AddIngredient<TiamatRelic>();
				recipe.AddTile(TileID.Anvils);
				recipe.Register();
			}
			else
			{
				var recipe = CreateRecipe();
				recipe.AddIngredient<GoblinSpike>();
				recipe.AddIngredient(ItemID.Bone, 30);
				recipe.AddTile(TileID.Anvils);
				recipe.Register();
			}
		}
	}
}