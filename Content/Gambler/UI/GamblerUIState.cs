using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
//using Terraria.UI.Chat;
using OrchidMod.Content.Gambler.Weapons.Chips;
using OrchidMod.Common.UIs;
using System.Collections.Generic;
using ReLogic.Content;

namespace OrchidMod.Content.Gambler.UI
{
	public class GamblerUIState : OrchidUIState
	{
		public Color backgroundColor = Color.White;
		public static Texture2D ressourceBar;
		public static Texture2D ressourceBarFull;
		public static Texture2D ressourceBarDiceFull;
		public static Texture2D ressourceBarTop;
		public static Texture2D ressourceBarDiceTop;

		public static Texture2D chip1;
		public static Texture2D chip2;
		public static Texture2D chip3;
		public static Texture2D chip4;
		public static Texture2D chip5;

		public static Texture2D UIDeck;
		public static Texture2D UICard;
		public static Texture2D UIRedraw;

		public static Texture2D UICardNext1;
		public static Texture2D UICardNext2;
		public static Texture2D UICardNext3;

		public static Texture2D UIDeckbuilding;
		public static Texture2D UIDeckbuildingBlock;
		
		// Chip Weapons
		
		public static Texture2D chipDirection;
		
		public static Texture2D chipDetonatorMain;
		public static Texture2D chipDetonatorBar;
		public static Texture2D chipDetonatorBarEnd;
		
		// Chip Weapons end

		public int cardID = -1;
		public int cardIDNext1 = -1;
		public int cardIDNext2 = -1;
		public int cardIDNext3 = -1;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
			=> layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));

		public override void OnInitialize()
		{
			ressourceBar = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/UI/Textures/GamblerUIBar", AssetRequestMode.ImmediateLoad).Value;
			ressourceBarFull = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/UI/Textures/GamblerUIBarFilled", AssetRequestMode.ImmediateLoad).Value;
			ressourceBarTop = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/UI/Textures/GamblerUIBarTop", AssetRequestMode.ImmediateLoad).Value;
			ressourceBarDiceFull = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/UI/Textures/GamblerUIBarDiceFilled", AssetRequestMode.ImmediateLoad).Value;
			ressourceBarDiceTop = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/UI/Textures/GamblerUIBarDiceTop", AssetRequestMode.ImmediateLoad).Value;

			chip1 = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/UI/Textures/UIChip1", AssetRequestMode.ImmediateLoad).Value;
			chip2 = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/UI/Textures/UIChip2", AssetRequestMode.ImmediateLoad).Value;
			chip3 = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/UI/Textures/UIChip3", AssetRequestMode.ImmediateLoad).Value;
			chip4 = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/UI/Textures/UIChip4", AssetRequestMode.ImmediateLoad).Value;
			chip5 = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/UI/Textures/UIChip5", AssetRequestMode.ImmediateLoad).Value;

			UIDeck = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/UI/Textures/UIDeck", AssetRequestMode.ImmediateLoad).Value;
			UIRedraw = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/UI/Textures/GamblerUIRedraw", AssetRequestMode.ImmediateLoad).Value;
			UICard = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/GamblerReset", AssetRequestMode.ImmediateLoad).Value;
			UICardNext1 = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/GamblerReset", AssetRequestMode.ImmediateLoad).Value;
			UICardNext2 = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/GamblerReset", AssetRequestMode.ImmediateLoad).Value;
			UICardNext3 = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/GamblerReset", AssetRequestMode.ImmediateLoad).Value;

			UIDeckbuilding = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/UI/Textures/DeckUI", AssetRequestMode.ImmediateLoad).Value;
			UIDeckbuildingBlock = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/UI/Textures/DeckUIBlock", AssetRequestMode.ImmediateLoad).Value;

			chipDirection = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/UI/Textures/ChipDirectionArrow", AssetRequestMode.ImmediateLoad).Value;
			chipDetonatorMain = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/UI/Textures/ChipDetonatorMain", AssetRequestMode.ImmediateLoad).Value;
			chipDetonatorBar = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/UI/Textures/ChipDetonatorBar", AssetRequestMode.ImmediateLoad).Value;
			chipDetonatorBarEnd = ModContent.Request<Texture2D>("OrchidMod/Content/Gambler/UI/Textures/ChipDetonatorBarEnd", AssetRequestMode.ImmediateLoad).Value;

			Width.Set(94f, 0f);
			Height.Set(180f, 0f);
			Left.Set(Main.screenWidth / 2 + 30f, 0f);
			Top.Set(Main.screenHeight / 2 - 50f, 0f);
			backgroundColor = Color.White;

			Recalculate();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Recalculate();
			Player player = Main.player[Main.myPlayer];
			OrchidGambler modPlayer = player.GetModPlayer<OrchidGambler>();
			
			Vector2 vector = new Vector2((float)((int)(Main.LocalPlayer.position.X - Main.screenPosition.X) - Main.GameViewMatrix.Translation.X - (float)(Main.LocalPlayer.bodyFrame.Width / 2) + (float)(Main.LocalPlayer.width / 2)), (float)((int)(Main.LocalPlayer.position.Y - Main.screenPosition.Y) - Main.GameViewMatrix.Translation.Y + (float)Main.LocalPlayer.height - (float)Main.LocalPlayer.bodyFrame.Height + 12f + player.gfxOffY)) + Main.LocalPlayer.bodyPosition + new Vector2((float)(Main.LocalPlayer.bodyFrame.Width / 2));
			vector *= Main.GameViewMatrix.Zoom;
			vector /= Main.UIScale;

			this.Left.Set(vector.X + 30f, 0f);
			this.Top.Set(vector.Y - 50f, 0f);

			CalculatedStyle dimensions = GetDimensions();
			Point point = new Point((int)dimensions.X, (int)dimensions.Y - 100);
			int width = (int)Math.Ceiling(dimensions.Width);
			int height = (int)Math.Ceiling(dimensions.Height);

			if (!player.dead)
			{
				if (modPlayer.gamblerUIChipSpinDisplay)
				{
					Vector2 position = new Vector2(point.X - 30, point.Y + 150);
					float angle = MathHelper.ToRadians(modPlayer.gamblerChipSpin);
					spriteBatch.Draw(chipDirection, position, null, backgroundColor, angle, chipDirection.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
				}
				
				if (player.HeldItem.ModItem is MeteorDetonator) {
					Rectangle rect = new Rectangle(point.X - 75, point.Y + 50, chipDetonatorMain.Width, chipDetonatorMain.Height);
					spriteBatch.Draw(chipDetonatorMain, rect, backgroundColor);
					
					float index = 720f / 8;
					
					if (modPlayer.gamblerChipSpin > index) {
						spriteBatch.Draw(chipDetonatorBar, new Rectangle(point.X - 61, point.Y + 72, 14, 6), backgroundColor);
					}
					
					if (modPlayer.gamblerChipSpin > index * 2 && modPlayer.gamblerChipSpin < index * 7) {
						spriteBatch.Draw(chipDetonatorBar, new Rectangle(point.X - 45, point.Y + 72, 14, 6), backgroundColor);
					}
					
					if (modPlayer.gamblerChipSpin > index * 3 && modPlayer.gamblerChipSpin < index * 6) {
						spriteBatch.Draw(chipDetonatorBar, new Rectangle(point.X - 29, point.Y + 72, 14, 6), backgroundColor);
					}
					
					if (modPlayer.gamblerChipSpin > index * 4 && modPlayer.gamblerChipSpin < index * 5) {
						spriteBatch.Draw(chipDetonatorBarEnd, new Rectangle(point.X - 13, point.Y + 64, 10, 22), backgroundColor);
					}
				}
				
				/* // OBSOLETE GAMBLER NPC UI CODE
				if (modPlayer.gamblerUIDeckDisplay)
				{
					if (player.talkNPC != -1)
					{
						spriteBatch.Draw(UIDeckbuilding, new Rectangle(point.X - 100, point.Y - 60, 140, 138), backgroundColor);
						int offSetX = 0;
						int offSetY = 0;
						string msg = "";
						string msg2 = "";
						string msg3 = "";
						int[] nbCards = new int[20];
						int maxReq = 0;
						int playerNbCards = OrchidModGamblerHelper.getNbGamblerCards(player, modPlayer);
						for (int i = 0; i < 20; i++)
						{
							nbCards[i] = 0;
						}
						for (int i = 0; i < playerNbCards; i++)
						{
							Item currentItem = modPlayer.gamblerCardsItem[i];
							Texture2D currentTexture = TextureAssets.Item[currentItem.type].Value;
							Rectangle currentRectangle = new Rectangle(point.X - 92 + offSetX, point.Y - 52 + offSetY, 20, 26);
							spriteBatch.Draw(currentTexture, currentRectangle, backgroundColor);
							offSetX += 26;
							if ((i - 4) % 5 == 0)
							{
								offSetX = 0;
								offSetY += 32;
							}
							if (currentItem.type != 0)
							{
								OrchidModGlobalItem orchidItem = currentItem.GetGlobalItem<OrchidModGlobalItem>();
								int cardReq = orchidItem.gamblerCardRequirement;
								nbCards[cardReq]++;
								maxReq = cardReq > maxReq ? cardReq : maxReq;
							}
						}

						offSetX = 0;
						offSetY = 0;

						for (int i = 0; i < playerNbCards; i++)
						{
							Item currentItem = modPlayer.gamblerCardsItem[i];
							Rectangle currentRectangle = new Rectangle(point.X - 92 + offSetX, point.Y - 52 + offSetY, 20, 26);
							offSetX += 26;
							if ((i - 4) % 5 == 0)
							{
								offSetX = 0;
								offSetY += 32;
							}
							if (currentItem.type != 0)
							{
								OrchidModGlobalItem orchidItem = currentItem.GetGlobalItem<OrchidModGlobalItem>();
								int cardReq = orchidItem.gamblerCardRequirement;
								bool canRemove = (playerNbCards > maxReq + 1) || (cardReq == maxReq);
								if (currentRectangle.Contains(new Point((int)Main.MouseScreen.X, (int)Main.MouseScreen.Y)))
								{
									if (((Main.mouseLeft && Main.mouseLeftRelease) || (Main.mouseRight && Main.mouseRightRelease)) && canRemove)
									{
										player.QuickSpawnItem(currentItem.type, 1);
										OrchidModGamblerHelper.removeGamblerCard(currentItem, player, modPlayer);
										if (OrchidModGamblerHelper.getNbGamblerCards(player, modPlayer) > 0)
										{
											OrchidModGamblerHelper.clearGamblerCardCurrent(player, modPlayer);
											OrchidModGamblerHelper.clearGamblerCardsNext(player, modPlayer);
											modPlayer.gamblerShuffleCooldown = 0;
											modPlayer.gamblerRedraws = 0;
											OrchidModGamblerHelper.drawGamblerCard(player, modPlayer);
										}
										else
										{
											OrchidModGamblerHelper.onRespawnGambler(player, modPlayer);
										}
									}
									msg = currentItem.Name;
									msg2 = canRemove ? "Can be removed" : "Cannot be removed";
									msg3 = canRemove ? "" : "Your highest cost card requires at least " + maxReq + " cards";
								}
								if (!canRemove)
								{
									Rectangle blockRectangle = new Rectangle(currentRectangle.X - 2, currentRectangle.Y - 2, currentRectangle.Width + 4, currentRectangle.Height + 4);
									spriteBatch.Draw(UIDeckbuildingBlock, blockRectangle, backgroundColor);
								}
							}
						}
						if (msg != "")
						{
							ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, msg, Main.MouseScreen + new Vector2(15f, 15f), new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor), 0f, Vector2.Zero, Vector2.One, -1f, 2f);
						}
						if (msg2 != "")
						{
							ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, msg2, Main.MouseScreen + new Vector2(15f, 40f), new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor), 0f, Vector2.Zero, Vector2.One, -1f, 2f);
						}
						if (msg3 != "")
						{
							ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, msg2, Main.MouseScreen + new Vector2(15f, 40f), new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor), 0f, Vector2.Zero, Vector2.One, -1f, 2f);
						}
					}
					else
					{
						modPlayer.gamblerUIDeckDisplay = false;
					}
				}
				*/

				if ((modPlayer.gamblerUIDisplayTimer > 0 || modPlayer.gamblerChips > 0) && modPlayer.gamblerUIFightDisplay)
				{

					if (modPlayer.gamblerCardCurrent.type != this.cardID)
					{
						UICard = this.getUiCardTexture(modPlayer, 0);
						this.cardID = modPlayer.gamblerCardCurrent.type;
					}

					if (modPlayer.gamblerCardNext[0].type != this.cardIDNext1)
					{
						UICardNext1 = this.getUiCardTexture(modPlayer, 1);
						this.cardIDNext1 = modPlayer.gamblerCardNext[0].type;
					}

					if (modPlayer.gamblerCardNext[1].type != this.cardIDNext2)
					{
						UICardNext2 = this.getUiCardTexture(modPlayer, 2);
						this.cardIDNext2 = modPlayer.gamblerCardNext[1].type;
					}

					if (modPlayer.gamblerCardNext[2].type != this.cardIDNext3)
					{
						UICardNext3 = this.getUiCardTexture(modPlayer, 3);
						this.cardIDNext3 = modPlayer.gamblerCardNext[2].type;
					}

					int drawHeight = 70;
					int drawSide = 0;
					int leftOffset = 0;
					int leftOffset2 = 0;
					Texture2D chip = chip1;

					spriteBatch.Draw(UIDeck, new Rectangle(point.X + 8, point.Y + 160, 26, 32), backgroundColor);
					spriteBatch.Draw(UICard, new Rectangle(point.X + 8, point.Y + 160, 20, 26), backgroundColor);

					if (modPlayer.gamblerSeeCards > 2) spriteBatch.Draw(UICardNext3, new Rectangle(point.X + 40, point.Y + 194, 20, 26), backgroundColor);
					if (modPlayer.gamblerSeeCards > 1) spriteBatch.Draw(UICardNext2, new Rectangle(point.X + 24, point.Y + 194, 20, 26), backgroundColor);
					if (modPlayer.gamblerSeeCards > 0) spriteBatch.Draw(UICardNext1, new Rectangle(point.X + 8, point.Y + 194, 20, 26), backgroundColor);

					for (int i = 0; i < modPlayer.gamblerRedraws; i++)
					{
						spriteBatch.Draw(UIRedraw, new Rectangle(point.X + 20 - drawSide, point.Y + 94, 14, 20), backgroundColor);
						drawSide += 4;
					}

					int unit = (int)(modPlayer.gamblerShuffleCooldownMax / 34) + 1;
					int val = 0;
					spriteBatch.Draw(ressourceBar, new Rectangle(point.X + 36, point.Y + 116, 12, 76), backgroundColor);
					while (val < modPlayer.gamblerShuffleCooldown)
					{
						spriteBatch.Draw(ressourceBarFull, new Rectangle(point.X + 40, point.Y + 116 + drawHeight, 4, 2), backgroundColor);
						drawHeight -= 2;
						val += unit;
					}

					if (val > 0)
					{
						drawHeight += 2;
						spriteBatch.Draw(ressourceBarTop, new Rectangle(point.X + 40, point.Y + 116 + drawHeight, 4, 2), backgroundColor);
					}

					if (modPlayer.gamblerDie != null)
					{
						Texture2D dice = modPlayer.gamblerDie.UITexture;

						Rectangle newBounds = dice.Bounds;
						newBounds.Height /= 13;
						newBounds.Width /= 7;
						Rectangle newBoundsNumber = dice.Bounds;
						newBoundsNumber.Height /= 13;
						newBoundsNumber.Width /= 7;
						newBounds.Y += newBounds.Height * 6;
						newBoundsNumber.Y += newBounds.Height * 6;
						newBoundsNumber.X += newBoundsNumber.Width * modPlayer.gamblerDieValue;

						Vector2 diePosition = new Vector2(point.X + 36, point.Y + 90);

						spriteBatch.Draw(dice, diePosition, newBounds, Color.White);
						spriteBatch.Draw(dice, diePosition, newBoundsNumber, Color.White);

						drawHeight = 70;
						val = 0;

						spriteBatch.Draw(ressourceBar, new Rectangle(point.X + 50, point.Y + 116, 12, 76), backgroundColor);
						while (val < modPlayer.gamblerDieDuration - ((modPlayer.gamblerDie.diceDuration * 60) / 33))
						{
							spriteBatch.Draw(ressourceBarDiceFull, new Rectangle(point.X + 54, point.Y + 116 + drawHeight, 4, 2), backgroundColor);
							drawHeight -= 2;
							val += (int)((modPlayer.gamblerDie.diceDuration * 60) / 33);
						}

						if (val > 0)
						{
							spriteBatch.Draw(ressourceBarDiceTop, new Rectangle(point.X + 54, point.Y + 116 + drawHeight, 4, 2), backgroundColor);
						}
					}

					drawHeight = 36;
					for (int i = 1; i < modPlayer.gamblerChips + 1; i++)
					{
						spriteBatch.Draw(chip, new Rectangle(point.X + 18 - leftOffset - leftOffset2, point.Y + 116 + drawHeight, 14, 6), backgroundColor);

						drawHeight -= 4;
						if (i % 10 == 0)
						{
							drawHeight = 36;
							leftOffset2 = leftOffset2 == 4 ? 0 : 4;
							switch (i)
							{
								case 10:
									chip = chip2;
									break;
								case 20:
									chip = chip3;
									break;
								case 30:
									chip = chip4;
									break;
								case 40:
									chip = chip5;
									break;
								default:
									break;
							}
						}
						leftOffset = leftOffset == 2 ? 0 : 2;
					}
				}
			}
		}

		public Texture2D getUiCardTexture(OrchidGambler modPlayer, int cardNb)
		{
			Texture2D cardTexture;
			Item card = new Item();
			if (modPlayer.gamblerCardCurrent.type != 0)
			{
				switch (cardNb)
				{
					case 0:
						card = modPlayer.gamblerCardCurrent;
						break;
					case 1:
						card = modPlayer.gamblerCardNext[0];
						break;
					case 2:
						card = modPlayer.gamblerCardNext[1];
						break;
					case 3:
						card = modPlayer.gamblerCardNext[2];
						break;
					default:
						cardTexture = UIRedraw;
						return cardTexture;
				}
				cardTexture = TextureAssets.Item[card.type].Value;
			}
			else
			{
				cardTexture = UIRedraw;
			}
			return cardTexture;
		}
	}
}