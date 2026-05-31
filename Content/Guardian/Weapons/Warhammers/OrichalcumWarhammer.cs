using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
	public class OrichalcumWarhammer : OrchidModGuardianHammer
	{
		public override void SafeSetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(0, 1, 65, 0);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.DD2_MonkStaffSwing;
			Item.knockBack = 6f;
			Item.shootSpeed = 22f;
			Item.damage = 123;
			Item.useTime = 32;
			Range = 50;
			ReturnSpeed = 1.6f;
			SwingSpeed = 2f;
			SwingChargeGain = 0.75f;
			GuardStacks = 1;
			SlamStacks = 2;
			HitCooldown = 20;
			Penetrate = true;
			TileBounce = true;
			BlockDuration = 240;
		}

		public override bool ThrowAI(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak, bool OffHand)
		{
			projectile.velocity *= 0.95f;
			if (!Weak && projectile.velocity.Length() < 4)
			{
				float rotationSpeed = 0.2f - projectile.velocity.Length() * 0.05f;
				projectile.rotation += projectile.velocity.X > 0 ? rotationSpeed : -rotationSpeed;
			}
			return true;
		}

		public override void OnThrowTileCollide(Player player, OrchidGuardian guardian, Projectile projectile, Vector2 oldVelocity, bool OffHand)
		{
			float speed = projectile.velocity.Length();
			if (speed > 2)
				projectile.velocity /= speed / 2f;
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.AddIngredient(ItemID.OrichalcumBar, 10);
			recipe.Register();
		}
	}
}
