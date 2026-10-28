using Terraria;
using Terraria.ModLoader;
using System;
using OrchidMod.Common.ModObjects;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;


namespace OrchidMod.Content.General.Projectiles
{
	public class OrchidTitaniumShard : ModProjectile
	{
		public override string Texture => $"Terraria/Images/Projectile_908";

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(908);
			Projectile.aiStyle = -1;
			Projectile.hide = true;
		}

		ref float myIndex => ref Projectile.ai[0];
		ref float ringPos => ref Projectile.localAI[0];
		ref float ringCount => ref Projectile.localAI[1];

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			OrchidPlayer orchidPlayer = player.GetModPlayer<OrchidPlayer>();
			if (!player.active || player.dead || !player.hasTitaniumStormBuff) { Projectile.Kill(); return; }
			if (Projectile.frameCounter == 0) //first frame after creation
			{
				if (!orchidPlayer.TitaniumShards.Contains(Projectile)) orchidPlayer.TitaniumShards.Add(Projectile);
				Projectile.frameCounter = 1;
				Projectile.frame = Main.rand.Next(12);
				Projectile.rotation = Main.rand.NextFloat() * ((float)Math.PI * 2f);
			}
			Projectile.rotation += MathHelper.PiOver2 * 0.01f;
			Projectile.position += player.position - player.oldPosition;
			ringPos = (orchidPlayer.Timer % 50 / 50f + myIndex) * MathHelper.TwoPi;
			ringCount = orchidPlayer.TitaniumShards.Count;
			Projectile.Center = Vector2.Lerp(Projectile.Center, player.Center + new Vector2((float)Math.Sin(ringPos), (float)Math.Cos(ringPos + player.miscCounterNormalized * MathHelper.TwoPi)) * (24 + ringCount * 6), 0.3f);
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			if (Math.Cos(ringPos) > 0 ^ Math.Sin(Main.player[Projectile.owner].miscCounterNormalized * MathHelper.TwoPi) < 0) overPlayers.Add(index);
			else behindProjectiles.Add(index);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			PlayerTitaniumStormBuffTextureContent playerTitaniumStormBuff = TextureAssets.RenderTargets.PlayerTitaniumStormBuff;
			playerTitaniumStormBuff.Request();
			if (playerTitaniumStormBuff.IsReady)
			{
				float scale = 1 + (float)(Math.Cos(ringPos) * Math.Sin(Main.player[Projectile.owner].miscCounterNormalized * MathHelper.TwoPi) * (0.15 + ringCount * 0.015));
				Main.EntitySpriteDraw(playerTitaniumStormBuff.GetTarget(), Projectile.Center - Main.screenPosition + (Main.GlobalTimeWrappedHourly * 8f + (float)Projectile.whoAmI).ToRotationVector2() * 4f, new Rectangle(24 * Projectile.frame, 0, 22, 26), lightColor, Projectile.rotation, new Vector2(11, 13), scale, SpriteEffects.None);
			}
			return false;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (Projectile.Center.X > Main.player[Projectile.owner].Center.X) modifiers.HitDirectionOverride = 1;
			else modifiers.HitDirectionOverride = -1;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}
	}
}
