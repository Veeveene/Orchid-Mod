using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common;
using OrchidMod.Common.UIs;
using OrchidMod.Content.Guardian.Projectiles.Misc;
using OrchidMod.Content.Guardian.Weapons.Misc;
using OrchidMod.Utilities;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using JetBrains.Annotations;
using log4net;
using OrchidMod.Content.Guardian.Misc;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace OrchidMod.Content.Guardian.UI
{
	public class GuardianUIState : OrchidUIState
	{
		public static bool NeedTextureReload;

		[CanBeNull]
		public static IGuardianCustomUIItem LastUsedCustomUIItem = null;

		public static Dictionary<Type, Texture2D> TextureCustomOffCache = new();
		public static Dictionary<Type, Texture2D> TextureCustomOnCache = new();
		public static Dictionary<Type, Texture2D> TextureCustomReadyCache = new();

		public static Texture2D textureBlockBarOn;
		public static Texture2D textureBlockBarOff;
		public static Texture2D textureBlockBarOnVoid;
		public static Texture2D textureBlockBarOffVoid;
		public static Texture2D textureBlockReady;

		public static Texture2D textureBlockBarHighlight;
		public static Texture2D textureSlamBarOn;
		public static Texture2D textureSlamBarOff;
		public static Texture2D textureSlamBarHighlight;

		public static Texture2D textureHammerMain;
		public static Texture2D textureHammerIcon;
		public static Texture2D textureHammerIconBig;

		public static Texture2D textureHammerOn;
		public static Texture2D textureHammerOff;
		public static Texture2D textureHammerReady;

		public static Texture2D textureGreathammerOn;
		public static Texture2D textureGreathammerOff;
		public static Texture2D textureGreathammerReady;

		public static Texture2D textureGauntletOn;
		public static Texture2D textureGauntletOff;
		public static Texture2D textureGauntletReady;

		public static Texture2D textureStandardOn;
		public static Texture2D textureStandardOff;
		public static Texture2D textureStandardReady;

		public static Texture2D textureRuneOn;
		public static Texture2D textureRuneOff;
		public static Texture2D textureRuneReady;

		public static Texture2D textureQuarterstaffOn;
		public static Texture2D textureQuarterstaffOff;
		public static Texture2D textureQuarterstaffReady;

		public static Texture2D textureKatarOn;
		public static Texture2D textureKatarOff;
		public static Texture2D textureKatarReady;

		public static Texture2D textureHorizonLanceOn;
		public static Texture2D textureHorizonLanceOff;
		public static Texture2D textureHorizonLanceReady;

		public static Texture2D textureGuardianNeedleOn;
		public static Texture2D textureGuardianNeedleOff;
		public static Texture2D textureGuardianNeedleReady;

		public static Texture2D textureCustomOn;
		public static Texture2D textureCustomOff;
		public static Texture2D textureCustomReady;
		public static Texture2D textureFallbackOn;
		public static Texture2D textureFallbackOff;
		public static Texture2D textureFallbackReady;

		public static Texture2D textureBlockOn;
		public static Texture2D textureBlockOff;

		public static Texture2D textureIconStandardOn;
		public static Texture2D textureIconStandardOff;
		public static Texture2D textureIconStandardHighlight;
		public static Texture2D textureIconRuneOn;
		public static Texture2D textureIconRuneOff;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
			=> layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
		public override void OnInitialize()
		{
			TextureCustomReadyCache = new();
			TextureCustomOffCache = new();
			TextureCustomOnCache = new();

			ReloadTextures();

			Width.Set(0f, 0f);
			Height.Set(0f, 0f);
			Left.Set(Main.screenWidth / 2, 0f);
			Top.Set(Main.screenHeight / 2, 0f);
		}

		public static void ReloadTextures()
		{
			string path = "OrchidMod/Content/Guardian/UI/Textures/";
			textureHammerMain = ModContent.Request<Texture2D>(path + "HammerBar", AssetRequestMode.ImmediateLoad).Value;
			textureHammerIcon = ModContent.Request<Texture2D>(path + "HammerIcon1", AssetRequestMode.ImmediateLoad).Value;
			textureHammerIconBig = ModContent.Request<Texture2D>(path + "HammerIcon2", AssetRequestMode.ImmediateLoad).Value;
			
			textureBlockBarOnVoid = ModContent.Request<Texture2D>(path + "BlockBarOnVoid", AssetRequestMode.ImmediateLoad).Value;
			textureBlockBarOffVoid = ModContent.Request<Texture2D>(path + "BlockBarOffVoid", AssetRequestMode.ImmediateLoad).Value;

			// Chance textures file path if the fancy config option is enabled
			bool fancy = ModContent.GetInstance<OrchidClientConfig>().GuardianUseFancyUI;
			if (fancy) path += "Fancy/";

			textureBlockBarOn = ModContent.Request<Texture2D>(path + "BlockBarOn", AssetRequestMode.ImmediateLoad).Value;
			textureBlockBarOff = ModContent.Request<Texture2D>(path + "BlockBarOff", AssetRequestMode.ImmediateLoad).Value;
			textureBlockBarHighlight = ModContent.Request<Texture2D>(path + "BlockBarHighlight", AssetRequestMode.ImmediateLoad).Value;

			textureSlamBarOn = ModContent.Request<Texture2D>(path + "SlamBarOn", AssetRequestMode.ImmediateLoad).Value;
			textureSlamBarOff = ModContent.Request<Texture2D>(path + "SlamBarOff", AssetRequestMode.ImmediateLoad).Value;
			textureSlamBarHighlight = ModContent.Request<Texture2D>(path + "SlamBarHighlight", AssetRequestMode.ImmediateLoad).Value;

			textureGauntletOn = ModContent.Request<Texture2D>(path + "GauntletOn", AssetRequestMode.ImmediateLoad).Value;
			textureGauntletOff = ModContent.Request<Texture2D>(path + "GauntletOff", AssetRequestMode.ImmediateLoad).Value;
			textureGauntletReady = ModContent.Request<Texture2D>(path + "GauntletReady", AssetRequestMode.ImmediateLoad).Value;

			textureGauntletOn = ModContent.Request<Texture2D>(path + "GauntletOn", AssetRequestMode.ImmediateLoad).Value;
			textureGauntletOff = ModContent.Request<Texture2D>(path + "GauntletOff", AssetRequestMode.ImmediateLoad).Value;
			textureGauntletReady = ModContent.Request<Texture2D>(path + "GauntletReady", AssetRequestMode.ImmediateLoad).Value;

			textureStandardOn = ModContent.Request<Texture2D>(path + "StandardOn", AssetRequestMode.ImmediateLoad).Value;
			textureStandardOff = ModContent.Request<Texture2D>(path + "StandardOff", AssetRequestMode.ImmediateLoad).Value;
			textureStandardReady = ModContent.Request<Texture2D>(path + "StandardReady", AssetRequestMode.ImmediateLoad).Value;

			textureRuneOn = ModContent.Request<Texture2D>(path + "RuneOn", AssetRequestMode.ImmediateLoad).Value;
			textureRuneOff = ModContent.Request<Texture2D>(path + "RuneOff", AssetRequestMode.ImmediateLoad).Value;
			textureRuneReady = ModContent.Request<Texture2D>(path + "RuneReady", AssetRequestMode.ImmediateLoad).Value;

			textureQuarterstaffOn = ModContent.Request<Texture2D>(path + "QuarterstaffOn", AssetRequestMode.ImmediateLoad).Value;
			textureQuarterstaffOff = ModContent.Request<Texture2D>(path + "QuarterstaffOff", AssetRequestMode.ImmediateLoad).Value;
			textureQuarterstaffReady = ModContent.Request<Texture2D>(path + "QuarterstaffReady", AssetRequestMode.ImmediateLoad).Value;

			textureKatarOn = ModContent.Request<Texture2D>(path + "KatarOn", AssetRequestMode.ImmediateLoad).Value;
			textureKatarOff = ModContent.Request<Texture2D>(path + "KatarOff", AssetRequestMode.ImmediateLoad).Value;
			textureKatarReady = ModContent.Request<Texture2D>(path + "KatarReady", AssetRequestMode.ImmediateLoad).Value;

			textureHammerOn = ModContent.Request<Texture2D>(path + "HammerOn", AssetRequestMode.ImmediateLoad).Value;
			textureHammerOff = ModContent.Request<Texture2D>(path + "HammerOff", AssetRequestMode.ImmediateLoad).Value;
			textureHammerReady = ModContent.Request<Texture2D>(path + "HammerReady", AssetRequestMode.ImmediateLoad).Value;

			textureGreathammerOn = ModContent.Request<Texture2D>(path + "GreathammerOn", AssetRequestMode.ImmediateLoad).Value;
			textureGreathammerOff = ModContent.Request<Texture2D>(path + "GreathammerOff", AssetRequestMode.ImmediateLoad).Value;
			textureGreathammerReady = ModContent.Request<Texture2D>(path + "GreathammerReady", AssetRequestMode.ImmediateLoad).Value;

			textureHorizonLanceOn = ModContent.Request<Texture2D>(path + "HorizonLanceOn", AssetRequestMode.ImmediateLoad).Value;
			textureHorizonLanceOff = ModContent.Request<Texture2D>(path + "HorizonLanceOff", AssetRequestMode.ImmediateLoad).Value;
			textureHorizonLanceReady = ModContent.Request<Texture2D>(path + "HorizonLanceReady", AssetRequestMode.ImmediateLoad).Value;

			textureGuardianNeedleOn = ModContent.Request<Texture2D>(path + "GuardianNeedleOn", AssetRequestMode.ImmediateLoad).Value;
			textureGuardianNeedleOff = ModContent.Request<Texture2D>(path + "GuardianNeedleOff", AssetRequestMode.ImmediateLoad).Value;
			textureGuardianNeedleReady = ModContent.Request<Texture2D>(path + "GuardianNeedleReady", AssetRequestMode.ImmediateLoad).Value;

			textureBlockOn = ModContent.Request<Texture2D>(path + "BlockOn", AssetRequestMode.ImmediateLoad).Value;
			textureBlockOff = ModContent.Request<Texture2D>(path + "BlockOff", AssetRequestMode.ImmediateLoad).Value;
			textureBlockReady = ModContent.Request<Texture2D>(path + "BlockReady", AssetRequestMode.ImmediateLoad).Value;

			textureIconStandardOn = ModContent.Request<Texture2D>(path + "IconStandardOn", AssetRequestMode.ImmediateLoad).Value;
			textureIconStandardOff = ModContent.Request<Texture2D>(path + "IconStandardOff", AssetRequestMode.ImmediateLoad).Value;
			textureIconStandardHighlight = ModContent.Request<Texture2D>(path + "IconStandardHighlight", AssetRequestMode.ImmediateLoad).Value;
			textureIconRuneOn = ModContent.Request<Texture2D>(path + "IconRuneOn", AssetRequestMode.ImmediateLoad).Value;
			textureIconRuneOff = ModContent.Request<Texture2D>(path + "IconRuneOff", AssetRequestMode.ImmediateLoad).Value;
			
			textureFallbackOn = ModContent.Request<Texture2D>(path + "FallbackOn", AssetRequestMode.ImmediateLoad).Value;
			textureFallbackOn.Name = "FallbackOn";
			textureFallbackOff = ModContent.Request<Texture2D>(path + "FallbackOff", AssetRequestMode.ImmediateLoad).Value;
			textureFallbackOff.Name = "FallbackOff";
			textureFallbackReady = ModContent.Request<Texture2D>(path + "FallbackReady", AssetRequestMode.ImmediateLoad).Value;
			textureFallbackReady.Name = "FallbackReady";

			TextureCustomOnCache.Clear();
			TextureCustomOffCache.Clear();
			TextureCustomReadyCache.Clear();
			LastUsedCustomUIItem = null;
			
			ILog logger = OrchidMod.Instance.Logger;
			foreach (var modItem in ModContent.GetContent<ModItem>())
			{
				if (modItem is OrchidModGuardianItem and IGuardianCustomUIItem customUIItem)
				{
					string typeName = modItem.Name;
					string textureName = customUIItem.TextureName;
					string myPath = path + textureName;

					if (textureName is null)
					{
						logger.DebugFormat("IGuardianCustomUIItem \"{0}\" has its \"TextureName\" property set to null; skipping", myPath);
						continue;
					}
					
					logger.DebugFormat("Considering path {0}", myPath);
					
					Texture2D textureOn;
					Texture2D textureOff;
					Texture2D textureReady;
				
					if (ModContent.RequestIfExists(customUIItem.TextureOnPath(fancy) ?? myPath + "On", out Asset<Texture2D> textOn, AssetRequestMode.ImmediateLoad))
					{
						textureOn = textOn.Value;
						textureOn.Name = textureName + "On";
						logger.InfoFormat("Found \"On\" texture for {0} at {1}", typeName, myPath + "On");
					}
					else
					{
						textureOn = textureFallbackOn;
						textureOn.Name = "FallbackOn";
						logger.WarnFormat("{0} has no \"On\" texture defined at {1}, using fallback", typeName, myPath + "On");
					}
					
					if (ModContent.RequestIfExists(customUIItem.TextureOffPath(fancy) ?? myPath + "Off", out Asset<Texture2D> textOff, AssetRequestMode.ImmediateLoad))
					{
						textureOff = textOff.Value;
						textureOff.Name = textureName + "Off";
						logger.InfoFormat("Found \"Off\" texture for {0} at {1}", typeName, myPath + "Off");
					}
					else
					{
						textureOff = textureFallbackOff;
						textureOff.Name = "FallbackOff";
						logger.WarnFormat("{0} has no \"Off\" texture defined at {1}, using fallback", typeName, myPath + "Off");
					}
						
					if (ModContent.RequestIfExists(customUIItem.TextureReadyPath(fancy) ?? myPath + "Ready", out Asset<Texture2D> textReady, AssetRequestMode.ImmediateLoad))
					{
						textureReady = textReady.Value;
						textureReady.Name = textureName + "Ready";
						logger.InfoFormat("Found \"Ready\" texture for {0} at {1}", typeName, myPath + "Ready");
					}
					else
					{
						textureReady = textureFallbackReady;
						textureReady.Name = "FallbackReady";
						logger.WarnFormat("{0} has no \"Ready\" texture defined at {1}, using fallback", typeName, myPath + "Ready");
					}
					
					logger.DebugFormat("Cached textures {0}, {1}, and {2} for IGuardianCustomUIItem \"{3}\"", textureOn.Name, textureOff.Name, textureReady.Name, typeName);
					
					TextureCustomOnCache.Add(customUIItem.GetType(), textureOn);
					TextureCustomOffCache.Add(customUIItem.GetType(), textureOff);
					TextureCustomReadyCache.Add(customUIItem.GetType(), textureReady);
				}
			}
			textureCustomOn = textureFallbackOn;
			textureCustomOff = textureFallbackOff;
			textureCustomReady = textureFallbackReady;
			logger.DebugFormat("Current entries for TextureCustomOnCache: {0}", TextureCustomOnCache.Values.Humanize());
			logger.DebugFormat("Current entries for TextureCustomOffCache: {0}", TextureCustomOffCache.Values.Humanize());
			logger.DebugFormat("Current entries for TextureCustomReadyCache: {0}", TextureCustomReadyCache.Values.Humanize());
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Recalculate();
			Player player = Main.LocalPlayer;
			OrchidGuardian modPlayer = player.GetModPlayer<OrchidGuardian>();

			if (!player.dead && modPlayer.GuardianDisplayUI > 0)
			{
				spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

				bool minHoldTimer = modPlayer.ChargeHoldTimer > ModContent.GetInstance<OrchidClientConfig>().GuardianMinHoldTimer;
				bool maxHoldTimer = modPlayer.ChargeHoldTimer > ModContent.GetInstance<OrchidClientConfig>().GuardianMaxHoldTimer;

				bool drawAtCursor = ModContent.GetInstance<OrchidClientConfig>().GuardianChargeCursor;
				Vector2 position = (player.position + new Vector2(player.width * 0.5f, player.height + player.gfxOffY + 12)).Floor();
				if (player.gravDir < 0) position.Y -= 81;
				Vector2 drawpos = position;
				position -= Main.screenPosition;
				SpriteEffects effect = player.gravDir > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

				Texture2D chargeTextureOn = null;
				Texture2D chargeTextureOff = null;
				Texture2D chargeTextureReady = null;

				int offSet = (int)(modPlayer.GuardianGuardMax / 2f * (textureBlockBarOn.Width + 2));
				int offSetIcons = offSet;
				for (int i = 0; i < modPlayer.GuardianGuardMax; i++)
				{
					bool check = modPlayer.GuardianGuard > i;
					bool voidCheck = modPlayer.GuardianVoid;
					
					Texture2D textureOff = textureBlockBarOff;
					Texture2D textureOn = textureBlockBarOn;
					if (voidCheck)
					{
						textureOff = textureBlockBarOffVoid;
						textureOn = textureBlockBarOnVoid;
					}
					
					Texture2D texture = check ? textureOn : textureOff;
					
					drawpos = new Vector2(position.X - offSet + (textureOn.Width + 2) * i, position.Y);
					Color bgColor = Color.White;
					Color fgColor = bgColor;
					Vector2 voidShake = Main.rand.NextVector2Unit() * Utils.Remap(modPlayer.GuardianGuard + modPlayer.GuardianGuardRecharging, 0, modPlayer.GuardianGuardMax / 2f, 1.5f, 0f);
					if (voidCheck)
					{
						if (Main.rand.NextBool((int)Utils.Remap(modPlayer.GuardianGuard + modPlayer.GuardianGuardRecharging, 0, modPlayer.GuardianGuardMax / 2f, 1, 20))) drawpos += voidShake;
						bgColor = modPlayer.GuardianGuard < 2 ? Color.Lerp(new Color(57, 0, 86), modPlayer.GuardianGuard < 1 ? new Color(255, 58, 228) : new(120, 0, 181), Utils.Turn01ToCyclic010(player.miscCounterNormalized * (modPlayer.GuardianGuard < 1 ? player.statLife / (float)player.statLifeMax2 < 0.8f ? 6 : 4 : 2))) : new (57, 0, 86);
					}
					
					spriteBatch.Draw(texture, drawpos, null, check ? fgColor : bgColor, 0f, Vector2.Zero, 1f, effect, 0f);
					
					Vector2 drawposHighlight = new Vector2(position.X - offSet - 2 + 18 * i, position.Y - 2);
					if (voidCheck) drawposHighlight += voidShake;
					if (i < modPlayer.MissingGuardsUI)
					{
						float flash = 0.5f + (float)Math.Cos(modPlayer.MissingResourcesTimer * 0.28) * 0.5f;
						spriteBatch.Draw(textureBlockBarHighlight, drawposHighlight, null, (check ? new Color(255, 92, 92) * 0.8f : new Color(169, 60, 60)) * (1 - flash), 0f, Vector2.Zero, 1f, effect, 0f);
						spriteBatch.Draw(texture, drawpos, null, check ? fgColor : (voidCheck ? new Color(255, 58, 228) : Color.White).Multiply(new Color(1f, flash, flash)), 0f, Vector2.Zero, 1f, effect, 0f);
					}
					else spriteBatch.Draw(texture, drawpos, null, check ? fgColor : bgColor, 0f, Vector2.Zero, 1f, effect, 0f);
					
					if (modPlayer.GuardCostUI > i && modPlayer.MissingGuardsUI <= i)
					{
						spriteBatch.Draw(textureBlockBarHighlight, drawposHighlight, null, (check ? Color.White * 0.8f : Color.DarkGray) * 0.8f, 0f, Vector2.Zero, 1f, effect, 0f);
					}
					if (modPlayer.GuardianGuard - 1 == i && modPlayer.GuardianGuardRecharging < 0)
					{
						int chargingWidth = (int)(-modPlayer.GuardianGuardRecharging * textureOn.Width + 0.5f);
						float flash = 0.6f + (float)Math.Sin(player.miscCounterNormalized * 2 * MathHelper.TwoPi) * 0.2f;
						spriteBatch.Draw(textureOff, drawpos + Vector2.UnitX * (textureOn.Width - chargingWidth), new Rectangle(textureOn.Width - chargingWidth, 0, chargingWidth, textureOn.Height),
							/*new Color(flash, flash, flash, 0.6f));*/ (bgColor * flash) with { A = 163 }
						);
					}
					else if (modPlayer.GuardianGuard == i && modPlayer.GuardianGuardRecharging > 0)
					{
						float flash = 0.5f + (float)Math.Sin((player.miscCounterNormalized + modPlayer.GuardianGuardRecharging * 1.5f) * 2 * MathHelper.TwoPi) * 0.1f;
						
						spriteBatch.Draw(textureOn, drawpos, new Rectangle(0, 0, (int)(modPlayer.GuardianGuardRecharging * textureOn.Width + 0.5f), textureBlockBarOn.Height), new Color(flash, flash, flash, 0.5f));
					}
				}

				offSet = (int)(modPlayer.GuardianSlamMax / 2f * (textureSlamBarOn.Width + 2));
				if (offSet > offSetIcons) offSetIcons = offSet; // Calculates the offset for banner and rune icon based on the largest offset between block (guard) and slam stacks
				for (int i = 0; i < modPlayer.GuardianSlamMax; i++)
				{
					bool check = modPlayer.GuardianSlam > i;
					Texture2D texture = check ? textureSlamBarOn : textureSlamBarOff;
					drawpos = new Vector2(position.X - offSet + 18 * i, position.Y + 18 * player.gravDir);
					Vector2 drawposHighlight = new Vector2(position.X - offSet - 2 + 18 * i, position.Y + 16 * player.gravDir + 2f * (player.gravDir - 1));
					if (i < modPlayer.MissingSlamsUI)
					{
						float flash = 0.5f + (float)Math.Cos(modPlayer.MissingResourcesTimer * 0.28) * 0.5f;
						spriteBatch.Draw(textureSlamBarHighlight, drawposHighlight, null, (check ? new Color(255, 92, 92) * 0.8f : new Color(169, 60, 60)) * (1 - flash), 0f, Vector2.Zero, 1f, effect, 0f);
						spriteBatch.Draw(texture, drawpos, null, check ? Color.White : new Color(1f, flash, flash), 0f, Vector2.Zero, 1f, effect, 0f);
					}
					else spriteBatch.Draw(texture, drawpos, null, Color.White, 0f, Vector2.Zero, 1f, effect, 0f);
					if (modPlayer.SlamCostUI > i && modPlayer.MissingSlamsUI <= i)
					{
						spriteBatch.Draw(textureSlamBarHighlight, drawposHighlight, null, (check ? Color.White * 0.8f : Color.DarkGray) * 0.8f, 0f, Vector2.Zero, 1f, effect, 0f);
					}
					if (modPlayer.GuardianSlam - 1 == i && modPlayer.GuardianSlamRecharging < 0)
					{
						int chargingWidth = (int)(-modPlayer.GuardianSlamRecharging * textureSlamBarOn.Width + 0.5f);
						float flash = 0.6f + (float)Math.Sin(player.miscCounterNormalized * 2 * MathHelper.TwoPi) * 0.2f;
						spriteBatch.Draw(textureSlamBarOff, drawpos + Vector2.UnitX * (textureSlamBarOn.Width - chargingWidth), new Rectangle(textureSlamBarOn.Width - chargingWidth, 0, chargingWidth, textureSlamBarOn.Height), new Color(flash, flash, flash, 0.6f));
					}
					else if (modPlayer.GuardianSlam == i && modPlayer.GuardianSlamRecharging > 0)
					{
						float flash = 0.5f + (float)Math.Sin((player.miscCounterNormalized + modPlayer.GuardianSlamRecharging * 1.5f) * 2 * MathHelper.TwoPi) * 0.1f;
						spriteBatch.Draw(textureSlamBarOn, drawpos, new Rectangle(0, 0, (int)(modPlayer.GuardianSlamRecharging * textureSlamBarOn.Width + 0.5f), textureSlamBarOn.Height), new Color(flash, flash, flash, 0.5f));
					}
					if (modPlayer.GuardianCounter && modPlayer.GuardianCounterTime > 0 && ((modPlayer.GuardianSlam <= 1 && i == 0) || modPlayer.GuardianSlam - 1 == i))
					{
						float flash = 0.4f + (float)Math.Sin(Main.timeForVisualEffects * 0.1f) * 0.2f;
						Color color = !modPlayer.GuardianCounterHorizon
							? new Color(flash, 0, 0, 1f - flash)
							: GuardianHorizonLanceAnchor.GetHorizonGlowColor(Math.Sin(Main.timeForVisualEffects * 0.05f), 0.4f + (float)Math.Cos(Main.timeForVisualEffects * 0.1f) * 0.1f, 0.8f - flash);
						int chargeHeight = (int)(Math.Min(modPlayer.GuardianCounterTime / 30f, 1f) * textureSlamBarHighlight.Height);
						spriteBatch.Draw(textureSlamBarHighlight, drawposHighlight + new Vector2(0, textureSlamBarHighlight.Height - chargeHeight), new Rectangle(0, textureSlamBarHighlight.Height - chargeHeight, textureSlamBarHighlight.Width, chargeHeight), color);
					}
				}

				offSet = (int)((textureIconRuneOn.Width + 2) * 0.5f);
				if (modPlayer.RuneProjectiles.Count > 0)
				{
					float colorMult = (modPlayer.RuneProjectiles[0].timeLeft > 275 || player.HeldItem.ModItem is OrchidModGuardianStandard) ? 1f : (float)Math.Abs(Math.Sin((modPlayer.RuneProjectiles[0].timeLeft * 0.5f) / Math.PI / 4f));
					offSet = (int)(offSet * (modPlayer.GuardianCurrentStandardAnchor == null ? 1f : 2f));
					drawpos = new Vector2(position.X - offSet, position.Y + 36 * player.gravDir + (player.gravDir - 1));
					spriteBatch.Draw(textureIconRuneOff, drawpos, null, Color.White * colorMult, 0f, Vector2.Zero, 1f, effect, 0f);
					spriteBatch.Draw(textureIconRuneOn, drawpos, null, Color.White * colorMult, 0f, Vector2.Zero, 1f, effect, 0f);
					offSet -= textureIconRuneOn.Width + 2;
				}

				if (modPlayer.GuardianCurrentStandardAnchor != null && modPlayer.GuardianCurrentStandardAnchor.ModProjectile is GuardianStandardAnchor anchor && anchor.StandardItem.ModItem is OrchidModGuardianStandard standardItem && anchor.BuffItem != null)
				{
					float standardDuration = modPlayer.GuardianCurrentStandardAnchor.ai[1];
					float standardDurationHighlight = 0f;
					if (standardDuration > standardItem.StandardDuration * modPlayer.GuardianStandardTimer)
					{
						standardDurationHighlight = standardDuration - standardItem.StandardDuration * modPlayer.GuardianStandardTimer;
						standardDuration = standardItem.StandardDuration * modPlayer.GuardianStandardTimer;

						if (standardDurationHighlight > standardItem.StandardDuration * modPlayer.GuardianStandardTimer)
						{ // can happen if modPlayer.GuardianStandardTimer lowers while a standard is active
							standardDurationHighlight = standardItem.StandardDuration * modPlayer.GuardianStandardTimer;
						}
					}

					float colorMult = standardDuration > 275 ? 1f : (float)Math.Abs(Math.Sin((standardDuration * 0.5f) / Math.PI / 4f));
					int remainingDurationOffset = (int)(textureIconStandardOn.Height * (1 - standardDuration / (standardItem.StandardDuration * modPlayer.GuardianStandardTimer)));
					drawpos = new Vector2(position.X - offSet, position.Y + 36 * player.gravDir + (player.gravDir - 1));

					spriteBatch.Draw(textureIconStandardOff, drawpos, null, Color.White * colorMult, 0f, Vector2.Zero, 1f, effect, 0f);
					Rectangle drawRectangle = new Rectangle(0, remainingDurationOffset, textureIconStandardOn.Width, textureIconStandardOn.Height - remainingDurationOffset);
					spriteBatch.Draw(textureIconStandardOn, drawpos + new Vector2(0, remainingDurationOffset), drawRectangle, Color.White * colorMult, 0f, Vector2.Zero, 1f, effect, 0f);

					if (standardDurationHighlight > 0)
					{ // draws a border if duration is >100% of the standard duration
						remainingDurationOffset = (int)(textureIconStandardHighlight.Height * (1 - standardDurationHighlight / (standardItem.StandardDuration * modPlayer.GuardianStandardTimer)));
						drawRectangle = new Rectangle(0, remainingDurationOffset, textureIconStandardHighlight.Width, textureIconStandardHighlight.Height - remainingDurationOffset);
						spriteBatch.Draw(textureIconStandardHighlight, drawpos + new Vector2(-2, remainingDurationOffset - 2), drawRectangle, Color.White * colorMult * 0.8f, 0f, Vector2.Zero, 1f, effect, 0f);
					}
				}

				if (player.HeldItem.ModItem is OrchidModGuardianItem guardianItem)
				{
					if (guardianItem is IGuardianCustomUIItem customUIItem)
					{ 
						if (LastUsedCustomUIItem == null || customUIItem.GetType() != LastUsedCustomUIItem.GetType())
						{
							Mod.Logger.DebugFormat("Equipped IGuardianCustomUIItem \"{0}\" of type {1}, trying to load custom textures from cache", guardianItem.Name, guardianItem.GetType());
							LastUsedCustomUIItem = customUIItem;
							textureCustomOff = TextureCustomOffCache.GetValueOrDefault(guardianItem.GetType(), textureFallbackOff);
							textureCustomOn = TextureCustomOnCache.GetValueOrDefault(guardianItem.GetType(), textureFallbackOn);
							textureCustomReady = TextureCustomReadyCache.GetValueOrDefault(guardianItem.GetType(), textureFallbackReady);

							Mod.Logger.DebugFormat("Loaded textures {0}, {1}, and {2} for IGuardianCustomUIItem \"{3}\"", textureCustomOff.Name, textureCustomOn.Name, textureCustomReady.Name, guardianItem.Name);
						}
					}
					else if (LastUsedCustomUIItem != null)
					{
						LastUsedCustomUIItem = null;
						textureCustomOff = textureFallbackOff;
						textureCustomOn = textureFallbackOn;
						textureCustomReady = textureFallbackReady;
						Mod.Logger.Debug("Unloaded custom textures to fallback values");
					}
					
					
					if (guardianItem is IGuardianCustomUIItem miscItem && (maxHoldTimer || (minHoldTimer && modPlayer.GuardianItemCharge > miscItem.ChargeThreshold(player))))
					{
						chargeTextureOn = textureCustomOn;
						chargeTextureOff = textureCustomOff;
						chargeTextureReady = textureCustomReady;
						
						goto IconDrawing;
					}
					
					if ((maxHoldTimer || (minHoldTimer && modPlayer.GuardianItemCharge > (70 * player.GetTotalAttackSpeed(DamageClass.Melee) - player.HeldItem.useTime) / 2.5f)) && guardianItem is OrchidModGuardianHammer)
					{
						if (ModContent.GetInstance<OrchidClientConfig>().GuardianUseOldHammerUi)
						{
							Vector2 hammerPosition = new Vector2(position.X - textureHammerMain.Width / 2, position.Y - 100 * player.gravDir);
							spriteBatch.Draw(textureHammerMain, hammerPosition, null, Color.White, 0f, Vector2.Zero, 1f, effect, 0f);

							int throwCharge = modPlayer.ThrowLevel();
							if (throwCharge > 0)
							{
								drawpos = hammerPosition + new Vector2(4, 2 * player.gravDir);
								Color color = new Color(87, 220, 0);
								spriteBatch.Draw(textureHammerIcon, drawpos, null, color, 0f, Vector2.Zero, 1f, effect, 0f);
							}

							if (throwCharge > 1)
							{
								drawpos = hammerPosition + new Vector2(16, 2 * player.gravDir);
								Color color = new Color(255, 223, 0);
								spriteBatch.Draw(textureHammerIcon, drawpos, null, color, 0f, Vector2.Zero, 1f, effect, 0f);
							}

							if (throwCharge > 2)
							{
								drawpos = hammerPosition + new Vector2(28, 2 * player.gravDir);
								Color color = new Color(255, 150, 0);
								spriteBatch.Draw(textureHammerIcon, drawpos, null, color, 0f, Vector2.Zero, 1f, effect, 0f);
							}

							if (throwCharge > 3)
							{
								drawpos = hammerPosition + new Vector2(40, 0);
								Color color = new Color(255, 27, 0);
								spriteBatch.Draw(textureHammerIconBig, drawpos, null, color, 0f, Vector2.Zero, 1f, effect, 0f);
							}
						}
						else
						{
							chargeTextureOff = textureHammerOff;
							chargeTextureOn = textureHammerOn;
							chargeTextureReady = textureHammerReady;
						}
					}

					if ((maxHoldTimer || (minHoldTimer && modPlayer.GuardianItemCharge > (70 * player.GetTotalAttackSpeed(DamageClass.Melee) - player.HeldItem.useTime) / 2.5f)) && guardianItem is OrchidModGuardianStandard)
					{
						chargeTextureOff = textureStandardOff;
						chargeTextureOn = textureStandardOn;
						chargeTextureReady = textureStandardReady;
					}

					// if ((maxHoldTimer || (minHoldTimer && modPlayer.GuardianItemCharge > (70 * player.GetTotalAttackSpeed(DamageClass.Melee) - player.HeldItem.useTime) / 2.5f)) && guardianItem is HorizonLance)
					// {
					// 	chargeTextureOn = textureHorizonLanceOn;
					// 	chargeTextureOff = textureHorizonLanceOff;
					// 	chargeTextureReady = textureHorizonLanceReady;
					// }

					// if ((maxHoldTimer || (minHoldTimer && modPlayer.GuardianItemCharge > (70 * player.GetTotalAttackSpeed(DamageClass.Melee) - player.HeldItem.useTime) / 2.5f)) && guardianItem is GuardianNeedle)
					// {
					// 	chargeTextureOn = textureGuardianNeedleOn;
					// 	chargeTextureOff = textureGuardianNeedleOff;
					// 	chargeTextureReady = textureGuardianNeedleReady;
					// }

					if ((maxHoldTimer || (minHoldTimer && modPlayer.GuardianItemCharge > (23 * player.GetTotalAttackSpeed(DamageClass.Melee) - player.HeldItem.useTime) / 2.5f)) && guardianItem is OrchidModGuardianRune)
					{
						chargeTextureOn = textureRuneOn;
						chargeTextureOff = textureRuneOff;
						chargeTextureReady = textureRuneReady;
					}

					if (guardianItem is OrchidModGuardianQuarterstaff)
					{
						if (maxHoldTimer || (minHoldTimer && modPlayer.GuardianItemCharge > (70 * player.GetTotalAttackSpeed(DamageClass.Melee) - player.HeldItem.useTime) / 2.5f))
						{
							chargeTextureOn = textureQuarterstaffOn;
							chargeTextureOff = textureQuarterstaffOff;
							chargeTextureReady = textureQuarterstaffReady;
						}
						else
						{
							int staffType = ModContent.ProjectileType<GuardianQuarterstaffAnchor>();
							Projectile staffAnchor = Main.projectile.FirstOrDefault(proj => proj.whoAmI < Main.maxProjectiles && proj.active && proj.owner == player.whoAmI && proj.type == staffType && proj.ai[2] > 0f);

							if (staffAnchor != null)
							{
								int val = 22;
								float block = staffAnchor.ai[2];
								float step = modPlayer.GetParryDuration(player.HeldItem, (player.HeldItem.ModItem as OrchidModGuardianQuarterstaff).ParryDuration);
								while (block < step)
								{
									block += step / 20f;
									val--;
								}

								Rectangle rectangle = textureBlockOn.Bounds;
								rectangle.Height = val;
								rectangle.Y = textureBlockOn.Height - val;
								drawpos = new Vector2(position.X - 9, position.Y - 92 * player.gravDir + 3f * (player.gravDir - 1));
								spriteBatch.Draw(textureBlockOff, drawpos, null, Color.White, 0f, Vector2.Zero, 1f, effect, 0f);
								drawpos = new Vector2(position.X - 9, position.Y - 92 * player.gravDir + textureBlockOn.Height - val + 3f * (player.gravDir - 1));
								if (player.gravDir < 0) drawpos.Y -= (textureBlockOn.Height - rectangle.Height);
								spriteBatch.Draw(textureBlockOn, drawpos, rectangle, Color.White, 0f, Vector2.Zero, 1f, effect, 0f);
								return;
							}
						}
					}

					if (guardianItem is OrchidModGuardianKatar katar && (maxHoldTimer || (minHoldTimer && modPlayer.GuardianItemCharge > (70 * player.GetTotalAttackSpeed(DamageClass.Melee) - (player.HeldItem.useTime * katar.ChargeSpeedMultiplier)) / 2.5f)))
					{
						chargeTextureOn = textureKatarOn;
						chargeTextureOff = textureKatarOff;
						chargeTextureReady = textureKatarReady;
					}

					if (guardianItem is OrchidModGuardianGauntlet gauntlet && (maxHoldTimer || (minHoldTimer && modPlayer.GuardianItemCharge > (70 * player.GetTotalAttackSpeed(DamageClass.Melee) - (player.HeldItem.useTime * gauntlet.ChargeSpeedMultiplier)) / 2.5f)))
					{
						chargeTextureOn = textureGauntletOn;
						chargeTextureOff = textureGauntletOff;
						chargeTextureReady = textureGauntletReady;
					}

					if (guardianItem is OrchidModGuardianShield shield && (maxHoldTimer || (minHoldTimer && modPlayer.GuardianItemCharge > (70 * player.GetTotalAttackSpeed(DamageClass.Melee) - (player.HeldItem.useTime * shield.ChargeSpeedMultiplier)) / 2.5f)))
					{
						chargeTextureOn = textureBlockOn;
						chargeTextureOff = textureBlockOff;
						chargeTextureReady = textureBlockReady;
					}

					if (guardianItem is OrchidModGuardianGreathammer greathammer && (maxHoldTimer || (minHoldTimer && modPlayer.GuardianItemCharge > (70 * player.GetTotalAttackSpeed(DamageClass.Melee) - (player.HeldItem.useTime * greathammer.WaitChargeGain)) / 2.5f)))
					{
						chargeTextureOn = textureGreathammerOn;
						chargeTextureOff = textureGreathammerOff;
						chargeTextureReady = textureGreathammerReady;
					}

					IconDrawing:
					// Parry duration display on standard weapons
					int shieldType = ModContent.ProjectileType<GuardianShieldAnchor>();
					int gauntletType = ModContent.ProjectileType<GuardianGauntletAnchor>();
					int lanceType = ModContent.ProjectileType<GuardianHorizonLanceAnchor>();
					int katarType = ModContent.ProjectileType<GuardianKatarAnchor>();

					Projectile charging = Main.projectile.FirstOrDefault(proj => proj.whoAmI < Main.maxProjectiles && proj.active && proj.owner == player.whoAmI && proj.type == gauntletType && proj.ai[2] > 0);
					Projectile blockAnchor = Main.projectile.FirstOrDefault(proj => proj.whoAmI < Main.maxProjectiles && proj.active && proj.owner == player.whoAmI && (proj.type == shieldType || proj.type == gauntletType || proj.type == lanceType || proj.type == katarType) && proj.ai[0] > 0f && proj.localAI[0] > 0f && (proj.type != gauntletType || proj.ai[2] <= 0));

					bool gauntletMinCharge = guardianItem is OrchidModGuardianGauntlet gauntletItem && (maxHoldTimer || (minHoldTimer && modPlayer.GuardianItemCharge > (70 * player.GetTotalAttackSpeed(DamageClass.Melee) - (player.HeldItem.useTime * gauntletItem.ChargeSpeedMultiplier)) / 2.5f));
					if (blockAnchor != null)
					{
						int val = 22;
						float block = blockAnchor.ai[0];
						while (block < blockAnchor.localAI[0])
						{
							block += blockAnchor.localAI[0] / 20f;
							val--;
						}

						Rectangle rectangle = textureBlockOn.Bounds;
						rectangle.Height = val;
						rectangle.Y = textureBlockOn.Height - val;
						drawpos = new Vector2(position.X - 9, position.Y - 94 * player.gravDir + 3f * (player.gravDir - 1));
						if (charging != null && gauntletMinCharge && !drawAtCursor)
							drawpos.X += 12;
						spriteBatch.Draw(textureBlockOff, drawpos, null, Color.White, 0f, Vector2.Zero, 1f, effect, 0f);
						drawpos.Y += (textureBlockOn.Height - val);
						if (player.gravDir < 0) drawpos.Y -= (textureBlockOn.Height - rectangle.Height);
						spriteBatch.Draw(textureBlockOn, drawpos, rectangle, Color.White, 0f, Vector2.Zero, 1f, effect, 0f);

						if (blockAnchor.type == shieldType || blockAnchor.type == lanceType || blockAnchor.type == katarType || charging == null || (charging != null && modPlayer.GuardianItemCharge == 0))
							return;
					}
					// end of parry duration stuff

					if (chargeTextureOn != null)
					{
						int val = chargeTextureOn.Height;
						if (modPlayer.GuardianItemCharge < 180f)
						{
							float charge = modPlayer.GuardianItemCharge;
							while (charge < 180f)
							{
								charge += 7.5f;
								val--;
							}
						}

						Rectangle rectangle = chargeTextureOn.Bounds;
						rectangle.Height = val;
						rectangle.Y = chargeTextureOn.Height - val;

						if (drawAtCursor)
						{
							spriteBatch.End();
							spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

							drawpos = Main.MouseScreen + new Vector2(18f, 18f);


							if (modPlayer.GuardianItemCharge >= 180f)
								spriteBatch.Draw(chargeTextureReady, drawpos - new Vector2(2, 2), null, Color.White * 0.8f, 0f, Vector2.Zero, 1f, effect, 0f);

							spriteBatch.Draw(chargeTextureOff, drawpos, null, Color.White, 0f, Vector2.Zero, 1f, effect, 0f);
							drawpos.Y += chargeTextureOn.Height - val;
							spriteBatch.Draw(chargeTextureOn, drawpos, rectangle, Color.White, 0f, Vector2.Zero, 1f, effect, 0f);

							spriteBatch.End();
							spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
						}
						else
						{
							int projectileType = ModContent.ProjectileType<GuardianGauntletAnchor>();
							Projectile blocking = Main.projectile.FirstOrDefault(proj => proj.whoAmI < Main.maxProjectiles && proj.active && proj.type == projectileType && proj.ai[0] > 0);

							if (modPlayer.GuardianItemCharge >= 180f)
							{
								drawpos = new Vector2(position.X - 11, position.Y - 96 * player.gravDir + 5f * (player.gravDir - 1));
								if (chargeTextureOn == textureGauntletOn && blocking != null) drawpos.X -= 12;
								spriteBatch.Draw(chargeTextureReady, drawpos, null, Color.White * 0.8f, 0f, Vector2.Zero, 1f, effect, 0f);
							}

							drawpos = new Vector2(position.X - 9, position.Y - 94 * player.gravDir + 3f * (player.gravDir - 1));
							if (chargeTextureOn == textureGauntletOn && blocking != null) drawpos.X -= 12;
							spriteBatch.Draw(chargeTextureOff, drawpos, null, Color.White, 0f, Vector2.Zero, 1f, effect, 0f);
							drawpos.Y += chargeTextureOn.Height - val;
							if (player.gravDir < 0) drawpos.Y -= (chargeTextureOn.Height - rectangle.Height);
							spriteBatch.Draw(chargeTextureOn, drawpos, rectangle, Color.White, 0f, Vector2.Zero, 1f, effect, 0f);
						}


						if (guardianItem.AnchorType != -1)
						{
							Projectile mainAnchor = Main.projectile.FirstOrDefault(proj => proj.whoAmI < Main.maxProjectiles && proj.active && proj.owner == player.whoAmI && proj.type == guardianItem.AnchorType);
							Color color = Color.White;
							switch (guardianItem)
							{
								case OrchidModGuardianHammer hammer:
									hammer.WarhammerPostDrawUI(spriteBatch, player, ref color, mainAnchor);
									break;
								case OrchidModGuardianShield pavise:
									pavise.PavisePostDrawUI(spriteBatch, player, ref color, mainAnchor);
									break;
								case OrchidModGuardianQuarterstaff quarterstaff:
									quarterstaff.QuarterstaffPostDrawUI(spriteBatch, player, ref color, mainAnchor);
									break;
								case OrchidModGuardianStandard standard:
									standard.StandardPostDrawUI(spriteBatch, player, ref color, mainAnchor);
									break;
								case OrchidModGuardianGauntlet or OrchidModGuardianKatar:
									Projectile altAnchor = Main.projectile.FirstOrDefault(proj => proj.whoAmI < Main.maxProjectiles && proj.active && proj.owner == player.whoAmI && proj.ModProjectile is GuardianGauntletAnchor {OffHandGauntlet: true} or GuardianKatarAnchor {OffHandKatar: true});
									switch (guardianItem)
									{
										case OrchidModGuardianGauntlet gauntlets:
											gauntlets.GauntletPostDrawUI(spriteBatch, player, ref color, mainAnchor, altAnchor);
											break;
										case OrchidModGuardianKatar katars:
										katars.KatarPostDrawUI(spriteBatch, player, ref color, mainAnchor, altAnchor);
											break;
									} 
									break;
							}
						}
					}
				}
				else
				{
					LastUsedCustomUIItem = null;
				}

				spriteBatch.End();
				spriteBatch.Begin(spriteBatchSnapshot);
			}
		}
	}
}