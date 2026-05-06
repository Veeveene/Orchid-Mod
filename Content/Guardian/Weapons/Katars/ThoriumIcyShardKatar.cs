using Microsoft.Xna.Framework;
using OrchidMod.Common.Attributes;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Katars
{
	[CrossmodContent("ThoriumMod")]
	public class ThoriumIcyShardKatar : OrchidModGuardianKatar
	{
		public override void SafeSetDefaults()
		{
			Item.width = 30;
			Item.height = 30;
			Item.knockBack = 5f;
			Item.damage = 37;
			Item.value = Item.sellPrice(0, 0, 6, 0);
			Item.rare = ItemRarityID.White;
			Item.useTime = 25;
			JabVelocity = 15f;
			ParryDuration = 10;
		}

		public override Color GetColor()
		{
			return new Color(166, 241, 243);
		}

		public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				var recipe = CreateRecipe();
				recipe.AddTile(TileID.WorkBenches);
				recipe.AddIngredient(thoriumMod, "IcyShard", 8);
				recipe.Register();
			}
		}

		public override void OnHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool fullyCharged)
		{
			if (OrchidMod.ThoriumMod != null)
			{
				int type = OrchidMod.ThoriumMod.Find<ModBuff>("Freezing").Type;
				target.AddBuff(type, fullyCharged ? 180 : 120);
			}
		}

		public override void OnHitParry(Player player, OrchidGuardian guardian, NPC target, Projectile projectile)
		{
			if (OrchidMod.ThoriumMod != null)
			{
				int type = OrchidMod.ThoriumMod.Find<ModBuff>("Freezing").Type;
				target.AddBuff(type, 120);
			}
		}
	}
}
