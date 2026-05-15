using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Projectiles.Katars
{
	public class HellstoneKatarProjectile : OrchidModGuardianProjectile
	{
		public List<int> HitNPCs;
		public override void SafeSetDefaults()
		{
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 321;
			Projectile.scale = 1f;
			Projectile.penetrate = -1;
			Projectile.alpha = 255;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
			HitNPCs = new List<int>();
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (Projectile.ai[0] != 0f || Projectile.ai[1] != 0f)
			{
				if (GuardianShieldAnchor.LineIntersectsRect(new Point((int)Projectile.Center.X, (int)Projectile.Center.Y), new Point((int)Projectile.ai[0], (int)Projectile.ai[1]), target.Hitbox))
				{
					return base.CanHitNPC(target);
				}
			}
			return false;
		}

		public override void SafeModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (!HitNPCs.Contains(target.whoAmI))
			{
				HitNPCs.Add(target.whoAmI);
				modifiers.FinalDamage *= 2f;
			}
		}

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidGuardian guardian)
		{
			target.AddBuff(BuffID.OnFire, 300);
		}

		public override void ModifyDamageHitbox(ref Rectangle hitbox)
		{
			Point origin = new Point((int)Projectile.position.X, (int)Projectile.position.Y);
			int length = (int)Projectile.ai[0];
			int height = (int)Projectile.ai[1];

			if (origin.X - length > 0)
			{
				int buffer = origin.X;
				origin.X = length;
				length = buffer;
			}

			if (origin.Y - height > 0)
			{
				int buffer = origin.Y;
				origin.Y = height;
				height = buffer;
			}

			hitbox = new Rectangle(origin.X, origin.Y, length, height);

			/*
			Dust dust = Dust.NewDustDirect(new Vector2(origin.X, origin.Y), 1, 1, DustID.GreenTorch);
			dust.noGravity = true;
			dust = Dust.NewDustDirect(new Vector2(origin.X + (length - origin.X), origin.Y), 1, 1, DustID.BlueTorch);
			dust.noGravity = true;
			dust = Dust.NewDustDirect(new Vector2(origin.X, origin.Y + (height - origin.Y)), 1, 1, DustID.PurpleTorch);
			dust.noGravity = true;
			dust = Dust.NewDustDirect(new Vector2(origin.X + (length - origin.X), origin.Y + (height - origin.Y)), 1, 1, DustID.RedTorch);
			dust.noGravity = true;
			*/
		}

		public override void AI()
		{
			if (Projectile.timeLeft >= 300)
			{
				Projectile.ai[0] = Owner.Center.X;
				Projectile.ai[1] = Owner.Center.Y;

				if (Projectile.timeLeft == 300)
				{
					SoundEngine.PlaySound(SoundID.Item74.WithPitchOffset(Main.rand.NextFloat(0.4f, 0.6f)), Owner.Center);
					Projectile.netUpdate = true;
				}
			}

			if (Projectile.ai[0] != 0f || Projectile.ai[1] != 0f)
			{ // Surely the player won't stand in the top-left corner of the world!
				Vector2 direction = Projectile.position - new Vector2(Projectile.ai[0], Projectile.ai[1]);

				int length = (int)((300 - direction.Length()) / 10f); // used for dust spawn randomness, the dash length should be 20 * 15 = 300 (katar dash velocity * katar dash duration)
				if (length < 1) length = 1;
				if (length > 4) length = 4;

				if (Main.rand.NextBool(length))
				{
					for (int i = 0; i < 3; i++)
					{
						Vector2 position = Projectile.position - Vector2.Normalize(direction) * Main.rand.NextFloat(direction.Length());

						if (Main.rand.NextBool(5))
						{
							Dust dust2 = Dust.NewDustDirect(position - new Vector2(4, 4), 8, 8, DustID.Smoke);
							dust2.scale = Main.rand.NextFloat(1f, 1.5f);
							dust2.velocity.Y = Main.rand.NextFloat(-0.5f, -1f);
							dust2.velocity.X *= 0.25f;

							if (Main.rand.NextBool(10))
							{
								Gore gore = Gore.NewGoreDirect(Owner.GetSource_FromAI(), position, Vector2.UnitY.RotatedByRandom(MathHelper.Pi), 61 + Main.rand.Next(3));
								gore.rotation = Main.rand.NextFloat(MathHelper.Pi);
								gore.scale *= Main.rand.NextFloat(0.2f, 0.4f);
								gore.velocity *= Main.rand.NextFloat(0.2f, 0.4f);
							}
						}

						Dust dust = Dust.NewDustDirect(position - new Vector2(4, 4), 8, 8, DustID.Torch);
						dust.scale = Main.rand.NextFloat(0.75f, 1.7f);
						dust.velocity *= 0.5f;
						dust.noGravity = true;

						dust = Dust.NewDustDirect(position - new Vector2(4, 4), 8, 8, DustID.Torch);
						dust.scale = Main.rand.NextFloat(0.75f, 1.7f);
						dust.velocity *= 0.5f;
						dust.velocity.Y = Main.rand.NextFloat(-2f, -1f);
						dust.noGravity = true;
					}
				}
			}
		}
	}
}