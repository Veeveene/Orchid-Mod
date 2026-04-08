using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using System;

namespace OrchidMod.Content.Guardian.Projectiles.Gauntlets
{
	public class ThoriumDemonBloodGauntletProjectile : OrchidModGuardianProjectile
	{
		public override string Texture => $"Terraria/Images/Projectile_950";

		public override void SafeSetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 132;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
			Projectile.hide = true;
			Strong = true;
		}

		public override void AI()
		{
			if (Projectile.timeLeft == 120)
			{
				Projectile.friendly = true;
				if (Projectile.ai[0] == 1f)
				{
					Projectile.timeLeft = 20;
				}
				else
				{
					Projectile.velocity = Vector2.UnitY.RotatedBy((Main.MouseWorld - Main.player[Projectile.owner].MountedCenter).ToRotation() - MathHelper.PiOver2) * 8;
					Projectile.extraUpdates = 89;
					Projectile.numUpdates = 89;
				}
			}
			else if (Projectile.timeLeft > 120)
			{
				Projectile.position += Main.player[Projectile.owner].position - Main.player[Projectile.owner].oldPosition;
			}
			else if (Projectile.timeLeft > 20)
			{
				Dust dust = Dust.NewDustPerfect(Projectile.position + new Vector2(5), DustID.TintableDustLighted, Vector2.Zero, Scale:1.2f - Projectile.timeLeft * 0.006f);
				dust.alpha = 255;
				dust.color = new Color(1f, 1f, 1f, 0f);
				dust = Dust.NewDustPerfect((Projectile.position + Projectile.oldPosition) / 2f + new Vector2(5), DustID.TintableDustLighted, Vector2.Zero, Scale:1.2f - Projectile.timeLeft * 0.006f - 0.003f);
				dust.alpha = 255;
				dust.color = new Color(1f, 1f, 1f, 0f);
			}
			if (Projectile.timeLeft == 20)
			{
				Projectile.extraUpdates = 0;
				Projectile.numUpdates = 0;
				Projectile.tileCollide = false;
				SoundEngine.PlaySound(SoundID.NPCDeath56.WithPitchOffset(0.3f), Projectile.Center);
				for (int i = 0; i < 30; i++)
				{
					Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Electric);
					if (i < 24) dust.velocity = dust.velocity * 3f - Projectile.velocity * 0.3f;
					else
					{
						dust.velocity -= Projectile.velocity * 2;
						dust.scale *= 0.8f;
					}
				}
				Projectile.velocity = Vector2.Zero;
			}
		}

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidGuardian guardian)
		{
			if (Projectile.timeLeft > 20)
			{
				Projectile.timeLeft = 21;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.velocity = oldVelocity;
			if (Projectile.timeLeft > 20)
			{
				Projectile.timeLeft = 21;
			}
			return false;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			if (Projectile.timeLeft > 120) overPlayers.Add(index);
			else behindProjectiles.Add(index);
		}

		public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
		{
			if (Projectile.timeLeft > 120)
			{
				int flashTimeLeft = Projectile.timeLeft - 120;
				Main.instance.LoadProjectile(79);
				Main.EntitySpriteDraw(TextureAssets.Projectile[79].Value, Projectile.Center - Main.screenPosition, null, new Color(0.5f, 0.5f, 0.5f, 0f) * (flashTimeLeft * 0.2f), Projectile.rotation, new Vector2(27), flashTimeLeft * 0.5f, SpriteEffects.None);
				Lighting.AddLight(Projectile.Center, new Vector3(flashTimeLeft * 0.5f));
			}
			if (Projectile.timeLeft > 20) return false;
			Main.EntitySpriteDraw(ModContent.Request<Texture2D>(Texture).Value, Projectile.Center - Main.screenPosition, null, new Color(1f, 0.125f, 0.25f, 0f) * (Projectile.timeLeft * 0.1f), 0, new Vector2(64), 2f * (float)Math.Cos(Projectile.timeLeft * 0.075), SpriteEffects.None);
			return false;
		}

		public override void ModifyDamageHitbox(ref Rectangle hitbox)
		{
			if (Projectile.timeLeft <= 20)
			{
				hitbox.X -= 120;
				hitbox.Y -= 120;
				hitbox.Width += 240;
				hitbox.Height += 240;
			}
		}
	}
}