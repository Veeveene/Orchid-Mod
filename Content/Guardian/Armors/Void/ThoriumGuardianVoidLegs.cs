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
	[AutoloadEquip(EquipType.Legs)]
	public class ThoriumGuardianVoidLegs : OrchidModGuardianEquipable
	{
		public override void SafeSetDefaults()
		{
			Item.width = 26;
			Item.height = 14;
			Item.value = Item.sellPrice(0, 7, 50);
			Item.rare = ItemRarityID.Pink;
			Item.defense = 8;
		}

		public override void UpdateEquip(Player player)
		{
			OrchidGuardian modPlayer = Guardian(player);
			modPlayer.GuardianGuardMax++;
			player.moveSpeed += 0.3f;
			player.jumpSpeedBoost += 1f;
			player.runAcceleration *= 1.15f;
			player.maxFallSpeed *= 1.5f;
		}

		public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (OrchidMod.ThoriumMod != null)
			{
				CreateRecipe()
				.AddTile(TileID.MythrilAnvil)
				.AddIngredient(thoriumMod, "VoidHeart")
				.AddIngredient(ItemID.HallowedBar, 11)
				.AddIngredient(ItemID.SoulofNight, 12)
				.Register();
			}
		}
	}
}
