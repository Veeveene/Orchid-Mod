using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Content.Shapeshifter.Accessories;
using OrchidMod.Content.Shapeshifter.Buffs.Debuffs;
using OrchidMod.Utilities;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Shapeshifter.DrawLayers
{
	public class ShapeshifterDrawLayer : PlayerDrawLayer
	{
		// Returning true in this property makes this layer appear on the minimap player head icon.
		public override bool IsHeadLayer => false;

		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
		{
			return drawInfo.drawPlayer.Shapeshifter().IsShapeshifted;
		}

		//public override Position GetDefaultPosition() => new Between(null, PlayerDrawLayers.FirstVanillaLayer);
		public override Position GetDefaultPosition()
		{
 			return new BeforeParent(PlayerDrawLayers.FrozenOrWebbedDebuff);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			Player player = drawInfo.drawPlayer;
			OrchidShapeshifter shapeshifter = player.Shapeshifter();
			ShapeshifterShapeshiftAnchor anchor = shapeshifter.ShapeshiftAnchor;
			Projectile anchorProjectile = anchor.Projectile;
			Color lightColor = Lighting.GetColor((int)(anchorProjectile.Center.X / 16f), (int)(anchorProjectile.Center.Y / 16f));

			if (anchor.SelectedItem < 0) return;
			if (anchor.ShapeshifterItem.ModItem is not OrchidModShapeshifterShapeshift shapeshifterItem || anchor.TextureShapeshift == null) return;

			if (shapeshifterItem.ShapeshiftShouldDraw(anchorProjectile, player, ref lightColor) && drawInfo.shadow == 0f)
			{
				bool drawHairColor = (shapeshifter.ShapeshifterHairpin && !anchor.SwapHairColorTrigger) || (anchor.SwapHairColorTrigger && !shapeshifter.ShapeshifterHairpin);
				Texture2D hairTexture = (anchor.TextureShapeshiftHairGray != null && drawHairColor) ? anchor.TextureShapeshiftHairGray : anchor.TextureShapeshiftHair;
				Color color = shapeshifterItem.GetColor(shapeshifter.ShapeshifterHairpin ? lightColor.MultiplyRGBA(player.hairColor) : lightColor, anchorProjectile, anchor, player, shapeshifter, true);
				Color colorGlow = shapeshifterItem.GetColorGlow(lightColor, anchorProjectile, anchor, player, shapeshifter);
				Color colorLight = shapeshifterItem.GetColor(lightColor, anchorProjectile, anchor, player, shapeshifter);
				Vector2 drawPosition = Vector2.Transform(anchorProjectile.Center - Main.screenPosition + Vector2.UnitY * player.gfxOffY, Main.GameViewMatrix.EffectMatrix).Floor();
				Rectangle drawRectangle = anchor.TextureShapeshift.Bounds;
				drawRectangle.Height = drawRectangle.Width;
				var effect = anchorProjectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

				if (player.HasBuff<ShapeshifterShapeshiftingCooldownDebuff>())
				{
					color = Color.Gray.MultiplyRGB(color);
					colorGlow = Color.Gray.MultiplyRGB(colorGlow);
					colorLight = Color.Gray.MultiplyRGB(colorLight);
				}

				drawRectangle.Y = drawRectangle.Height * anchor.Frame;
				SpriteBatch spriteBatch = Main.spriteBatch;
				shapeshifterItem.ShapeshiftPreDraw(spriteBatch, anchorProjectile, anchor, drawPosition, drawRectangle, effect, player, color); // drawn first, before any drawlayers

				spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
				spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });

				if (anchor.BlinkEffect < 20)
				{ // blink animation
					float scalemult = (float)Math.Sin(anchor.BlinkEffect * 0.157f) * 0.2f + 1f;
					spriteBatch.Draw(anchor.TextureShapeshift, drawPosition, drawRectangle, colorLight * 1.5f, anchorProjectile.rotation, drawRectangle.Size() * 0.5f, anchorProjectile.scale * scalemult, effect, 0f);
					spriteBatch.Draw(hairTexture, drawPosition, drawRectangle, color * 1.5f, anchorProjectile.rotation, drawRectangle.Size() * 0.5f, anchorProjectile.scale * scalemult, effect, 0f);
					//drawInfo.DrawDataCache.Add(new DrawData(anchor.TextureShapeshift, drawPosition, drawRectangle, colorLight * 1.5f, anchorProjectile.rotation, drawRectangle.Size() * 0.5f, anchorProjectile.scale, effect, 0));
					//drawInfo.DrawDataCache.Add(new DrawData(hairTexture, drawPosition, drawRectangle, color * 1.5f, anchorProjectile.rotation, drawRectangle.Size() * 0.5f, anchorProjectile.scale, effect, 0));
				}

				for (int i = 0; i < anchor.OldPosition.Count; i++)
				{
					drawRectangle.Y = drawRectangle.Height * anchor.OldFrame[i];
					Vector2 drawPosition2 = Vector2.Transform(anchor.OldPosition[i] - Main.screenPosition + Vector2.UnitY * player.gfxOffY, Main.GameViewMatrix.EffectMatrix);
					spriteBatch.Draw(anchor.TextureShapeshift, drawPosition2, drawRectangle, colorLight * 0.075f * (i + 1), anchor.OldRotation[i], drawRectangle.Size() * 0.5f, anchorProjectile.scale, effect, 0f);
					spriteBatch.Draw(hairTexture, drawPosition2, drawRectangle, color * 0.075f * (i + 1), anchor.OldRotation[i], drawRectangle.Size() * 0.5f, anchorProjectile.scale, effect, 0f);
					//drawInfo.DrawDataCache.Add(new DrawData(anchor.TextureShapeshift, drawPosition2, drawRectangle, colorLight * 0.075f * (i + 1), anchor.OldRotation[i], drawRectangle.Size() * 0.5f, anchorProjectile.scale, effect, 0));
					//drawInfo.DrawDataCache.Add(new DrawData(hairTexture, drawPosition2, drawRectangle, color * 0.075f * (i + 1), anchor.OldRotation[i], drawRectangle.Size() * 0.5f, anchorProjectile.scale, effect, 0));
				}

				spriteBatch.End();
				spriteBatch.Begin(spriteBatchSnapshot);

				drawRectangle.Y = drawRectangle.Height * anchor.Frame;

				if (shapeshifter.ShapeshifterHairpin || drawHairColor)
				{ // draws hair before the shampoo shaders if the player uses the hairpin
					DrawData drawData2 = new DrawData(hairTexture, drawPosition, drawRectangle, color, anchorProjectile.rotation, drawRectangle.Size() * 0.5f, anchorProjectile.scale, effect, 0);
					if (shapeshifter.ShapeshifterHairpin)
					{ // remove the hair layer shader
						for (int i = 0; i < player.armor.Length; i++)
						{
							if (player.armor[i].type == ModContent.ItemType<ShapeshifterHairpin>())
							{
								if (i > 9) i -= 10;
								if (player.dye[i].type != ItemID.None)
								{
									drawData2.shader = GameShaders.Armor.GetShaderIdFromItemId(player.dye[i].type);
								}
								break;
							}
						}
					}

					drawInfo.DrawDataCache.Add(drawData2);
				}

				int shader = -1;

				for (int i = 0; i < player.armor.Length; i++)
				{
					if (player.armor[i].type == ModContent.ItemType<ShapeshifterShampoo>())
					{
						if (i > 9) i -= 10;
						if (player.dye[i].type != ItemID.None)
						{
							shader = GameShaders.Armor.GetShaderIdFromItemId(player.dye[i].type);
						}
						break;
					}
				}

				if (!drawHairColor && !shapeshifter.ShapeshifterHairpin)
				{ // draws hair after the shader if the player doesn't use the hairpin
					DrawData drawData2 = new DrawData(hairTexture, drawPosition, drawRectangle, color, anchorProjectile.rotation, drawRectangle.Size() * 0.5f, anchorProjectile.scale, effect, 0);
					if (shader != -1) drawData2.shader = shader;
					drawInfo.DrawDataCache.Add(drawData2);
				}

				DrawData drawData = new DrawData(anchor.TextureShapeshift, drawPosition, drawRectangle, colorLight, anchorProjectile.rotation, drawRectangle.Size() * 0.5f, anchorProjectile.scale, effect, 0);
				if (shader != -1) drawData.shader = shader;
				drawInfo.DrawDataCache.Add(drawData);

				if (anchor.TextureShapeshiftGlow != null)
				{
					DrawData drawData2 = new DrawData(anchor.TextureShapeshiftGlow, drawPosition, drawRectangle, colorGlow, anchorProjectile.rotation, drawRectangle.Size() * 0.5f, anchorProjectile.scale, effect, 0);
					if (shader != -1) drawData2.shader = shader;
					drawInfo.DrawDataCache.Add(drawData2);
				}
			}
		}
	}
}
