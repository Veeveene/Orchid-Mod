using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common.UIs;
using OrchidMod.Utilities;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace OrchidMod.Content.Shapeshifter.UI
{
	public class ShapeshifterUIState : OrchidUIState
	{
		public static Texture2D TextureUI;
		public static Texture2D TextureUIBack;
		public static Texture2D TextureUITransformation;
		public static Texture2D TextureUITransformationOff;
		public static Texture2D TextureUITransformationGold;
		public static Texture2D TextureUIDash;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
			=> layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));

		public override void OnInitialize()
		{
			TextureUITransformation ??= ModContent.Request<Texture2D>("OrchidMod/Content/Shapeshifter/UI/Textures/IconTransformation", AssetRequestMode.ImmediateLoad).Value;
			TextureUITransformationGold ??= ModContent.Request<Texture2D>("OrchidMod/Content/Shapeshifter/UI/Textures/IconTransformationGold", AssetRequestMode.ImmediateLoad).Value;
			TextureUITransformationOff ??= ModContent.Request<Texture2D>("OrchidMod/Content/Shapeshifter/UI/Textures/IconTransformationOff", AssetRequestMode.ImmediateLoad).Value;
			TextureUIDash ??= ModContent.Request<Texture2D>("OrchidMod/Content/Shapeshifter/UI/Textures/IconDash", AssetRequestMode.ImmediateLoad).Value;

			Width.Set(0f, 0f);
			Height.Set(0f, 0f);
			Left.Set(Main.screenWidth / 2, 0f);
			Top.Set(Main.screenHeight / 2, 0f);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Recalculate();
			Player player = Main.LocalPlayer;
			OrchidShapeshifter shapeshifter = player.GetModPlayer<OrchidShapeshifter>();

			if (!player.dead)
			{
				if (shapeshifter.ShapeshifterUITransformationTimer > 0)
				{
					Texture2D texture = TextureUITransformation;
					if (shapeshifter.ShapeshifterFastShapeshiftTimer < 300)
					{
						texture = TextureUITransformationOff;
					}
					// TODO : add gold texture fetch here when implemented

					Vector2 drawpos = (player.position + new Vector2(player.width * 0.5f - TextureUITransformation.Width * 0.5f, -(8 - player.gfxOffY + TextureUITransformation.Height))).Floor() - Main.screenPosition;
					float colorMult = (float)Math.Sin(shapeshifter.ShapeshifterUITransformationTimer * 0.05235f); // 1 -> 0 over 30 frames 

					spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

					spriteBatch.Draw(texture, drawpos, null, Color.White * colorMult, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

					spriteBatch.End();
					spriteBatch.Begin(spriteBatchSnapshot);
				}
				else if (shapeshifter.ShapeshifterUIDashTimer > 0)
				{
					Vector2 drawpos = (player.position + new Vector2(player.width * 0.5f - TextureUIDash.Width * 0.5f, -(8 - player.gfxOffY + TextureUIDash.Height))).Floor() - Main.screenPosition;
					float colorMult = (float)Math.Sin(shapeshifter.ShapeshifterUIDashTimer * 0.05235f); // 1 -> 0 over 30 frames 

					spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

					spriteBatch.Draw(TextureUIDash, drawpos, null, Color.White * colorMult, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

					spriteBatch.End();
					spriteBatch.Begin(spriteBatchSnapshot);
				}

				if (TextureUI != null && shapeshifter.IsShapeshifted)
				{
					if (shapeshifter.Shapeshift.ShapeshiftTypeUI != ShapeshifterShapeshiftTypeUI.None)
					{
						Vector2 position = (player.position + new Vector2(player.width * 0.5f, player.height + player.gfxOffY + 12)).Floor();
						Vector2 drawpos = position;
						position -= Main.screenPosition;

						spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
						spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

						int uiCount = 0;
						int uiCountMax = 0;

						shapeshifter.Shapeshift.ShapeshiftGetUIInfo(shapeshifter.ShapeshiftAnchor.Projectile, shapeshifter.ShapeshiftAnchor, player, shapeshifter, ref uiCount, ref uiCountMax);

						if (shapeshifter.Shapeshift.ShapeshiftTypeUI == ShapeshifterShapeshiftTypeUI.List)
						{ // uiCount is the number of "active" symbol out of "uiCountMax"
							int offSet = (int)(uiCountMax / 2f * (TextureUIBack.Width + 2));
							int offSetIcons = offSet;
							for (int i = 0; i < uiCountMax; i++)
							{
								Texture2D texture = uiCount > i ? TextureUI : TextureUIBack;
								drawpos = new Vector2(position.X - offSet + (texture.Width + 2) * i, position.Y);
								spriteBatch.Draw(texture, drawpos, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
							}
						}

						spriteBatch.End();
						spriteBatch.Begin(spriteBatchSnapshot);
					}
				}
			}
		}
	}
}