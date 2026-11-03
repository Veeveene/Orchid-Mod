using Microsoft.Xna.Framework;
using OrchidMod.Content.Guardian.Accessories;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Shapeshifter.Accessories
{
	public class GuideTorches : OrchidModShapeshifterItem
	{
		public override void SafeSetDefaults()
		{
			Item.width = 24;
			Item.height = 32;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Blue;
			Item.accessory = true;
		}

		public static void PlaceTorchCheck(Player player)
		{
			OrchidShapeshifter shapeshifter = Shapeshifter(player);
			if (shapeshifter.IsShapeshifted && player.whoAmI == Main.myPlayer)
			{
				if (Main.mouseMiddle && Main.mouseMiddleRelease && shapeshifter.ShapeshiftAnchor.LeftCLickCooldown <= 0f && shapeshifter.ShapeshiftAnchor.RightCLickCooldown <= 0f)
				{
					Item item = null;
					foreach (Item itemTorch in player.inventory)
					{
						if (ItemID.Sets.Torches[itemTorch.type])
						{
							item = itemTorch;
						}
					}

					if (item != null)
					{
						Vector2 position = Main.MouseWorld - player.Center;
						float maxReach = 96f;
						if (position.Length() > maxReach) position = Vector2.Normalize(position) * maxReach;
						position = player.Center + position;

						Point point = new Point((int)(position.X / 16f), (int)(position.Y / 16f));
						Tile tile = Framing.GetTileSafely(point);
						if (!tile.HasTile)
						{
							int refType = item.createTile;
							int refStyle = item.placeStyle;
							player.BiomeTorchPlaceStyle(ref refType, ref refStyle);
							shapeshifter.ShapeshiftAnchor.LeftCLickCooldown = 10;
							shapeshifter.ShapeshiftAnchor.RightCLickCooldown = 10;
							if (WorldGen.PlaceObject(point.X, point.Y, refType, style: refStyle))
							{
								item.stack--;

								if (Main.netMode == NetmodeID.MultiplayerClient)
								{
									NetMessage.SendTileSquare(player.whoAmI, point.X, point.Y);
								}
							}
						}
					}
					else
					{
						SoundStyle soundStyle = SoundID.LiquidsWaterLava;
						soundStyle.Volume *= 0.5f;
						SoundEngine.PlaySound(soundStyle, player.Center);
						CombatText.NewText(player.Hitbox, Color.OrangeRed, "no torch", dot: true);
					}
				}
			}
		}

		public override bool CanRightClick() => true;

		public override bool ConsumeItem(Player player) => false;

		public override void RightClick(Player player)
		{
			int prefix = Item.prefix;
			Item.SetDefaults(ModContent.ItemType<GuideTorchesInactive>());
			Item.Prefix(prefix);
		}

		public override void UpdateInventory(Player player)
		{
			PlaceTorchCheck(player);
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			PlaceTorchCheck(player);
		}
	}
}