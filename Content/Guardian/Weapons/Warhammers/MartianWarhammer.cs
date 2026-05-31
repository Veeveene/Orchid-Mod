using Microsoft.Xna.Framework;
using System;
using OrchidMod.Common.ModObjects;
using Terraria;
using Terraria.Localization;
using Terraria.Audio;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
	public class MartianWarhammer : OrchidModGuardianHammer
	{
		public override void SafeSetDefaults()
		{
			Item.width = 44;
			Item.height = 46;
			Item.value = Item.sellPrice(0, 10, 0, 0);
			Item.rare = ItemRarityID.Yellow;
			Item.UseSound = SoundID.Zombie49;
			Item.knockBack = 6f;
			Item.shootSpeed = 80f;
			Item.damage = 320;
			Item.useTime = 20;
			Range = 38;
			GuardStacks = 2;
			ReturnSpeed = 8f;
			HitCooldown = 15;
			TileCollide = false;
			Penetrate = true;
			BlockDuration = 360;
		}

		public override void ExtraAI(Player player, OrchidGuardian guardian, Projectile projectile, bool OffHand)
		{
			if (projectile.ai[1] <= -1)
			{
				if (guardian.GuardianItemCharge >= 180 && projectile.ai[1] > -30) projectile.ai[1]++;
			}
		}

		public override bool ThrowAI(Player player, OrchidGuardian guardian, Projectile projectile, bool weak, bool OffHand)
		{
			GuardianHammerAnchor anchor = projectile.ModProjectile as GuardianHammerAnchor;
			NPC target = null;
			if (anchor.range == Range)
			{
				projectile.localNPCHitCooldown = 10;
				projectile.ResetLocalNPCHitImmunity();
				projectile.ai[1] = 37;
			}
			Vector2 teleportFrom = Vector2.Zero;
			switch (projectile.ai[1])
			{
				case 37:
				{
					anchor.ResetHitStatus(!weak);
					if (weak) projectile.ai[1] = 13;
					else SoundEngine.PlaySound(SoundID.Item93.WithPitchOffset(0.3f).WithVolumeScale(0.8f), projectile.Center);
					projectile.friendly = true;
					projectile.localNPCHitCooldown = 10;
					projectile.ai[2] = -1;
					anchor.range--;
					teleportFrom = projectile.Center;
					float distanceClosest = 240f;
					foreach (NPC npc in Main.npc)
					{
						if (npc.whoAmI == projectile.ai[2]) continue;
						float distance = npc.Center.Distance(Main.MouseWorld);
						if (OrchidModProjectile.IsValidTarget(npc) && npc.type != NPCID.TargetDummy && projectile.Center.Distance(npc.Center) < projectile.velocity.Length() * 6 && distance < distanceClosest)
						{
							target = npc;
							distanceClosest = distance;
						}
					}
					if (target != null) projectile.ai[2] = target.whoAmI;
					else
					{
						if (Main.MouseWorld.Distance(projectile.Center) < projectile.velocity.Length() * 6) projectile.Center = Main.MouseWorld;
						else projectile.position += projectile.velocity * 6;
					}
					projectile.velocity = Vector2.Zero;
					break;
				}
				case 25:
				{
					teleportFrom = projectile.Center;
					float distanceClosest = 480f;
					foreach (NPC npc in Main.npc)
					{
						if (npc.whoAmI == projectile.ai[2]) continue;
						float distance = projectile.Center.Distance(npc.Center);
						if (OrchidModProjectile.IsValidTarget(npc) && distance < distanceClosest)
						{
							target = npc;
							distanceClosest = distance;
						}
					}
					if (target != null) projectile.ai[2] = target.whoAmI;
					else
					{
						projectile.position += new Vector2(projectile.ai[1] * 8 * Main.rand.NextFloat(), 0).RotatedByRandom(MathHelper.Pi);
						projectile.ai[2] = -1;
					}
					break;
				}
				case 13: goto case 25;
				case 1:
				{
					projectile.Kill();
					SoundEngine.PlaySound(SoundID.Item75, (projectile.Center + player.Center) * 0.5f);
					DrawLaser(projectile.Center, player.Center);
					return false;
				}
			}
			if (projectile.ai[2] != -1)
			{
				if (target != null)
				{
					target = Main.npc[(int)projectile.ai[2]];
					if (target != null && target.active && target.Distance(projectile.Center) < 480)
						projectile.Center = target.Center;
					else projectile.ai[2] = -1;
				}
			}
			if (teleportFrom != Vector2.Zero)
			{
				DrawLaser(teleportFrom, projectile.Center);
				for (int i = 0; i < 10; i++)
				{
					Dust dust = Dust.NewDustDirect(projectile.Center, 0, 0, DustID.Electric);
					dust.velocity.SafeNormalize(Vector2.Zero);
					dust.noGravity = true;
				}
				anchor.Ding = false;
				SoundEngine.PlaySound(projectile.ai[2] == -1 ? SoundID.Item75 : SoundID.Item91, (teleportFrom + projectile.Center)  * 0.5f);
			}
			projectile.rotation += player.direction;
			projectile.ai[1]--;
			return false;
		}

		static void DrawLaser(Vector2 start, Vector2 end)
		{
			Vector2 dir = (end - start).SafeNormalize(Vector2.Zero);
			end -= dir * 16;
			int maxIterations = Math.Min((int)start.Distance(end) / 8, 100);
			for (int i = 0; i < maxIterations; i++)
			{
				Dust dust = Dust.NewDustDirect((start * (maxIterations - i) + end * i) / maxIterations, 0, 0, 220 + Main.rand.Next(2), Alpha: 100);
				dust.scale = 0.5f + Main.rand.NextFloat() * 0.3f;
				dust.velocity = (dust.velocity + dir * 8) / 2.5f;
				dust.noGravity = true;
				dust.noLight = true;
				dust.frame.Y = 80;
			}
		}

		public override void OnThrowHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak, bool OffHand)
		{
			GuardianHammerAnchor anchor = projectile.ModProjectile as GuardianHammerAnchor;
			if (!anchor.Ding)
			{
				SoundEngine.PlaySound(SoundID.Item94, projectile.Center);
				anchor.Ding = true;
			}
		}

		public override void OnMeleeHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged, bool OffHand)
		{

			if (!((GuardianHammerAnchor)projectile.ModProjectile).WeakHit && !FullyCharged)
			{
				CombatText.NewText(player.Hitbox, new Color(175, 255, 175), Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.Charged"), false);
				SoundEngine.PlaySound(SoundID.Item92, player.Center);
				guardian.GuardianItemCharge = 210;
			}

			for (int i = 0; i < 10; i++)
			{
				Dust dust = Dust.NewDustDirect(projectile.Center, 0, 0, DustID.Electric);
				dust.noGravity = true;
				dust.velocity *= 2f;
			}
		}
	}
}
