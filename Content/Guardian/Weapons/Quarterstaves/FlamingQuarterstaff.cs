using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	public class FlamingQuarterstaff : OrchidModGuardianQuarterstaff
	{
		public override void SafeSetDefaults()
		{
			Item.width = 38;
			Item.height = 42;
			Item.value = Item.sellPrice(0, 0, 30, 0);
			Item.rare = ItemRarityID.White;
			Item.useTime = 30;
			ParryDuration = 60;
			Item.knockBack = 9f;
			Item.damage = 35;
		}

		public override void PostDrawQuarterstaff(SpriteBatch spriteBatch, Projectile projectile, Player player, Color lightColor)
		{
			Vector2 pos = projectile.Center - Vector2.UnitX.RotatedBy(projectile.rotation + MathHelper.Pi * 0.75f) * 32;
			if (!player.wet || Main.tile[(int)pos.X / 16, (int)pos.Y / 16].CheckingLiquid)
			{
				Main.instance.LoadNPC(25);
				Main.EntitySpriteDraw(TextureAssets.Npc[25].Value, pos + Main.rand.NextVector2Square(-1.5f, 1.5f) + new Vector2(0, player.gfxOffY) - Main.screenPosition, null, new Color(0.5f, 0.5f, 0.5f, 0f), projectile.rotation, new Vector2(8), 0.67f, SpriteEffects.None);
				//Lighting.AddLight(projectile.Center - Vector2.UnitX.RotatedBy(projectile.rotation + MathHelper.Pi * 0.75f) * 32, new Vector3(0.5f));
			}
		}

		public override void ExtraAIQuarterstaff(Player player, OrchidGuardian guardian, Projectile projectile)
		{
			Vector2 pos = projectile.Center - Vector2.UnitX.RotatedBy(projectile.rotation + MathHelper.Pi * 0.75f) * 32;
			if (!player.wet || Main.tile[(int)pos.X / 16, (int)pos.Y / 16].LiquidAmount < (pos.Y + player.gfxOffY) % 16 * 16 && !Main.tile[(int)pos.X / 16, (int)pos.Y / 16].HasUnactuatedTile)
			{
				bool bigAttack = projectile.ai[0] > 14 || projectile.ai[2] < 0;
				for (int i = 0; i < (!bigAttack ? 2 : 3); i++)
				{
					Dust dust = Dust.NewDustDirect(pos - new Vector2(8), 12, 12, DustID.Torch, Scale: Main.rand.NextFloat(0.5f, 1f), SpeedY: -Main.rand.NextFloat(3f));
					switch (Main.rand.Next(10))
					{
						default:
							dust.velocity *= 0.25f;
							dust.velocity += player.velocity * 0.5f;
							dust.scale *= 2.5f;
							goto case 8;
						case 6:
						case 7:
						case 8:
							dust.noGravity = true;
							dust.velocity *= 0.8f;
							if (bigAttack)
							{
								if (projectile.ai[0] > 14) //swing
									dust.velocity += new Vector2(-player.direction * (float)Math.Cos(projectile.ai[0] * 0.2f), -1).RotatedBy(projectile.rotation + MathHelper.PiOver4) * Main.rand.NextFloat(4f, 8f);
								else //counter
									dust.velocity += new Vector2(1, -1).RotatedBy(projectile.rotation + Main.rand.NextFloat(MathHelper.PiOver2)) * Main.rand.NextFloat(8f);
								if (Main.rand.NextBool())
								{
									dust.scale += Main.rand.NextFloat(2f);
									dust.velocity *= Main.rand.NextFloat(0.2f, 0.6f);
								}
								dust.fadeIn += Main.rand.NextFloat(2.5f);
							}
							else if (projectile.ai[0] <= -30 && projectile.ai[0] >= -39) //jab
							{
								dust.velocity += new Vector2(-1, 1).RotatedBy(projectile.rotation) * (projectile.ai[0] + 30) * Main.rand.NextFloat(0.6f, 1.2f);
								dust.fadeIn += Main.rand.NextFloat(1f);
							}
							break;
						case 9:
							dust.scale *= Main.rand.NextFloat(0.5f, 1f);
							break;
					}
				}
			}
		}

		public override void OnHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool jabAttack, bool counterAttack)
		{
			target.AddBuff(BuffID.OnFire, jabAttack ? 45 : 180);
		}

		public override void OnHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool jabAttack, bool counterAttack)
		{
			if (!jabAttack)
			{
				target.AddBuff(BuffID.OnFire3, 40);
			}
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddIngredient<Quarterstaff>();
			recipe.AddIngredient(ItemID.Torch, 99);
			recipe.Register();
		}
	}
}
