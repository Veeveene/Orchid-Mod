using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common.ModObjects;
using OrchidMod.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Projectiles.Warhammers
{
	public class ThoriumTerrariumWarhammerProjectile : OrchidModGuardianProjectile
	{
		public Color currentColor = new(255, 255, 255, 100);

		public int rColor = 255;
		public bool rColorAllow;
		public int gColor = 128;
		public bool gColorAllow = true;
		public int bColor;
		public bool bColorAllow = true;

		public List<Vector2> OldPosition;
		public List<float> OldRotation;

		public Vector2 OriginalVelocity;

		public override void SafeSetDefaults()
		{
			Projectile.width = 44;
			Projectile.height = 44;
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 240;
			Projectile.scale = 1f;
			Projectile.penetrate = -1;
			Projectile.alpha = 255;
			Projectile.tileCollide = false;
			OldPosition = [];
			OldRotation = [];

			OriginalVelocity = Vector2.Zero;
		}

        public static readonly int[] PotentialDusts =
		[
			DustID.GemRuby,
            DustID.InfernoFork,
            DustID.GemTopaz,
            DustID.GemEmerald,
            DustID.Frost,
            DustID.GemSapphire,
            DustID.GemAmethyst
        ];


		public override void OnSpawn(IEntitySource source)
		{
			OrchidModProjectile.spawnDustCircle(Projectile.position, PotentialDusts[Main.rand.Next(7)], 16, 20, expandingSpeed: 0.4f);
			Projectile.timeLeft -= Main.rand.Next(21);

			OriginalVelocity = Projectile.velocity;
			Projectile.velocity *= float.Epsilon;
		}

		public override void AI()
		{
			Projectile.velocity += OriginalVelocity * 0.02f;
			if (Projectile.velocity.Length() > OriginalVelocity.Length()) Projectile.velocity = OriginalVelocity;

			Projectile.rotation += Projectile.velocity.Length() * 0.1f * Projectile.direction;

			rColor += (rColorAllow ? 10 : -10);
			gColor += (gColorAllow ? 10 : -10);
			bColor += (bColorAllow ? 10 : -10);
			
			if (rColor >= 255) rColorAllow = false;
			else if (rColor <= 0) rColorAllow = true;
			if (gColor >= 255) gColorAllow = false;
			else if (gColor <= 0) gColorAllow = true;
			if (bColor >= 255) bColorAllow = false;
			else if (bColor <= 0) bColorAllow = true;

			currentColor = new Color(rColor, gColor, bColor, 175);

			if ((OldPosition.Count > 10 || Projectile.penetrate == -1) && OldPosition.Count > 0)
			{
				OldPosition.RemoveAt(0);
				OldRotation.RemoveAt(0);
			}
		}

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidGuardian guardian)
		{
            var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				int debuffType = thoriumMod.Find<ModBuff>("TerrariumBacklash").Type;
				target.AddBuff(debuffType, 120);
			}
		}

		public override Color? GetAlpha(Color lightColor) => currentColor;

		public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
		{
			spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
			spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });

			SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Texture2D projTexture = TextureAssets.Projectile[Projectile.type].Value;

			for (int i = 0; i < OldPosition.Count; i++)
			{
				Vector2 drawPosition = OldPosition[i] - Main.screenPosition;

				spriteBatch.Draw(projTexture, drawPosition, null, Projectile.GetAlpha(lightColor * 0.25f) * 0.1f * (i + 1), OldRotation[i], projTexture.Size() * 0.5f, Projectile.scale * (i + 1) * 0.1f, effects, 0f);
			}

            spriteBatch.Draw(projTexture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor * 0.25f), Projectile.rotation, projTexture.Size() * 0.5f, Projectile.scale, effects, 0f);


			// Draw code ends here

			spriteBatch.End();
			spriteBatch.Begin(spriteBatchSnapshot);
			return false;
		}
	}
}