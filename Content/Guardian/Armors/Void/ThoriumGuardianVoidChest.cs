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
	[AutoloadEquip(EquipType.Body)]
	public class ThoriumGuardianVoidChest : OrchidModGuardianEquipable
	{
		public override void SafeSetDefaults()
		{
			Item.width = 34;
			Item.height = 22;
			Item.value = Item.sellPrice(0, 7, 50);
			Item.rare = ItemRarityID.Pink;
			Item.defense = 11;
		}

		public override void UpdateEquip(Player player)
		{
			OrchidGuardian modPlayer = player.GetModPlayer<OrchidGuardian>();
			modPlayer.GuardianGuardRecharge += 0.75f;
			modPlayer.GuardianSlamRecharge += 0.75f;
			modPlayer.GuardianGuardMax++;
			modPlayer.GuardianSlamMax++;
			player.GetAttackSpeed(DamageClass.Melee) += 0.15f;
			player.GetCritChance<GuardianDamageClass>() += 6;
			player.aggro += 600;
		}

		public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (OrchidMod.ThoriumMod != null)
			{
				CreateRecipe()
				.AddTile(TileID.MythrilAnvil)
				.AddIngredient(thoriumMod, "VoidHeart")
				.AddIngredient(ItemID.HallowedBar, 14)
				.AddIngredient(ItemID.SoulofNight, 16)
				.Register();
			}
		}
	}
}
