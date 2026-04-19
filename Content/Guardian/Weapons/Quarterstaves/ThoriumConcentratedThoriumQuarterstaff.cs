using Microsoft.Xna.Framework;
using OrchidMod.Common.Attributes;
using OrchidMod.Content.Guardian.Projectiles.Quarterstaves;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	[CrossmodContent("ThoriumMod")]
	public class ThoriumConcentratedThoriumQuarterstaff : OrchidModGuardianQuarterstaff
	{
		public override void SafeSetDefaults()
		{
			Item.width = 58;
			Item.height = 58;
			Item.value = Item.sellPrice(0, 5);
			Item.rare = ItemRarityID.Yellow;
			Item.useTime = 36;
			Item.shootSpeed = 20f;
			ParryDuration = 150;
			Item.knockBack = 5f;
			Item.damage = 273;
			GuardStacks = 2;
			SlamStacks = 1;
		}

		public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (OrchidMod.ThoriumMod != null)
				CreateRecipe()
				.AddTile(thoriumMod, "SoulForgeNew")
				.AddIngredient(thoriumMod, "ConcentratedThorium", 8)
				.Register();
		}

		public override void OnHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool jabAttack, bool counterAttack)
		{
			if (OrchidMod.ThoriumMod != null)
			{
				for (int i = 0; i < Main.rand.Next(3); i++) 
				{
					int projectileType = OrchidMod.ThoriumMod.Find<ModProjectile>("ThoriumSpark").Type;
					int damage = (int)(Item.damage * 0.2f);
					Vector2 velocity = Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * 14f;
					Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromAI(), target.Center, velocity, projectileType, damage, Item.knockBack, projectile.owner);
					newProjectile.CritChance = guardian.GetGuardianCrit(Item.crit);
					newProjectile.DamageType = ModContent.GetInstance<GuardianDamageClass>();
					newProjectile.scale = 1.5f;
				}
				
			}
		}

		public override void OnAttack(Player player, OrchidGuardian guardian, Projectile projectile, bool jabAttack, bool counterAttack)
		{
			if (IsLocalPlayer(player) && !jabAttack && !counterAttack)
			{
				Vector2 velocity = Vector2.UnitY.RotatedBy((player.Center - Main.MouseWorld).ToRotation() + MathHelper.PiOver2) * Item.shootSpeed;
				SoundEngine.PlaySound(SoundID.Item21, player.Center);
				Projectile.NewProjectileDirect(projectile.GetSource_FromAI(), projectile.Center, velocity, ModContent.ProjectileType<ThoriumConcentratedThoriumQuarterstaffProjectile>(), guardian.GetGuardianDamage(Item.damage * 0.1f), 0f, projectile.owner);
			}
		}

		public override void OnHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool jabAttack, bool counterAttack)
		{
			if (OrchidMod.ThoriumMod != null)
			{
				int projectileType = OrchidMod.ThoriumMod.Find<ModProjectile>("ThoriumSpark").Type;
				int amount = jabAttack ? 3 : 7;
				int damage = (int)(Item.damage * 0.2f);

				for (int i = 0; i < amount; i++)
				{
					Vector2 velocity = Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * 14f;
					Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromAI(), target.Center, velocity, projectileType, damage, Item.knockBack, projectile.owner);
					newProjectile.CritChance = guardian.GetGuardianCrit(Item.crit);
					newProjectile.DamageType = ModContent.GetInstance<GuardianDamageClass>();
					newProjectile.scale = 1.5f;
				}
			}
		}
	}
}
