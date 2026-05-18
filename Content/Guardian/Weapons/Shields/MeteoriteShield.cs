using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	public class MeteoriteShield : OrchidModGuardianShield
	{
		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 0, 40, 0);
			Item.width = 30;
			Item.height = 42;
			Item.UseSound = SoundID.Item1;
			Item.knockBack = 10f;
			Item.damage = 121;
			Item.rare = ItemRarityID.Green;
			Item.useTime = 25;
			distance = 45f;
			slamDistance = 100f;
			blockDuration = 200;
		}

		public override void Push(Player player, Projectile shield, NPC npc)
		{
			npc.AddBuff(BuffID.OnFire, 600);
		}

		public override void SlamHit(Player player, Projectile shield, NPC npc, bool WeakSlam)
		{
			npc.AddBuff(BuffID.OnFire, WeakSlam ? 150 : 600);
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.Anvils);
			recipe.AddIngredient(ItemID.MeteoriteBar, 20);
			recipe.Register();
		}
	}
}
