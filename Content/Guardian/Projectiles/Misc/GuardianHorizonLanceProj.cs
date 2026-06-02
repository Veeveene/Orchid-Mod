using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Projectiles.Misc
{
	public class GuardianHorizonLanceProj : OrchidModGuardianProjectile
	{
		public int TimeSpent = 0;
		public int HitCount = 0;
		private static Texture2D TextureMain;
		private static Texture2D TextureBlur;
		public List<Vector2> Positions;
		public List<int> HitNPCs;

		public override void SafeSetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 90000;
			Projectile.scale = 1f;
			Projectile.penetrate = 1;
			Projectile.alpha = 255;
			TextureMain ??= ModContent.Request<Texture2D>(Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			TextureBlur ??= ModContent.Request<Texture2D>(Texture + "_Blur", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			Positions = new List<Vector2>();
			HitNPCs = new List<int>();
			Initialized = false;
			Strong = true;
		}

		public override void AI()
		{
			if (Projectile.velocity.Length() > 0)
			{
				Projectile.rotation = Projectile.velocity.ToRotation();
				Projectile.velocity *= 0f;
				Initialized = true;
			}

			if (Initialized)
			{
				TimeSpent++;

				if (TimeSpent <= 30)
				{
					Positions.Add(Projectile.Center + Vector2.UnitY.RotatedBy(Projectile.rotation) * 14f * Positions.Count);
					Positions.Add(Projectile.Center + Vector2.UnitY.RotatedBy(Projectile.rotation) * 14f * Positions.Count);
				}

				if (TimeSpent % 30 == 0 && HitNPCs.Count > 0)
				{
					if (HitCount < 5)
					{
						HitCount++;
					}
					HitNPCs.Clear();
				}
			}

			if (Projectile.ai[1] != 0 && Projectile.timeLeft > 120)
			{ // reduces projectile lifespan if another one is spawned
				Projectile.timeLeft = 30;
				Projectile.netUpdate = true;
			}

			if (IsLocalOwner)
			{
				foreach (NPC npc in Main.npc)
				{
					if (IsValidTarget(npc) && !HitNPCs.Contains(npc.whoAmI))
					{
						foreach (Vector2 pos in Positions)
						{
							if (npc.Hitbox.Contains(new Point((int)pos.X, (int)pos.Y)))
							{
								HitNPCs.Add(npc.whoAmI);
								Owner.ApplyDamageToNPC(npc, (int)(Projectile.damage * (1f - HitCount * 0.1f)), 0f, 1, Main.rand.Next(100) < Projectile.CritChance, ModContent.GetInstance<GuardianDamageClass>());
								break;
							}
						}
					}
				}
			}
		}

		public static Color GetColor(Player player, bool firstColor)
		{
			switch(player.name)
			{
				default:
					return firstColor ? new Color(216, 61, 5) : new Color(112, 161, 255);
				case "Verveine":
					return firstColor ? new Color(100, 150, 0) : new Color(0, 150, 150);
				case "Orchid":
					return firstColor ? new Color(255, 0, 60) : new Color(255, 213, 223);
				case "Haidex":
					return Main.DiscoColor;
				case "CreepZoneTNT":
					return firstColor ? new Color(79, 121, 66) : new Color(253, 238, 0);
				case "IceSpider":
					return firstColor ? new Color(22, 156, 156) : new Color(169, 254, 255);
				case "Xrylene":
					return firstColor ? new Color(247, 67, 85) : new Color(88, 255, 46);
				case "direwolf420":
					return firstColor ? new Color(0, 221, 221) : new Color(44, 36, 133);
				case "Feutor":
					return firstColor ? new Color(255, 106, 0) : new Color(133, 4, 4);
				case "L. Mack":
					return firstColor ? new Color(121, 101, 90) : new Color(102, 50, 19);
				case "Beefeus":
					return new Color(255, 0, 60);
				case "Slime":
					return firstColor ? new Color(0, 255, 255) : new Color(255, 252, 85);
				case "Amber":
					return firstColor ? new Color(200, 130, 100) : new Color(180, 50, 200);
				case "Freya":
					return firstColor ? new Color(255, 0, 0) : new Color(155, 150, 255);
				case "Sonzie":
					return firstColor ? new Color(170, 22, 50) : new Color(20, 122, 0);
				case "Barometz":
					return firstColor ? new Color(250, 120, 177) : new Color(50, 70, 177);
				case "DivermanSam":
					return firstColor ? new Color(0, 150, 150) : new Color(200, 150, 50);
				case "BluNinja":
					return firstColor ? new Color(5, 82, 255) : new Color(255, 0, 0);
				case "Adrian":
					return firstColor ? new Color(200, 0, 200) : new Color(0, 200, 200);
				case "LucidLizard":
					return firstColor ? new Color(255, 200, 0) : new Color(255, 0, 100);
				case "KindP4nda":
					return firstColor ? new Color(150, 0, 17) : new Color(0, 150, 17);
				case "GitGudWO":
					return firstColor ? new Color(150, 150, 150) : new Color(200, 0, 0);
				case "Kibl":
					return firstColor ? new Color(0, 50, 200) : new Color(0, 150, 50);
				case "mcgen":
					return firstColor ? new Color(135, 80, 135) : new Color(135, 135, 80);
				case "Laeyrr":
					return firstColor ? new Color(60, 120, 100) : new Color(110, 70, 200);
				case "Alino4kaHvoshch":
					return new Color(107, 157, 254);
				case "Mio":
					return new Color(237, 76, 76);
				case "Macron":
					return firstColor ? new Color(227, 250, 0) : new Color(127, 0, 255);
				case "Spear McGee":
					return firstColor ? new Color(55, 17, 150) : new Color(255, 167, 50);
				case "Wember":
					return firstColor ? new Color(255, 15, 150) : new Color(0, 175, 200);
				case "Chest0":
					return firstColor ? new Color(50, 85, 255) : new Color(150, 135, 55);
				case "Beebo Greebo":
					return firstColor ? new Color(130, 190, 30) : new Color(230, 40, 80);
				case "KiarArt":
					return firstColor ? new Color(123, 68, 223) : new Color(73, 218, 177);
			}
		}

		public static void DoColorGradient(Player player, ref Color color, int gradientFactor)
		{
			Color firstColor = GetColor(player, true);
			Color secondColor = GetColor(player, false);

			byte unitR = (byte)(Math.Abs(firstColor.R - secondColor.R) / gradientFactor);
			byte unitG = (byte)(Math.Abs(firstColor.G - secondColor.G) / gradientFactor);
			byte unitB = (byte)(Math.Abs(firstColor.B - secondColor.B) / gradientFactor);

			if (firstColor.R < secondColor.R) color.R += unitR;
			else color.R -= unitR;

			if (firstColor.G < secondColor.G) color.G += unitG;
			else color.G -= unitG;

			if (firstColor.B < secondColor.B) color.B += unitB;
			else color.B -= unitB;
		}

		public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
		{
			spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
			//spriteBatch.Begin(spriteBatchSnapshot);
			spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });
			//Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
			//GameShaders.Misc["OrchidMod:HorizonGlow"].Apply();

			if (!Initialized) return false;

			float colorMult = 1f;
			if (Projectile.timeLeft < 20) colorMult *= Projectile.timeLeft / 20f;
			Player owner = Owner;
			Color color = GetColor(owner, true);
			for (int i = 0; i < Positions.Count; i++)
			{
				if (i > 5 && i < 55)
				{
					DoColorGradient(owner, ref color, 50);
				}

				Rectangle rectangle = TextureMain.Bounds;
				Vector2 drawPosition = Positions[i] - Main.screenPosition;
				Color drawcolor = (Color.White * ((float)Math.Sin((TimeSpent + i) * 0.33f) * 0.1f + 0.9f)).MultiplyRGB(color) * colorMult;

				if (i < 9)
				{
					rectangle.Width -= 2 * (9 - i);
					rectangle.X += (9 - i);
					drawPosition += Vector2.UnitX.RotatedBy(Projectile.rotation) * (9 - i);
				}
				else if (i > Positions.Count - 9)
				{
					rectangle.Width += 2 * (Positions.Count - (i + 1) - 8);
					rectangle.X -= (Positions.Count - (i + 1) - 8);
					drawPosition -= Vector2.UnitX.RotatedBy(Projectile.rotation) * (Positions.Count - (i + 1) - 8);
				}

				drawPosition -= Vector2.UnitX.RotatedBy(Projectile.rotation) * (float)Math.Sin((TimeSpent + i * 5) * 0.1) * 2.5f;

				if (i > 10 && i < 50)
				{
					float blurscale = (float)Math.Sin(MathHelper.Pi / 60f * i) * 1.5f;
					spriteBatch.Draw(TextureBlur, drawPosition, null, drawcolor * 0.8f, Projectile.rotation, TextureBlur.Size() * 0.5f, Projectile.scale * blurscale, SpriteEffects.None, 0f);
				}

				spriteBatch.Draw(TextureMain, drawPosition, rectangle, Color.DarkGray.MultiplyRGB(drawcolor), Projectile.rotation, TextureMain.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0f);

				rectangle.Width -= 4;
				rectangle.X -= 2;
				drawPosition += Vector2.UnitX.RotatedBy(Projectile.rotation) * 2;

				spriteBatch.Draw(TextureMain, drawPosition, rectangle, drawcolor, Projectile.rotation, TextureMain.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0f);
			}

			spriteBatch.End();
			spriteBatch.Begin(spriteBatchSnapshot);
			return false;
		}
	}
}