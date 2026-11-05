using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common.Global.Projectiles;
using OrchidMod.Content.General.Projectiles;
using System;
using OrchidMod.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Common.ModObjects
{
	public abstract class OrchidModProjectile : ModProjectile
	{
		public bool projectileTrail = false; // Will the projectile leave a trail of afterimages ?
		public float projectileTrailOffset = 0f; // Offcenters the afterimages a bit. useless without projectileTrail activated. Looks terrible on most projectiles.
		public bool Initialized; // Used in various AI.
		public bool projOwner = false;


		/// <summary>If true, then the index of this projectile's owner matches the local player's index. Used for running code only on the owner's end.</summary>
		public bool IsLocalOwner => Projectile.owner == Main.myPlayer;
		/// <summary>Shorthand for the Player instance this projectile belongs to. Recommended to cache this.</summary>
		public Player Owner => Main.player[Projectile.owner];
		
		/// <summary>Shorthand for the <see cref="Owner"/>'s OrchidPlayer instance. Recommended to cache this.</summary>
		public OrchidPlayer ModPlayer => Main.player[Projectile.owner].OrchidPlayer();
		public static bool IsValidTarget(NPC npc, bool includecritter = false) => npc.active && npc.life > 0 && !npc.dontTakeDamage && !npc.friendly && (includecritter || !npc.CountsAsACritter);

		public sealed override bool PreAI()
		{
			/*if (!spawned)
			{
				spawned = true;
				OnSpawn();
			}*/

			return OrchidPreAI();
		}

		public virtual bool OrchidPreAI() { return true; }

		public virtual void AltSetDefaults() { }

		public virtual void SafeSetDefaults() { }

		public virtual void SafePostAI() { }

		public sealed override void SetDefaults()
		{
			AltSetDefaults();
		}

		public sealed override bool PreDraw(ref Color lightColor)
		{
			if (projectileTrail)
			{
				PreDrawTrail(Main.spriteBatch, lightColor);
			}
			return OrchidPreDraw(Main.spriteBatch, ref lightColor);
		}

		public virtual bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor) { return true; }

		public sealed override void PostAI()
		{
			SafePostAI();
			if (projectileTrail)
			{
				PostAITrail();
			}
		}

		public void Bounce(Vector2 oldVelocity, float speedMult = 1f, bool reducePenetrate = false)
		{
			if (reducePenetrate)
			{
				Projectile.penetrate--;
				if (Projectile.penetrate < 0) Projectile.Kill();
			}
			if (Projectile.velocity.X != oldVelocity.X) Projectile.velocity.X = -oldVelocity.X * speedMult;
			if (Projectile.velocity.Y != oldVelocity.Y) Projectile.velocity.Y = -oldVelocity.Y * speedMult;
		}

		public static void DrawProjectileGlowmask(Projectile projectile, SpriteBatch spriteBatch, Texture2D texture, Color color, float offsetX = 0, float offsetY = 0)
		{
			spriteBatch.Draw(texture, projectile.position - Main.screenPosition + projectile.Size * 0.5f + new Vector2(offsetX, offsetY), null, color, projectile.rotation, texture.Size() * 0.5f, projectile.scale, projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
		}

		// We could maybe update this system, it looks like it has potential?
		public void PreDrawTrail(SpriteBatch spriteBatch, Color lightColor)
		{
			float offSet = projectileTrailOffset + 0.5f;
			Vector2 drawOrigin = new Vector2(TextureAssets.Projectile[Projectile.type].Value.Width * offSet, Projectile.height * offSet);
			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
				Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0.3f);
			}
		}

		public void PostAITrail()
		{
			for (int num46 = Projectile.oldPos.Length - 1; num46 > 0; num46--)
			{
				Projectile.oldPos[num46] = Projectile.oldPos[num46 - 1];
			}
			Projectile.oldPos[0] = Projectile.position;
		}

		public static void SetSpriteBatch(SpriteBatch spriteBatch, SpriteSortMode spriteSortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, Effect effect = null, bool end = true)
		{
			// Use SetSpriteBatch(spriteBatch: spriteBatch); to set vanilla settings

			if (end) spriteBatch.End();
			//spriteBatch.Begin(spriteSortMode, blendState ?? BlendState.AlphaBlend, samplerState ?? Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, effect, Main.GameViewMatrix.TransformationMatrix);
			spriteBatch.Begin(spriteSortMode, blendState ?? BlendState.AlphaBlend, samplerState ?? Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Main.GameViewMatrix.TransformationMatrix);
		}

		public void ResetIFrames() => ResetIFrames(Projectile);

		public static void ResetIFrames(Projectile projectile)
		{
			for (int l = 0; l < Main.npc.Length; l++)
			{
				NPC target = Main.npc[l];
				if (projectile.Hitbox.Intersects(target.Hitbox))
				{
					target.immune[projectile.owner] = 0;
				}
			}
		}

		public static void spawnExplosionGore(Projectile projectile)
		{
			for (int g = 0; g < 2; g++)
			{
				int goreIndex = Gore.NewGore(projectile.GetSource_FromThis(), new Vector2(projectile.position.X + projectile.width / 2 - 24f, projectile.position.Y + projectile.height / 2 - 24f), default, Main.rand.Next(61, 64), 1f);
				Main.gore[goreIndex].scale = 1f + Main.rand.NextFloat();
				Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1.5f;
				Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1.5f;
				Main.gore[goreIndex].rotation = Main.rand.NextFloat();
				goreIndex = Gore.NewGore(projectile.GetSource_FromThis(), new Vector2(projectile.position.X + projectile.width / 2 - 24f, projectile.position.Y + projectile.height / 2 - 24f), default, Main.rand.Next(61, 64), 1f);
				Main.gore[goreIndex].scale = 1f + Main.rand.NextFloat();
				Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1.5f;
				Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1.5f;
				Main.gore[goreIndex].rotation = Main.rand.NextFloat();
				goreIndex = Gore.NewGore(projectile.GetSource_FromThis(), new Vector2(projectile.position.X + projectile.width / 2 - 24f, projectile.position.Y + projectile.height / 2 - 24f), default, Main.rand.Next(61, 64), 1f);
				Main.gore[goreIndex].scale = 1f + Main.rand.NextFloat();
				Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1.5f;
				Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1.5f;
				Main.gore[goreIndex].rotation = Main.rand.NextFloat();
				goreIndex = Gore.NewGore(projectile.GetSource_FromThis(), new Vector2(projectile.position.X + projectile.width / 2 - 24f, projectile.position.Y + projectile.height / 2 - 24f), default, Main.rand.Next(61, 64), 1f);
				Main.gore[goreIndex].scale = 1f + Main.rand.NextFloat();
				Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1.5f;
				Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1.5f;
				Main.gore[goreIndex].rotation = Main.rand.NextFloat();
			}
		}

		public static int spawnGenericExplosion(Projectile projectile, int damage, float kb, int dimensions = 250, int damageType = 0, bool explosionGore = false, bool explosionSound = false)
		{
			if (explosionSound) SoundEngine.PlaySound(SoundID.Item14, projectile.position);
			if (explosionGore) spawnExplosionGore(projectile);
			int projType = ModContent.ProjectileType<GenericExplosion>();
			int newProjectileInt = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center.X, projectile.Center.Y, 0f, 0f, projType, damage, kb, projectile.owner);
			Projectile newProjectile = Main.projectile[newProjectileInt];
			newProjectile.width = dimensions;
			newProjectile.height = dimensions;
			newProjectile.position.X = projectile.Center.X - newProjectile.width / 2;
			newProjectile.position.Y = projectile.Center.Y - newProjectile.width / 2;
			newProjectile.netUpdate = true;

			if (damageType != 0)
			{
				OrchidGlobalProjectile modProjectileNew = newProjectile.GetGlobalProjectile<OrchidGlobalProjectile>();

				if (damageType == 1) modProjectileNew.shamanProjectile = true;
				if (damageType == 2) modProjectileNew.alchemistProjectile = true;
				if (damageType == 3) modProjectileNew.gamblerProjectile = true;
				if (damageType == 4) modProjectileNew.dancerProjectile = true;
			}

			return newProjectileInt;
		}

		public static void spawnDustCircle(Vector2 position, int dustType, double distToCenter, int number, bool noGravity = true, float dustScale = 1f, float velocityMult = 1f, float expandingSpeed = 0f, bool expandingHorizontal = true, bool expandingVertical = true, bool inwards = false, int offsetX = 0, int offsetY = 0, bool randomness = false, bool noLight = false)
		{
			int angle = 360 / number;
			for (int i = 0; i < number; i++)
			{
				double dustDeg = i * angle;
				double dustRad = dustDeg * (Math.PI / 180);

				float posX = position.X - (int)(Math.Cos(dustRad) * distToCenter) - offsetX;
				float posY = position.Y - (int)(Math.Sin(dustRad) * distToCenter) - offsetY;

				Vector2 dustPosition = new Vector2(posX, posY);
				int dust = Dust.NewDust(dustPosition, 1, 1, dustType);
				Dust myDust = Main.dust[dust];

				if (expandingSpeed != 0f)
				{
					float distX = position.X - posX;
					float distY = position.Y - posY;
					float magnitude = (float)Math.Sqrt(distX * distX + distY * distY);
					Vector2 vector = new Vector2(expandingVertical ? distX : 0, expandingHorizontal ? distY : 0);
					vector *= expandingSpeed / magnitude;
					myDust.velocity *= 0f;
					myDust.velocity += vector;
					myDust.velocity *= inwards ? 1f : -1f;
					myDust.velocity = myDust.velocity.RotatedByRandom(randomness ? 10 : 0);
					myDust.noLight = noLight;
				}

				myDust.velocity *= velocityMult;
				myDust.scale = dustScale;
				myDust.noGravity = noGravity;
			}
		}

		public static Vector2 GetNearestTargetPosition(Projectile projectile, float maxDistance = 480f)
		{
			Vector2 validPosition = Vector2.Zero;
			foreach (NPC npc in Main.npc)
			{
				float distance = npc.Center.Distance(projectile.Center);
				if (npc.active && !npc.CountsAsACritter && !npc.friendly && !npc.dontTakeDamage && distance < maxDistance)
				{
					maxDistance = distance;
					validPosition = npc.Center;
				}
			}
			return validPosition;
		}

		public static void setShamanBond(Projectile projectile, int empowermentType)
		{
			OrchidGlobalProjectile modProjectile = projectile.GetGlobalProjectile<OrchidGlobalProjectile>();
			modProjectile.shamanEmpowermentType = empowermentType;
		}

		public static void inheritShamanBond(Projectile projectileReference, Projectile projectile)
		{
			OrchidGlobalProjectile modProjectileReference = projectileReference.GetGlobalProjectile<OrchidGlobalProjectile>();
			OrchidGlobalProjectile modProjectile = projectile.GetGlobalProjectile<OrchidGlobalProjectile>();
			modProjectile.shamanEmpowermentType = modProjectileReference.shamanEmpowermentType;
		}
	}
}
