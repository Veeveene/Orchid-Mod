using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Content.Shapeshifter.Accessories;
using OrchidMod.Content.Shapeshifter.Weapons.Warden;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Shapeshifter.Projectiles.Warden
{
	public class WardenSnailProj : OrchidModShapeshifterProjectile
	{
		private static Texture2D TextureMain;
		private static Texture2D TextureGlow;
		public List<Vector2> OldPosition;
		public List<float> OldRotation;
		public int Bounces = 0;

		public override void SafeSetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 600;
			Projectile.scale = 1f;
			Projectile.penetrate = 1;
			Projectile.alpha = 255;
			Projectile.tileCollide = false;
			TextureMain ??= ModContent.Request<Texture2D>(Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			TextureGlow ??= ModContent.Request<Texture2D>(Texture + "_Glow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			OldPosition = new List<Vector2>();
			OldRotation = new List<float>();
			Bounces = 0;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			fallThrough = Projectile.timeLeft > 595;
			return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (Projectile.penetrate < 1) return false;
			return base.CanHitNPC(target);
		}

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidShapeshifter shapeshifter)
		{
			Projectile.penetrate = -1;
			Player owner = Owner;
			if (!owner.dead && owner.active)
			{
				Vector2 toOwner = Vector2.Normalize(owner.position - target.position);
				Projectile.velocity.X = toOwner.X * Math.Abs(Projectile.velocity.X) * 0.75f;
				Projectile.ai[1] ++;
			}
			Projectile.netUpdate = true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.penetrate > 0 || oldVelocity.Length() > 8f)
			{
				float bouncestrength = 0.5f;
				if (Projectile.ai[0] == 0)
				{
					Projectile.ai[0] = 1f;
					Projectile.damage = (int)(Projectile.damage * 0.75f);
					bouncestrength = 0.75f;
					Projectile.netUpdate = true;
				}

				if (oldVelocity.Length() > 8f && Bounces < 3)
				{
					Bounces++;
					SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);

					for (int i = 0; i < (int)(bouncestrength * 10); i++)
					{
						Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
						dust.scale = Main.rand.NextFloat(0.6f, 0.8f);
						dust.velocity *= Main.rand.NextFloat(0.25f, 0.75f);
					}
				}

				if (oldVelocity.Y != Projectile.velocity.Y)
				{
					Projectile.velocity.Y = -oldVelocity.Y * bouncestrength;
				}

				if (oldVelocity.X != Projectile.velocity.X)
				{
					Projectile.velocity.X = -oldVelocity.X * bouncestrength;
				}
			}
			else
			{
				Projectile.penetrate = -1;
			}

			return false;
		}

		public override void AI()
		{
			OldPosition.Add(Projectile.Center);
			OldRotation.Add(Projectile.rotation);

			Projectile.rotation += Projectile.velocity.X / 30f;

			if (!Initialized)
			{
				Initialized = true;
				Projectile.tileCollide = true;
				SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.Center);
				Projectile.localAI[1] = Projectile.ai[1];

				for (int i = 0; i < 5; i++)
				{
					Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Smoke);
					dust.scale *= Main.rand.NextFloat(1f, 1.5f);
					dust.velocity *= Main.rand.NextFloat(0.5f, 0.75f);
				}

				for (int i = 0; i < 2; i++)
				{
					Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromAI(), Projectile.Center + new Vector2(Main.rand.NextFloat(-24f, 0f), Main.rand.NextFloat(-24f, 0f)), Vector2.UnitY.RotatedByRandom(MathHelper.Pi), 61 + Main.rand.Next(3));
					gore.rotation = Main.rand.NextFloat(MathHelper.Pi);
					gore.velocity += Projectile.velocity * Main.rand.NextFloat(0.1f, 0.25f);
				}
			}

			if (Projectile.timeLeft < 420 && Projectile.velocity.Length() < 3f)
			{
				Projectile.localAI[2] = 1;
				Projectile.penetrate = -1;
				Projectile.netUpdate = true;
			} 

			if (Projectile.localAI[2] > 0 || Projectile.localAI[1] < Projectile.ai[1])
			{ // tries to find owner
				if (Projectile.Hitbox.Distance(Owner.Center) < 32f)
				{
					Projectile.ai[2] = 1f;
					Projectile.tileCollide = false;
				}

				if (Projectile.velocity.Length() < 5f)
				{ // for glow sine
					Projectile.localAI[0]++;
				}
			}

			if (Projectile.ai[2] == 1f)
			{ // homes towards owner for easier picked
				Projectile.velocity = Vector2.Normalize(Owner.Center - Projectile.Center) * 5f;

				if (Owner.Hitbox.Intersects(Projectile.Hitbox))
				{
					OrchidShapeshifter shapeshifter = Shapeshifter();
					if (shapeshifter.IsShapeshifted)
					{
						if (shapeshifter.ShapeshiftAnchor.ShapeshifterItem.ModItem is WardenSnail snail)
						{
							shapeshifter.ShapeshiftAnchor.ai[1] = Projectile.ai[1];
							shapeshifter.ShapeshiftAnchor.NeedNetUpdate = true;
						}
					}

					if (Projectile.ai[1] > Projectile.localAI[1])
					{
						shapeshifter.modPlayer.TryHeal(shapeshifter.GetShapeshifterHealing(5));
					}
					Projectile.Kill();
				}

			}
			else
			{ // else normal gravity movement
				Projectile.velocity.Y += 0.3f;
				if (Projectile.penetrate < 0)
				{
					Projectile.velocity.X *= 0.975f;
				}
				else
				{
					Projectile.velocity.X *= 0.99f;
				}
			}

			if (OldPosition.Count > 5)
			{
				OldPosition.RemoveAt(0);
				OldRotation.RemoveAt(0);
			}
		}

		public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
		{
			float colorMult = 1f;
			if (Projectile.timeLeft < 7) colorMult *= Projectile.timeLeft / 7f;

			for (int i = 0; i < OldPosition.Count; i++)
			{
				Vector2 drawPosition2 = OldPosition[i] - Main.screenPosition;
				spriteBatch.Draw(TextureMain, drawPosition2, null, lightColor * 0.075f * (i + 1) * colorMult, OldRotation[i], TextureMain.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0f);
			}
			Vector2 drawPosition = Projectile.Center - Main.screenPosition;

			Player player = Owner;
			for (int i = 0; i < player.armor.Length; i++)
			{
				if (player.armor[i].type == ModContent.ItemType<ShapeshifterShampoo>())
				{
					if (i > 9) i -= 10;
					if (player.dye[i].type != ItemID.None)
					{
						Main.instance.PrepareDrawnEntityDrawing(Projectile, GameShaders.Armor.GetShaderIdFromItemId(player.dye[i].type), null);
					}
					break;
				}
			}

			if ((Projectile.localAI[2] > 0 || Projectile.localAI[1] < Projectile.ai[1]) && Projectile.velocity.Length() < 5f && IsLocalOwner)
			{
				float colorMult2 = (5f - Projectile.velocity.Length()) * 0.2f + (float)Math.Sin(Projectile.localAI[0] * 0.075f) * 0.5f;
				Main.EntitySpriteDraw(TextureGlow, drawPosition, null, Color.White * 0.35f * colorMult * colorMult2, Projectile.rotation, TextureGlow.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0f);
			}

			Main.EntitySpriteDraw(TextureMain, drawPosition, null, lightColor * colorMult, Projectile.rotation, TextureMain.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}