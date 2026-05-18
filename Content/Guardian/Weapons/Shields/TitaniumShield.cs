using OrchidMod.Common.ModObjects;
using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	public class TitaniumShield : OrchidModGuardianShield
	{
		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 2, 85, 0);
			Item.width = 42;
			Item.height = 48;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item73;
			Item.knockBack = 12f;
			Item.damage = 160;
			Item.rare = ItemRarityID.LightRed;
			Item.useTime = 30;
			distance = 55f;
			slamDistance = 120f;
			blockDuration = 200;
			shouldFlip = true;
		}

		public override void HoldItemFrame(Player player)
		{
			player.GetModPlayer<OrchidGuardian>().GuardianCounter = true;
		}

		public override void Protect(Player player, Projectile shield)
		{
			player.GetModPlayer<OrchidPlayer>().SpawnTitaniumShards(shield.GetSource_FromThis());
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item20, player.Center);
		}

		public override bool Block(Player player, Projectile shield, Projectile projectile)
		{
			player.GetModPlayer<OrchidPlayer>().SpawnTitaniumShards(shield.GetSource_FromThis());
			return true;
		}

		public override void Slam(Player player, Projectile shield, bool WeakSlam)
		{
			if (player.GetModPlayer<OrchidGuardian>().GuardianCounterTime > 0 && !WeakSlam)
			{
				player.GetModPlayer<OrchidPlayer>().SpawnTitaniumShards(shield.GetSource_FromThis(), 4);
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item82, player.Center);
			}
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.AddIngredient(ItemID.TitaniumBar, 12);
			recipe.Register();
		}
	}
}
