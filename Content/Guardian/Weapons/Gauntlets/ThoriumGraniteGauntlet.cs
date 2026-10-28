using System;
using Microsoft.Xna.Framework;
using OrchidMod.Common.Attributes;
using OrchidMod.Common.ModObjects;
using OrchidMod.Content.Guardian.Projectiles.Gauntlets;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Gauntlets
{
	[CrossmodContent("ThoriumMod")]
	public class ThoriumGraniteGauntlet : OrchidModGuardianGauntlet
	{
		public override void SafeSetDefaults()
		{
			Item.width = 42;
			Item.height = 38;
			Item.knockBack = 10f;
			Item.damage = 128;
			Item.value = Item.sellPrice(0, 0, 50, 0);
			Item.rare = ItemRarityID.Orange;
			Item.useTime = 20;
			StrikeVelocity = 12f;
			ParryDuration = 60;
			hasArm = true;
			hasShoulder = true;
		}

		public override Color GetColor(bool offHand)
		{
			return new Color(0, 192, 255);
		}

		public override bool OnPunch(Player player, OrchidGuardian guardian, Projectile projectile, bool offHandGauntlet, bool fullyManuallyCharged, ref bool charged, ref int damage)
		{
			if (charged && guardian.UseGuard(1, true))
			{
				Vector2 strikeVelocity = Vector2.UnitX.RotatedBy((Main.MouseWorld - player.MountedCenter).ToRotation()) * 8;
				Vector2 strikeEndPosition = projectile.Center + strikeVelocity * 8;
				Rectangle area = new((int)projectile.Center.X, (int)projectile.Center.Y, (int)strikeEndPosition.X, (int)strikeEndPosition.Y);
				if (area.Width < area.X) (area.X, area.Width) = (area.Width, area.X);
				if (area.Height < area.Y) (area.Y, area.Height) = (area.Height, area.Y);
				area.Width -= area.X;
				area.Height -= area.Y;
				area.Inflate(24, 24);
				if (guardian.DoDeflect(area, out bool caughtProjectile, out bool killedNPC, anchor: projectile.ModProjectile, EnemyPredicate: new Predicate<NPC>(T => !T.friendly && !T.CountsAsACritter && (T.lifeMax < 2 || (T.velocity - player.velocity).Length() > 6f)), lenience: 5f))
				{
					guardian.UseGuard(1);
					guardian.DoParryItemParry(null);
					int punchDamage = guardian.GetGuardianDamage(Item.damage);
					if (guardian.DeflectedEnemies.Count > 0)
					{
						guardian.GuardianGuardRecharging += 0.5f; //we hit at least one npc with a slam, so get half a guard
						foreach (NPC target in guardian.DeflectedEnemies) //DoDeflect supports dealing damage as part of the deflect, but we want to manually control the damage and don't have access to DeflectItem methods because this is a ParryItem
						{
							if (!target.dontTakeDamage)
							{
								NPC.HitInfo info = target.CalculateHitInfo(punchDamage, strikeVelocity.X > 1 ? 1 : -1, Main.rand.Next(100) < player.GetWeaponCrit(Item), 1f, ModContent.GetInstance<GuardianDamageClass>());
								if (info.Damage >= target.life) killedNPC = true; //at least one npc was killed, so projectile retains full damage
								else info.Damage /= 2; //npc wasn't killed, so deal half damage
								target.StrikeNPC(info);
							}
						}
					}
					Projectile counterProj = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), projectile.Center + strikeVelocity * 4, Vector2.Zero, ModContent.ProjectileType<ThoriumGraniteGauntletProjectile>(), punchDamage, Item.knockBack, projectile.owner);
					counterProj.CritChance = player.GetWeaponCrit(Item);
					counterProj.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
					if (caughtProjectile || killedNPC) //projectile plays full animation, travels and deals full damage
					{
						counterProj.damage *= 2;
						SoundEngine.PlaySound(SoundID.Item37.WithPitchOffset(0.3f), player.Center);
					}
					else //projectile instantly explodes for half damage
					{
						counterProj.ai[0] = 1;
						counterProj.timeLeft -= 4;
						SoundEngine.PlaySound(SoundID.Item37.WithPitchOffset(0.7f), player.Center);
					}
					return false;
				}
			}
			return true;
		}

		public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				var recipe = CreateRecipe();
				recipe.AddTile(TileID.Anvils);
				recipe.AddIngredient(thoriumMod, "GraniteEnergyCore", 8);
				recipe.Register();
			}
		}
	}
}
