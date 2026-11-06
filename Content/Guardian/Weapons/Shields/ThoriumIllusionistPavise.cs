using Microsoft.Xna.Framework;
using OrchidMod.Common.Attributes;
using OrchidMod.Content.Guardian.Projectiles.Shields;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	[CrossmodContent("ThoriumMod")]
	public class ThoriumIllusionistPavise : OrchidModGuardianShield
	{
		public static List<int> TypesArrow;
		public static List<int> TypesBoulder;
		public static List<int> TypesLaser;
		public static List<int> TypesScythe;
		public static List<int> TypesStinger;
		public static List<int> TypesFeather;

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.width = 32;
			Item.height = 42;
			Item.UseSound = SoundID.Item1;
			Item.knockBack = 10f;
			Item.damage = 112;
			Item.rare = ItemRarityID.Green;
			Item.useTime = 30;
			distance = 45f;
			slamDistance = 80f;
			blockDuration = 210;
		}

		public override void Load()
		{
			TypesArrow = new List<int>()
			{
				ProjectileID.PoisonDart,
				ProjectileID.PoisonDartTrap,
				ProjectileID.WoodenArrowHostile,
				ProjectileID.FlamingArrow
			};

			TypesBoulder = new List<int>()
			{
				ProjectileID.Boulder,
				ProjectileID.LifeCrystalBoulder,
				ProjectileID.BouncyBoulder,
				ProjectileID.MoonBoulder,
				ProjectileID.MiniBoulder,
				ProjectileID.RollingCactus
			};

			TypesLaser = new List<int>()
			{
				ProjectileID.EyeLaser,
				ProjectileID.PinkLaser,
				ProjectileID.DeathLaser,
				ProjectileID.SniperBullet,
				ProjectileID.BulletDeadeye,
				ProjectileID.FrostBeam,
				ProjectileID.RainNimbus
			};

			TypesScythe = new List<int>()
			{
				ProjectileID.DemonSickle
			};


			TypesFeather = new List<int>()
			{
				ProjectileID.HarpyFeather
			};

			TypesStinger = new List<int>()
			{
				ProjectileID.JungleSpike,
				ProjectileID.IceSpike,
				ProjectileID.QueenBeeStinger,
				ProjectileID.Stinger
			};
		}

		public override void Push(Player player, OrchidGuardian guardian, Projectile shield, NPC npc)
		{
			if (npc.aiStyle == NPCAIStyleID.Spell)
			{
				IEntitySource source = player.GetSource_ItemUse(Item);
				Vector2 direction = Vector2.Normalize(shield.Center - player.Center).RotatedByRandom(MathHelper.ToRadians(5f));
				int projectileDamage = guardian.GetGuardianDamage(Item.damage * 0.5f);
				int projectileTypeDefault = ModContent.ProjectileType<ThoriumIllusionistPaviseMagic>();
				Projectile newProjectileDefault = Projectile.NewProjectileDirect(source, shield.Center + direction * 32f, direction * 4f, projectileTypeDefault, projectileDamage, 3f, player.whoAmI);
				newProjectileDefault.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
				player.ApplyDamageToNPC(npc, Item.damage, 0f, player.direction, Main.rand.Next(100) < shield.CritChance,  ModContent.GetInstance<GuardianDamageClass>());
			}
		}

		public override bool Block(Player player, OrchidGuardian guardian, Projectile shield, Projectile projectile)
		{
			if (IsLocalPlayer(player))
			{
				IEntitySource source = player.GetSource_ItemUse(Item);
				Vector2 direction = Vector2.Normalize(shield.Center - player.Center).RotatedByRandom(MathHelper.ToRadians(5f));

				if (TypesArrow.Contains(projectile.type))
				{ // Arrow
					int projectileDamage = guardian.GetGuardianDamage(Item.damage * 0.5f);
					int projectileType = ModContent.ProjectileType<ThoriumIllusionistPaviseArrow>();
					Projectile newProjectile = Projectile.NewProjectileDirect(source, shield.Center + direction * 32f, direction * 15f, projectileType, projectileDamage, 3f, player.whoAmI);
					newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
					newProjectile.rotation = newProjectile.velocity.ToRotation() + MathHelper.PiOver2;
					return true;
				}

				if (TypesBoulder.Contains(projectile.type))
				{ // Boulder
					int projectileDamage = guardian.GetGuardianDamage(Item.damage);
					int projectileType = ModContent.ProjectileType<ThoriumIllusionistPaviseBoulder>();
					Projectile newProjectile = Projectile.NewProjectileDirect(source, shield.Center + direction * 32f, direction * 10f, projectileType, projectileDamage, 15f, player.whoAmI);
					newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
					return true;
				}

				if (TypesLaser.Contains(projectile.type))
				{ // Lasers / Bullets
					int projectileDamage = guardian.GetGuardianDamage(Item.damage * 0.5f);
					int projectileType = ModContent.ProjectileType<ThoriumIllusionistPaviseLaser>();
					Projectile newProjectile = Projectile.NewProjectileDirect(source, shield.Center + direction * 32f, direction * 2f, projectileType, projectileDamage, 0f, player.whoAmI);
					newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
					newProjectile.rotation = newProjectile.velocity.ToRotation();
					return true;
				}

				if (TypesScythe.Contains(projectile.type))
				{ // Scythe
					int projectileDamage = guardian.GetGuardianDamage(Item.damage * 0.33f);
					int projectileType = ModContent.ProjectileType<ThoriumIllusionistPaviseScythe>();
					Projectile newProjectile = Projectile.NewProjectileDirect(source, shield.Center + direction * 32f, direction * 0.2f, projectileType, projectileDamage, 0f, player.whoAmI);
					newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
					newProjectile.rotation = newProjectile.velocity.ToRotation();
					return true;
				}

				if (TypesStinger.Contains(projectile.type))
				{ // Stinger
					int projectileDamage = guardian.GetGuardianDamage(Item.damage * 0.5f);
					int projectileType = ModContent.ProjectileType<ThoriumIllusionistPaviseStinger>();
					Projectile newProjectile = Projectile.NewProjectileDirect(source, shield.Center + direction * 32f, direction * 12f, projectileType, projectileDamage, 3f, player.whoAmI);
					newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
					newProjectile.rotation = newProjectile.velocity.ToRotation() + MathHelper.PiOver2;
					return true;
				}

				if (TypesFeather.Contains(projectile.type))
				{ // Feather
					int projectileDamage = guardian.GetGuardianDamage(Item.damage * 0.5f);
					int projectileType = ModContent.ProjectileType<ThoriumIllusionistPaviseFeather>();
					Projectile newProjectile = Projectile.NewProjectileDirect(source, shield.Center + direction * 32f, direction * 8f, projectileType, projectileDamage, 0f, player.whoAmI);
					newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
					newProjectile.rotation = newProjectile.velocity.ToRotation();
					return true;
				}

				// default (anything else)
				int projectileDamageDefault = guardian.GetGuardianDamage(Item.damage * 0.5f);
				int projectileTypeDefault = ModContent.ProjectileType<ThoriumIllusionistPaviseMagic>();
				Projectile newProjectileDefault = Projectile.NewProjectileDirect(source, shield.Center + direction * 32f, direction * 4f, projectileTypeDefault, projectileDamageDefault, 3f, player.whoAmI);
				newProjectileDefault.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
				return true;
			}

			return true;
		}
	}
}
