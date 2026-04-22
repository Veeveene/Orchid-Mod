using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Utilities;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Projectiles.Warhammers
{
	public class ThoriumTerrariumWarhammerProjectile : OrchidModGuardianProjectile
	{
		public List<Vector2> OldPosition;
		public List<float> OldRotation;

		public override void SafeSetDefaults()
		{
			Projectile.width = 44;
			Projectile.height = 44;
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 240;
			Projectile.scale = 1f;
			Projectile.penetrate = 1;
			Projectile.alpha = 255;
			Projectile.tileCollide = false;
			OldPosition = new List<Vector2>();
			OldRotation = new List<float>();
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
			Projectile.timeLeft -= Main.rand.Next(21);
		}

		public override void AI()
		{
			NPC target = Main.npc[(int)Projectile.ai[0]];
            if (target != null && target.active && !target.friendly && !target.dontTakeDamage)
            {   
                Vector2 newVelocity = Vector2.Normalize(target.Center - Projectile.Center) * 0.8f;
                Projectile.velocity = Projectile.velocity * 0.95f + newVelocity;
            }

			Projectile.rotation += Projectile.ai[1] * Projectile.direction;

            if (Main.rand.NextBool(6))
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, PotentialDusts[Main.rand.Next(7)]);


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

		public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
		{
			spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
			spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });

			SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Texture2D projTexture = TextureAssets.Projectile[Projectile.type].Value;

			Color color = Main.DiscoColor;

			for (int i = 0; i < OldPosition.Count; i++)
			{
				Vector2 drawPosition = OldPosition[i] - Main.screenPosition;
				spriteBatch.Draw(projTexture, drawPosition, null, color * 0.1f * (i + 1), OldRotation[i], projTexture.Size() * 0.5f, Projectile.scale * (i + 1) * 0.11f, effects, 0f);
			}

            spriteBatch.Draw(projTexture, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, projTexture.Size() * 0.5f, Projectile.scale, effects, 0f);


			// Draw code ends here

			spriteBatch.End();
			spriteBatch.Begin(spriteBatchSnapshot);
			return false;
		}
	}
}