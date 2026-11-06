using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common;
using OrchidMod.Content.Guardian.Projectiles.Misc;
using OrchidMod.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.Player;

namespace OrchidMod.Content.Guardian
{
	public class GuardianStandardAnchor : OrchidModGuardianAnchor
	{
		public static Texture2D TextureAura;
		public int TimeSpent = 0;
		public bool Ding = false;
		public bool NeedNetUpdate = false;
		public float SyncedValue = -1f; // Used to sync some standard related behaviour in mp
		public bool SyncValue = false; // If true, SyncedValue will be synced on the next netupdate

		public bool Worn => Projectile.ai[1] > 0f; // Standard buff remaining duration
		public bool Reinforced => Projectile.ai[2] == 1f && ((BuffItem == StandardItem) || (Main.player[Projectile.owner].HeldItem.ModItem is not OrchidModGuardianStandard && Worn)); // Has the item been used twice (stronger effects)
		public Item BuffItem = null;
		public int SelectedItem { get; set; } = -1;
		public Item StandardItem => Main.player[Projectile.owner].inventory[SelectedItem];

		public float Alpha
		{
			get => Projectile.localAI[0];
			set => Projectile.localAI[0] = value;
		}

		public override void Load()
		{
			TextureAura ??= ModContent.Request<Texture2D>(Texture + "_Aura", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(SelectedItem);
			writer.Write(BuffItem != null && BuffItem == StandardItem);

			writer.Write(SyncValue);
			if (SyncValue)
			{
				writer.Write(SyncedValue);
			}
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			SelectedItem = reader.ReadInt32();
			if (reader.ReadBoolean()) BuffItem = StandardItem;
			if (reader.ReadBoolean()) SyncedValue = reader.ReadSingle();
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			var owner = Main.player[Projectile.owner];
			if (owner.HeldItem.ModItem is OrchidModGuardianStandard) overPlayers.Add(index); // Display the flag over the player if it is being held
		}

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
			Projectile.localNPCHitCooldown = 20;
			Projectile.netImportant = true;
		}

		public void OnChangeSelectedItem(Player owner)
		{
			OrchidGuardian guardian = owner.GetModPlayer<OrchidGuardian>();
			Projectile.ai[0] = 0f;
			guardian.GuardianItemCharge = 0;
			if (!Worn || owner.inventory[owner.selectedItem].ModItem is OrchidModGuardianStandard) SelectedItem = owner.selectedItem;
			Projectile.netUpdate = true;
		}

		public override void AI()
		{
			var owner = Main.player[Projectile.owner];
			OrchidGuardian guardian = owner.GetModPlayer<OrchidGuardian>();

			if ((!Worn && (SelectedItem < 0 || StandardItem == null || StandardItem.ModItem is not OrchidModGuardianStandard)) || !owner.active || owner.dead)
			{
				Projectile.Kill();
				return;
			}
			else
			{
				bool heldStandard = owner.HeldItem.ModItem is OrchidModGuardianStandard;

				if (IsLocalOwner)
				{
					if (heldStandard)
					{
						if (Main.MouseWorld.X > owner.Center.X && owner.direction != 1) owner.ChangeDir(1);
						else if (Main.MouseWorld.X < owner.Center.X && owner.direction != -1) owner.ChangeDir(-1);
						if (NeedNetUpdate)
						{
							NeedNetUpdate = false;
							Projectile.netUpdate = true;
						}
					}
					else if (!Worn)
					{
						Projectile.Kill();
						return;
					}
					else
					{
						Item heldItem = owner.HeldItem;
						if (heldItem.damage > 0 && heldItem.ModItem is not OrchidModGuardianItem && (heldItem.pick + heldItem.hammer + heldItem.axe == 0) && Projectile.ai[1] > 277f)
						{ // owner is not holding a guardian item, standard fades a lot faster
							Projectile.ai[1] -= 2f;
						}
					}
				}
				else
				{
					if (!heldStandard)
					{
						Projectile.ai[0] = 0f;
					}

					if (Projectile.ai[0] == 0f)
					{ // Adresses a visual issue
						guardian.GuardianItemCharge = 0;
					}
				}

				TimeSpent++;
				Projectile.timeLeft = 5;

				if (owner.velocity.X > 0) // This part allows moving the flag based on player movement
				{
					Projectile.localAI[1] += Math.Abs(Projectile.localAI[1] - 1) * 0.1f;
				}
				else if (owner.velocity.X < 0)
				{
					Projectile.localAI[1] -= Math.Abs(-1 - Projectile.localAI[1]) * 0.1f;
				}
				else Projectile.localAI[1] *= 0.95f;

				if (Worn)
				{ // Handles buffs given to nearby players, npcs, etc
					if (BuffItem == null || BuffItem.ModItem is not OrchidModGuardianStandard buffItem)
					{
						// This message was never useful anyways
						//Main.NewText("Error with a Standard effect, please tell me how that happened!", Color.Red);
						Projectile.Kill();
						return;
					}
					else
					{
						bool AnyNear = false; // Used for Alpha stuff
						float range = buffItem.AuraRange * guardian.GuardianStandardRange;

						if (buffItem.AffectNearbyPlayers)
						{ // Called on every client to affect nearby players
							foreach (Player player in Main.player)
							{
								if (player.active && !player.dead && player.Center.Distance(owner.Center) < (range + player.width * 0.5f))
								{
									if (buffItem.NearbyPlayerEffect(player.GetModPlayer<OrchidGuardian>().GuardianStandardStats, player, guardian, player == owner, Projectile.ai[2] == 1f))
									{ // If the player is affected by the standard, checks the guardian's modplayer for bonus effects
										guardian.StandardNearbyPlayerEffect(player.GetModPlayer<OrchidGuardian>().GuardianStandardStats, player, guardian, player == owner, Projectile.ai[2] == 1f);
									}
									if (!AnyNear) AnyNear = buffItem.DrawAura(true, player.whoAmI == owner.whoAmI, false, IsLocalOwner, Projectile.ai[2] == 1f);
								}
							}
						}

						if (buffItem.AffectNearbyNPCs)
						{ // Called on every client to affect nearby NPCs
							foreach (NPC npc in Main.npc)
							{
								if (npc.active && !npc.friendly && !npc.CountsAsACritter && npc.Center.Distance(owner.Center) < (range + npc.width * 0.5f))
								{
									if (buffItem.NearbyNPCEffect(owner, guardian, npc, IsLocalOwner, Projectile.ai[2] == 1f))
									{ // If the npc is affected by the standard, checks the guardian's modplayer for bonus effects
										guardian.StandardNearbyNPCEffect(owner, guardian, npc, IsLocalOwner, Projectile.ai[2] == 1f);
									}
									if (!AnyNear) AnyNear = buffItem.DrawAura(false, false, true, IsLocalOwner, Projectile.ai[2] == 1f);
								}
							}
						}

						if (AnyNear && Projectile.ai[1] > 50f)
						{ // Used for the Alpha of the effect radius aura
							Alpha += 0.01f;
							if (Alpha > 0.5f) Alpha = 0.5f;
						}
						else
						{
							Alpha -= 0.01f;
							if (Alpha < 0f) Alpha = 0f;
						}

						guardian.GuardianCurrentStandardAnchor = Projectile;
						guardian.GuardianStandardBuffer = true;

						if (!heldStandard)
						{
							Projectile.ai[1]--;
						}

						if (Projectile.ai[1] <= 0)
						{
							if (Projectile.ai[2] > 0)
							{ // Standard is reinforced -> goes back to charged
								Projectile.ai[1] = 245f; // magic number so it looks good with the UI sin(). This is almost exactly 4 seconds
								SoundEngine.PlaySound(SoundID.LiquidsWaterLava, Projectile.Center);
							}
							else
							{
								Projectile.ai[1] = 0;
								BuffItem = null;
								if (!heldStandard && IsLocalOwner) Projectile.Kill();
							}
							Projectile.ai[2] = 0;
						}

						buffItem.ExtraAIStandardWorn(this, Projectile, owner, guardian);
					}
				}

				if (StandardItem.ModItem is OrchidModGuardianStandard guardianItem)
				{
					if (Projectile.ai[0] == 1f)
					{ // Being charged by the player
						if (guardian.GuardianItemCharge < 180f)
						{
							guardian.GuardianItemCharge += 45f / guardianItem.Item.useTime * owner.GetTotalAttackSpeed(DamageClass.Melee);
							if (guardian.GuardianItemCharge > 180f) guardian.GuardianItemCharge = 180f;
						}

						if (guardian.GuardianItemCharge >= 180f && !Ding && IsLocalOwner)
						{
							Ding = true;
							SoundEngine.PlaySound(guardian.DingSound, owner.Center);
						}

						if ((!owner.controlUseItem || !heldStandard) && IsLocalOwner)
						{
							if (guardian.GuardianItemCharge >= 180f)
							{
								SoundEngine.PlaySound(guardianItem.Item.UseSound, owner.Center);

								if (BuffItem == StandardItem && !Reinforced)
								{
									Projectile.ai[2] = 1f;
									CombatText.NewText(owner.Hitbox, OrchidColors.GuardianTooltip, Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.Reinforced"));
								}
								else
								{
									if (BuffItem != StandardItem)
									{
										Projectile.ai[2] = 0f;
										SyncedValue = guardianItem.BaseSyncedValue;
										SyncValue = true;
										guardian.ResetStandards(true);
									}
									if (!Reinforced) CombatText.NewText(owner.Hitbox, OrchidColors.GuardianTooltip, Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.Charged"), false);
								}

								BuffItem = StandardItem;
								Projectile.ai[1] += guardianItem.StandardDuration * guardian.GuardianStandardTimer;
								if (Projectile.ai[1] > (guardianItem.StandardDuration * guardian.GuardianStandardTimer) * 2f)
								{ // standards duration stacks up to 200%
									Projectile.ai[1] = (guardianItem.StandardDuration * guardian.GuardianStandardTimer) * 2f;
								}

								foreach (Projectile proj in Main.projectile)
								{
									if (proj.type == ModContent.ProjectileType<GuardianHorizonLanceAnchor>() && proj.active && proj.owner == Projectile.owner)
									{
										proj.ai[1] = 30f;
										proj.netUpdate = true;
										break;
									}
								}

								guardian.AddGuard(guardianItem.GuardStacks);
								guardian.AddSlam(guardianItem.SlamStacks);

								int projectileType = ModContent.ProjectileType<StandardAuraProjectile>();
								Projectile auraProj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), owner.Center, Vector2.Zero, projectileType, 0, 0f, owner.whoAmI);

								guardian.OnAttack(Reinforced ? AttackID.StandardReinforce : AttackID.StandardCharge, guardianItem);
							}

							guardian.GuardianItemCharge = 0;
							Projectile.ai[0] = 0f;
							Projectile.netUpdate = true;
						}

						Projectile.Center = owner.MountedCenter.Floor() + new Vector2(8f * owner.direction, -(12 + guardian.GuardianItemCharge * 0.03f));
						Projectile.rotation = MathHelper.PiOver4 * (0.25f - guardian.GuardianItemCharge * 0.0025f) * owner.direction - MathHelper.PiOver4;

						owner.SetCompositeArmFront(true, CompositeArmStretchAmount.Full, MathHelper.PiOver2 * -(0.6f + guardian.GuardianItemCharge * 0.0025f) * owner.direction);
						owner.SetCompositeArmBack(true, CompositeArmStretchAmount.Quarter, MathHelper.PiOver2 * -(1f + guardian.GuardianItemCharge * 0.0025f) * owner.direction);
					}
					else if (Worn && !heldStandard)
					{ // Display on player back
						Projectile.Center = owner.MountedCenter.Floor();
						//Projectile.rotation = MathHelper.PiOver4 * 0.5f * owner.direction - MathHelper.PiOver4;
					}
					else
					{ // Idle - flag is held further and lower
						Ding = false;

						Projectile.Center = owner.MountedCenter.Floor() + new Vector2(16f * owner.direction, -8);
						Projectile.rotation = MathHelper.PiOver4 * 0.5f * owner.direction - MathHelper.PiOver4;

						owner.SetCompositeArmFront(true, CompositeArmStretchAmount.Full, MathHelper.PiOver2 * -0.8f * owner.direction);
						owner.SetCompositeArmBack(true, CompositeArmStretchAmount.Quarter, MathHelper.PiOver2 * -1.2f * owner.direction);
					}
					guardianItem.ExtraAIStandardHeld(this, Projectile, owner, guardian);
				}
			}
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 5; i++)
			{
				Main.dust[Dust.NewDust(Projectile.Center, 0, 0, DustID.Smoke)].velocity *= 0.25f;
			}
		}

		public void UpdateAndSyncValue(float valueToSync)
		{
			SyncedValue = valueToSync;
			SyncValue = true;
			NeedNetUpdate = true;
		}

		public override bool? CanCutTiles() => false;

		public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
		{
			if (SelectedItem < 0 || SelectedItem > 58) return false;
			if (StandardItem.ModItem is not OrchidModGuardianStandard guardianItem) return false;
			if (!ModContent.HasAsset(guardianItem.ShaftTexture)) return false;

			var player = Main.player[Projectile.owner];
			var color = Lighting.GetColor((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f), Color.White);
			if (Projectile.ai[1] < 30f && player.HeldItem.ModItem is not OrchidModGuardianStandard && !Reinforced) color *= Projectile.ai[1] / 30f;

			if (player.HeldItem.ModItem is not OrchidModGuardianStandard && Worn) guardianItem = (OrchidModGuardianStandard)BuffItem.ModItem;

			if (guardianItem.PreDrawStandard(spriteBatch, Projectile, player, ref color))
			{
				var texture = ModContent.Request<Texture2D>(guardianItem.ShaftTexture).Value;

				float drawRotation = Projectile.rotation;
				Vector2 posproj = Projectile.Center;

				if (player.gravDir == -1)
				{
					drawRotation = -drawRotation + MathHelper.PiOver2;
					posproj.Y = (player.Bottom.Floor() + player.position.Floor()).Y - posproj.Y + (posproj.Y - player.Center.Floor().Y) * 2f;
				}

				var drawPosition = Vector2.Transform(posproj - Main.screenPosition + Vector2.UnitY * player.gfxOffY, Main.GameViewMatrix.EffectMatrix);

				if (Worn && player.HeldItem.ModItem is not OrchidModGuardianStandard)
				{
					drawPosition += new Vector2(-8f * player.direction, -12 * player.gravDir);
					drawRotation = MathHelper.PiOver4 * 0.5f * -player.direction - MathHelper.PiOver4;

					if (player.gravDir == -1)
					{
						drawRotation = -drawRotation + MathHelper.PiOver2;
					}
				}

				var effect = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
				spriteBatch.Draw(texture, drawPosition, null, color, drawRotation, texture.Size() * 0.5f, Projectile.scale, effect, 0f);
				
				bool drawFlag = !guardianItem.DrawCustomFlag(spriteBatch, Projectile, player, lightColor, drawPosition, drawRotation);

				float windSpeed = 0.25f * Main.windSpeedCurrent;
				float flagRotation = MathHelper.PiOver4 * Projectile.localAI[1] * player.gravDir * 0.5f + (float)Math.Sin(TimeSpent * (Math.Abs(windSpeed) > 0.05f ? windSpeed : 0.05f)) * 0.1f;
				Vector2 flagOffset = Vector2.UnitX.RotatedBy(drawRotation) * -1.5f * Projectile.localAI[1];

				if (drawFlag || Alpha > 0f)
				{
					//had to dance around it in this way to make sure spritebatchsnapshot is defined within the scope of any commands that restart the spritebatch while also making it so flag textures are only defined if drawflag is true so custom drawn standards don't need empty images
					//unfortunately this means spritebatch NEEDS to be ended at least once to get the snapshot
					//please untangle this later
					spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
					if (Reinforced || Alpha > 0f)
						spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });

					if (Alpha > 0f && BuffItem != null)
					{ // Aura around the player showing range
						OrchidModGuardianStandard standard = (BuffItem.ModItem as OrchidModGuardianStandard);
						float lightalpha = (color.R + color.G + color.B) / 765f;
						if (lightalpha < 0.5f) lightalpha = 0.5f;
						float alphamult = (float)(Math.Sin(TimeSpent * 0.075f) * 0.075f + 1f) * Alpha * lightalpha;
						Vector2 drawPositionAura = Vector2.Transform(player.Center.Floor() - Main.screenPosition + Vector2.UnitY * player.gfxOffY, Main.GameViewMatrix.EffectMatrix);
						spriteBatch.Draw(TextureAura, drawPositionAura, null, standard.GetColor() * alphamult, 0f, TextureAura.Size() * 0.5f, 0.00625f * standard.AuraRange * player.GetModPlayer<OrchidGuardian>().GuardianStandardRange, SpriteEffects.None, 0f); ;
					}

					if (drawFlag)
					{
						Texture2D textureEnd = ModContent.Request<Texture2D>(guardianItem.FlagEndTexture).Value;
						Texture2D textureQuarter = ModContent.Request<Texture2D>(guardianItem.FlagTwoQuarterTexture).Value;
						Texture2D textureTwoQuarter = ModContent.Request<Texture2D>(guardianItem.FlagQuarterTexture).Value;
						Texture2D textureUp = ModContent.Request<Texture2D>(guardianItem.FlagUpTexture).Value;
						if (Reinforced)
						{ // Flag glow effect when reinforced
							Color glowColor = Color.White;
							if (Projectile.ai[1] < 30f && player.HeldItem.ModItem is not OrchidModGuardianStandard) glowColor *= Projectile.ai[1] / 30f;
							spriteBatch.Draw(textureEnd, drawPosition + flagOffset * 1.3f, null, glowColor, drawRotation + flagRotation * 2.4f, texture.Size() * 0.5f, Projectile.scale * 1.1f, effect, 0f);
							spriteBatch.Draw(textureQuarter, drawPosition + flagOffset * 1.2f, null, glowColor, drawRotation + flagRotation * 1.4f, texture.Size() * 0.5f, Projectile.scale * 1.1f, effect, 0f);
							spriteBatch.Draw(textureTwoQuarter, drawPosition + flagOffset * 1.1f, null, glowColor, drawRotation + flagRotation * 1f, texture.Size() * 0.5f, Projectile.scale * 1.1f, effect, 0f);
							spriteBatch.Draw(textureUp, drawPosition + flagOffset, null, glowColor, drawRotation + flagRotation * 0.5f, texture.Size() * 0.5f, Projectile.scale * 1.1f, effect, 0f);
						}
						if (Reinforced || Alpha > 0f)
							spriteBatch.End();
						spriteBatch.Begin(spriteBatchSnapshot);
						spriteBatch.Draw(textureEnd, drawPosition + flagOffset * 1.3f, null, color, drawRotation + flagRotation * 2.4f, texture.Size() * 0.5f, Projectile.scale, effect, 0f);
						spriteBatch.Draw(textureQuarter, drawPosition + flagOffset * 1.2f, null, color, drawRotation + flagRotation * 1.4f, texture.Size() * 0.5f, Projectile.scale, effect, 0f);
						spriteBatch.Draw(textureTwoQuarter, drawPosition + flagOffset * 1.1f, null, color, drawRotation + flagRotation * 1f, texture.Size() * 0.5f, Projectile.scale, effect, 0f);
						spriteBatch.Draw(textureUp, drawPosition + flagOffset, null, color, drawRotation + flagRotation * 0.5f, texture.Size() * 0.5f, Projectile.scale, effect, 0f);
					}
					else
					{
						spriteBatch.End();
						spriteBatch.Begin(spriteBatchSnapshot);
					}
				}
			}
			guardianItem.PostDrawStandard(spriteBatch, Projectile, player, color);

			return false;
		}
	}
}
