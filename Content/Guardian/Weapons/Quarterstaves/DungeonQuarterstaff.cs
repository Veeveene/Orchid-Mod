using Microsoft.Xna.Framework;
using OrchidMod.Content.Guardian.Projectiles.Quarterstaves;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	public class DungeonQuarterstaff : OrchidModGuardianQuarterstaff
	{
		private int boltCounter = 0;

		public override void SafeSetDefaults()
		{
			Item.width = 46;
			Item.height = 46;
			Item.value = Item.sellPrice(0, 1, 75, 0);
			Item.rare = ItemRarityID.Green;
			Item.useTime = 30;
			ParryDuration = 75;
			Item.shootSpeed = 8f;
			Item.knockBack = 6.5f;
			Item.damage = 73;
			JabDamage = 0.25f;
			JabChargeGain = 0.5f;
		}

		public override void OnAttack(Player player, OrchidGuardian guardian, Projectile projectile, bool jabAttack, bool counterAttack)
		{
			if (IsLocalPlayer(player) && jabAttack)
			{
				Vector2 velocity = Vector2.UnitY.RotatedBy((player.Center - Main.MouseWorld).ToRotation() + MathHelper.PiOver2) * Item.shootSpeed;
				Vector2 tipPosition = projectile.Center - Vector2.UnitY.RotatedBy(projectile.rotation + MathHelper.PiOver4) * projectile.width * 0.5f;
				SpawnProjectile(velocity, projectile, guardian, tipPosition, true);
			}

			boltCounter = 0;
			SoundEngine.PlaySound(SoundID.Item21, player.Center);
		}

		public override void ExtraAIQuarterstaff(Player player, OrchidGuardian guardian, Projectile projectile)
		{
			for (int i = -1; i < 2; i+= 2)
			{
				float len = i == -1 ? projectile.width - 32f : projectile.width;
				Vector2 tipPosition = projectile.Center - Vector2.UnitY.RotatedBy(projectile.rotation + MathHelper.PiOver4) * len * 0.5f * i;
				if (player.direction == 1) tipPosition.X -= 12;

				int rand = (projectile.ai[0] != 1 || projectile.ai[2] != 0) ? 2 : 6;
				if (Main.rand.NextBool(rand) && (projectile.ai[0] != 0 || projectile.ai[2] != 0))
				{
					Dust dust = Dust.NewDustDirect(tipPosition, 8, 8, DustID.BlueTorch);
					dust.scale = Main.rand.NextFloat(1f, 1.5f);
					dust.noGravity = true;
				}

				if (Main.rand.NextBool(4))
				{
					Dust dust = Dust.NewDustDirect(tipPosition, 8, 8, DustID.WaterCandle);
					dust.noGravity = true;
				}
			}

			if (projectile.ai[0] > 1f)
			{ // Fires 2 projectiles in the middle of the swing arc (might be slightly offset by attack speed)
				if ((projectile.ai[0] < 17f && boltCounter == 1) || (projectile.ai[0] < 33f && boltCounter == 0))
				{
					boltCounter++;
					Vector2 velocity = Vector2.UnitY.RotatedBy(projectile.ai[1]) * Item.shootSpeed;
					Vector2 tipPosition = projectile.Center - Vector2.UnitY.RotatedBy(projectile.rotation + MathHelper.PiOver4) * projectile.width * 0.1f;
					SpawnProjectile(velocity, projectile, guardian, tipPosition, false);
					SoundEngine.PlaySound(SoundID.Item21.WithPitchOffset(boltCounter == 1 ? -0.1f : 0.3f), player.Center);
				}
			}

			if (projectile.ai[2] < 0f)
			{ // Fires 3 projectiles while counterattacking
				if ((projectile.ai[2] >= -13.3f && boltCounter == 2) || (projectile.ai[2] >= -26.6f && boltCounter == 1) || boltCounter == 0)
				{
					boltCounter++;
					Vector2 velocity = Vector2.UnitY.RotatedBy((player.Center - Main.MouseWorld).ToRotation() + MathHelper.PiOver2) * Item.shootSpeed;
					SpawnProjectile(velocity, projectile, guardian, player.Center, false);
					SoundEngine.PlaySound(SoundID.Item21.WithPitchOffset(boltCounter * 0.4f - 0.5f), player.Center);
				}
			}
		}

		public void SpawnProjectile(Vector2 velocity, Projectile projectile, OrchidGuardian guardian, Vector2 position, bool jab)
		{
			int damage = guardian.GetGuardianDamage(Item.damage * 0.5f);
			int projectileType = ModContent.ProjectileType<DungeonQuarterstaffProjectile>();
			Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromAI(), position, velocity, projectileType, damage, Item.knockBack, projectile.owner, 0f, jab ? 1 : 0);
			newProjectile.CritChance = guardian.GetGuardianCrit(Item.crit);
			newProjectile.rotation = newProjectile.velocity.ToRotation();
			newProjectile.netUpdate = true;
		}
	}
}
