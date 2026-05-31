using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using OrchidMod.Common.Attributes;
using OrchidMod.Common.ModObjects;
using OrchidMod.Content.Guardian.Buffs.Debuffs;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
	[CrossmodContent("ThoriumMod")]
	public class ThoriumGrandThunderBirdWarhammer : OrchidModGuardianHammer
	{

		public override void SafeSetDefaults()
		{
			Item.width = 36;
			Item.height = 36;
			Item.value = Item.sellPrice(0, 0, 20, 0);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.DD2_MonkStaffSwing;
			Item.knockBack = 6f;
			Item.shootSpeed = 10f;
			Item.damage = 84;
			Item.useTime = 48;
			Range = 30;
			GuardStacks = 1;
			SlamStacks = 1;
			ReturnSpeed = 1f;
			SwingSpeed = 1.33f;
			SwingChargeGain = 0.75f;
			BlockDuration = 150;
		}

		public static readonly SoundStyle ThoriumCloudSpawn = new("ThoriumMod/Sounds/Custom/GrandCloudSpawn");
		public static readonly SoundStyle ThoriumZapNoise = new("ThoriumMod/Sounds/Custom/GrandZapNoise");
		public static readonly SoundStyle ThoriumThunderTalon = new("ThoriumMod/Sounds/Custom/ThunderTalon");
		public static readonly SoundStyle ThoriumParalyzeSound = new("ThoriumMod/Sounds/Custom/ParalyzeSound"); //thank you jopo for modder's toolkit

		List<int> Targets;
		SlotId soundSlot;

		public override void ExtraAI(Player player, OrchidGuardian guardian, Projectile projectile, bool OffHand)
		{
			Vector2 pos = projectile.ai[1] > 0 ? projectile.Center : player.Center; 
			Targets = new List<int>();
			List<float> targetDists = new List<float> {120f};
			foreach (NPC npc in Main.npc)
			{
				if (OrchidModProjectile.IsValidTarget(npc))
				{
					float dist = Math.Max(npc.Center.Distance(pos) - npc.width * 0.5f, 0);
					if (npc.HasBuff<ThoriumGrandThunderBirdWarhammerDebuff>()) dist = dist / 2f - 120f;
					if (dist < targetDists.Last())
					{
						for (int i = 0; i < targetDists.Count; i++)
						{
							if (dist < targetDists[i])
							{
								Targets.Insert(i, npc.whoAmI);
								targetDists.Insert(i, dist);
								break;
							}
						}
						if (targetDists.Count >= 6)
						{
							Targets.RemoveAt(4);
							targetDists.RemoveAt(5);
						}
					}
				}
			}

			GuardianHammerAnchor guardProj = projectile.ModProjectile as GuardianHammerAnchor;
			bool canTether = !guardProj.WeakHit && (projectile.ai[1] <= 0 || (guardProj.Strong && guardProj.NotHitYet));
			if (canTether)
			{
				SoundEngine.TryGetActiveSound(soundSlot, out ActiveSound activeSound);
				if (activeSound == null)
				{
					var tracker = new ProjectileAudioTracker(projectile);
					soundSlot = SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryCircle, projectile.Center, sound =>
					{
						if (guardProj.WeakHit || (projectile.ai[1] > 0 && (!guardProj.Strong || !guardProj.NotHitYet)))
						{
							sound.Stop();
							return false;
						}
						sound.Position = projectile.position;
						sound.Pitch = -0.8f + 0.2f * Targets.Count;
						sound.Volume = 0.15f * Targets.Count * (projectile.ai[1] < -1 ? 1 / -projectile.ai[1] : 1);
						return tracker.IsActiveAndInGame();
					});
				}
			
				if (Targets.Count > 0)
				{
					if (projectile.ai[1] == 0 && guardian.GuardianItemCharge < 210f)
					{
						guardian.GuardianItemCharge += 7.5f * Targets.Count / Item.useTime * player.GetTotalAttackSpeed(DamageClass.Melee);
						if (guardian.GuardianItemCharge > 210f)
							guardian.GuardianItemCharge = 210f;
					}

					//if (projectile.ai[1] >= 0 && Main.rand.NextBool(60 - Targets.Count * 10)) SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryCircle.WithPitchOffset(0.2f * Targets.Count).WithVolumeScale(0.1f + 0.05f  * Targets.Count), projectile.Center);

					Vector2 gemPos = projectile.Center + new Vector2(8 * projectile.spriteDirection, -8).RotatedBy(projectile.ai[1] > 0 ? projectile.rotation : guardian.GuardianItemCharge * 0.0065f * player.gravDir * projectile.spriteDirection);

					if (projectile.ai[1] >= 0 && canTether && Main.rand.Next(6) < Targets.Count)
					{
						if (Main.rand.NextBool(3))
						{
							Dust zapDust = Dust.NewDustDirect(gemPos - new Vector2(4), 0, 0, DustID.Teleporter, Scale: Main.rand.NextFloat(0.4f) + Targets.Count * 0.1f);
							if (Main.rand.NextBool())
							{
								zapDust.noGravity = true;
								zapDust.scale *= 4f;
							}
						}
						else
						{
							Dust zapDust = Dust.NewDustDirect(gemPos - new Vector2(4), 0, 0, DustID.Electric, Scale: Main.rand.NextFloat(0.4f) + Targets.Count * 0.075f);
							zapDust.velocity *= 0.2f;
							if (Main.rand.NextBool())
							{
								zapDust.noGravity = true;
								zapDust.scale *= 1.5f;
							}
						}
					}
					foreach (int target in Targets)
					{
						bool conductive = Main.npc[target].HasBuff<ThoriumGrandThunderBirdWarhammerDebuff>();
						if (canTether && Main.rand.Next(conductive ? 200 : 400) * (projectile.ai[1] < 0 ? -projectile.ai[1] : 1) < Main.npc[target].Center.Distance(gemPos))
						{
							Vector2 toTarget = Main.npc[target].Center - gemPos;
							if (conductive)
							{
								Dust.NewDustDirect(gemPos - new Vector2(4) + toTarget * Main.rand.NextFloat(), 0, 0, DustID.Teleporter, Scale: 1f).noGravity = true;
							}
							else
							{
								Dust zapDust = Dust.NewDustDirect(gemPos - new Vector2(4) + toTarget * Main.rand.NextFloat(), 0, 0, DustID.Electric, Scale: 0.5f);
								zapDust.noGravity = true;
								zapDust.velocity *= 0.25f;
							}
						}
					}
				}
			}
		}

		public override void OnSwing(Player player, OrchidGuardian guardian, Projectile projectile, bool FullyCharged, bool OffHand)
		{
			if (Targets.Count > 0)
			{
				int kills = ShockTargets(projectile, player);

				if (OrchidMod.ThoriumMod != null)
				{
					SoundEngine.PlaySound(ThoriumCloudSpawn.WithPitchOffset(0.4f - 0.1f * Targets.Count).WithVolumeScale(0.15f + 0.05f * Targets.Count), projectile.Center);
				}

				if (!((GuardianHammerAnchor)projectile.ModProjectile).WeakHit)
				{
					if (guardian.GuardianItemCharge < 210f)
					{
						guardian.GuardianItemCharge += 15f * (Targets.Count + kills / 2f) * player.GetTotalAttackSpeed(DamageClass.Melee);
						if (guardian.GuardianItemCharge > 210f)
							guardian.GuardianItemCharge = 210f;
					}
				}
			}
		}

		int ShockTargets(Projectile projectile, Player player)
		{
			int killedTargets = 0;
			foreach (int target in Targets)
			{
				OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
				NPC npc = Main.npc[target];
				bool conducted = npc.HasBuff<ThoriumGrandThunderBirdWarhammerDebuff>();
				if (player.whoAmI == Main.myPlayer)
				{
					NPCKillAttempt isKill = new(npc);
					player.ApplyDamageToNPC(npc, guardian.GetGuardianDamage(Item.damage * (conducted ? 0.75f : 0.5f)), Item.knockBack / 12f, npc.Center.X > player.Center.X ? 1 : -1, Main.rand.Next(100) < guardian.GetGuardianCrit(), ModContent.GetInstance<GuardianDamageClass>(), true);
					if (isKill.DidNPCDie()) killedTargets++;
				}
				Vector2 gemPos = projectile.Center + new Vector2(8 * projectile.spriteDirection, -8).RotatedBy(projectile.ai[1] > 0 ? projectile.rotation : guardian.GuardianItemCharge * 0.0065f * player.gravDir * projectile.spriteDirection);
				Vector2 currPoint = gemPos;
				Vector2 nextPoint;
				float angleToEnemy = gemPos.AngleTo(npc.Center);
				float dist = (gemPos - npc.Center).Length();
				bool whichWay = Main.rand.NextBool();
				int jumps = Math.Min(1 + (int)(dist / 16f), 20);
				for (int i = 1; i < jumps + 1; i++)
				{
					nextPoint = (gemPos * (jumps - i) / jumps) + (npc.Center * i / jumps) + new Vector2(4f - Math.Abs(i - (jumps / 2)) * 8f / jumps, 0).RotatedBy(angleToEnemy + ((whichWay = !whichWay) ? 1.57f : -1.57f)) * (1f + Main.rand.NextFloat(conducted ? 5 : 3));
					for (int j = 0; j < 4; j++)
					{
						Dust zapDust = Dust.NewDustDirect((currPoint * (4 - j) / 4f) + (nextPoint * j / 4f), 0, 0, conducted ? DustID.Teleporter : DustID.Electric, Scale: conducted ? 2f : 0.75f);
						zapDust.noGravity = true;
						zapDust.velocity *= 0.05f;
					}
					currPoint = nextPoint;
				}
			}
			return killedTargets;
		}

		public override void OnThrowHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak, bool OffHand)
		{
			target.AddBuff(ModContent.BuffType<ThoriumGrandThunderBirdWarhammerDebuff>(), Weak ? 300 : 600);
			if (!Weak) ShockTargets(projectile, player);

			if (OrchidMod.ThoriumMod != null)
			{
				SoundEngine.PlaySound(ThoriumThunderTalon.WithPitchOffset(-0.3f - 0.1f * Targets.Count).WithVolumeScale(0.5f + 0.05f * Targets.Count), projectile.Center);
			}
		}

		public override void OnThrowHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak, bool OffHand)
		{
			target.AddBuff(ModContent.BuffType<ThoriumGrandThunderBirdWarhammerDebuff>(), 300);
			if (OrchidMod.ThoriumMod != null)
			{
				SoundEngine.PlaySound(ThoriumZapNoise.WithPitchOffset(-0.1f).WithVolumeScale(0.25f), projectile.Center);
			}
		}

		public override void OnMeleeHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged, bool OffHand)
		{
			target.AddBuff(ModContent.BuffType<ThoriumGrandThunderBirdWarhammerDebuff>(), 300);
			if (OrchidMod.ThoriumMod != null)
			{
				SoundEngine.PlaySound(ThoriumZapNoise.WithPitchOffset(-0.1f).WithVolumeScale(0.25f), projectile.Center);
			}
		}

		public override void OnThrow(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak, bool OffHand)
		{
			if (Weak && OrchidMod.ThoriumMod != null) SoundEngine.PlaySound(ThoriumParalyzeSound.WithPitchOffset(-0.6f).WithVolumeScale(0.1f * Targets.Count), projectile.Center);
		}
	}
}
