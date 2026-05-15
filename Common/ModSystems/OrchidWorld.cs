using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;
using Terraria.WorldBuilding;
using Terraria.IO;
using OrchidMod.Content.Gambler.Weapons.Cards;
using OrchidMod.Content.Alchemist.Weapons.Fire;
using OrchidMod.Content.Alchemist.Weapons.Water;
using OrchidMod.Content.Alchemist.Weapons.Nature;
using OrchidMod.Content.Alchemist.Weapons.Air;
using OrchidMod.Content.Gambler.Misc;
using OrchidMod.Content.Alchemist.Weapons.Catalysts;
using OrchidMod.Content.Gambler.Weapons.Chips;
using OrchidMod.Content.Alchemist.Accessories;
using OrchidMod.Content.Gambler.Decks;
using OrchidMod.Content.Gambler.Accessories;
using OrchidMod.Utilities;
using OrchidMod.Content.Guardian.Weapons.Runes;
using OrchidMod.Content.Guardian.Weapons.Shields;
using OrchidMod.Content.Guardian.Accessories;
using OrchidMod.Content.Guardian.Weapons.Gauntlets;
using OrchidMod.Content.Guardian.Weapons.Standards;
using OrchidMineshaft.Tiles.Chests;
using OrchidMod.Content.General.Misc;
using OrchidMod.Content.Shapeshifter.Weapons.Warden;
using OrchidMod.Content.Shapeshifter.Weapons.Predator;
using OrchidMod.Content.Guardian.Misc;
using OrchidMod.Content.Guardian.Weapons.Quarterstaves;
using OrchidMod.Content.Shapeshifter.Weapons.Symbiote;
using OrchidMod.Content.Shapeshifter.Accessories;
using OrchidMod.Content.Shapeshifter.Misc;
using OrchidMod.Content.Guardian.Weapons.Katars;

namespace OrchidMod.Common.ModSystems
{
	public class OrchidWorld : ModSystem
	{
		public static bool foundChemist = false;
		public static bool foundSlimeCard = false;

		public static float alchemistMushroomArmorProgress = 0.5f; // 0.5f -> 1f -> 0.5f -> ...

		public override void OnWorldLoad()
		{
			foundChemist = false;
			foundSlimeCard = false;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			var orchidTags = new List<string>();

			if (foundChemist)
			{
				orchidTags.Add("chemist");
			}

			if (foundSlimeCard)
			{
				orchidTags.Add("slimecard");
			}

			tag.Add("orchidTags", orchidTags);
		}

		public override void LoadWorldData(TagCompound tag)
		{
			var orchidTags = tag.GetList<string>("orchidTags");
			foundChemist = orchidTags.Contains("chemist");
			foundSlimeCard = orchidTags.Contains("slimecard");
		}

		public override void NetSend(BinaryWriter writer)
		{
			var flags = new BitsByte();
			flags[0] = foundChemist;
			flags[1] = foundSlimeCard;
			writer.Write(flags);
		}

		public override void NetReceive(BinaryReader reader)
		{
			BitsByte flags = reader.ReadByte();
			foundChemist = flags[0];
		}

		public override void PreUpdateWorld()
		{
			// Mushroom Armor Lighting Progress and ... Shroomite Scepter
			{
				alchemistMushroomArmorProgress = (float)Math.Sin(Main.GlobalTimeWrappedHourly * (Math.PI / 2f)) * 0.25f + 0.75f;
			}
		}

		private void placeLilies()
		{
			for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 0.015); k++)
			{
				int x = WorldGen.genRand.Next(0, Main.maxTilesX);
				int y = WorldGen.genRand.Next((int)GenVars.rockLayer, Main.maxTilesY);

				if (!Framing.GetTileSafely(x, y).HasTile && !Framing.GetTileSafely(x + 1, y).HasTile &&
				!Framing.GetTileSafely(x, y - 1).HasTile && !Framing.GetTileSafely(x + 1, y - 1).HasTile)
				{
					if (Framing.GetTileSafely(x, y + 1).TileType == 60 && Framing.GetTileSafely(x + 1, y + 1).TileType == 60)
					{

						for (int w = 0; w < 2; w++)
						{
							for (int q = 0; q < 2; q++)
							{
								Tile tile = Framing.GetTileSafely(x + w, y - q);
								tile.ClearTile();
							}
						}
						WorldGen.PlaceTile(x, y, TileType<Content.General.Materials.JungleLilyTile>(), style: WorldGen.genRand.Next(4));
					}
				}
			}
		}

		private void placeVerbena()
		{
			int amount = (int)(Main.maxTilesX * Main.maxTilesY * 0.0000004f);
			int failsafe = 0;
			while(failsafe < 40000 && amount > 0)
			{
				failsafe++;
				int x = WorldGen.genRand.Next(0, Main.maxTilesX);
				int y = WorldGen.genRand.Next((int)GenVars.rockLayer, Main.maxTilesY);

				if (!Framing.GetTileSafely(x, y).HasTile && !Framing.GetTileSafely(x + 1, y).HasTile &&
				!Framing.GetTileSafely(x, y - 1).HasTile && !Framing.GetTileSafely(x + 1, y - 1).HasTile)
				{
					if (Framing.GetTileSafely(x, y + 1).TileType == 60 && Framing.GetTileSafely(x + 1, y + 1).TileType == 60)
					{

						for (int w = 0; w < 2; w++)
						{
							for (int q = 0; q < 2; q++)
							{
								Tile tile = Framing.GetTileSafely(x + w, y - q);
								tile.ClearTile();
							}
						}
						WorldGen.PlaceTile(x, y, TileType<VerveineQuarterstaffTile>());
						amount--;
					}
				}
			}
		}

		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			// int ShiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
			// if (ShiniesIndex != -1) {
			// tasks.Insert(ShiniesIndex + 1, new PassLegacy("Static Quartz", placeQuartz));
			// }

			int LivingTreesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Living Trees"));
			if (LivingTreesIndex != -1)
			{
				tasks.Insert(LivingTreesIndex + 1, new PassLegacy("Post Terrain", delegate (GenerationProgress progress, GameConfiguration gameConfiguration)
				{
					if (ModContent.GetInstance<OrchidServerConfig>().EnableContentAlchemist)
					{
						progress.Message = "Generating Jungle Lilies";
						placeLilies();
					}

					progress.Message = "Generating Farterstaves";
					placeVerbena();
				}));
			}

			int ChestsIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Surface Chests"));
			if (ChestsIndex != -1)
			{
				tasks.Insert(ChestsIndex + 1, new PassLegacy("Post Terrain", delegate (GenerationProgress progress, GameConfiguration gameConfiguration)
				{
					// Get dungeon size field infos. These fields are private for some reason
					int MinX = GenVars.dMinX + 25;
					int MaxX = GenVars.dMaxX - 25;
					int MaxY = GenVars.dMaxY - 25;

					progress.Message = "Orchid Mod: Biome Chests";

					// int rounds = 1;
					// Terra Custom Support
					// Type TerraCustom_Settings = typeof(Main).Assembly.GetType("Terraria.TerraCustom.Setting");
					// if (TerraCustom_Settings != null)
					// {
					// OrchidMod.mod.Logger.Info("Getting Biome Chest Set Amount From TerraCustom Settings");
					// object terraCustom_setting = typeof(Main).GetField("setting", BindingFlags.Static | BindingFlags.Public).GetValue(null);
					// rounds = (int)(TerraCustom_Settings.GetProperty("BiomeChestSets", BindingFlags.Instance | BindingFlags.Public).GetValue(terraCustom_setting, null));
					// }

					/* REIMPLEMENT THIS
					for (int i = 0; i < rounds; i++)
					{
						Chest chest = null;
						int attempts = 0;
						while (chest == null && attempts < 10000)
						{
							attempts++;
							int x = WorldGen.genRand.Next(MinX, MaxX);
							int y = WorldGen.genRand.Next((int)Main.worldSurface, MaxY);
							if (Main.wallDungeon[Main.tile[x, y].WallType] && !Main.tile[x, y].HasTile)
							{
								while (y < Main.maxTilesY - 210)
								{
									if (!WorldGen.SolidTile(x, y))
									{
										y++;
										continue;
									}

									int chestIndex = WorldGen.PlaceChest(x - 1, y - 1, (ushort)TileType<Content.General.Placeables.ShamanBiomeChestTile>(), false, 1);

									if (chestIndex < 0)
									{
										break;
									}

									chest = Main.chest[chestIndex];
								}
							}
						}
					}
					*/
				}));
			}
		}

		public bool placeInChest(Chest chest, int itemToPlace, int quantity)
		{
			if (chest != null)
			{
				for (int inventoryIndex = 39; inventoryIndex > 1; inventoryIndex--)
				{
					if (chest.item[inventoryIndex - 1].type != ItemID.None)
					{
						chest.item[inventoryIndex].SetDefaults(chest.item[inventoryIndex - 1].type);
						chest.item[inventoryIndex].stack = chest.item[inventoryIndex - 1].stack;
					}
				}
				chest.item[1].SetDefaults(itemToPlace);
				if (quantity > 1)
				{
					chest.item[1].stack = quantity;
				}
			}
			else
			{
				return false;
			}
			return true;
		}

		public override void PostWorldGen() // TODO : Better chest spawn & item guarantee code
		{
			List<ChestLoot> chestLoots = new List<ChestLoot>();
			List<Chest> usedChests = new List<Chest>();

			// Surface Chests
			chestLoots.Add(new ChestLoot(ItemType<Quarterstaff>(), ChestType.SurfaceWooden, 20));
			chestLoots.Add(new ChestLoot(ItemType<GuideShield>(), ChestType.SurfaceWooden, 20));

			// Water Chests
			// chestLoots.Add(new ChestLoot(ItemType<XXXXXXXXXX>(), ChestType.Water, 20));

			// Living Tree Chests
			chestLoots.Add(new ChestLoot(ItemType<LivingRune>(), ChestType.LivingTree, 50));

			// Gold Chests
			chestLoots.Add(new ChestLoot(ItemType<EnchantedRune>(), ChestType.Gold, 20));
			chestLoots.Add(new ChestLoot(ItemType<GoldChestKatar>(), ChestType.Gold, 20));
			chestLoots.Add(new ChestLoot(ItemType<EnchantedPavise>(), ChestType.Gold, 20));

			// Ice Chests
			chestLoots.Add(new ChestLoot(ItemType<IceStandard>(), ChestType.Ice, 20));

			// Dungeon Chests (Locked)
			chestLoots.Add(new ChestLoot(ItemType<DungeonQuarterstaff>(), ChestType.DungeonLocked, 20));

			// Shadow Chests
			chestLoots.Add(new ChestLoot(ItemType<NightShield>(), ChestType.Shadow, 30));
			chestLoots.Add(new ChestLoot(ItemType<HellRune>(), ChestType.Shadow, 30));
			chestLoots.Add(new ChestLoot(ItemType<GuardianRunePotion>(), ChestType.Shadow, 50, needToPlace: 0, ignoreChestLimit: true));

			// Jungle Chests
			chestLoots.Add(new ChestLoot(ItemType<JungleGauntlet>(), ChestType.Ivy, 20));
			chestLoots.Add(new ChestLoot(ItemType<GuardianHoneyPotion>(), ChestType.Ivy, 50, needToPlace: 0, ignoreChestLimit: true));

			// Sky Island Chests
			chestLoots.Add(new ChestLoot(ItemType<SkywareShield>(), ChestType.SkyIsland, 30));

			// Underground Desert Chests
			chestLoots.Add(new ChestLoot(ItemType<DesertStandard>(), ChestType.Sandstone, 20));

			// Granite Chests
			chestLoots.Add(new ChestLoot(ItemType<SturdySlab>(), ChestType.Granite, 50));

			// Mushroom Chests
			chestLoots.Add(new ChestLoot(ItemType<GlowingMushroomGauntlet>(), ChestType.Mushroom, 50));

			if (ModContent.GetInstance<OrchidServerConfig>().EnableContentAlchemist)
			{
				chestLoots.Add(new ChestLoot(ItemType<EmberVial>(), ChestType.SurfaceWooden, 20));
				chestLoots.Add(new ChestLoot(ItemType<SeafoamVial>(), ChestType.Water, 20));
				chestLoots.Add(new ChestLoot(ItemType<LivingSapVial>(), ChestType.LivingTree, 50));
				chestLoots.Add(new ChestLoot(ItemType<CloudInAVial>(), ChestType.Gold, 20));
				chestLoots.Add(new ChestLoot(ItemType<FartInAVial>(), ChestType.Gold, 1, needToPlace: 0, ignoreChestLimit: true));
				chestLoots.Add(new ChestLoot(ItemType<BlizzardInAVial>(), ChestType.Ice, 20));
				chestLoots.Add(new ChestLoot(ItemType<IceChestFlask>(), ChestType.Ice, 20));
				chestLoots.Add(new ChestLoot(ItemType<DungeonFlask>(), ChestType.DungeonLocked, 20));
				chestLoots.Add(new ChestLoot(ItemType<DungeonCatalyst>(), ChestType.DungeonLocked, 20));
				chestLoots.Add(new ChestLoot(ItemType<ShadowChestFlask>(), ChestType.Shadow, 30));
				chestLoots.Add(new ChestLoot(ItemType<KeystoneOfTheConvent>(), ChestType.Shadow, 30));
				chestLoots.Add(new ChestLoot(ItemType<BloomingBud>(), ChestType.Ivy, 20));
				chestLoots.Add(new ChestLoot(ItemType<SunplateFlask>(), ChestType.SkyIsland, 30));
			}

			if (ModContent.GetInstance<OrchidServerConfig>().EnableContentGambler)
			{
				chestLoots.Add(new ChestLoot(ItemType<TiamatRelic>(), ChestType.DungeonLocked, 50, 1, 3, true));
				chestLoots.Add(new ChestLoot(ItemType<EmbersCard>(), ChestType.SurfaceWooden, 20));
				chestLoots.Add(new ChestLoot(ItemType<BubbleCard>(), ChestType.Water, 20));
				chestLoots.Add(new ChestLoot(ItemType<SapCard>(), ChestType.LivingTree, 50));
				chestLoots.Add(new ChestLoot(ItemType<GoldChestCard>(), ChestType.Gold, 20));
				chestLoots.Add(new ChestLoot(ItemType<DeckEnchanted>(), ChestType.Gold, 5, needToPlace: 0, ignoreChestLimit: true));
				chestLoots.Add(new ChestLoot(ItemType<IceChestCard>(), ChestType.Ice, 20));
				chestLoots.Add(new ChestLoot(ItemType<Rusalka>(), ChestType.DungeonLocked, 20));
				chestLoots.Add(new ChestLoot(ItemType<DeckBone>(), ChestType.DungeonLocked, 5, needToPlace: 0, ignoreChestLimit: true));
				chestLoots.Add(new ChestLoot(ItemType<ImpDiceCup>(), ChestType.Shadow, 30));
				chestLoots.Add(new ChestLoot(ItemType<DeckJungle>(), ChestType.Ivy, 5, needToPlace: 0, ignoreChestLimit: true));
				chestLoots.Add(new ChestLoot(ItemType<IvyChestCard>(), ChestType.Ivy, 20));
				chestLoots.Add(new ChestLoot(ItemType<BundleOfClovers>(), ChestType.Ivy, 20));
			}

			if (ModContent.GetInstance<OrchidServerConfig>().EnableContentShapeshifter)
			{
				chestLoots.Add(new ChestLoot(ItemType<WardenSnail>(), ChestType.SurfaceWooden, 20));
				chestLoots.Add(new ChestLoot(ItemType<SymbioteToad>(), ChestType.LivingTree, 50));
				chestLoots.Add(new ChestLoot(ItemType<PredatorIceFox>(), ChestType.Ice, 20));
				chestLoots.Add(new ChestLoot(ItemType<WardenTortoise>(), ChestType.Ivy, 20));
				chestLoots.Add(new ChestLoot(ItemType<WardenSalamortar>(), ChestType.Shadow, 30));
				chestLoots.Add(new ChestLoot(ItemType<ShawlFeather>(), ChestType.SkyIsland, 30));
				chestLoots.Add(new ChestLoot(ItemType<PredatorUndine>(), ChestType.DungeonLocked, 20));
				chestLoots.Add(new ChestLoot(ItemType<ShapeshifterSurvivalPotion>(), ChestType.SkyIsland, 50, needToPlace: 0, ignoreChestLimit: true));
			}

			for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
			{
				Chest chest = Main.chest[chestIndex];
				HandleSpecialChests(chest);

				List<ChestLoot> possibleLoot = new List<ChestLoot>();
				foreach (ChestLoot loot in chestLoots)
				{
					if (loot.ValidChest(chest) && loot.RollPlace())
					{
						if (!usedChests.Contains(chest) && !loot.ignoreChestLimit)
							possibleLoot.Add(loot);

						if (loot.ignoreChestLimit)
						{
							placeInChest(chest, loot.itemType, loot.quantity);
							loot.placed++;
						}
					}
				}

				ChestLoot chosenLoot = null;
				int lowestPriority = int.MaxValue;
				foreach (ChestLoot loot in possibleLoot)
				{
					if (loot.placed < lowestPriority)
					{
						chosenLoot = loot;
						lowestPriority = loot.placed;
					}
				}

				if (chosenLoot != null)
				{
					placeInChest(chest, chosenLoot.itemType, chosenLoot.quantity);
					chosenLoot.placed++;
					usedChests.Add(chest);
				}
			}

			for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
			{
				Chest chest = Main.chest[chestIndex];
				foreach (ChestLoot loot in chestLoots)
				{
					if (loot.ValidChest(chest) && loot.placed < loot.needToPlace)
					{
						placeInChest(chest, loot.itemType, loot.quantity);
						loot.placed++;
					}
				}
			}
		}

		public void HandleSpecialChests(Chest chest)
		{
			if (chest != null && Main.tile[chest.x, chest.y].TileType == (ushort)TileType<MinersLockboxTile>())
			{
				int[] specialItemPoll = { ItemType<EnchantedRune>(), ItemType<EnchantedPavise>() };
				int rand = Main.rand.Next(specialItemPoll);
				if (ModContent.GetInstance<OrchidServerConfig>().EnableContentGambler) placeInChest(chest, ItemType<DetonatorCard>(), 1);
				placeInChest(chest, rand, 1);
			}

			if (chest != null && Main.tile[chest.x, chest.y].TileType == (ushort)TileType<ShamanBiomeChestTile>())
			{
				//chest.item[0].SetDefaults(ItemType<ShroomiteScepter>());
				chest.item[1].SetDefaults(183); // Glowing Mushroom
				chest.item[1].stack = Main.rand.Next(10) + 20;
				chest.item[2].SetDefaults(188); // Healing Potion
				chest.item[2].stack = Main.rand.Next(5) + 3;
				chest.item[3].SetDefaults(298); // Shine Potion
				chest.item[3].stack = Main.rand.Next(3) + 1;
				chest.item[4].SetDefaults(289); // Regeneration Potion
				chest.item[4].stack = Main.rand.Next(3) + 1;
			}

		}
	}

	public class ChestLoot
	{
		public int itemType;
		public ChestType chestType;
		public int quantity;
		public int needToPlace;
		public int chance;
		public int placed;
		public bool ignoreChestLimit;
		public bool RollPlace() => Main.rand.Next(100) < chance;

		public ChestLoot(int itemType, ChestType chestType, int chance, int quantity = 1, int needToPlace = 1, bool ignoreChestLimit = false)
		{
			this.itemType = itemType;
			this.quantity = quantity;
			this.chestType = chestType;
			this.needToPlace = needToPlace;
			this.ignoreChestLimit = ignoreChestLimit;
			this.chance = chance;
			placed = 0;
		}

		public bool ValidChest(Chest chest)
		{
			if (chest == null || !(Framing.GetTileSafely(chest.x, chest.y) is Tile tile)) return false;
			if (!tile.HasTile || !TileID.Sets.IsAContainer[tile.TileType]) return false;

			switch (chestType)
			{
				case ChestType.SurfaceWooden:
					bool flag = true;
					flag &= tile.TileType == TileID.Containers;
					flag &= tile.TileFrameX == 0 * 36;
					flag &= !tile.WallType.Between(WallID.BlueDungeonSlabUnsafe, WallID.GreenDungeonTileUnsafe);
					flag &= !tile.WallType.Between(WallID.BlueDungeonUnsafe, WallID.PinkDungeonUnsafe);
					return flag;
				case ChestType.Water:
					return tile.TileType == TileID.Containers && tile.TileFrameX == 17 * 36;
				case ChestType.LivingTree:
					return tile.TileType == TileID.Containers && tile.TileFrameX == 12 * 36;
				case ChestType.SkyIsland:
					return tile.TileType == TileID.Containers && tile.TileFrameX == 13 * 36;
				case ChestType.Gold:
					return tile.TileType == TileID.Containers && tile.TileFrameX == 1 * 36 && tile.WallType != WallID.SandstoneBrick;
				case ChestType.DungeonLocked:
					return tile.TileType == TileID.Containers && tile.TileFrameX == 2 * 36;
				case ChestType.Shadow:
					return tile.TileType == TileID.Containers && tile.TileFrameX == 4 * 36;
				case ChestType.Ice:
					return tile.TileType == TileID.Containers && tile.TileFrameX == 11 * 36;
				case ChestType.Ivy:
					return tile.TileType == TileID.Containers && tile.TileFrameX == 10 * 36;
				case ChestType.Sandstone:
					return tile.TileType == TileID.Containers2 && tile.TileFrameX == 10 * 36;
				case ChestType.Granite:
					return tile.TileType == TileID.Containers && tile.TileFrameX == 50 * 36;
				case ChestType.Mushroom:
					return tile.TileType == TileID.Containers && tile.TileFrameX == 32 * 36;
					//Containers: Mahogany 8, Webbed 15, Lihzard 16, Mushroom 32, Marble 51
					//Containers2: Dead Man's 4
				default:
					return false;
			}
		}
	}

	public enum ChestType
	{
		SurfaceWooden,
		Gold,
		DungeonLocked,
		Shadow,
		Ivy,
		Sandstone,
		Water,
		LivingTree,
		SkyIsland,
		Ice,
		Granite,
		Mushroom
	}
}