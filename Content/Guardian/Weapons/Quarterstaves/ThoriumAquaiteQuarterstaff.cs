using Microsoft.Xna.Framework;
using OrchidMod.Content.Guardian.Projectiles.Quarterstaves;
using System;
using Terraria;
using Terraria.Localization;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using OrchidMod.Common.Attributes;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	[CrossmodContent("ThoriumMod")]
	public class ThoriumAquaiteQuarterstaff : OrchidModGuardianQuarterstaff
	{
		public bool bonusChargeHit;
		
		public override void SafeSetDefaults()
		{
			Item.width = 42;
			Item.height = 42;
			Item.value = Item.sellPrice(0, 0, 30, 0);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item71.WithPitchOffset(0.5f).WithVolumeScale(0.5f);
			Item.useTime = 20;
			ParryDuration = 45;
			Item.knockBack = 5f;
			Item.damage = 44;
			Item.shootSpeed = 26f;
			JabStyle = 1;
			JabSpeed = 0.9f;
			JabChargeGain = 1.5f;
			ChargedDamage = 0.5f;
			SwingStyle = 0;
			SwingSpeed = 1.4f;
			CounterSpeed = 1.5f;
		}

		public override void OnHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool jabAttack, bool counterAttack)
		{
			if (jabAttack)
			{
				GuardianQuarterstaffAnchor anchor = projectile.ModProjectile as GuardianQuarterstaffAnchor;
				if (anchor.DamageReset == 0)
				{
					bonusChargeHit = true;
				}
			}
			else if (!counterAttack)
			{
				Projectile.perIDStaticNPCImmunity[ModContent.ProjectileType<ThoriumAquaiteQuarterstaffProjectile>()][target.whoAmI] = Main.GameUpdateCount + 20;
			}
		}

		public override void OnHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool jabAttack, bool counterAttack)
		{
			if (jabAttack && bonusChargeHit && guardian.GuardianItemCharge < 180 && guardian.GuardianItemCharge > 0)
			{
				GuardianQuarterstaffAnchor anchor = projectile.ModProjectile as GuardianQuarterstaffAnchor;
				if (anchor.DamageReset == 1)
				{
					guardian.GuardianItemCharge += 90 * player.GetTotalAttackSpeed(DamageClass.Melee);
					if (guardian.GuardianItemCharge > 180) guardian.GuardianItemCharge = 180;
				}	
			}
		}

		public override void OnAttack(Player player, OrchidGuardian guardian, Projectile projectile, bool jabAttack, bool counterAttack)
		{
			if (jabAttack)
			{
				bonusChargeHit = false;
			}
			else if (!counterAttack)
			{
				SoundEngine.PlaySound(SoundID.Item66, player.Center);
				Vector2 vel = -Vector2.UnitX.RotatedBy((player.Center - Main.MouseWorld).ToRotation()) * Item.shootSpeed;
				Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromAI(), player.Center, vel, ModContent.ProjectileType<ThoriumAquaiteQuarterstaffProjectile>(), (int)(Item.damage * 2.5f), Item.knockBack * 2, projectile.owner);
				newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
			}
		}

		public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				var recipe = CreateRecipe();
				recipe.AddTile(TileID.Anvils);
				recipe.AddIngredient(thoriumMod, "AquaiteBar", 12);
				recipe.AddIngredient(thoriumMod, "DepthScale", 4);
				recipe.Register();
			}
		}
	}
}
