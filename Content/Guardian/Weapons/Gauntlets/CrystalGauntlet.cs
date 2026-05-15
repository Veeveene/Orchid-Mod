using System;
using Microsoft.Xna.Framework;
using OrchidMod.Content.General.Prefixes;
using OrchidMod.Content.Guardian.Projectiles.Gauntlets;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Gauntlets
{
	public class CrystalGauntlet : OrchidModGuardianGauntlet
	{
		public override void SafeSetDefaults()
		{
			Item.width = 36;
			Item.height = 42;
			Item.knockBack = 1f;
			Item.damage = 120;
			Item.value = Item.sellPrice(0, 8, 0, 0);
			Item.rare = ItemRarityID.LightPurple;
			Item.useTime = 8;
			StrikeVelocity = 12.5f;
			ParryDuration = 180;
			PunchSpeed = 1.5f;
			hasBackGauntlet = true;
		}

		public override Color GetColor(bool offHand)
		{
			if (!offHand) return new Color(247, 119, 224); //pink
			else return new Color(119, 179, 247); //blue

			//idk why this is called every frame instead of being cached...
			//i guess you can make pulsing visuals?
			/*return Main.rand.Next(3) switch
			{
				0 => new Color(247, 119, 224),
				1 => new Color(188, 119, 247),
				_ => new Color(119, 179, 247)
			};*/
		}

		public override void PlayGuardSound(Player player, OrchidGuardian guardian, Projectile anchor)
		{
			SoundEngine.PlaySound(SoundID.Item28.WithPitchOffset(-0.1f), player.Center);
		}

		public override void PlayParrySound(Player player, OrchidGuardian guardian, Projectile anchor)
		{
			SoundEngine.PlaySound(SoundID.Item30.WithPitchOffset(clashImmune / -300f), player.Center);
		}

		int clashImmune;

		public override bool PreGuard(Player player, OrchidGuardian guardian, Projectile anchor)
		{
			if (guardian.UseGuard(1))
			{
				clashImmune = Math.Min(player.immuneTime + 40, 180);
				InvincibilityDuration = clashImmune;
				return true;
			}
			else return false;
		}

		public override void OnParryGauntlet(Player player, OrchidGuardian guardian, Entity aggressor, Projectile anchor)
		{
			int shardDamage = player.GetWeaponDamage(Item) / 6;
			float speed = StrikeVelocity * Item.GetGlobalItem<GuardianPrefixItem>().GetSlamDistance() * 0.35f;
			for (int i = 0; i < 4; i++)
			{
				Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), player.Center, new Vector2(i % 2 == 0 ? speed : -speed, i < 2 ? speed : -speed) + player.velocity, ModContent.ProjectileType<CrystalGauntletProjectile>(), shardDamage, player.GetWeaponKnockback(Item), player.whoAmI);
				newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
			}
		}

		public override void SafeHoldItem(Player player)
		{
			//this line is to prevent weird behavior while weapon switching
			//todo later: implement weapon switching hook properly and make it so stored iframes from this weapon are returned if guarding is interrupted
			if (clashImmune > 0 && !player.GetModPlayer<OrchidGuardian>().GuardianParry)
			{
				clashImmune = 0;
				InvincibilityDuration = 40;
			}
			int immunity = Math.Min(Math.Max(player.immuneTime, clashImmune), 180);
			if (immunity > 40)
			{
				int dustType = Main.rand.Next(immunity) switch
				{
					< 60 => DustID.BlueCrystalShard,
					< 120 => DustID.PurpleCrystalShard,
					_ => DustID.UndergroundHallowedEnemies // pinkcrystalshard is ugly for some reason
				};
				//int dustType = Main.rand.NextBool() ? DustID.BlueCrystalShard : DustID.PurpleCrystalShard;
				Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, dustType, Alpha: 100);
				dust.noGravity = true;
				dust.scale *= immunity * 0.01f;
				dust.velocity *= 0.8f + immunity * 0.004f;
				dust.velocity += player.velocity * 0.8f;
			}
		}

		public override bool OnPunch(Player player, OrchidGuardian guardian, Projectile projectile, bool offHandGauntlet, bool fullyManuallyCharged, ref bool charged, ref int damage)
		{
			if (!charged) charged = guardian.UseSlam(1); //always consume slams before jabbing
			if (charged)
			{
				int shardDamage = player.GetWeaponDamage(Item) / 6;
				float speed = StrikeVelocity * Item.GetGlobalItem<GuardianPrefixItem>().GetSlamDistance() * 0.5f;
				Vector2 velocity = Vector2.UnitX.RotatedBy((Main.MouseWorld - player.Center).ToRotation()) * speed;
				Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, velocity.RotatedBy(-0.5f) + player.velocity, ModContent.ProjectileType<CrystalGauntletProjectile>(), shardDamage, player.GetWeaponKnockback(Item), player.whoAmI);
				Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, velocity.RotatedBy(0.5f) + player.velocity, ModContent.ProjectileType<CrystalGauntletProjectile>(), shardDamage, player.GetWeaponKnockback(Item), player.whoAmI);
			}
			return true;
		}
	}
}
