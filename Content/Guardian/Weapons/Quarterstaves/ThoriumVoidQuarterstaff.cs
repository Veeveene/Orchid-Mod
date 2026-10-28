using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using OrchidMod.Assets;
using OrchidMod.Common.Attributes;
using OrchidMod.Common.ModObjects;
using OrchidMod.Common;
using OrchidMod.Utilities;
using OrchidMod.Content.Guardian.Buffs;
using OrchidMod.Content.Guardian.Projectiles.Quarterstaves;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;


namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	[CrossmodContent("ThoriumMod")]
	public class ThoriumVoidQuarterstaff : OrchidModGuardianQuarterstaff
	{
		public static Texture2D TextureLine;
		public static Texture2D TextureLineBlur;
		public static Texture2D TextureRing;
		public static Texture2D StaffTextureAura;

		public int HitNPCIndex = -1;
		
		public override void SafeSetStaticDefaults()
		{
			TextureLine ??= ModContent.Request<Texture2D>("OrchidMod/Content/Guardian/Projectiles/Misc/GuardianHorizonLanceProj", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			TextureLineBlur ??= ModContent.Request<Texture2D>("OrchidMod/Content/Guardian/Projectiles/Misc/GuardianHorizonLanceProj_Blur", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			TextureRing ??= OrchidAssets.GetExtraTexture(20, AssetRequestMode.ImmediateLoad).Value;
			StaffTextureAura ??= ModContent.Request<Texture2D>(QuarterstaffTexture + "_Aura", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			
			ItemID.Sets.GamepadWholeScreenUseRange[Type] = true;
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
			SwingStyle = 2;
			JabSpeed = 1.0f;
			SwingSpeed = 1.2f;
			CounterSpeed = 1.0f;
			ChargeRate = 0.5f;
		}

		public override void SafeHoldItem(Player player)
		{
			// ChargeRate = player.HasBuff(ModContent.BuffType<GuardianVoidQuarterstaffBuff>()) ? 2f : 0.5f;
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
				Vector2 oldCenter = projectile.Center;
				projectile.scale *= 1.25f;
				projectile.width = (int)(projectile.width * 1.25f);
				projectile.height = (int)(projectile.height * 1.25f);
				projectile.Center = oldCenter;

        		Vector2 tipPosition = projectile.Center - Vector2.UnitY.RotatedBy(projectile.rotation + MathHelper.PiOver4) * projectile.width * 0.5f;
				if (player.direction == 1) tipPosition.X -= 12;

				int dustType = OrchidMod.ThoriumMod.Find<ModDust>("VoidHeartDust").Type;

				for (int i = 0; i < 10; i++) {
					Dust.NewDustDirect(player.Center, 24, 24, dustType, Scale: 3f);
				}

				Vector2 target = CalcFirstThingInLine(player, 600, 1f, out float width, out NPC hitNPC);
				
				if (IsLocalPlayer(player)) Main.SetCameraLerp(0.1f, 10);

				player.RemoveAllGrapplingHooks();

				Vector2 oldPosition = player.Center;
				Vector2 oldProjCenter = projectile.Center;
				
				Vector2 segment = Vector2.Normalize(target - player.Center);
				Vector2 finalPosition = target - player.Size * 0.5f + segment * width;
				if (hitNPC != null) finalPosition = hitNPC.CircularizeHitbox().ClosestPointInCircle(player.MountedCenter);
				player.DoQueenSlimeHookTeleport(finalPosition.Floor());
				projectile.Center += (player.position - oldPosition);
				projectile.netUpdate = true;

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
						for (int i = 0; i < 3; i++) SpawnVoidBlobs(player, target, guardian.GetGuardianDamage(Item.damage * 0.5f));
				} 
				else 
				{
					for (int i = 0; i < (player.HasBuff(ModContent.BuffType<GuardianVoidQuarterstaffBuff>()) ? 6 : 4); i++) 
						SpawnVoidBlobs(player, target, guardian.GetGuardianDamage(Item.damage * 0.5f));
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

		public override bool PreDrawQuarterstaff(SpriteBatch spriteBatch, Projectile projectile, Player player, ref Color lightColor, SpriteEffects effects)
		{
			if (player.HasBuff<GuardianVoidQuarterstaffBuff>())
			{
				Vector2 drawPos = (projectile.Center + player.getGFXOffY()).Floor() - Main.screenPosition;
				
				float rotationoffset = -MathHelper.PiOver4 + MathHelper.PiOver4 * player.direction;
				if (StaffTextureAura != null)
				{
					spriteBatch.End(out SpriteBatchSnapshot snapshot);
					spriteBatch.Begin(snapshot with { BlendState = BlendState.Additive, RasterizerState = RasterizerState.CullNone });

					for (int i = -1; i < 2; i++)
					{
						Color color = Color.Lerp(Color.Purple, Color.Magenta, (player.miscCounter + 30f * (i + 1)  % 60) / 60f) * (0.6f + 0.2f * (Math.Abs(i) + 1));
						Vector2 shift = Vector2.UnitX.RotatedBy((player.miscCounterNormalized) * 3f * MathHelper.TwoPi) * Utils.Turn01ToCyclic010(player.miscCounterNormalized * 4f % 4) * 4f * projectile.scale * i;
						Main.EntitySpriteDraw(StaffTextureAura, drawPos + shift, null, color, projectile.rotation + rotationoffset, StaffTextureAura.Size() * 0.5f, projectile.scale * 1.02f, effects);
					}
					
					spriteBatch.End();
					spriteBatch.Begin(snapshot);
				}
			}
			return true;
		}

		int offSet = 0;
		private Vector2 finalPoint;
		private float width;
		private NPC hitNPC;

		private int oldMiscCounter;
		public override void PostDrawQuarterstaff(SpriteBatch spriteBatch, Projectile projectile, Player player, Color lightColor, SpriteEffects effects)
		{
			if (player == Main.LocalPlayer && player.GetModPlayer<OrchidGuardian>().GuardianItemCharge >= 180f && projectile.ai[0] is 0 or 1)
			{
				OrchidPlayer modPlayer = player.OrchidPlayer();
				OrchidGuardian guardian = player.Guardian();
				
				spriteBatch.End(out SpriteBatchSnapshot snapshot);
				spriteBatch.Begin(snapshot with { BlendState = BlendState.Additive, RasterizerState = RasterizerState.CullNone });

				float miscCountAngle = player.miscCounterNormalized * MathHelper.TwoPi;

				Vector2 drawPositionRing = Vector2.Transform(player.VisualMountedCenter() - Main.screenPosition, Main.GameViewMatrix.EffectMatrix);
				
				spriteBatch.Draw(TextureRing, drawPositionRing , null, new Color(120, 0, 181) * 0.5f, miscCountAngle, TextureRing.Size() * 0.5f, 0.28f + 0.007f * MathF.Sin(player.miscCounterNormalized * MathHelper.TwoPi), SpriteEffects.None, 0f);
				spriteBatch.Draw(TextureRing, drawPositionRing, null, Color.Lerp(new Color(255, 58, 228), new Color(57, 0, 86), player.miscCounter % 60) * 0.5f, -miscCountAngle, TextureRing.Size() * 0.5f, 0.28f + 0.014f * MathF.Cos(player.miscCounterNormalized * MathHelper.TwoPi * 2.01f), SpriteEffects.None, 0f);

				finalPoint = CalcFirstThingInLine(player, 400, 1.5f, out width, out hitNPC);

				Vector2 chainDirection = finalPoint - player.VisualMountedCenter();
				Vector2 segment = Vector2.Normalize(chainDirection) * TextureLine.Height * 0.3f;
				
				int nbSegments = 0;

				while(chainDirection.Length() > (segment * nbSegments).Length()) nbSegments++;

				int nbSegmentsMax = nbSegments;
				
				if (++offSet >= 80) offSet = 0;
				while (nbSegments > 0)
				{
					Color drawColor = Color.Lerp(Color.Purple, Color.Magenta, (player.miscCounter - 2 * offSet % 80) % 150 / 150f);

					if (nbSegments == nbSegmentsMax)
					{
						Vector2 drawPositionReticle = Vector2.Transform(finalPoint - Main.screenPosition, Main.GameViewMatrix.EffectMatrix);
						spriteBatch.Draw(TextureRing, drawPositionReticle, null, drawColor, player.miscCounterNormalized * MathHelper.TwoPi, TextureRing.Size() * 0.5f, 0.007f * width, SpriteEffects.None, 0f);
					}
					
					nbSegments--;
					chainDirection -= segment;
					
					Vector2 reticlePoint = player.Center + chainDirection;
					Vector2 drawPositionLine = Vector2.Transform(reticlePoint - Main.screenPosition, Main.GameViewMatrix.EffectMatrix);
					
					if (hitNPC != null && nbSegments > nbSegmentsMax - (int)(width * 0.25f)) continue;
					
					spriteBatch.Draw(TextureLineBlur, drawPositionLine, null, drawColor * 0.8f, segment.ToRotation(), TextureLineBlur.Size() * 0.5f, 0.2f, SpriteEffects.None, 0f);
					spriteBatch.Draw(TextureLine, drawPositionLine, null, drawColor, segment.ToRotation(), TextureLine.Size() * 0.5f, 0.05f, SpriteEffects.None, 0f);

					if (nbSegments <= 10f) return;
				}

				spriteBatch.End();
				spriteBatch.Begin(snapshot);
			}
			
			oldMiscCounter = (int)Main.timeForVisualEffects;
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

		public static Vector2 CalcFirstThingInLine(Player player, int maxSteps, float stepLength, out float width, out NPC hitNPC)
		{
			hitNPC = null;
			width = 7.143f;
			
			Vector2 finalPoint = player.VisualMountedCenter();

			if (player.whoAmI != Main.myPlayer || Main.dedServ) return finalPoint;
			

			Vector2 velocity = player.Center.DirectionTo(Main.MouseWorld) * stepLength;

			for (int i = 0; i < maxSteps; i++) 
			{
				Vector2 point = finalPoint + velocity;

				hitNPC = Main.npc.FirstOrDefault(npc => OrchidModProjectile.IsValidTarget(npc) && npc.CircularizeHitbox(2f).HasPointInCircle(point));
				if (hitNPC != null)
				{
					width = hitNPC.GetLargestDimension() * 0.5f;
					return hitNPC.Center;
				}

				Tile hitTile = Framing.GetTileSafely(point);
				if (hitTile.HasTile && Main.tileSolid[hitTile.TileType] && !Main.tileSolidTop[hitTile.TileType])
				{
					finalPoint -= velocity;
					
					Vector2 intendedVelocity = player.velocity * 0.05f;
					Vector2 addedVelocity = Vector2.Zero;

					for (int j = 0; j < 20; j++)
						addedVelocity += Collision.TileCollision(finalPoint - player.Size * 0.4f + addedVelocity, intendedVelocity, (int)(player.width * 0.8f), (int)(player.height * 0.8f), false, false, (int)player.gravDir);

					return finalPoint + addedVelocity;
				}

				finalPoint += velocity;
			}

			return finalPoint;
		}

		public void SpawnVoidBlobs(Player player, NPC target, int damage) 
		{
			// var thoriumMod = OrchidMod.ThoriumMod;
			// if (thoriumMod == null) return;
			// int projType = thoriumMod.Find<ModProjectile>("WrithingSheathPro").Type;
			// float maxDimension = Math.Max(target.width / 2f, target.height / 2f);
			//
			int projType = ModContent.ProjectileType<ThoriumVoidQuarterstaffProjectile>();
			Vector2 direction = Main.rand.NextVector2Unit();
			// Projectile proj = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), target.Center + direction * (maxDimension + 60), Vector2.Zero, projType, damage, 4f, Main.myPlayer, target.whoAmI);
			// proj.DamageType = ModContent.GetInstance<GuardianDamageClass>();

			Projectile.NewProjectileDirect(Item.GetSource_FromThis(), player.Center + player.velocity + direction * target.GetLargestDimension() * 0.5f, direction * 20f, projType, damage, Item.knockBack * 0.25f, ai1: target.whoAmI);
		}
	}
}
