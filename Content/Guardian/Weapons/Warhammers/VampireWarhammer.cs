using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Audio;
using OrchidMod.Content.General.Prefixes;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
	public class VampireWarhammer : OrchidModGuardianHammer
	{
		public override void SafeSetDefaults()
		{
			Item.width = 38;
			Item.height = 38;
			Item.value = Item.sellPrice(0, 5, 0, 0);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.DD2_DarkMageHealImpact;
			Item.knockBack = 6f;
			Item.shootSpeed = 10f;
			Item.damage = 190;
			Item.useTime = 35;
			Range = 20;
			GuardStacks = 1;
			ReturnSpeed = 0.75f;
			SwingSpeed = 1.5f;
			SwingChargeGain = 1.5f;
			BlockDuration = 40;
			BlockDamage = 1;
			HitCooldown = 20;
		}

		public override void ExtraAI(Player player, OrchidGuardian guardian, Projectile projectile, bool OffHand)
		{
			int dur = ((GuardianHammerAnchor)projectile.ModProjectile).BlockDuration;
			if (dur != 0)
			{
				int dir = projectile.velocity.X > 0 ? 1 : -1;

				if (dur > 0)
				{
					if (dur < BlockDuration)
						projectile.velocity = Vector2.Lerp(projectile.velocity, projectile.oldVelocity, dur / 40f);
					else if (dur < (int)(BlockDuration * guardian.GuardianBlockDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration()) - 1)
						projectile.velocity = projectile.oldVelocity;
				}

				for (int i = 0; i < 4; i++)
				{
					Dust dust = Dust.NewDustDirect(projectile.Center, 0, 0, DustID.BlueFairy, Alpha: 255);
					Vector2 dustOffs = Main.rand.NextVector2CircularEdge(20f, 20f);
					dust.noGravity = true;
					dust.position += dustOffs;
					dust.velocity = dust.velocity * 0.2f + projectile.velocity * 0.5f + dustOffs.RotatedBy(dir * 2f) * 0.3f;
					dust.scale *= 0.2f + Main.rand.NextFloat();
				}

				float speed = projectile.velocity.Length();
				if (speed > 0.1f)
				{
					Vector2 dustOffs = projectile.velocity.RotatedBy(MathHelper.PiOver2) * 20 / speed;
					Dust dust = Dust.NewDustPerfect(projectile.Center + dustOffs, DustID.BlueFairy, Scale: 0.25f + speed * 0.02f, Alpha: 255);
					dust.velocity = projectile.velocity * 0.05f;
					dust = Dust.NewDustPerfect(projectile.Center - dustOffs, DustID.BlueFairy, Scale: 0.25f + speed * 0.02f, Alpha: 255);
					dust.velocity = projectile.velocity * 0.05f;
				}
				speed = MathHelper.Lerp(speed, projectile.oldVelocity.Length(), 0.5f);
				if (speed > 0.1f)
				{
					Vector2 interPos = projectile.Center - (projectile.oldPosition - projectile.position) * 0.5f;
					Vector2 interVel = Vector2.Lerp(projectile.velocity, projectile.oldVelocity, 0.5f);
					Vector2 dustOffs = interVel.RotatedBy(MathHelper.PiOver2) * 20 / speed;
					Dust dust = Dust.NewDustPerfect(interPos + dustOffs, DustID.BlueFairy, Scale: 0.25f + speed * 0.02f, Alpha: 255);
					dust.velocity = interVel * 0.05f;
					dust = Dust.NewDustPerfect(interPos - dustOffs, DustID.BlueFairy, Scale: 0.25f + speed * 0.02f, Alpha: 255);
					dust.velocity = interVel * 0.05f;
				}

				if (projectile.soundDelay <= 0)
				{
					SoundEngine.PlaySound(SoundID.Item7.WithVolumeScale(0.6f), projectile.Center);
					projectile.soundDelay = 10;
				}
			}
		}

		bool giveResource = false;

		public override bool ThrowAI(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak, bool OffHand)
		{
			if (!Weak)
			{
				GuardianHammerAnchor anchor = projectile.ModProjectile as GuardianHammerAnchor;
				//one frame shorter than normal block duration to avoid triggering onblockthrow
				anchor.BlockDuration = (int)(BlockDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianBlockDuration + 9);
				projectile.ai[1] = 0;
				projectile.friendly = true;
				projectile.knockBack = 0f;
				projectile.tileCollide = true;
				anchor.ResetHitStatus(true);
				projectile.ResetLocalNPCHitImmunity();
				projectile.localNPCHitCooldown = HitCooldown;
				SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, projectile.Center);
				SoundEngine.PlaySound(SoundID.Item105.WithVolumeScale(0.6f), projectile.Center);
				giveResource = true;
				return false;
			}
			return true;
		}

		public override void OnBlockThrow(Player player, OrchidGuardian guardian, Projectile projectile)
		{
			//one frame shorter after onblockthrow is triggered to sync up with normal throws
			((GuardianHammerAnchor)projectile.ModProjectile).BlockDuration--;
			SoundEngine.PlaySound(SoundID.Item105.WithVolumeScale(0.6f), projectile.Center);
			giveResource = false;
		}

		public override void OnBlockHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit)
		{
			if (giveResource) guardian.AddGuard();
			giveResource = false;
		}

		public override bool ModifyHit(Player player, OrchidGuardian guardian, Projectile projectile, NPC target, ref NPC.HitModifiers modifiers, bool FullyCharged, bool Melee, bool Block, bool firstHit)
		{
			GuardianHammerAnchor anchor = projectile.ModProjectile as GuardianHammerAnchor;
			if (anchor.BlockDuration != 0)
			{
				if (anchor.HitCount < 6)
					anchor.HitCount++;
				modifiers.FinalDamage *= 5f / (anchor.HitCount + 4);
			}
			else if (target.type == NPCID.Vampire || target.type == NPCID.VampireBat)
				modifiers.FinalDamage *= 10;
			return true;
		}

		public override void OnMeleeHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged, bool OffHand)
		{
			if (target.type == NPCID.Vampire || target.type == NPCID.VampireBat && guardian.GuardianItemCharge < 210)
			{
				CombatText.NewText(player.Hitbox, new Color(255, 175, 175), Language.GetTextValue("Mods.OrchidMod.Items.VampireWarhammer.InstantChargeText"), false);
				guardian.GuardianItemCharge = 210;
			}
		}

		/*public override bool ThrowAI(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak)
		{
			int dur = ((GuardianHammerAnchor)projectile.ModProjectile).range;
			int dir = projectile.velocity.X > 0 ? 1 : -1;

			projectile.rotation += dir * 0.1f;
			if (dur > 0)
				projectile.velocity *= 0.99f;

			Dust dust = Dust.NewDustDirect(projectile.Center, 0, 0, DustID.BlueFairy, Alpha: 255);
			Vector2 dustOffs = Main.rand.NextVector2CircularEdge(20f, 20f);
			dust.noGravity = true;
			dust.position += dustOffs * (1 - Main.rand.NextFloat() * Main.rand.NextFloat());
			dust.velocity = dust.velocity * 0.2f + projectile.velocity * 0.5f + dustOffs.RotatedBy(dir * 2f) * 0.3f;

			float speed = projectile.velocity.Length();
			if (speed > 0.1f)
			{
				dustOffs = projectile.velocity.RotatedBy(MathHelper.PiOver2) * 20 / speed;
				dust = Dust.NewDustPerfect(projectile.Center + dustOffs, DustID.BlueFairy, Scale: 0.2f + speed * 0.05f, Alpha: 255);
				dust.velocity = projectile.velocity * 0.2f;
				dust = Dust.NewDustPerfect(projectile.Center - dustOffs, DustID.BlueFairy, Scale: 0.2f + speed * 0.05f, Alpha: 255);
				dust.velocity = projectile.velocity * 0.2f;
			}

			return true;
		}*/
	}
}
