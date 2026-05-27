using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Content.Guardian.Projectiles.Misc;
using OrchidMod.Utilities;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Projectiles.Shields
{
	public class HorizonShieldProj : OrchidModGuardianProjectile
	{
		private static Texture2D TextureMain;
		public List<Vector2> OldPosition;
		public List<float> OldRotation;
		public int TimeSpent = 0;
		public List<int> HitNPCs;

		public bool Reinforced => Projectile.ai[0] == 1f;

		public override void SafeSetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 120;
			Projectile.scale = 1f;
			Projectile.alpha = 96;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.alpha = 255;
			TextureMain ??= ModContent.Request<Texture2D>(Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			OldPosition = new List<Vector2>();
			OldRotation = new List<float>();
			HitNPCs = new List<int>();
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 60;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(Projectile.timeLeft);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			Projectile.timeLeft = reader.ReadInt32();
		}

		public override void AI()
		{
			TimeSpent++;
			OldPosition.Add(Projectile.Center);
			OldRotation.Add(Projectile.rotation);

			Projectile.rotation = Projectile.velocity.ToRotation();

			if (OldPosition.Count > (Reinforced ? 20 : 15))
			{
				OldPosition.RemoveAt(0);
				OldRotation.RemoveAt(0);
			}

			if (!Reinforced && Projectile.timeLeft > 65)
			{
				Projectile.timeLeft = 65;
			}


			if (TimeSpent > 15)
			{
				NPC closestTarget = null;
				float distanceClosest = 600f;
				foreach (NPC npc in Main.npc)
				{
					float distance = Projectile.Center.Distance(npc.Center);
					if (IsValidTarget(npc) && distance < distanceClosest && !HitNPCs.Contains(npc.whoAmI)) 
					{
						closestTarget = npc;
						distanceClosest = distance;
					}
				}

				if (closestTarget != null && Projectile.timeLeft > 10)
				{
					Vector2 newVelocity = Vector2.Normalize(closestTarget.Center - Projectile.Center) * 0.85f;
					Projectile.velocity = Projectile.velocity * 0.94f + newVelocity;
				}
			}
		}

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidGuardian guardian)
		{
			HitNPCs.Add(target.whoAmI);
			if (Projectile.timeLeft > 10 && !Reinforced)
			{
				Projectile.timeLeft = 10;
				Projectile.velocity *= 0.001f;
				Projectile.netUpdate = true;
			}
		}

		public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
		{
			spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
			spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });

			// Draw code here

			float colorMult = 1f;
			if (Projectile.timeLeft < 10) colorMult *= Projectile.timeLeft / 10f;
			Color color = GuardianHorizonLanceProj.GetColor(Owner, true);
			//Color color = new Color(216, 61, 30);

			for (int i = 0; i < OldPosition.Count; i++)
			{
				if (i > 8 + ( Reinforced ? 4 : 0) && i <= 12 + (Reinforced ? 4 : 0))
				{
					/*
					color.R -= 26;
					color.G += 23;
					color.B += 56;
					*/

					GuardianHorizonLanceProj.DoColorGradient(Owner, ref color, 4);
				}

				Vector2 drawPosition = OldPosition[i] - Main.screenPosition;
				spriteBatch.Draw(TextureMain, drawPosition, null, color * 0.11f * (i + 1) * colorMult, OldRotation[i], TextureMain.Size() * 0.5f, Projectile.scale * (i + 1) * 0.07f, SpriteEffects.None, 0f);
			}

			// Draw code ends here

			spriteBatch.End();
			spriteBatch.Begin(spriteBatchSnapshot);
			return false;
		}
	}
}