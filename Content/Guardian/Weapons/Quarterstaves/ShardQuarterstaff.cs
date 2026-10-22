using Microsoft.Xna.Framework;
using OrchidMod.Common;
using OrchidMod.Content.Guardian.Projectiles.Quarterstaves;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	public class ShardQuarterstaff : OrchidModGuardianQuarterstaff
	{
		public override void SafeSetDefaults()
		{
			Item.width = 48;
			Item.height = 48;
			Item.value = Item.sellPrice(0, 5, 0, 0);
			Item.rare = ItemRarityID.Pink;
			Item.useTime = 30;
			ParryDuration = 90;
			Item.knockBack = 8f;
			Item.damage = 192;
		}

		public override void OnAttack(Player player, OrchidGuardian guardian, Projectile projectile, bool jabAttack, bool counterAttack)
		{
			if (counterAttack && IsLocalPlayer(player))
			{
				int damage = guardian.GetGuardianDamage(Item.damage * 0.25f);
				int projectileType = ModContent.ProjectileType<ShardQuarterstaffProjectile>();
				Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromAI(), projectile.Center, player.velocity, projectileType, damage, 0f, projectile.owner, 0.8f, Main.rand.NextFloat(3.14f));
				newProjectile.CritChance = guardian.GetGuardianCrit(Item.crit);
			}
		}

		public override void OnHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool jabAttack, bool counterAttack)
		{
			if (counterAttack) target.AddBuff(BuffID.Confused, 60);
			else
			{
				Vector2 pos = projectile.Center + new Vector2(28, -28).RotatedBy(projectile.rotation);
				Vector2 vel = new Vector2(1, -1).RotatedBy(projectile.rotation);
				Color color = Color.White with {A = 0};
				bool whiteBlack = false;
				if (!jabAttack && projectile.ModProjectile is GuardianQuarterstaffAnchor anchor)
				{
					if (anchor.DamageReset != 0)
					{
						whiteBlack = true;
						vel = vel.RotatedBy(1.4f * player.direction);
					}
					else
					{
						vel = vel.RotatedBy(-1.4f * player.direction);
					}
				}
				else whiteBlack = Main.rand.NextBool();
				if (whiteBlack)
				{
					Dust dust = Dust.NewDustPerfect(pos, DustID.Wraith, vel, Alpha: 50, newColor: color, Scale: 2f);
					dust.noGravity = true;
					dust.fadeIn = 2f;
					for (int i = 0; i < 15; i++)
					{
						dust = Dust.NewDustPerfect(pos, DustID.Snow, Alpha: 150, newColor: color, Scale: Main.rand.NextFloat(0.75f, 1.5f));
						dust.velocity = dust.velocity * 1.5f + vel * Main.rand.NextFloat(5f);
						dust.noGravity = true;
						if (!jabAttack)
							dust.fadeIn = Main.rand.NextFloat(1.5f);
					}
				}
				else
				{
					Dust dust = Dust.NewDustPerfect(pos, DustID.Snow, vel, Alpha: 150, newColor: color, Scale: 2f);
					dust.noGravity = true;
					dust.fadeIn = 2f;
					for (int i = 0; i < 15; i++)
					{
						dust = Dust.NewDustPerfect(pos, DustID.Wraith, Alpha: 50, newColor: color, Scale: Main.rand.NextFloat(0.75f, 1.5f));
						dust.velocity = dust.velocity * 1.5f + vel * Main.rand.NextFloat(5f);
						dust.noGravity = true;
						if (!jabAttack)
							dust.fadeIn = Main.rand.NextFloat(1.5f);
					}
				}
			}
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.AddIngredient(ItemID.DarkShard, 1);
			recipe.AddIngredient(ItemID.LightShard, 1);
			recipe.AddIngredient(ItemID.SoulofNight, 7);
			recipe.AddIngredient(ItemID.SoulofLight, 7);
			recipe.Register();
		}
	}
}
