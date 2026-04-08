using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using OrchidMod.Common.Attributes;
using OrchidMod.Common.ModObjects;
using OrchidMod.Common;
using OrchidMod.Utilities;
using OrchidMod.Content.Guardian.Buffs;
using Terraria.Audio;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	[CrossmodContent("ThoriumMod")]
	public class ThoriumVoidQuarterstaff : OrchidModGuardianQuarterstaff
	{
		public static Texture2D TextureAura;
		public override void SetStaticDefaults()
		{
			TextureAura ??= ModContent.Request<Texture2D>("OrchidMod/Content/Guardian/StandardAuraProjectile", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
		}

		public override void SafeSetDefaults()
		{
			Item.width = 60;
			Item.height = 56;
			Item.value = Item.sellPrice(0, 4);
			Item.rare = ItemRarityID.Pink;
			Item.useTime = 60;
			ParryDuration = 120;
			Item.knockBack = 6f;
			Item.damage = 150;
			JabChargeGain = 0.25f;
			SlamStacks = 2;
			SwingStyle = 2;
			JabSpeed = 1.0f;
			SwingSpeed = 1.2f;
			CounterSpeed = 1.0f;
		}

		public override void SafeHoldItem(Player player)
		{
			if (player.HasBuff(ModContent.BuffType<GuardianVoidQuarterstaffBuff>())) 
			{
				JabSpeed = 0.1667f;
				SwingSpeed = 0.2f;
				CounterSpeed = 0.1667f;

			}
			else {
				JabSpeed = 1.0f;
				SwingSpeed = 1.2f;
				CounterSpeed = 1.0f;
			}
		}

		public override void ExtraAIQuarterstaff(Player player, OrchidGuardian guardian, Projectile projectile)
		{
			if (projectile.ModProjectile is GuardianQuarterstaffAnchor anchor) {
				Vector2 tipPosition = projectile.Center - Vector2.UnitY.RotatedBy(projectile.rotation + MathHelper.PiOver4) * projectile.width * 0.4f;
				if (player.direction == 1) tipPosition.X -= 12;

				projectile.localAI[0]--;
				if (projectile.localAI[0] < 0) projectile.localAI[0] = 0;

				if (OrchidMod.ThoriumMod != null && Main.rand.NextBool(10))
				{
					int dustType = OrchidMod.ThoriumMod.Find<ModDust>("VoidHeartDust").Type;
					Dust.NewDustDirect(tipPosition, 16, 16, dustType);
				}
				if (player.HasBuff<GuardianVoidQuarterstaffBuff>() && guardian.GuardianItemCharge >= 180f && !anchor.Ding)
				{ // Try to fix sound cue not working consistently while supercharged
					anchor.Ding = true;
					if (ModContent.GetInstance<OrchidClientConfig>().GuardianAltChargeSounds) SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, player.Center);
					else SoundEngine.PlaySound(SoundID.MaxMana, player.Center);
				}
			}
		}

		public override void OnAttack(Player player, OrchidGuardian guardian, Projectile projectile, bool jabAttack, bool counterAttack) 
		{ 
			if (OrchidMod.ThoriumMod != null && !jabAttack && !counterAttack) 
			{
				projectile.scale *= 1.25f;
				projectile.width = (int)(projectile.width * 1.25f);
				projectile.height = (int)(projectile.height * 1.25f);	

        		Vector2 tipPosition = projectile.Center - Vector2.UnitY.RotatedBy(projectile.rotation + MathHelper.PiOver4) * projectile.width * 0.5f;
				if (player.direction == 1) tipPosition.X -= 12;

				Vector2 velocity = Vector2.UnitY.RotatedBy((player.Center - Main.MouseWorld).ToRotation() + MathHelper.PiOver2);
				

				int dustType = OrchidMod.ThoriumMod.Find<ModDust>("VoidHeartDust").Type;

				for (int i = 0; i < 10; i++) {
					Dust.NewDustDirect(player.Center, 24, 24, dustType, Scale: 3f);
				}

				int distance = 0;
				for (int i = 0; i < 600; i++) 
				{
					Vector2 point = player.Center + velocity * i;
					if (i > 40 && i % 20 == 0 && Main.rand.NextBool(10))
					{
						Dust dust = Dust.NewDustPerfect(point, dustType);
						dust.noGravity = true;
					}
					distance++;

					NPC hitEnemy = Main.npc.FirstOrDefault(npc => npc.active && npc.whoAmI < Main.maxNPCs && !npc.friendly && Collision.CheckAABBvAABBCollision(point, player.Hitbox.Size(), npc.position, npc.Hitbox.Size()));
					Tile hitTile = Framing.GetTileSafely((int)(point.X / 16f), (int)(point.Y / 16));
					if (hitEnemy != null || (hitTile.HasTile && Main.tileSolid[hitTile.TileType] && !Main.tileSolidTop[hitTile.TileType])) 
						break;
				}

				player.RemoveAllGrapplingHooks();
				player.Center += velocity * (distance - 20);

				projectile.localAI[0] = 10;
			
				SoundEngine.PlaySound(SoundID.Item8 with {Volume = 4f});
			}
		}

		public override void OnHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool jabAttack, bool counterAttack)
		{
			if (OrchidMod.ThoriumMod != null)
			{
                int debuffType = OrchidMod.ThoriumMod.Find<ModBuff>("LightCurse").Type;
				target.AddBuff(debuffType, jabAttack ? 45 : 180);
			}
		}

		public override void OnHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool jabAttack, bool counterAttack)
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (!jabAttack && thoriumMod != null)
			{
                int debuffType = thoriumMod.Find<ModBuff>("LightCurse").Type;
				target.AddBuff(debuffType, 180);
				if (!counterAttack) {
					player.AddBuff(ModContent.BuffType<GuardianVoidQuarterstaffBuff>(), 240);	

					if (projectile.localAI[0] > 0 && !player.immune) {
						player.GetModPlayer<OrchidPlayer>().PlayerImmunity = 40;
						player.immuneTime = 40;
						player.immune = true;
						projectile.localAI[0] = 0;
					}
									

					float launchAngle = 45f;
					float launchPower = 13.5f;
					bool reduceGain = player.controlDown;
					bool increaseGain = player.controlUp;
					if (player.gravDir == -1) (reduceGain, increaseGain) = (increaseGain, reduceGain);

					if (reduceGain) launchAngle -= 22.5f;
					else if (increaseGain) launchAngle += 22.5f; 

					player.velocity.Y = -launchPower * (float)Math.Sin(MathHelper.ToRadians(launchAngle)) * player.gravDir;
					if (player.direction == 1)
						player.velocity.X = -launchPower * (float)Math.Cos(MathHelper.ToRadians(launchAngle));
					else
						player.velocity.X = launchPower * (float)Math.Cos(MathHelper.ToRadians(launchAngle));

					if (player.HasBuff(ModContent.BuffType<GuardianVoidQuarterstaffBuff>()))
						SpawnVoidDaggers(player, target, guardian.GetGuardianDamage(Item.damage * 0.015f));
				} 
				else 
				{
					for (int i = 0; i < (player.HasBuff(ModContent.BuffType<GuardianVoidQuarterstaffBuff>()) ? 5 : 3); i++) 
						SpawnVoidDaggers(player, target, guardian.GetGuardianDamage(Item.damage * 0.012f));
				}
			}
		}

		public override void PostDrawQuarterstaff(SpriteBatch spriteBatch, Projectile projectile, Player player, Color lightColor)
		{
			if (player.GetModPlayer<OrchidGuardian>().GuardianItemCharge >= 180f && projectile.ai[0] is 0 or 1) {
				spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
				spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });
				// float alphamult = 0.2f + Math.Abs((Main.LocalPlayer.GetModPlayer<OrchidPlayer>().Timer120 - 60) / 240f);
				// Vector2 drawPositionAura = Vector2.Transform(player.Center.Floor() - Main.screenPosition + Vector2.UnitY * player.gfxOffY, Main.GameViewMatrix.EffectMatrix);
				// spriteBatch.Draw(TextureAura, drawPositionAura, null, new Color(188, 0, 163) * alphamult, 0f, TextureAura.Size() * 0.5f, 4.2f, SpriteEffects.None, 0f);

				for (int i = 0; i < 600; i++) 
				{
					Vector2 velocity = Vector2.UnitY.RotatedBy((player.Center - Main.MouseWorld).ToRotation() + MathHelper.PiOver2);
					Vector2 point = player.Center + velocity * i;

					if (OrchidMod.ThoriumMod != null && i > 40 && i % 20 == 0 && Main.rand.NextBool(10))
					{
						int dustType = OrchidMod.ThoriumMod.Find<ModDust>("VoidHeartDust").Type;
						Dust dust = Dust.NewDustPerfect(point, dustType);
						dust.noGravity = true;
					}

					NPC hitEnemy = Main.npc.FirstOrDefault(npc => npc.active && npc.whoAmI < Main.maxNPCs && !npc.friendly && Collision.CheckAABBvAABBCollision(point, player.Hitbox.Size(), npc.position, npc.Hitbox.Size()));
					if (hitEnemy != null) 
					{
						float maxDimension = Math.Max(hitEnemy.width / 2f, hitEnemy.height / 2f);
						Vector2 drawPositionOutlineAura = Vector2.Transform(hitEnemy.Center.Floor() - Main.screenPosition, Main.GameViewMatrix.EffectMatrix);
						spriteBatch.Draw(TextureAura, drawPositionOutlineAura, null, new Color(188, 0, 163), 0f, TextureAura.Size() * 0.5f, 0.007f * maxDimension, SpriteEffects.None, 0f);
						break;
					}

					Tile hitTile = Framing.GetTileSafely((int)(point.X / 16f), (int)(point.Y / 16));
					if (hitTile.HasTile && Main.tileSolid[hitTile.TileType] && !Main.tileSolidTop[hitTile.TileType])
						break;

				}

				spriteBatch.End();
				spriteBatch.Begin(spriteBatchSnapshot);
			}
			
		}

		public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (OrchidMod.ThoriumMod != null)
			{
				CreateRecipe()
				.AddTile(TileID.MythrilAnvil)
				.AddIngredient(thoriumMod, "VoidHeart")
				.AddIngredient(ItemID.HallowedBar, 15)
				.AddIngredient(ItemID.SoulofNight, 8)
				.AddIngredient(ItemID.Ebonwood, 60)
				.Register();

				CreateRecipe()
				.AddTile(TileID.MythrilAnvil)
				.AddIngredient(thoriumMod, "VoidHeart")
				.AddIngredient(ItemID.HallowedBar, 15)
				.AddIngredient(ItemID.SoulofNight, 8)
				.AddIngredient(ItemID.Shadewood, 60)
				.Register();
			}
		}

		public void SpawnVoidDaggers(Player player, NPC target, int damage) 
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod == null) return;
			int projType = thoriumMod.Find<ModProjectile>("WrithingSheathPro").Type;
			float maxDimension = Math.Max(target.width / 2f, target.height / 2f);
			
			Vector2 direction =  Vector2.UnitX.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi * 12.5f / 9f, MathHelper.Pi * 12.5f / 9f));
			Projectile proj = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), target.Center + direction * (maxDimension + 60), Vector2.Zero, projType, damage, 4f, Main.myPlayer, target.whoAmI);
			proj.DamageType = ModContent.GetInstance<GuardianDamageClass>();
		}
	}
}
