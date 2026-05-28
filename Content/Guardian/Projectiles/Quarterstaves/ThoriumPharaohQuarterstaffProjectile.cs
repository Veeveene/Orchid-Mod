using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;

namespace OrchidMod.Content.Guardian.Projectiles.Quarterstaves
{
	public class ThoriumPharaohQuarterstaffProjectile : OrchidModGuardianProjectile
	{
		public List<QuarterstaffProjectileSand> Sand;
		public List<int> HitNPCs;
		public int TimeSpent = 0;

		public override void SafeSetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 60;
			Projectile.scale = 1f;
			Projectile.penetrate = -1;
			Projectile.alpha = 255;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			HitNPCs = new List<int>();
		}

		public override void ModifyDamageHitbox(ref Rectangle hitbox)
		{
			hitbox.X -= 5;
			hitbox.Y -= 5;
			hitbox.Width += 10;
			hitbox.Height += 10;
		}

		public override bool? CanHitNPC(NPC target)
		{
			int count = 0;
			foreach (Projectile projectile in Main.projectile)
			{
				if (projectile.type == Type && Projectile.owner == projectile.owner && projectile.active && projectile.ModProjectile != null && Math.Abs(projectile.timeLeft - Projectile.timeLeft) <= 1)
				{
					if (projectile.ModProjectile is ThoriumPharaohQuarterstaffProjectile sand && sand.HitNPCs.Contains(target.whoAmI))
					{
						count++;
					}
				}
			}

			if (count >= 4 || Projectile.timeLeft < 30 || !Collision.CanHitLine(Projectile.Center, 2, 2, target.position, target.width, target.height)) return false;

			return base.CanHitNPC(target);
		}

		public override void SafeModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			// Damage dealt is divided by the amount of hits the enemy has already taken from the same attack, (100% -> 50% -> 25% -> 25%) Can nonly hit up to 4 times for 200% total damage

			int count = 1;
			foreach (Projectile projectile in Main.projectile)
			{
				if (Projectile.owner == projectile.owner && projectile.active && projectile.ModProjectile != null && Math.Abs(projectile.timeLeft - Projectile.timeLeft) <= 1)
				{
					if (projectile.ModProjectile is ThoriumPharaohQuarterstaffProjectile sand && sand.HitNPCs.Contains(target.whoAmI))
					{
						count++;
					}
				}
			}

			if (count > 4)
			{
				count = 4;
			}

			modifiers.FinalDamage /= count;
			HitNPCs.Add(target.whoAmI);
		}

		/*
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.velocity *= 0f;
			return false;
		}
		*/

		public override void AI()
		{
			TimeSpent++;
			if (!Initialized)
			{
				Initialized = true;

				Sand = new List<QuarterstaffProjectileSand>();

				for (int i = 0; i < 12; i++)
				{
					Vector2 sporeOffset = Vector2.UnitY.RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi));
					Vector2 sporeVelocity = Vector2.Normalize(sporeOffset) * Main.rand.NextFloat(0f, 1f);

					Sand.Add(new QuarterstaffProjectileSand(sporeOffset, sporeVelocity));
				}

			}

			if (Projectile.ai[0] != 0 && Projectile.timeLeft > 30)
			{
				Projectile.timeLeft = 30;
			}

			float colorMult = (float)Math.Sin(TimeSpent * 0.1f) * 0.2f + 0.8f; // Spore cloud emits light that gets dimmer as the projectile is about to expire
			if (Projectile.timeLeft < 30) colorMult *= Projectile.timeLeft / 30f;
			Lighting.AddLight(Projectile.Center, 0.443f * colorMult, 0.572f * colorMult, 1f * colorMult);

			if (Main.rand.NextBool(5))
			{
				Sand[Main.rand.Next(Sand.Count)].Kill = true;
				Vector2 sporeOffset = Vector2.UnitY.RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi));
				Vector2 sporeVelocity = Vector2.Normalize(sporeOffset) * Main.rand.NextFloat(0f, 1f);

				Sand.Add(new QuarterstaffProjectileSand(sporeOffset, sporeVelocity));
			}

			for (int i = Sand.Count - 1; i >= 0; i --)
			{
				QuarterstaffProjectileSand spore = Sand[i];

				spore.Update();
				if (spore.Scale < 0f)
				{
					Sand.Remove(spore);
				}
			}

			Projectile.ai[1]++;
			if (Projectile.ai[1] > 15)
			{
				Projectile.velocity *= 0.8f;
			}

			if (Projectile.ai[1] < 25)
			{
				Gore sand = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.position, new Vector2(0, 0), Main.rand.Next(220, 223));
				sand.rotation = Projectile.rotation;
				sand.alpha = 200;
				sand.velocity *= 0;
				sand.scale = Projectile.scale * Main.rand.NextFloat(0.5f, 1.2f);
			}
		}

		public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
		{
			if (!Initialized) return false;
			//spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
			//spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });

			// Draw code here

			Texture2D projTexture = TextureAssets.Projectile[Projectile.type].Value;

			float colorMult = 1f;
			if (Projectile.timeLeft < 30) colorMult *= Projectile.timeLeft / 30f;
			if (Projectile.ai[1] < 15)
			{
				if (Projectile.ai[1] < 10)
				{
					colorMult = 0f;
				}
				else
				{
					colorMult = (Projectile.ai[1] - 10f) * 0.2f; 
				}
			}


			Vector2 drawPosition = Projectile.Center - Main.screenPosition;

			foreach (QuarterstaffProjectileSand sand in Sand)
			{
				Rectangle drawRectangle = projTexture.Bounds;
				drawRectangle.Height /= 3;
				drawRectangle.Y = sand.Frame * drawRectangle.Height;

				Vector2 sporeDrawPosition = drawPosition + sand.Offset;
				Color colorGlow = Color.White * sand.Glow * colorMult;

				SpriteEffects spriteEffects = sand.Flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

				spriteBatch.Draw(projTexture, sporeDrawPosition, drawRectangle, colorGlow, sand.Rotation, drawRectangle.Size() * 0.5f, sand.Scale, spriteEffects, 0f);
			}

			//spriteBatch.End();
			//spriteBatch.Begin(spriteBatchSnapshot);

			return false;
		}

		public class QuarterstaffProjectileSand
		{
			public Vector2 Offset;
			public Vector2 Velocity;
			public int Timer;
			public int Frame;
			public Vector2 TargetPosition;
			public float Scale;
			public float ScaleTarget;
			public float Rotation;
			public float RotationAdditive;
			public float Glow;
			public bool Flip;
			public bool Kill;

			public QuarterstaffProjectileSand(Vector2 offset_, Vector2 velocity_)
			{
				Kill = false;
				Offset = offset_;
				Velocity = velocity_;
				Timer = Main.rand.Next(30);
				Frame = Main.rand.Next(3);
				TargetPosition = Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(16f);
				Scale = Main.rand.NextFloat(0f, 0.1f);
				ScaleTarget = Main.rand.NextFloat(0.5f, 1f);
				Glow = Main.rand.NextFloat(0.35f, 0.7f);
				Flip = Main.rand.NextBool();
				Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
				RotationAdditive = Main.rand.NextFloat(-0.05f, 0.05f);
			}

			public void Update()
			{
				Timer++;
				if (Timer > 40)
				{
					Timer = Main.rand.Next(30);
					TargetPosition = Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(24f);
				}

				if (Kill)
				{
					Scale -= 0.05f;
				}
				else if (Scale < ScaleTarget)
				{
					Scale += 0.03f;
				}

				Velocity += (TargetPosition - Offset) * 0.01f;
				if (Velocity.Length() > 0.75f)
				{
					Velocity = Vector2.Normalize(Velocity) * 0.5f;
				}

				Offset += Velocity;
				Rotation += RotationAdditive;
			}
		}
	}
}