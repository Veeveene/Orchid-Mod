using Microsoft.Xna.Framework;
using OrchidMod.Content.Guardian.Projectiles.Katars;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Katars
{
	public class HellstoneKatar : OrchidModGuardianKatar
	{
		public override void SafeSetDefaults()
		{
			Item.width = 34;
			Item.height = 34;
			Item.knockBack = 6f;
			Item.damage = 71;
			Item.value = Item.sellPrice(0, 0, 54, 0);
			Item.rare = ItemRarityID.Orange;
			Item.useTime = 30;
			JabVelocity = 20f;
			ParryDuration = 15;
		}

		public override Color GetColor()
		{
			return new Color(255, 179, 47);
		}

		public override void OnDashKatar(Player player, OrchidGuardian guardian, Projectile anchor)
		{
			int damage = guardian.GetGuardianDamage(Item.damage * 0.3f);
			int projectileType = ModContent.ProjectileType<HellstoneKatarProjectile>();
			Projectile newprojectile = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, projectileType, damage, 0f, player.whoAmI);
			newprojectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
			SoundEngine.PlaySound(SoundID.Item73.WithPitchOffset(Main.rand.NextFloat(0.4f, 0.6f)), player.Center);
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.Anvils);
			recipe.AddIngredient(ItemID.HellstoneBar, 20);
			recipe.Register();
		}
	}
}
