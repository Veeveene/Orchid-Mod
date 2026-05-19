using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using OrchidMod.Common.Attributes;
using OrchidMod.Common.ModObjects;
using OrchidMod.Common;
using OrchidMod.Utilities;
using OrchidMod.Content.Guardian.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using static Terraria.Player;


namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	[CrossmodContent("ThoriumMod")]
	public class ThoriumVoidQuarterstaff : OrchidModGuardianQuarterstaff
	{

		public static Texture2D TextureLine;
		public static Texture2D TextureLineBlur;
		public static Texture2D TextureRing;
		public override void SetStaticDefaults()
		{
			TextureLine ??= ModContent.Request<Texture2D>("OrchidMod/Content/Guardian/Projectiles/Misc/GuardianHorizonLanceProj", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			TextureLineBlur ??= ModContent.Request<Texture2D>("OrchidMod/Content/Guardian/Projectiles/Misc/GuardianHorizonLanceProj_Blur", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			TextureRing ??= ModContent.Request<Texture2D>("OrchidMod/Content/Guardian/StandardAuraProjectile", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
		}

		public override void SafeSetDefaults()
		{
			Item.width = 60;
			Item.height = 56;
			Item.value = Item.sellPrice(0, 4);
			Item.rare = ItemRarityID.Pink;
			Item.useTime = 40;
			ParryDuration = 120;
			Item.knockBack = 6f;
			Item.damage = 200;
			JabChargeGain = 0.4f;
			GuardStacks = 1;
			SwingStyle = 2;
			JabSpeed = 1.0f;
			SwingSpeed = 1.2f;
			CounterSpeed = 1.0f;
		}

		public override void SafeHoldItem(Player player)
		{
			if (player.HasBuff(ModContent.BuffType<GuardianVoidQuarterstaffBuff>())) 
			{
				JabSpeed = 0.3333f;
				SwingSpeed = 0.4f;
				CounterSpeed = 0.3333f;

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
				
				if (IsLocalPlayer(player))
				{
					Main.SetCameraLerp(0.1f, 10);
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
						for (int i = 0; i < 2; i++) SpawnVoidDaggers(player, target, guardian.GetGuardianDamage(Item.damage * 0.012f));
				} 
				else 
				{
					for (int i = 0; i < (player.HasBuff(ModContent.BuffType<GuardianVoidQuarterstaffBuff>()) ? 6 : 4); i++) 
						SpawnVoidDaggers(player, target, guardian.GetGuardianDamage(Item.damage * 0.012f));
				}
			}
		}

		// public override bool PreSwingAI(Player player, OrchidGuardian guardian, Projectile anchor)
		// {
		// 	anchor.rotation = anchor.ai[1] - MathHelper.PiOver4 + (float)Math.Cos(0.102f * (anchor.ai[0] - 9)) * 1.9f * player.direction + MathHelper.Pi;
		// 	anchor.Center = player.MountedCenter.Floor() + Vector2.UnitY.RotatedBy(anchor.ai[1] + (float)Math.Cos(0.102f * (anchor.ai[0] - 9)) * 1.8f * player.direction) * 24f * (1 + 1.2f * (float)Math.Cos(0.102f * (anchor.ai[0] - 9)));
		// 	player.SetCompositeArmFront(true, CompositeArmStretchAmount.Full, MathHelper.PiOver4 * player.direction + anchor.ai[1] + 0.1f - (float)Math.Cos(0.102f * (anchor.ai[0] - 9)) * player.direction);
		// 	player.SetCompositeArmBack(true, CompositeArmStretchAmount.Full, anchor.ai[1] - 0.1f + (float)Math.Cos(0.102f * (anchor.ai[0]- 9)) * 0.2f * player.direction);
			
		// 	return false;
		// }

		int offSet = 0;
		public override void PostDrawQuarterstaff(SpriteBatch spriteBatch, Projectile projectile, Player player, Color lightColor)
		{
			if (player == Main.LocalPlayer && player.GetModPlayer<OrchidGuardian>().GuardianItemCharge >= 180f && projectile.ai[0] is 0 or 1) {
				spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
				spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });

				if (++offSet >= 80) offSet = 0;

				NPC hitNPC = CalcFirstThingInLine(player, 600, out List<Vector2> samplePoints, out Vector2 finalPoint);
				
				for (int i = 0; i < samplePoints.Count; i++) 
				{
					Vector2 reticlePoint = samplePoints[i];
					if (hitNPC != null) reticlePoint = player.Center + (player.Center.DirectionTo(hitNPC.Center) * 4 * (i + 10));

					Color drawColor = Color.Lerp(Color.Purple, Color.Magenta, (((i + offSet) % 80)/80f));
					// if (hitNPC != null) drawColor = Color.Lerp(Color.DarkBlue, Color.Aqua, (((i + offSet) % 80)/80f));

					Vector2 drawPositionLine = Vector2.Transform(reticlePoint - Main.screenPosition, Main.GameViewMatrix.EffectMatrix);

					spriteBatch.Draw(TextureLineBlur, drawPositionLine, null, drawColor * 0.8f, (reticlePoint - player.Center).ToRotation(), TextureLineBlur.Size() * 0.5f, 0.2f, SpriteEffects.None, 0f);
					spriteBatch.Draw(TextureLine, drawPositionLine, null, drawColor, (reticlePoint - player.Center).ToRotation(), TextureLine.Size() * 0.5f, 0.05f, SpriteEffects.None, 0f);

					if (i == samplePoints.Count - 1) {
						if (hitNPC != null) 
						{
							float maxDimension = Math.Max(hitNPC.width / 2f, hitNPC.height / 2f);
							Vector2 drawPositionOutlineAura = Vector2.Transform(hitNPC.Center - Main.screenPosition, Main.GameViewMatrix.EffectMatrix);
							spriteBatch.Draw(TextureRing, drawPositionOutlineAura, null, drawColor, 0f, TextureRing.Size() * 0.5f, 0.007f * maxDimension, SpriteEffects.None, 0f);
						}
						else {
							Vector2 drawPositionReticle = Vector2.Transform(finalPoint - Main.screenPosition, Main.GameViewMatrix.EffectMatrix);
							spriteBatch.Draw(TextureRing, drawPositionReticle, null, drawColor, 0f, TextureRing.Size() * 0.5f, 0.05f, SpriteEffects.None, 0f);
						}
						break;
					}
					
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

		public static NPC CalcFirstThingInLine(Player player, int maxSteps, out List<Vector2> samplePoints, out Vector2 finalPoint)
		{
			NPC hitNPC = null;
			samplePoints = [];
			finalPoint = Vector2.Zero;

			Vector2 velocity = player.Center.DirectionTo(Main.MouseWorld);

			for (int i = 0; i < maxSteps; i++) 
			{
				Vector2 point = player.Center + velocity * i;
				if (i > 40 && i % 4 == 0) samplePoints.Add(point);

				hitNPC = Main.npc.FirstOrDefault(npc => npc.active && npc.whoAmI < Main.maxNPCs && !npc.friendly && Collision.CheckAABBvAABBCollision(point, player.Hitbox.Size(), npc.position, npc.Hitbox.Size()));
				if (hitNPC != null && !hitNPC.dontTakeDamage) 
				{
					finalPoint = point;
					break;
				}

				Tile hitTile = Framing.GetTileSafely((int)(point.X / 16f), (int)(point.Y / 16));
				if (hitTile.HasTile && Main.tileSolid[hitTile.TileType] && !Main.tileSolidTop[hitTile.TileType]) 
				{
					finalPoint = point;
					break;
				} 

				finalPoint = point;
			}

			return hitNPC;
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
