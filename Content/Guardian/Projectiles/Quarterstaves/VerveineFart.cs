using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System;
using Terraria.Audio;
using OrchidMod.Assets;

namespace OrchidMod.Content.Guardian.Projectiles.Quarterstaves
{
	public class VerveineFart : OrchidModGuardianProjectile
	{
		public override string Texture => $"Terraria/Images/Gore_435";

		public static readonly SoundStyle VerbenaSuper = new(OrchidAssets.SoundsPath + "VerbenaSuper");

		public override void SafeSetDefaults()
		{
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.timeLeft = 90;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.hide = true;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 15;
			Projectile.alpha = 255;
		}

		public override void SafeModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.HitDirectionOverride = target.velocity.X > 0 ? -1 : 1;
		}

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidGuardian guardian)
		{
			if (Projectile.ai[0] == 0)
			{
				target.AddBuff(BuffID.Poisoned, 300);
				target.AddBuff(BuffID.Stinky, 300);
			}
			else
			{
				target.AddBuff(BuffID.Slimed, 180);
			}
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			if (Projectile.ai[0] == 0)
			{
				target.AddBuff(BuffID.Poisoned, 300);
				target.AddBuff(BuffID.Stinky, 300);
			}
			else
			{
				target.AddBuff(BuffID.Slimed, 180);
			}
		}

		public override bool CanHitPlayer(Player target)
		{
			if (Projectile.trap)
			{
				if (Projectile.ai[1] == 1) return false;
				if (Projectile.ai[0] == 0 && target.HasBuff(BuffID.Stinky)) return false;
				if (Projectile.ai[0] == 1 && target.HasBuff(BuffID.Slimed)) return false;
			}
			return base.CanHitPlayer(target);
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			modifiers.HitDirectionOverride = -target.direction;
			modifiers.IncomingDamageMultiplier *= 0.25f;
		}

		public override void AI()
		{
			if (Projectile.timeLeft == 90)
			{
				bool super = Projectile.ai[1] == 1;
				SoundEngine.PlaySound(super ? VerbenaSuper : SoundID.Item16, Projectile.position);
				int goreType = Projectile.ai[0] == 0 ? GoreID.FartCloud1 : 375;
				int amount = super ? 80 : 24;
				for (int i = 0; i < amount; i++)
				{
					Gore fartCloud = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center - new Vector2(16, 8), Vector2.UnitY * 1.75f, goreType + Main.rand.Next(3));
					if (super)
					{
						fartCloud.velocity *= 4f;
						fartCloud.velocity.Y *= 1.5f;
						fartCloud.scale *= Main.rand.NextFloat(8f) - fartCloud.velocity.Length();
					}
					else
					{
						fartCloud.velocity.X *= 0.5f;
						fartCloud.velocity.Y *= 1.5f;
						fartCloud.scale *= Main.rand.NextFloat(1.5f);
					}
					fartCloud.rotation += Main.rand.NextFloat(MathHelper.TwoPi);
				}
				if (Projectile.ai[0] == 1) Projectile.timeLeft = 60;
				if (super) Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileID.Explosives, 500, 10, Projectile.owner);
			}
		}
	}
}