using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using OrchidMod.Common.Attributes;
using OrchidMod.Common.ModObjects;
using OrchidMod.Content.Guardian.Projectiles.Gauntlets;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.NPC;

namespace OrchidMod.Content.Guardian.Weapons.Gauntlets
{
	[CrossmodContent("ThoriumMod")]
	public class ThoriumDemonBloodGauntlet : OrchidModGuardianGauntlet
	{

		private static Mod ThoriumMod;
		private static Type ThoriumPlayer;
		private static FieldInfo BloodHarvest;
		private static FieldInfo BloodCharge;

		public ModPlayer ThoriumPlayerInstance;
		public bool? BloodHarvestInstance
		{
			get => (bool)BloodHarvest?.GetValue(ThoriumPlayerInstance);
			set => BloodHarvest?.SetValue(ThoriumPlayerInstance, value);
		}
		public int? BloodChargeInstance
		{
			get => (int)BloodCharge?.GetValue(ThoriumPlayerInstance);
			set => BloodCharge?.SetValue(ThoriumPlayerInstance, value);
		}

		public override void Load() 
		{
			var thoriumMod = OrchidMod.ThoriumMod;
				if (thoriumMod != null)
				{
					ThoriumMod = thoriumMod;
					ThoriumPlayer = thoriumMod.Code.GetType("ThoriumMod.ThoriumPlayer");
					BloodHarvest = ThoriumPlayer.GetType().GetField("bloodHarvest", BindingFlags.Public | BindingFlags.Instance);
					BloodCharge = ThoriumPlayer.GetType().GetField("bloodHarvest", BindingFlags.Public | BindingFlags.Instance);
				}
		}

		public override void Unload()
		{
			ThoriumMod = null;
			ThoriumPlayer = null;
			BloodHarvest = null;
			BloodCharge = null;

			ThoriumPlayerInstance = null;
			BloodHarvestInstance = null;
			BloodChargeInstance = null;
		}

		public override void SafeSetDefaults()
		{
			Item.width = 46;
			Item.height = 46;
			Item.knockBack = 14f;
			Item.damage = 512;
			Item.value = Item.sellPrice(0, 4, 68, 0);
			Item.rare = ItemRarityID.Yellow;
			Item.useTime = 25;
			StrikeVelocity = 20f;
			ParryDuration = 120;
			hasArm = true;
			hasShoulder = true;
		}

		public override Color GetColor(bool offHand)
		{
			return new Color(255, 32, 62);
		}

		public override bool OnPunch(Player player, OrchidGuardian guardian, Projectile projectile, bool offHandGauntlet, bool fullyManuallyCharged, ref bool charged, ref int damage)
		{
			if (charged && guardian.UseGuard(1, true))
			{
				bool deflect = false;
				bool instantExplode = true;
				Vector2 strikeVelocity = Vector2.UnitY.RotatedBy((Main.MouseWorld - player.MountedCenter).ToRotation() - MathHelper.PiOver2) * 8;
				Vector2 strikeEndPosition = projectile.Center + strikeVelocity * 10;
				int punchDamage = guardian.GetGuardianDamage(Item.damage);
				int highestDeflectedDamage = 0;
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile deflectProj = Main.projectile[i];
					if(deflectProj.active && deflectProj.hostile && deflectProj.damage > 0 && Collision.CheckAABBvLineCollision(deflectProj.position + deflectProj.velocity - new Vector2(16), new Vector2(deflectProj.width + 32, deflectProj.height + 32), projectile.Center, strikeEndPosition))
					{
						if (!deflect && guardian.UseGuard())
						{
							deflect = true;
							guardian.OnBlockProjectileFirst(projectile, deflectProj);
						}
						if (deflect)
						{
							instantExplode = false;
							guardian.OnBlockProjectile(projectile, deflectProj);
							if (deflectProj.damage > highestDeflectedDamage) highestDeflectedDamage = deflectProj.damage;
							deflectProj.Kill();
						}
					}
				}
				for (int i = 0; i < Main.maxNPCs; i++)
				{
					NPC deflectEnemy = Main.npc[i];
					if (deflectEnemy.active && !deflectEnemy.friendly && Collision.CheckAABBvLineCollision(deflectEnemy.position + deflectEnemy.velocity - new Vector2(16), new Vector2(deflectEnemy.width + 32, deflectEnemy.height + 32), projectile.Center, strikeEndPosition) && (deflectEnemy.lifeMax < 2 || (deflectEnemy.velocity - player.velocity).Length() > 6f))
					{
						if (!deflect && guardian.UseGuard())
						{
							deflect = true;
							guardian.OnBlockNPCFirst(projectile, deflectEnemy);
						}
						if (deflect)
						{
							guardian.OnBlockNPC(projectile, deflectEnemy);
							if (deflectEnemy.damage > highestDeflectedDamage) highestDeflectedDamage = deflectEnemy.damage;
							if (!deflectEnemy.dontTakeDamage)
							{
								NPC.HitInfo info = deflectEnemy.CalculateHitInfo(punchDamage, strikeVelocity.X > 1 ? 1 : -1, false, 1f, ModContent.GetInstance<GuardianDamageClass>());
								if (info.Damage >= deflectEnemy.life) instantExplode = false; 
								deflectEnemy.StrikeNPC(info);
								
								if (ThoriumMod != null && BloodHarvest != null && !(bool)BloodHarvestInstance && BloodCharge != null && BloodChargeInstance < 5) BloodChargeInstance++;
								else BloodChargeInstance = 4;
							}
						}
					}
				}
				if (deflect)
				{
					//SoundEngine.PlaySound(SoundID.Item106.WithPitchOffset(0.2f), player.Center);
					//SoundEngine.PlaySound(SoundID.Item72, player.Center);
					//player.GetModPlayer<OrchidPlayer>().PlayerImmunity = player.immuneTime = InvincibilityDuration;
					//player.immune = true;
					guardian.DoParryItemParry(null);
					Projectile counterProj = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), projectile.Center + strikeVelocity * 4, Vector2.Zero, ModContent.ProjectileType<ThoriumDemonBloodGauntletProjectile>(), Math.Clamp(highestDeflectedDamage, punchDamage, 1000), Item.knockBack, projectile.owner);
					counterProj.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
					counterProj.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
					if (!instantExplode)
					{
						counterProj.damage = (int)(counterProj.damage * 1.5f);
						SoundEngine.PlaySound(SoundID.Item37.WithPitchOffset(0.4f), player.Center);
					}
					else
					{
						counterProj.ai[0] = 1;
						counterProj.timeLeft -= 4;
						SoundEngine.PlaySound(SoundID.Item37.WithPitchOffset(0.6f), player.Center);
					}
					guardian.modPlayer.TryHeal(20);
					return false;
				}
			}
			return true;
		}

		public override void OnHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, HitInfo hit, bool charged)
		{
			if (charged) {
				if (ThoriumMod != null && BloodHarvest != null && !(bool)BloodHarvestInstance && BloodCharge != null && BloodChargeInstance < 5) 
				{
					BloodChargeInstance = 0;
					int bloodHarvestDebuff = ThoriumMod.Find<ModBuff>("BloodHarvest").Type;
					player.AddBuff(bloodHarvestDebuff, 600);
				}
			}
			
		}


		public override void AddRecipes()
		{
			if (ThoriumMod != null)
			{
				CreateRecipe()
				.AddIngredient(ThoriumMod, "DemonBloodShard", 8)
				.AddTile(ThoriumMod, "SoulForgeNew")
				.Register();
			}
		}
	}
}
