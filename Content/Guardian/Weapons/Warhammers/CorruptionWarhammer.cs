using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
	public class CorruptionWarhammer : OrchidModGuardianHammer
	{

		public override void SafeSetDefaults()
		{
			Item.width = 36;
			Item.height = 36;
			Item.value = Item.sellPrice(0, 0, 27, 0);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.DD2_MonkStaffSwing;
			Item.knockBack = 9f;
			Item.shootSpeed = 10f;
			Item.damage = 182;
			Item.useTime = 32;
			Range = 28;
			SlamStacks = 2;
			SwingChargeGain = 1.5f;
			ReturnSpeed = 0.9f;
			BlockDuration = 255;
		}

		public override bool ThrowAI(Player player, OrchidGuardian guardian, Projectile projectile, bool weak, bool OffHand)
		{

			if (Main.rand.NextBool(5))
			{
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Corruption, Scale: Main.rand.NextFloat(0.8f, 1f));
				dust.velocity = dust.velocity * 0.25f + projectile.velocity * 0.2f;
				dust.noGravity = true;
			}

			return true;
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.Anvils);
			recipe.AddIngredient(ItemID.DemoniteBar, 10);
			recipe.Register();
		}
	}
}
