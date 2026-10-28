using Microsoft.Xna.Framework;
using OrchidMod.Common.Attributes;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	[CrossmodContent("ThoriumMod")]
	public class ThoriumThoriumQuarterstaff : OrchidModGuardianQuarterstaff
	{
		public override void SafeSetDefaults()
		{
			Item.width = 58;
			Item.height = 58;
			Item.value = Item.sellPrice(0, 0, 28, 0);
			Item.rare = ItemRarityID.Blue;
			Item.useTime = 30;
			ParryDuration = 70;
			Item.knockBack = 5.5f;
			Item.damage = 39;
		}

		public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (OrchidMod.ThoriumMod != null)
			{
				var recipe = CreateRecipe();
				recipe.AddTile(thoriumMod, "ThoriumAnvil");
				recipe.AddIngredient(thoriumMod, "ThoriumBar", 8);
				recipe.Register();
			}
		}

		public override void OnHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool jabAttack, bool counterAttack)
		{
			if (OrchidMod.ThoriumMod != null)
			{
				int projectileType = OrchidMod.ThoriumMod.Find<ModProjectile>("ThoriumSpark").Type;
				int amount = jabAttack ? 1 : 2;
				int damage = (int)(Item.damage * 0.25f);

				for (int i = 0; i < amount; i++)
				{
					Vector2 velocity = Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * 10f;
					Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromAI(), target.Center, velocity, projectileType, damage, Item.knockBack, projectile.owner);
					newProjectile.CritChance = guardian.GetGuardianCrit(Item.crit);
					newProjectile.DamageType = ModContent.GetInstance<GuardianDamageClass>();
				}
			}
		}
	}
}
