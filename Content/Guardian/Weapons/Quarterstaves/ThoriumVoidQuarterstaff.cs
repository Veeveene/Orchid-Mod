using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using OrchidMod.Assets;
using OrchidMod.Common.Attributes;
using OrchidMod.Common.ModObjects;
using OrchidMod.Common.Global.Projectiles;
using OrchidMod.Utilities;
using OrchidMod.Content.Guardian.Buffs;
using OrchidMod.Content.Guardian.Projectiles.Quarterstaves;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Localization;


namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	[CrossmodContent("ThoriumMod")]
	public class ThoriumVoidQuarterstaff : OrchidModGuardianQuarterstaff
	{
		public static Texture2D TextureLine;
		public static Texture2D TextureLineBlur;
		public static Texture2D TextureRing;
		public static Texture2D StaffTextureAura;
		public int[] Offset;
		
		public static Texture2D TextureTrail;
		public static Texture2D TextureTrailGlow;
		public List<Vector2> OldPositionTrail;
		public List<float> OldSizeTrail;

		public bool HasChargedHit;
		public bool ShouldDrawRiftTrail;
		
		public float GlowIntensity = 0;
		
		public int HitNPCIndex = -1;
		public Vector2 HitNPCLockVector = Vector2.Zero;
		public int HitNPCLockTimer = 0;
		
		public static Color VoidColor(Player player, bool dark = false, float speed = 4f, float offset = 0f)
		{
			float progress = (player.miscCounterNormalized + offset) * speed;
			Color lightColor = dark ? new Color(188, 0, 163) : new Color(255, 58, 228);
			Color darkColor = dark ? new Color(57, 0, 86) : new Color(120, 0, 181);
			return Color.Lerp(lightColor, darkColor, Utils.Turn01ToCyclic010(progress % 1f));
		}
		
		public override void SafeSetStaticDefaults()
		{
			TextureLine ??= ModContent.Request<Texture2D>("OrchidMod/Content/Guardian/Projectiles/Misc/GuardianHorizonLanceProj", AssetRequestMode.ImmediateLoad).Value;
			TextureLineBlur ??= ModContent.Request<Texture2D>("OrchidMod/Content/Guardian/Projectiles/Misc/GuardianHorizonLanceProj_Blur", AssetRequestMode.ImmediateLoad).Value;
			TextureRing ??= OrchidAssets.GetExtraTexture(20, AssetRequestMode.ImmediateLoad).Value;
			StaffTextureAura ??= ModContent.Request<Texture2D>(QuarterstaffTexture + "_Aura", AssetRequestMode.ImmediateLoad).Value;
			TextureTrail ??= OrchidAssets.GetExtraTexture(8, AssetRequestMode.ImmediateLoad).Value;
			TextureTrailGlow ??= OrchidAssets.GetExtraTexture(11, AssetRequestMode.ImmediateLoad).Value;
			
			ItemID.Sets.GamepadWholeScreenUseRange[Type] = true;
		}

		protected override float TipOffset => 0.25f;

		public override void SafeSetDefaults()
		{
			Item.width = 60;
			Item.height = 56;
			Item.value = Item.sellPrice(0, 4);
			Item.rare = ItemRarityID.Pink;
			Item.useTime = 40;
			ParryDuration = 40;
			Item.knockBack = 6f;
			Item.damage = 196;
			JabChargeGain = 0.4f;
			SwingStyle = 2;
			JabSpeed = 1.0f;
			SwingSpeed = 0.4f;
			CounterSpeed = 1.1f;
			ChargeRate = 0.5f;
			CounterDamage = 0.5f;
			CounterHits = 4;
			GuardStacks = 0;
			
			OldPositionTrail = [];
			OldSizeTrail = [];
			
			Offset = new int[80];
			for (int i = 0; i < Offset.Length; i++) Offset[i] = i;
		}

		public override void SafeHoldItem(Player player)
		{
			if (player.HasBuff<GuardianVoidQuarterstaffBuff>())
			{
				if (GlowIntensity < 1) GlowIntensity += 0.2f;
				else if (GlowIntensity > 1) GlowIntensity = 1;
			}
			else
			{
				if (GlowIntensity > 0) GlowIntensity -= 0.05f;
				else if (GlowIntensity < 0) GlowIntensity = 0;
			}

			if (HitNPCIndex != -1 && HitNPCLockTimer > 0)
			{
				player.gravControl = false;
				player.gravControl2 = false;
			}
		}

		public override void ExtraAIQuarterstaff(Player player, OrchidGuardian guardian, Projectile projectile)
		{
			if (projectile.ModProjectile is GuardianQuarterstaffAnchor anchor)
			{
				Vector2 tipPosition = GetTipPosition(projectile);
				if (player.direction == 1) tipPosition.X -= 12;

				projectile.localAI[0]--;
				if (projectile.localAI[0] < 0) projectile.localAI[0] = 0;

				if (OrchidMod.ThoriumMod != null && Main.rand.NextBool(10))
				{
					int dustType = OrchidMod.ThoriumMod.Find<ModDust>("VoidHeartDust").Type;
					Dust.NewDustDirect(tipPosition, 16, 16, dustType);
				}
				
				if (GlowIntensity > 0) Lighting.AddLight(projectile.Center, VoidColor(player).ToVector3() * GlowIntensity);

				if (projectile.ai[0] > 1)
				{
					if (projectile.extraUpdates != 2)
					{
						projectile.extraUpdates = 2;
						projectile.netUpdate = true;
					}
					
					if (projectile.ai[0] > 11)
					{
						if (ShouldDrawRiftTrail && projectile.ai[0] < 31 && projectile.ai[2] == 0)
						{
							OldPositionTrail.Add(tipPosition);
							OldSizeTrail.Add(0.05f + 0.95f * (float)Math.Sin(0.0785f * (projectile.ai[0] - 1)));
						}
						if (OldPositionTrail.Count > 20)
						{
							OldPositionTrail.RemoveAt(0);
							OldSizeTrail.RemoveAt(0);
						}

						if (HitNPCIndex != -1 && Main.npc[HitNPCIndex] is {} target && OrchidModProjectile.IsValidTarget(target) && HitNPCLockTimer > 0)
						{
							Vector2 targetPoint = target.Center + target.velocity + HitNPCLockVector;
							if (!Collision.SolidCollision(targetPoint - player.Size * 0.5f, player.width, player.height))
								player.Center = target.Center + target.velocity + HitNPCLockVector;
							else HitNPCLockTimer = -1;
							HitNPCLockTimer--;
							if (HitNPCLockTimer < 0)
							{
								HitNPCLockTimer = 0;
								HitNPCIndex = -1;
								HitNPCLockVector = Vector2.Zero;
							}
						}
						else
						{
							HitNPCLockTimer = 0;
							HitNPCIndex = -1;
							HitNPCLockVector = Vector2.Zero;
						}
					}
				}
				else
				{
					if (projectile.extraUpdates != 0)
					{
						projectile.extraUpdates = 0;
						projectile.netUpdate = true;
					}
					
					if (OldPositionTrail.Count > 0)
					{
						OldPositionTrail.RemoveAt(0);
						OldSizeTrail.RemoveAt(0);
					}

					if (!player.HasBuff<GuardianVoidQuarterstaffBuff>())
					{
						// if (guardian.GuardianVoidQuarterstaff > 0)
						// {
						// 	player.AddBuff(ModContent.BuffType<GuardianVoidQuarterstaffDebuff>(), 40 * guardian.GuardianVoidQuarterstaff);
						// 	SoundEngine.PlaySound(SoundID.Item104, player.Center);
						// 	player.Hurt(guardian.VoidDeath, (int)(player.statLifeMax2 * 0.065333f) * guardian.GuardianVoidQuarterstaff, -1, dodgeable: false, scalingArmorPenetration: 1f, knockback: 0);
						// 	guardian.GuardianVoidQuarterstaff = 0;
						// }
						if (ShouldDrawRiftTrail) ShouldDrawRiftTrail = false;
					}
				}
			}
		}

		public override void OnAttack(Player player, OrchidGuardian guardian, Projectile projectile, bool jabAttack, bool counterAttack) 
		{ 
			if (!jabAttack && !counterAttack)
			{
				Vector2 oldCenter = projectile.Center;
				projectile.scale *= 1.25f;
				projectile.width = (int)(projectile.width * 1.25f);
				projectile.height = (int)(projectile.height * 1.25f);
				projectile.Center = oldCenter;

        		Vector2 tipPosition = projectile.Center - Vector2.UnitY.RotatedBy(projectile.rotation + MathHelper.PiOver4) * projectile.width * 0.5f;
				if (player.direction == 1) tipPosition.X -= 12;

				if (OrchidMod.ThoriumMod.TryFind("VoidHeartDust", out ModDust dust))
					for (int i = 0; i < 10; i++) Dust.NewDustDirect(player.Center, 24, 24, dust.Type, Scale: 3f);

				Vector2 target = CalcFirstThingInLine(player, 600, 1f, out float width, out NPC hitNPC);
				
				if (IsLocalPlayer(player)) Main.SetCameraLerp(0.1f, 10);

				player.RemoveAllGrapplingHooks();
					
				guardian.modPlayer.SetDodgeImmuneTime(15);

				Vector2 oldPosition = player.Center;
				Vector2 segment = Vector2.Normalize(target - player.Center);
				Vector2 finalPosition = target - player.Size * 0.5f + segment * width;
				if (hitNPC != null)
				{
					finalPosition = hitNPC.Center + hitNPC.DirectionTo(player.Center) * (width + (ShouldDrawRiftTrail ? 48 : 36) * projectile.scale);
					HitNPCIndex = hitNPC.whoAmI;
					HitNPCLockTimer = 60;
					HitNPCLockVector = finalPosition - hitNPC.Center;
				}
				player.DoQueenSlimeHookTeleport(finalPosition.Floor());
				projectile.Center += (player.position - oldPosition);
				projectile.netUpdate = true;

				projectile.localAI[0] = 30;
			
				SoundEngine.PlaySound(SoundID.Item8, player.Center);
			}
		}

		public override void QuarterstaffModifyHitNPC(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, ref NPC.HitModifiers modifiers, bool jabAttack, bool counterAttack, bool firstHit)
		{
			if (!jabAttack && !counterAttack && HitNPCIndex != -1 && HitNPCLockTimer > 0 && target.whoAmI != HitNPCIndex) modifiers.FinalDamage *= 0.75f;
		}

		public override void SafeModifyTooltips(List<TooltipLine> tooltips, OrchidGuardian guardian)
		{
			int index = tooltips.FindIndex(ttip => ttip.Mod.Equals("Terraria") && ttip.Name.Equals("Knockback"));
			tooltips.Insert(index + 1, new TooltipLine(Mod, "GuardianGrants", Language.GetTextValue(Mod.GetLocalizationKey("Items.ThoriumVoidQuarterstaff.Grants")))
			{
				OverrideColor = new Color(175, 255, 175)
			});
		}

		public override void OnHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool jabAttack, bool counterAttack)
		{
			int debuffType = OrchidMod.ThoriumMod.TryFind("LightCurse", out ModBuff lightCurse) ? lightCurse.Type : BuffID.ShadowFlame;
			
			target.AddBuff(debuffType, jabAttack ? 45 : 180);
			if (!jabAttack && !counterAttack && HitNPCIndex != -1 && HitNPCLockTimer > 0 && target.whoAmI == HitNPCIndex) 
			{
				if (projectile.localAI[0] > 0) {
					guardian.modPlayer.SetDodgeImmuneTime(InvincibilityDuration);
					projectile.localAI[0] = 0;
				}

				float launchAngle = MathHelper.WrapAngle(Vector2.Normalize(player.Center - target.Center).ToRotation());
				float launchPower = 13.5f;
				bool reduceGain = player.controlDown;
				bool increaseGain = player.controlUp;;

				if (player.Center.X < target.Center.X ^ player.gravDir == -1) (reduceGain, increaseGain) = (increaseGain, reduceGain);
				
				if (increaseGain) launchAngle += 22.5f;
				else if (reduceGain) launchAngle -= 22.5f;

				player.velocity = Vector2.UnitX.RotatedBy(launchAngle) * launchPower;
				player.wingTime += player.wingTimeMax * 0.25f;
				
				HitNPCIndex = -1;
				HitNPCLockTimer = 0;
				HitNPCLockVector = Vector2.Zero;

				if (!player.HasBuff<GuardianVoidQuarterstaffBuff>())
				{
					SoundEngine.PlaySound(SoundID.Item117, player.Center);
					
					for (int i = 0; i < 15; i++)
					{
						Dust dust = Dust.NewDustPerfect(player.Center, DustID.Shadowflame, Main.rand.NextVector2CircularEdge(3f, 3f), Scale: 2f);
						dust.noGravity = true;
					}
				}
				
				if (ShouldDrawRiftTrail) for (int i = 0; i < 3; i++) SpawnVoidBlobs(player, target, guardian.GetGuardianDamage(Item.damage * 0.4f));
				
				player.AddBuff<GuardianVoidQuarterstaffBuff>(240);
				// guardian.GuardianVoidQuarterstaffTimer = 60;
				HasChargedHit = true;
			} 
		}

		public override void OnHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool jabAttack, bool counterAttack)
		{
			if (!jabAttack && !counterAttack)
			{
				if (player.HasBuff<GuardianVoidQuarterstaffBuff>())
					guardian.GuardianGuardRecharging += 0.5f;
				else guardian.AddGuard();
			}

		}

		public override void OnAttackEnd(Player player, OrchidGuardian guardian, Projectile projectile, bool jabAttack, bool counterAttack)
		{
			if (!jabAttack && !counterAttack) 
			{
				if (HasChargedHit)
				{
					ShouldDrawRiftTrail = true;
				}
				else
				{
					ShouldDrawRiftTrail = false;
					// if (player.HasBuff<GuardianVoidQuarterstaffBuff>())
					// {
					// 	player.ClearBuff(ModContent.BuffType<GuardianVoidQuarterstaffBuff>());;
					// 	if (guardian.GuardianVoidQuarterstaff > 0)
					// 	{
					// 		player.AddBuff(ModContent.BuffType<GuardianVoidQuarterstaffDebuff>(), 40 * guardian.GuardianVoidQuarterstaff);
					// 		SoundEngine.PlaySound(SoundID.Item104, player.Center);
					// 		player.Hurt(guardian.VoidDeath, (int)(player.statLifeMax2 * 0.065333f) * guardian.GuardianVoidQuarterstaff, -1, dodgeable: false, scalingArmorPenetration: 1f, knockback: 0);
					// 	}
					// 	guardian.GuardianVoidQuarterstaff = 0;
					// }
				}
			}
			
			if (HasChargedHit) HasChargedHit = false;		
		}

		public override void OnParryQuarterstaff(Player player, OrchidGuardian guardian, Entity aggressor, Projectile anchor) 
		{
			if (player.HasBuff<GuardianVoidQuarterstaffBuff>())
			{
				NPC target;
				if (aggressor is NPC npc)
				{
					if (OrchidModProjectile.IsValidTarget(npc)) target = npc;
					else goto ResetBlobStacks;
				}
				else if (aggressor is Projectile proj)
				{
					NPC projOwner = Main.npc[proj.GetGlobalProjectile<OrchidGlobalProjectile>().NPCOwner];
					if (projOwner != null) target = projOwner;
					else goto ResetBlobStacks;
				}
				else
				{
					NPC[] nearby = Main.npc.Where(T => OrchidModProjectile.IsValidTarget(T) && T.DistanceSQ(player.Center) < 57600f).ToArray();
					if (nearby.Length > 0) target = nearby[Main.rand.Next(nearby.Length)];
					else goto ResetBlobStacks;
				}
				// for (int i = 0; i < guardian.GuardianVoidQuarterstaff; i++)
				// {
				// 	SpawnVoidBlobs(player, target, guardian.GetGuardianDamage(Item.damage * 0.4f));
				// }
				ResetBlobStacks: 
				guardian.GuardianVoidQuarterstaff = 0;
			} 
		}

		public override bool PreSwingAI(Player player, OrchidGuardian guardian, Projectile anchor)
		{
			// Uses custom swing animation with wider angle while empowered
			if (ShouldDrawRiftTrail)
			{
				anchor.rotation = anchor.ai[1] + 3 * MathHelper.PiOver4 + (float)Math.Cos(0.0786f * (anchor.ai[0] - 1)) * 2.8f * -player.direction;
				anchor.Center = player.MountedCenter.Floor() + Vector2.UnitY.RotatedBy(anchor.ai[1] + (float)Math.Cos(0.0786f * (anchor.ai[0] - 1)) * 2.6f * -player.direction) * 24f * (1 + 1.2f * (float)Math.Cos(0.0786f * (anchor.ai[0] - 1)));
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.PiOver4 * player.direction + anchor.ai[1] + 0.1f - (float)Math.Cos(0.102f * (anchor.ai[0] - 9)) * player.direction);
				player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, anchor.ai[1] - 0.1f + (float)Math.Cos(0.102f * (anchor.ai[0]- 9)) * 0.2f * player.direction);
				
				return false;
			}
			
			return true;
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

		public override bool PreDrawQuarterstaff(SpriteBatch spriteBatch, Projectile projectile, Player player, ref Color lightColor, SpriteEffects effects)
		{
			if (GlowIntensity > 0)
			{
				Vector2 drawPos = projectile.Center + player.getGFXOffY() - Main.screenPosition;
				
				float rotationoffset = -MathHelper.PiOver4 + MathHelper.PiOver4 * player.direction;
				if (StaffTextureAura != null)
				{
					spriteBatch.End(out SpriteBatchSnapshot snapshot);
					spriteBatch.Begin(snapshot with { BlendState = BlendState.Additive, RasterizerState = RasterizerState.CullNone });

					Color color = VoidColor(player) * 0.6f * GlowIntensity;
					Main.EntitySpriteDraw(StaffTextureAura, drawPos, null, color, projectile.rotation + rotationoffset, StaffTextureAura.Size() * 0.5f, projectile.scale * Utils.Remap(GlowIntensity, 0, 1, 1f, 1.05f), effects);
					
					spriteBatch.End();
					spriteBatch.Begin(snapshot);
				}
			}

			if (OldPositionTrail.Count > 0)
			{
				Vector2 tipPos = projectile.Center - Vector2.UnitY.RotatedBy(projectile.rotation + MathHelper.PiOver4) * projectile.width * projectile.scale * 0.1f;
				spriteBatch.End(out SpriteBatchSnapshot snapshot);
				SpriteBatchSnapshot glowSnapshot = snapshot with { BlendState = BlendState.Additive, RasterizerState = RasterizerState.CullNone };
				spriteBatch.Begin(glowSnapshot);

				for (int i = 0; i < OldPositionTrail.Count; i++)
				{
					Color color = Lighting.GetColor(OldPositionTrail[i].ToTileCoordinates(), Color.Lerp(new Color(57, 0, 86), new Color(188, 0, 163), i / 20f));
					float scale = Utils.WrappedLerp(0.4f, 1f, i / 20f) * projectile.scale * OldSizeTrail[i];

					Vector2 glowPos = OldPositionTrail[i] - Main.screenPosition;
					if (i < OldPositionTrail.Count - 1)
						glowPos -= Vector2.Normalize(OldPositionTrail[i + 1] - OldPositionTrail[i]) * 0.5f;
					spriteBatch.Draw(TextureTrailGlow, glowPos, null, color * 0.8f, 0, TextureTrailGlow.Size() * 0.5f, scale * 0.75f, SpriteEffects.None, 0f);

					spriteBatch.End();
					spriteBatch.Begin(snapshot);
					spriteBatch.Draw(TextureTrail, OldPositionTrail[i] - Main.screenPosition, null, color, 0, TextureTrail.Size() * 0.5f, scale * 0.5f, SpriteEffects.None, 0f);

					if (i != OldPositionTrail.Count - 1)
					{
						spriteBatch.End();
						spriteBatch.Begin(glowSnapshot);
					}
				}
				// spriteBatch.End();
				// spriteBatch.Begin(snapshot);
			}

			return true;
		}

		public override void PostDrawQuarterstaff(SpriteBatch spriteBatch, Projectile projectile, Player player, Color lightColor, SpriteEffects effects)
		{
			OrchidGuardian guardian = Guardian(player);
			if (IsLocalPlayer(player) && guardian.GuardianItemCharge >= 180f && projectile.ai[0] is 0 or 1)
			{
				spriteBatch.End(out SpriteBatchSnapshot snapshot);
				spriteBatch.Begin(snapshot with { BlendState = BlendState.Additive, RasterizerState = RasterizerState.CullNone });

				float miscCountAngle = player.miscCounterNormalized * MathHelper.TwoPi;

				Vector2 drawPositionRing = Vector2.Transform(player.VisualMountedCenter() - Main.screenPosition, Main.GameViewMatrix.EffectMatrix);
				
				spriteBatch.Draw(TextureRing, drawPositionRing , null, new Color(120, 0, 181) * 0.5f, miscCountAngle, TextureRing.Size() * 0.5f, 0.28f + 0.007f * MathF.Sin(miscCountAngle), SpriteEffects.None, 0f);
				spriteBatch.Draw(TextureRing, drawPositionRing, null, VoidColor(player, true) * 0.5f, -miscCountAngle, TextureRing.Size() * 0.5f, 0.28f + 0.014f * MathF.Cos(miscCountAngle * 2.01f), SpriteEffects.None, 0f);

				Vector2 finalPoint = Main.ReverseGravitySupport(CalcFirstThingInLine(player, 400, 1.5f, out float width, out NPC hitNPC) - Main.screenPosition) + Main.screenPosition;
				
				if (guardian.GuardianDebugVisuals > 0) spriteBatch.DrawString(FontAssets.CombatText[0].Value, finalPoint.Floor().ToString(), player.VisualCenter() - Vector2.UnitY * 24 * player.gravDir - Main.screenPosition, Color.MediumPurple, 0, default, 1f, (player.gravDir < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None), 0f);

				Vector2 chainDirection = finalPoint - player.VisualMountedCenter();
				Vector2 segment = Vector2.Normalize(chainDirection) * TextureLine.Height * 0.15f;
				
				int nbSegments = 0;

				while(chainDirection.Length() > (segment * nbSegments).Length()) nbSegments++;

				int nbSegmentsMax = nbSegments;
				
				while (nbSegments > 0)
				{
					Color drawColor = VoidColor(player, offset: Offset[(nbSegmentsMax - nbSegments) % 80] / 80f, speed: 1.5f);
					if (nbSegments == nbSegmentsMax)
					{
						Vector2 drawPositionReticle = Vector2.Transform(finalPoint - Main.screenPosition, Main.GameViewMatrix.EffectMatrix);
						spriteBatch.Draw(TextureRing, drawPositionReticle, null, new Color(120, 0, 181) * 0.5f, -miscCountAngle, TextureRing.Size() * 0.5f, 0.007f * (width + MathF.Sin(miscCountAngle)), SpriteEffects.None, 0f);
						spriteBatch.Draw(TextureRing, drawPositionReticle, null, drawColor * 0.5f, miscCountAngle, TextureRing.Size() * 0.5f, 0.007f  * (width + MathF.Cos(miscCountAngle * 2.01f)) , SpriteEffects.None, 0f);
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

				for (int i = 0; i < 80; i++)
				{
					Offset[i]++;
					if (Offset[i] == 80) Offset[i] = 0;
				}

				spriteBatch.End();
				spriteBatch.Begin(snapshot);
			}
		}

		public static Vector2 CalcFirstThingInLine(Player player, int maxSteps, float stepLength, out float width, out NPC hitNPC)
		{
			hitNPC = null;
			width = 7.143f;
			
			Vector2 finalPoint = player.VisualMountedCenter();

			if (player.whoAmI != Main.myPlayer || Main.dedServ) return finalPoint;
			
			Vector2 velocity = player.Center.DirectionTo(Main.ReverseGravitySupport(Main.MouseScreen) + Main.screenPosition) * stepLength;
			
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
			int projType = ModContent.ProjectileType<ThoriumVoidQuarterstaffProjectile>();
			Vector2 direction = Main.rand.NextVector2Unit();

			Projectile.NewProjectileDirect(Item.GetSource_FromThis(), player.Center + player.velocity + direction * target.GetLargestDimension() * 0.5f, direction * 20f, projType, damage, Item.knockBack * 0.25f, ai1: target.whoAmI);
			
			SoundEngine.PlaySound(SoundID.Item72 with {Volume = 0.75f, PitchRange = (-0.8f, 0f)}, player.Center);
		}
	}
}
