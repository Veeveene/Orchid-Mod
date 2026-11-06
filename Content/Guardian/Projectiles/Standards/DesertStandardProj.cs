using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using OrchidMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Projectiles.Standards
{
	public class DesertStandardProj : OrchidModGuardianProjectile
	{
		public override void SafeSetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 12;
			Projectile.penetrate = 3;
			Projectile.hide = true;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 40;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (target.whoAmI == Projectile.ai[0]) return false;
			Player player = Main.player[Projectile.owner];
			OrchidGuardian guardian = player.Guardian();
			GuardianStandardAnchor standardAnchor = guardian.GuardianCurrentStandardAnchor.ModProjectile as GuardianStandardAnchor; 
			if (target.Center.Distance(player.Center) > (standardAnchor.BuffItem.ModItem as OrchidModGuardianStandard).AuraRange * guardian.GuardianStandardRange + target.width * 0.5f) return false;
			return base.CanHitNPC(target);
		}

		public override void AI()
		{
			if (Projectile.timeLeft == 12)
			{
				Projectile.friendly = true;
				Projectile.damage = (int)(Projectile.ai[1] * Projectile.penetrate * 0.25f);
			}
			Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Electric);
			dust.velocity = new Vector2(0, -0.5f) + dust.velocity * Main.rand.NextFloat();
			dust.scale *= 0.5f;
			Projectile.localAI[0] = (12 - Projectile.timeLeft) * 32;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if ((projHitbox.Center.ToVector2() - targetHitbox.Center.ToVector2()).Length() < Projectile.localAI[0]) return true;
			return false;
		}

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidGuardian guardian)
		{
			Projectile.friendly = false;
			Vector2 currPoint = Projectile.Center;
			Vector2 nextPoint;
			float angleToEnemy = Projectile.Center.AngleTo(target.Center);
			float dist = (Projectile.Center - target.Center).Length();
			bool whichWay = Main.rand.NextBool();
			int jumps = Math.Min(1 + (int)(dist / 16f), 20);
			for (int i = 1; i < jumps + 1; i++)
			{
				nextPoint = (Projectile.Center * (jumps - i) / jumps) + (target.Center * i / jumps) + new Vector2(4f - Math.Abs(i - (jumps / 2)) * 8f / jumps, 0).RotatedBy(angleToEnemy + ((whichWay = !whichWay) ? 1.57f : -1.57f)) * (1f + Main.rand.NextFloat() * 3f);
				for (int j = 0; j < 4; j++)
				{
					Dust zapDust = Dust.NewDustDirect((currPoint * (4 - j) / 4f) + (nextPoint * j / 4f), 0, 0, DustID.Electric);
					zapDust.noGravity = true;
					zapDust.velocity *= 0.05f;
				}
				currPoint = nextPoint;
			}
			SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap, Projectile.Center);
			Projectile.Center = target.Center;
			Projectile.timeLeft = 13;
			Projectile.localAI[0] = 0;
		}
	}
}