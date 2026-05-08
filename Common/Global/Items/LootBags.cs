using OrchidMod.Content.Alchemist.Accessories;
using OrchidMod.Content.Alchemist.Weapons.Air;
using OrchidMod.Content.Alchemist.Weapons.Catalysts;
using OrchidMod.Content.Alchemist.Weapons.Fire;
using OrchidMod.Content.Alchemist.Weapons.Water;
using OrchidMod.Content.General.Materials;
using OrchidMod.Content.Gambler.Accessories;
using OrchidMod.Content.Gambler.Decks;
using OrchidMod.Content.Gambler.Misc;
using OrchidMod.Content.Gambler.Weapons.Cards;
using OrchidMod.Content.Gambler.Weapons.Chips;
using OrchidMod.Content.Gambler.Weapons.Dice;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using OrchidMod.Content.General.Armor.Vanity;
using OrchidMod.Content.Guardian.Weapons.Warhammers;
using OrchidMod.Content.Guardian.Weapons.Runes;
using OrchidMod.Content.Guardian.Misc;
using OrchidMod.Content.Guardian.Accessories;
using OrchidMod.Content.Guardian.Weapons.Standards;
using OrchidMod.Content.Guardian.Weapons.Shields;
using OrchidMod.Content.General.Mounts;
using OrchidMod.Content.General.Melee;
using OrchidMod.Content.Shapeshifter.Weapons.Warden;
using OrchidMod.Content.Guardian.Weapons.Gauntlets;
using OrchidMod.Content.Shapeshifter.Weapons.Predator;
using OrchidMod.Content.Guardian.Weapons.Quarterstaves;
using OrchidMod.Utilities;
using OrchidMod.Content.Shapeshifter.Accessories;
using OrchidMod.Content.Shapeshifter.Misc;
using OrchidMod.Content.Guardian.Armors.Misc;
using OrchidMod.Content.Guardian.Weapons.Katars;

namespace OrchidMod.Common.Global.Items
{
	public partial class OrchidGlobalItem
	{
		/*
		public override void OpenVanillaBag(string context, Player player, int arg)
		{
			switch (context)
			{
				case "bossBag": OpenBossBag(player, arg); break;
				case "crate": OpenCrate(player, arg); break;
				case "lockBox": OpenGoldenLockBox(player); break;
			}
		}
		*/

		// temp ..

		public override void RightClick(Item item, Player player)
		{
			OpenBossBag(player, item.type);
			OpenCrate(player, item.type);
			if (item.type == ItemID.LockBox) OpenGoldenLockBox(player);
		}

		// ...

		private static void OpenBossBag(Player player, int arg)
		{
			bool EnableContentGambler = ModContent.GetInstance<OrchidServerConfig>().EnableContentGambler;
			bool EnableContentAlchemist = ModContent.GetInstance<OrchidServerConfig>().EnableContentAlchemist;
			bool EnableContentShapeshifter = ModContent.GetInstance<OrchidServerConfig>().EnableContentShapeshifter;

			switch (arg)
			{
				case ItemID.KingSlimeBossBag:
					{
						if (EnableContentShapeshifter) QuickSpawnItem<WardenSlime>(player, 1, 3);
						if (EnableContentAlchemist) QuickSpawnItem<KingSlimeFlask>(player, 1, 3);
						if (EnableContentGambler) QuickSpawnItem<KingSlimeCard>(player, 1, 3);
					}
					break;
				case ItemID.EyeOfCthulhuBossBag:
					{
						if (EnableContentGambler) QuickSpawnItem<EyeCard>(player, 1, 3);
						if (Main.rand.NextBool(20))
						{
							QuickSpawnItem<SquareMinecart>(player);
							QuickSpawnItem<PrototypeSecrecy>(player);
						}
					}
					break;
				case ItemID.EaterOfWorldsBossBag:
					{
						if (EnableContentAlchemist) QuickSpawnItem<PreservedCorruption>(player, 1, 3);
						if (EnableContentGambler) QuickSpawnItem<EaterCard>(player, 1, 3);
					}
					break;
				case ItemID.BrainOfCthulhuBossBag:
					{
						if (EnableContentAlchemist) QuickSpawnItem<PreservedCrimson>(player, 1, 3);
						if (EnableContentGambler) QuickSpawnItem<BrainCard>(player, 1, 3);
					}
					break;
				case ItemID.QueenBeeBossBag:
					{
						if (EnableContentGambler)
						{
							QuickSpawnRandomItemFromList(
								player: player,
								items: new()
								{
								(ModContent.ItemType<QueenBeeCard>(), 1),
								(ModContent.ItemType<HoneyDie>(), 1)
								},
								chanceDenominator: 2
							);
						}
						if (EnableContentAlchemist) QuickSpawnItem<QueenBeeFlask>(player, 1, 3);
						QuickSpawnItem<BeeRune>(player, 1, 3);
					}
					break;
				case ItemID.SkeletronBossBag:
					{
						if (EnableContentGambler) QuickSpawnItem<SkeletronCard>(player, 1, 1);
					}
					break;
				case ItemID.WallOfFleshBossBag:
					{
						QuickSpawnRandomItemFromList(
							player: player,
							items: new()
							{
								(ModContent.ItemType<ShapeshifterEmblem>(), 1),
								(ModContent.ItemType<GuardianEmblem>(), 1)
							}
						);
					}
					break;
				case ItemID.QueenSlimeBossBag:
					{
						QuickSpawnItem<GuardianCrystalNinjaHelm>(player, 1);
					}
					break;
				case ItemID.PlanteraBossBag:
					{
						QuickSpawnItem<OrnateOrchid>(player, 1, 20);
						QuickSpawnItem<PlanteraStandard>(player, 1, 3);
					}
					break;
				case ItemID.GolemBossBag:
					{
						QuickSpawnItem<TempleWarhammer>(player, 1, 3);
					}
					break;
				case ItemID.FairyQueenBossBag:
					{
						QuickSpawnItem<GuardianEmpressMaterial>(player, 20 + Main.rand.Next(10), 1);
					}
					break;
				case ItemID.MoonLordBossBag:
					{ // Vanilla is 2 random items from the loot pool
						QuickSpawnRandomItemFromList(
							player: player,
							items: new()
							{
								(ModContent.ItemType<MoonLordRune>(), 1),
								(ModContent.ItemType<MoonLordShield>(), 1)
							}
						);
					}
					break;
				default:
					break;
			}

			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod == null) goto SkipThorium;

			if (thoriumMod.IsItemTypeEquals("ThunderBirdBag", arg))
			{
				QuickSpawnItem<ThoriumGrandThunderBirdWarhammer>(player, 1, 2);
				return;
			}

			if (thoriumMod.IsItemTypeEquals("ScouterBag", arg))
			{
				QuickSpawnItem<ThoriumStarScouterStandard>(player, 1, 3);
				return;
			}

			if (thoriumMod.IsItemTypeEquals("CountBag", arg))
			{
				QuickSpawnItem<ThoriumViscountQuarterstaff>(player, 1, 3);
				return;
			}

			if (thoriumMod.IsItemTypeEquals("GraniteBag", arg))
			{
				QuickSpawnItem<ThoriumGraniteKatar>(player, 1, 3);
				return;
			}

			// ThunderBirdBag JellyFishBag GraniteBag CountBag BeholderBag BoreanBag LichBag AbyssionBag

			SkipThorium:
			return;
		}

		private static void OpenCrate(Player player, int arg)
		{
			bool EnableContentGambler = ModContent.GetInstance<OrchidServerConfig>().EnableContentGambler;
			bool EnableContentAlchemist = ModContent.GetInstance<OrchidServerConfig>().EnableContentAlchemist;
			bool EnableContentShapeshifter = ModContent.GetInstance<OrchidServerConfig>().EnableContentShapeshifter;

			switch (arg)
			{
				case ItemID.Present:
					QuickSpawnItem<PresentQuarterstaff>(player, 1, 150);
					break;
				case ItemID.WoodenCrate:
				case ItemID.WoodenCrateHard:
					{
						if (EnableContentAlchemist) QuickSpawnItem<EmberVial>(player, 1, 20);
						if (EnableContentAlchemist) QuickSpawnItem<TsunamiInAVial>(player, 1, 20);
						if (EnableContentGambler) QuickSpawnItem<EmbersCard>(player, 1, 20);
						if (EnableContentShapeshifter) QuickSpawnItem<WardenSnail>(player, 1, 20);
						QuickSpawnItem<Quarterstaff>(player, 1, 20);
						QuickSpawnItem<GuideShield>(player, 1, 20);
					}
					break;
				case ItemID.IronCrate:
				case ItemID.IronCrateHard:
					{
						if (EnableContentAlchemist) QuickSpawnItem<EmberVial>(player, 1, 10);
						if (EnableContentAlchemist) QuickSpawnItem<TsunamiInAVial>(player, 1, 10);
						if (EnableContentGambler) QuickSpawnItem<EmbersCard>(player, 1, 10);
						QuickSpawnItem<Quarterstaff>(player, 1, 10);
						QuickSpawnItem<GuideShield>(player, 1, 10);
					}
					break;
				case ItemID.CorruptFishingCrate:
				case ItemID.CorruptFishingCrateHard:
					if (EnableContentAlchemist) QuickSpawnItem<DemoniteCatalyst>(player, 1, 5);
					break;
				case ItemID.CrimsonFishingCrate:
				case ItemID.CrimsonFishingCrateHard:
					if (EnableContentAlchemist) QuickSpawnItem<CrimtaneCatalyst>(player, 1, 5);
					break;
				case ItemID.JungleFishingCrate:
				case ItemID.JungleFishingCrateHard:
					{
						if (EnableContentAlchemist) QuickSpawnItem<JungleLily>(player, 1, 2);
						if (EnableContentAlchemist) QuickSpawnItem<BloomingBud>(player, 1, 5);
						if (EnableContentGambler) QuickSpawnItem<DeckJungle>(player, 1, 20);
						if (EnableContentGambler) QuickSpawnItem<IvyChestCard>(player, 1, 5);
						if (EnableContentGambler) QuickSpawnItem<BundleOfClovers>(player, 1, 5);
						if (EnableContentShapeshifter) QuickSpawnItem<WardenTortoise>(player, 1, 5);
						QuickSpawnItem<GuardianHoneyPotion>(player, 1, 2);
						QuickSpawnItem<JungleGauntlet>(player, 1, 5);
					}
					break;
				case ItemID.FloatingIslandFishingCrate:
				case ItemID.FloatingIslandFishingCrateHard:
					if (EnableContentAlchemist) QuickSpawnItem<SunplateFlask>(player, 1, 5);
					if (EnableContentShapeshifter) QuickSpawnItem<ShawlFeather>(player, 1, 5);
					if (EnableContentShapeshifter) QuickSpawnItem<ShapeshifterSurvivalPotion>(player, 1, 2);
					QuickSpawnItem<SkywareShield>(player, 1, 5);
					break;
				case ItemID.OasisCrate:
				case ItemID.OasisCrateHard:
					{
						QuickSpawnItem<DesertStandard>(player, 1, 4);
						/*QuickSpawnRandomItemFromList(
							player: player,
							items: new()
							{ // Keeping this, mostly as an example for the future
								(ModContent.ItemType<DesertWarhammer>(), 1),
								(ModContent.ItemType<DesertStandard>(), 1),
							},
							chanceDenominator: 2
						);*/
					}
					break;
				case ItemID.LavaCrate:
				case ItemID.LavaCrateHard:
					{
						if (EnableContentAlchemist) QuickSpawnItem<ShadowChestFlask>(player, 1, 5);
						if (EnableContentAlchemist) QuickSpawnItem<KeystoneOfTheConvent>(player, 1, 5);
						if (EnableContentGambler) QuickSpawnItem<ImpDiceCup>(player, 1, 5);
						if (EnableContentShapeshifter) QuickSpawnItem<WardenSalamortar>(player, 1, 5);
						QuickSpawnItem<NightShield>(player, 1, 5);
						QuickSpawnItem<HellRune>(player, 1, 5);
						QuickSpawnItem<GuardianRunePotion>(player, 1, 2);
					}
					break;

				case ItemID.FrozenCrate:
				case ItemID.FrozenCrateHard:
					{
						if (EnableContentAlchemist) QuickSpawnItem<BlizzardInAVial>(player, 1, 5);
						if (EnableContentAlchemist) QuickSpawnItem<IceChestFlask>(player, 1, 5);
						if (EnableContentShapeshifter) QuickSpawnItem<PredatorIceFox>(player, 1, 5);
						QuickSpawnItem<IceStandard>(player, 1, 5);
					}
					break;
				default:
					break;
			}
		}

		private static void OpenGoldenLockBox(Player player)
		{
			bool EnableContentGambler = ModContent.GetInstance<OrchidServerConfig>().EnableContentGambler;
			bool EnableContentAlchemist = ModContent.GetInstance<OrchidServerConfig>().EnableContentAlchemist;
			bool EnableContentShapeshifter = ModContent.GetInstance<OrchidServerConfig>().EnableContentShapeshifter;

			if (EnableContentAlchemist) QuickSpawnItem<DungeonFlask>(player, 1, 5);
			if (EnableContentAlchemist) QuickSpawnItem<DungeonCatalyst>(player, 1, 5);
			if (EnableContentGambler) QuickSpawnItem<Rusalka>(player, 1, 5);
			if (EnableContentGambler) QuickSpawnItem<TiamatRelic>(player, 1, 2);
			if (EnableContentGambler) QuickSpawnItem<DeckBone>(player, 1, 40);
			if (EnableContentShapeshifter) QuickSpawnItem<PredatorUndine>(player, 1, 5);
			QuickSpawnItem<DungeonQuarterstaff>(player, 1, 5);
		}

		/* // For some reason, opening an obsidian lockbox doesn't call this method.
		private static void OpenLockBox(Player player, int arg)
		{
			switch (arg)
			{
				case ItemID.LockBox:
					QuickSpawnRandomItemFromList(
						player: player,
						items: new()
						{
							(ModContent.ItemType<SpiritedWater>(), 1),
							(ModContent.ItemType<DungeonFlask>(), 1),
							(ModContent.ItemType<DungeonCatalyst>(), 1),
							(ModContent.ItemType<Rusalka>(), 1)
						},
						chanceDenominator: 2
					);
					QuickSpawnItem<TiamatRelic>(player, 1, 2);
					QuickSpawnItem<DeckBone>(player, 1, 40);
					break;
				case ItemID.ObsidianLockbox:
					QuickSpawnRandomItemFromList(
						player: player,
						items: new()
						{
							(ModContent.ItemType<ShadowChestFlask>(), 1),
							(ModContent.ItemType<FireBatScepter>(), 1),
							(ModContent.ItemType<KeystoneOfTheConvent>(), 1),
							(ModContent.ItemType<ImpDiceCup>(), 1)
						},
						chanceDenominator: 2
					);
					break;
			}
		}
		*/

		private static void QuickSpawnRandomItemFromList(Player player, List<(int type, int stack)> items, int chanceDenominator = 1)
		{
			if (items == null || items.Count == 0) return;

			(int type, int stack) = items[Main.rand.Next(items.Count)];

			QuickSpawnItem(player, type, stack, chanceDenominator);
		}

		private static void QuickSpawnItem<T>(Player player, int stack = 1, int chanceDenominator = 1) where T : ModItem
			=> QuickSpawnItem(player, ModContent.ItemType<T>(), stack, chanceDenominator);

		private static void QuickSpawnItem(Player player, int type, int stack = 1, int chanceDenominator = 1)
		{
			if (!Main.rand.NextBool(chanceDenominator)) return;

			player.QuickSpawnItem(player.GetSource_OpenItem(type), type, stack);
		}
	}
}