using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common.ModObjects;
using OrchidMod.Content.General.Prefixes;
using ReLogic.Content;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian
{
	public class GuardianShieldAnchor : OrchidModGuardianAnchor
	{
		public int SelectedItem { get; set; } = -1;
		public Item ShieldItem => Main.player[Projectile.owner].inventory[this.SelectedItem];
		
		public int ShieldAnimFrame = 0;

		public bool shieldEffectReady = true;
		public bool NeedNetUpdate = false;

		public byte isSlamming = 0;
		public Vector2 aimedLocation = Vector2.Zero;
		public Vector2 oldOwnerPos = Vector2.Zero;

		public Vector2 hitbox = Vector2.Zero;
		public Vector2 hitboxOrigin = Vector2.Zero;

		public float networkedRotation => Projectile.ai[2];

		// ...
		public bool IsRotationLocked;
		private float LockedRotation;
		public override void SafeSetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 60;
			Projectile.penetrate = -1;
			Projectile.netImportant = true;
			Projectile.alpha = 255;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 120;
		}

		public void OnChangeSelectedItem(Player owner)
		{
			SelectedItem = owner.selectedItem;
			Projectile.ai[0] = 0f;
			Projectile.ai[1] = 0f;
			Projectile.netUpdate = true;
			Projectile.spriteDirection = 1;
		}

		public override void SafeModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			var owner = Main.player[Projectile.owner];
			if (ShieldItem.ModItem is OrchidModGuardianShield) (ShieldItem.ModItem as OrchidModGuardianShield).PaviseModifyHitNPC(owner, owner.GetModPlayer<OrchidGuardian>(), target, Projectile, ref modifiers, FirstHit);
		}

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidGuardian guardian)
		{
			var owner = Main.player[Projectile.owner];
			var item = ShieldItem;
			if (item == null || !(item.ModItem is OrchidModGuardianShield guardianItem))
			{
				Projectile.Kill();
				return;
			}

			guardianItem.SlamHit(owner, Projectile, target);
			if (FirstHit)
			{
				guardianItem.SlamHitFirst(owner, Projectile, target);
			}
		}

		public override void AI()
		{
			var owner = Main.player[Projectile.owner];
			var death = false;

			if (!owner.active || owner.dead)
			{
				Projectile.Kill();
				return;
			}

			var item = ShieldItem;
			if (item == null || !(item.ModItem is OrchidModGuardianShield guardianItem))
			{
				Projectile.Kill();
				return;
			}

			if (SelectedItem < 0 || !(owner.HeldItem.ModItem is OrchidModGuardianShield))
			{
				Projectile.netUpdate = true;
				death = true;
			}

			if (!death)
			{
				OrchidGuardian guardian = owner.GetModPlayer<OrchidGuardian>();
				Projectile.scale = guardian.GuardianWeaponScale;

				if (NeedNetUpdate)
				{
					NeedNetUpdate = false;
					Projectile.netUpdate = true;
				}

				float addedDistance = 0f;

				if (Projectile.ai[1] > 0f)
				{ // Shield bash
					if (isSlamming == 0)
					{
						isSlamming = 1;
						Projectile.damage = owner.GetModPlayer<OrchidGuardian>().GetGuardianDamage(guardianItem.Item.damage);
						Projectile.CritChance = (int)(owner.GetCritChance<GuardianDamageClass>() + owner.GetCritChance<GenericDamageClass>() + guardianItem.Item.crit);
						Projectile.knockBack = guardianItem.Item.knockBack;
						Projectile.ResetLocalNPCHitImmunity();
						Projectile.friendly = true;
						ResetHitStatus(true);

						if (IsLocalOwner)
						{
							var texture = ModContent.Request<Texture2D>((ShieldItem.ModItem as OrchidModGuardianShield)?.ShieldTexture).Value;
							Projectile.width = (int)(texture.Height * guardian.GuardianWeaponScale / guardianItem.ShieldFrames);
							Projectile.height = (int)(texture.Height * guardian.GuardianWeaponScale / guardianItem.ShieldFrames);
						}

						if (owner.boneGloveItem != null && !owner.boneGloveItem.IsAir && owner.boneGloveTimer == 0)
						{ // Bone glove compatibility, from vanilla code
							owner.boneGloveTimer = 60;
							Vector2 center = owner.Center;
							Vector2 vector = owner.DirectionTo(owner.ApplyRangeCompensation(0.2f, center, Main.MouseWorld)) * 10f;
							Projectile.NewProjectile(owner.GetSource_ItemUse(owner.boneGloveItem), center.X, center.Y, vector.X, vector.Y, ProjectileID.BoneGloveProj, 25, 5f, owner.whoAmI);
						}
					}

					float slamDistance = (int)(guardianItem.slamDistance * guardianItem.Item.GetGlobalItem<GuardianPrefixItem>().GetSlamDistance() * owner.GetTotalAttackSpeed(DamageClass.Melee));
					addedDistance = (float)Math.Sin(MathHelper.Pi / 60f * Projectile.ai[1]) * slamDistance;
					Projectile.ai[1] -= 60f / guardianItem.Item.useTime;

					if (Projectile.ai[1] <= 0f)
					{
						guardianItem.SlamEnd(owner, Projectile);
						Projectile.ai[1] = 0f;
						isSlamming = 0;
						Projectile.friendly = false;
						IsRotationLocked = false;
						Projectile.netUpdate = true;
					}
				}

				if (Projectile.ai[0] > 0f)
				{
					if (Projectile.ai[0] >= (int)(guardianItem.blockDuration * item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianBlockDuration))
					{
						Vector2 oldDimensions = new Vector2(Projectile.width, Projectile.height);
						var texture = ModContent.Request<Texture2D>(guardianItem.ShieldTexture).Value;
						Projectile.width = (int)(texture.Height * guardian.GuardianWeaponScale / guardianItem.ShieldFrames);
						Projectile.height = (int)(texture.Height * guardian.GuardianWeaponScale / guardianItem.ShieldFrames);
						aimedLocation += (oldDimensions * 0.5f - new Vector2(texture.Height * guardian.GuardianWeaponScale / guardianItem.ShieldFrames, texture.Height * guardian.GuardianWeaponScale / guardianItem.ShieldFrames) * 0.5f).Floor();
					}

					aimedLocation += owner.Center.Floor() - oldOwnerPos.Floor();
					Point p1 = new Point((int)hitboxOrigin.X, (int)hitboxOrigin.Y);
					Point p2 = new Point((int)(hitboxOrigin.X + hitbox.X), (int)(hitboxOrigin.Y + hitbox.Y));

					//guardian.GuardianSlamRechargeTime = (int)(OrchidGuardian.GuardianRechargeTime * guardian.GuardianSlamRecharge);

					for (int l = 0; l < Main.projectile.Length; l++)
					{
						Projectile proj = Main.projectile[l];
						if (proj.active && proj.hostile && proj.damage > 0 && !OrchidGuardian.ProjectilesBlockBlacklist.Contains(proj.type))
						{
							if (LineIntersectsRect(p1, p2, proj.Hitbox) || proj.Hitbox.Intersects(Projectile.Hitbox))
							{
								bool killProj = guardianItem.Block(owner, Projectile, proj);
								guardian.OnBlockProjectile(Projectile, proj);
								if (shieldEffectReady)
								{
									guardian.OnBlockProjectileFirst(Projectile, proj);
									guardianItem.Protect(owner, Projectile);
									shieldEffectReady = false;
									SoundEngine.PlaySound(SoundID.Item37, owner.Center);
								}
								if (killProj) proj.Kill();
								SoundEngine.PlaySound(SoundID.Dig, owner.Center);
							}
						}
					}

					for (int k = 0; k < Main.maxNPCs; k++)
					{
						NPC target = Main.npc[k];
						if (target.active && !target.dontTakeDamage && !target.friendly && this.LineIntersectsRect(p2, p1, target.Hitbox))
						{
							bool contained = false;
							foreach(BlockedEnemy blockedEnemy in guardian.GuardianBlockedEnemies)
							{
								if (blockedEnemy.npc == target)
								{ // Enemy already blocked, reset the timer
									blockedEnemy.time = (int)Projectile.ai[0] + 60;
									contained = true;
									break;
								}
							}

							if (!contained)
							{ // First time blocking an enemy
								guardian.OnBlockNPCNew(Projectile, target);
								guardian.GuardianBlockedEnemies.Add(new BlockedEnemy(target, (int)Projectile.ai[0] + 60));
								SoundEngine.PlaySound(SoundID.Dig, owner.Center);
							}

							if (target.knockBackResist > 0f)
							{ // Push enemy if possible
								Vector2 push = Projectile.Center - owner.Center;
								push.Normalize();
								push += owner.Center - oldOwnerPos;
								target.velocity = push;
							}

							guardianItem.Push(owner, Projectile, target);
							guardian.OnBlockNPC(Projectile, target);
							if (shieldEffectReady)
							{ // First parry stuff
								guardian.OnBlockNPCFirst(Projectile, target);
								guardianItem.Protect(owner, Projectile);
								shieldEffectReady = false;
								SoundEngine.PlaySound(SoundID.Item37, owner.Center);
							}
						}
					}

					Projectile.ai[0] --;
					if (Projectile.ai[0] <= 0f)
					{
						if (guardianItem.BlockEnd(owner, Projectile))
						{
							spawnDusts();
						}
						Projectile.ai[0] = 0f;
					}
				}
				else
				{
					if (Main.myPlayer == Projectile.owner)
					{
						aimedLocation = Main.MouseWorld - owner.Center.Floor();
						aimedLocation.Normalize();
						
						aimedLocation = Vector2.UnitX.RotatedBy(IsRotationLocked ? LockedRotation : OrchidModGuardianShield.GetSnappedAngle(guardianItem, owner,aimedLocation.ToRotation()));
						Projectile.velocity = aimedLocation * float.Epsilon;
						aimedLocation *= (guardianItem.distance + addedDistance) * -1f;

						Projectile.rotation = aimedLocation.ToRotation();
						if (guardianItem.shouldFlip)
						{
							if (aimedLocation.X < 0 || (isSlamming is 1 or 2 && IsRotationLocked && -Vector2.UnitX.RotatedBy(LockedRotation).X < 0))
							{
								Projectile.spriteDirection = -1;
							}
							else Projectile.spriteDirection = 1;
						}
						Projectile.direction = Projectile.spriteDirection;

						aimedLocation = owner.Center.Floor() - aimedLocation - new Vector2(Projectile.width / 2f, Projectile.height / 2f);

						if (Math.Abs(networkedRotation - Projectile.rotation) > 0.025f)
						{
							Projectile.ai[2] = Projectile.rotation; // networked rotation
							Projectile.netUpdate = true;
						}
					}
				}

				if (IsLocalOwner)
				{
					Projectile.position = aimedLocation;
				}
				else
				{
					Projectile.Center = owner.Center.Floor() - networkedRotation.ToRotationVector2() * (guardianItem.distance + addedDistance);
					Projectile.rotation = networkedRotation;
				}

				Projectile.timeLeft = 5;

				if (isSlamming == 1) // Slam() is called here so the projectile has the time to reposition properly before effects such as projectile spawns are called
				{
					isSlamming = 2;
					if (guardianItem.lockSlamRotation)
					{
						IsRotationLocked = true;
						LockedRotation = Projectile.rotation + MathHelper.Pi;
						Projectile.netUpdate = true;
					}
					guardianItem.Slam(owner, Projectile);
					guardian.GuardianCounterTime = 0;
				} 

				UpdateHitbox();
				if (guardian.GuardianShowDebugVisuals) SeeHitbox();
			}

			oldOwnerPos = owner.Center;
			guardianItem.ExtraAIShield(Projectile);
		}

		// https://stackoverflow.com/questions/5514366/how-to-know-if-a-line-intersects-a-rectangle
		public bool LineIntersectsRect(Point p1, Point p2, Rectangle r)
		{
			return LineIntersectsLine(p1, p2, new Point(r.X, r.Y), new Point(r.X + r.Width, r.Y)) ||
				   LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y), new Point(r.X + r.Width, r.Y + r.Height)) ||
				   LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y + r.Height), new Point(r.X, r.Y + r.Height)) ||
				   LineIntersectsLine(p1, p2, new Point(r.X, r.Y + r.Height), new Point(r.X, r.Y)) ||
				   (r.Contains(p1) && r.Contains(p2));
		}

		private bool LineIntersectsLine(Point l1p1, Point l1p2, Point l2p1, Point l2p2)
		{
			float q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
			float d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

			if (d == 0)
			{
				return false;
			}

			float r = q / d;

			q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
			float s = q / d;

			if (r < 0 || r > 1 || s < 0 || s > 1)
			{
				return false;
			}

			return true;
		}
		// end of Stackoverflow code

		public void UpdateHitbox()
		{
			this.hitboxOrigin = Projectile.Center + Vector2.UnitY * Projectile.gfxOffY;
			this.hitboxOrigin -= new Vector2(0f, (Projectile.height) / 2f).RotatedBy(Projectile.rotation);
			hitboxOrigin -= new Vector2(4f, 4f);

			this.hitbox = new Vector2(0f, Projectile.height).RotatedBy(Projectile.rotation);
		}

		public void SeeHitbox()
		{
			Vector2 vector = this.hitbox;
			vector.Normalize();
			for (int i = 0; i < this.hitbox.Length(); i++)
			{
				Vector2 pos = this.hitboxOrigin + vector * i;
				Dust dust = Main.dust[Dust.NewDust(pos, 0, 0, DustID.Torch)];
				dust.velocity *= 0f;
				dust.noGravity = true;
			}
		}

		public void spawnDusts()
		{
			int dustType = 31;
			Vector2 pos = new Vector2(Projectile.position.X, Projectile.position.Y);
			for (int i = 0; i < 5; i++)
			{
				Main.dust[Dust.NewDust(pos, 20, 20, dustType)].velocity *= 0.25f;
			}
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 3; i++)
			{
				Main.dust[Dust.NewDust(Projectile.Center, 0, 0, DustID.Smoke)].velocity *= 0.25f;
			}
		}

		public override bool? CanCutTiles() => Projectile.ai[1] > 0f;

		public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
		{
			if (SelectedItem < 0 || SelectedItem > 58) return false;
			if (!(ShieldItem.ModItem is OrchidModGuardianShield guardianItem)) return false;
			if (!ModContent.HasAsset(guardianItem.ShieldTexture)) return false;

			Player player = Main.player[Projectile.owner];
			OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
			Color color = Lighting.GetColor((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f), Color.White);

			if (guardianItem.PreDrawShield(spriteBatch, Projectile, player, ref color))
			{
				Texture2D texture = ModContent.Request<Texture2D>(guardianItem.ShieldTexture).Value;
				Vector2 drawPosition = Projectile.Center - Main.screenPosition + Vector2.UnitY * player.gfxOffY;
				SpriteEffects effect = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
				float colorMult = (Projectile.ai[1] + Projectile.ai[0] > 0 ? 1f : (0.4f + Math.Abs((1f * Main.player[Main.myPlayer].GetModPlayer<OrchidPlayer>().Timer120 - 60) / 120f)));
				float flippedRotation = Projectile.rotation + (Projectile.spriteDirection == 1 ? 0 : MathHelper.Pi);
				
				Rectangle frame = texture.Frame(1, guardianItem.ShieldFrames, 0, ShieldAnimFrame % guardianItem.ShieldFrames);
				
				spriteBatch.Draw(texture, drawPosition, frame, color * colorMult, flippedRotation, frame.Size() * 0.5f, Projectile.scale, effect, 0f);

				if (ModContent.RequestIfExists<Texture2D>(guardianItem.ShieldTexture + "_Glow", out Asset<Texture2D> assetglow))
				{
					Color glowColor = guardianItem.GetPaviseGlowmaskColor(player, guardian, Projectile, lightColor);
					spriteBatch.Draw(assetglow.Value, drawPosition, frame, glowColor, flippedRotation, frame.Size() * 0.5f, Projectile.scale, effect, 0f);
				}
			}
			guardianItem.PostDrawShield(spriteBatch, Projectile, player, color);

			return false;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(this.SelectedItem);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			this.SelectedItem = reader.ReadInt32();
		}
	}
}