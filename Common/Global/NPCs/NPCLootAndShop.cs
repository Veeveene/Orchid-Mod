using OrchidMod.Content.Alchemist.Weapons.Fire;
using OrchidMod.Content.Alchemist.Weapons.Nature;
using OrchidMod.Content.Alchemist.Weapons.Water;
using OrchidMod.Content.Gambler.Accessories;
using OrchidMod.Content.Gambler.Weapons.Cards;
using OrchidMod.Common.ItemDropRules.Conditions;
using OrchidMod.Utilities;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using OrchidMod.Content.General.Pets;
using static Terraria.ModLoader.ModContent;
using OrchidMod.Content.Guardian.Weapons.Warhammers;
using OrchidMod.Content.Guardian.Weapons.Runes;
using OrchidMod.Content.Guardian.Misc;
using OrchidMod.Content.Guardian.Weapons.Shields;
using OrchidMod.Content.Guardian.Accessories;
using OrchidMod.Common.ModSystems;
using OrchidMod.Content.Guardian.Armors.Misc;
using OrchidMod.Content.Guardian.Weapons.Gauntlets;
using OrchidMod.Content.Guardian.Weapons.Standards;
using OrchidMod.Content.General.Armor.Vanity;
using OrchidMod.Content.Shapeshifter.Accessories;
using OrchidMod.Content.Shapeshifter.Misc;
using OrchidMod.Content.Shapeshifter.Weapons.Warden;
using OrchidMod.Content.Guardian.Weapons.Quarterstaves;
using OrchidMod.Content.Shapeshifter.Weapons.Sage;
using OrchidMod.Content.Gambler.Weapons.Dice;
using OrchidMod.Content.Alchemist.Weapons.Air;
using OrchidMod.Content.Alchemist.Misc.Scrolls;
using OrchidMod.Content.Alchemist.Accessories;
using OrchidMod.Content.Gambler.Misc;
using OrchidMod.Content.Gambler.Decks;
using OrchidMod.Content.Shapeshifter.Weapons.Predator;
using OrchidMod.Content.Guardian.Weapons.Misc;
using OrchidMod.Content.Guardian.Weapons.Katars;

namespace OrchidMod.Common.Global.NPCs
{
	public class NPCLootAndShop : GlobalNPC
	{
		public override void ModifyShop(NPCShop shop)
		{
			switch (shop.NpcType)
			{
				case NPCID.Demolitionist:
					{
						shop.Add(ItemType<GunpowderFlask>(), OrchidConditions.EnableContentAlchemist);
					}
					break;
				case NPCID.Cyborg:
					{
						shop.Add(ItemType<NanitesGauntlet>());
					}
					break;
				case NPCID.Dryad:
					{
						shop.Add(ItemType<ShapeshifterBlankEffigy>(), OrchidConditions.EnableContentShapeshifter);
					}
					break;
				case NPCID.Merchant:
					{
						shop.Add(ItemType<GuardianBuffStation>(), [Condition.Hardmode]);
						shop.Add(ItemType<JewelerGauntlet>(), [Condition.DownedEyeOfCthulhu, OrchidConditions.DisabledThorium]);
					}
					break;
				case NPCID.SkeletonMerchant:
					{
						shop.Add(ItemType<GuardianGitHelm>());
						shop.Add(ItemType<GuideTorches>(), OrchidConditions.EnableContentShapeshifter);
					}
					break;
				case NPCID.Clothier:
					{
						shop.Add(ItemType<EmpressPlateHead>(), [Condition.DownedEmpressOfLight, Condition.InHallow]);
						shop.Add(ItemType<EmpressPlateChest>(), [Condition.DownedEmpressOfLight, Condition.InHallow]);
						shop.Add(ItemType<EmpressPlateLegs>(), [Condition.DownedEmpressOfLight, Condition.InHallow]);
					}
					break;
				case NPCID.BestiaryGirl:
					{
						shop.Add(ItemType<ShapeshifterShampoo>(), [Condition.BestiaryFilledPercent(30), OrchidConditions.EnableContentShapeshifter]);
					}
					break;
				case NPCID.Stylist:
					{
						shop.Add(ItemType<ShapeshifterHairpin>(), OrchidConditions.EnableContentShapeshifter);
					}
					break;
				case NPCID.GoblinTinkerer:
					{
						shop.Add(ItemType<GoblinSpike>());
					}
					break;
			}

			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod == null) goto SkipThorium;

			if (thoriumMod.IsNPCTypeEquals("Blacksmith", shop.NpcType))
			{
				OrchidUtils.AddItemToShop<JewelerGauntlet>(shop);
				return;
			}

			if (thoriumMod.IsNPCTypeEquals("Cook", shop.NpcType))
			{
				shop.Add(ItemType<ThoriumCookWarhammer>(), [Condition.DownedEyeOfCthulhu]);
				return;
			}

			SkipThorium:
			return;
		}


		public override void SetupTravelShop(int[] shop, ref int nextSlot)
		{
			if (Main.rand.NextBool() && WorldGen.shadowOrbSmashed && ModContent.GetInstance<OrchidServerConfig>().EnableContentShapeshifter)
			{
				OrchidUtils.AddItemToShop<HarnessYouxia>(shop, ref nextSlot);
			}

			if (Main.rand.NextBool())
			{
				if (WorldGen.shadowOrbSmashed) OrchidUtils.AddItemToShop<Skateboard>(shop, ref nextSlot);
			}
			else if (ModContent.GetInstance<OrchidServerConfig>().EnableContentGambler)
			{
				OrchidUtils.AddItemToShop<PileOfChips>(shop, ref nextSlot);
			}

			if (Main.hardMode)
			{
				if (Main.rand.NextBool())
				{
					OrchidUtils.AddItemToShop<BijouShield>(shop, ref nextSlot);
				}
				else
				{
					OrchidUtils.AddItemToShop<HockeyQuarterstaff>(shop, ref nextSlot);
				}
			}
		}

		public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
		{
			Mod thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				if (thoriumMod.IsNPCTypeEquals("Tracker", npc.type))
				{
					//int runeWizardMirrorType = thoriumMod.Find<ModItem>("WormHoleMirror").Type; //i considered adding rune rune here
					int spiderWingsType = thoriumMod.Find<ModItem>("DridersGrace").Type;
					int trackerType = thoriumMod.Find<ModNPC>("Tracker").Type;
					foreach (Item item in items)
					{
						if (item != null)
						{
							if (item.type == spiderWingsType)
							{
								/*
								if (NPCShopDatabase.TryGetNPCShop(NPCShopDatabase.GetShopName(trackerType, "Shop"), out AbstractNPCShop shop))
								{

								}
								*/

								for (int i = 0; i < items.Length - 1; i++)
								{
									if (items[i] == null)
									{
										items[i] = new Item(ModContent.ItemType<GuardianNeedle>(), 1, 0);
										items[i].shopCustomPrice = 8; // same as Drider's Grace
										items[i].shopSpecialCurrency = item.shopSpecialCurrency;
										//Main.NewText("ping");
										break;
									}
								}
								break;
							}
						}
					}
				}
			}
		}

		public override void ModifyGlobalLoot(GlobalLoot globalLoot)
		{
			// globalLoot.Add(new ItemDropWithConditionRule(ItemType<ShroomKey>(), 2500, 1, 1, new OrchidDropConditions.ShroomKeyCondition(), 1));
		}

		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
		{
			IItemDropRuleCondition EnableContentGambler = new OrchidDropConditions.EnableContentGambler();
			IItemDropRuleCondition EnableContentAlchemist = new OrchidDropConditions.EnableContentAlchemist();
			IItemDropRuleCondition EnableContentShapeshifter = new OrchidDropConditions.EnableContentShapeshifter();
			IItemDropRuleCondition EnableContentGamblerNotExpert = new OrchidDropConditions.EnableContentGamblerNotExpert();
			IItemDropRuleCondition EnableContentAlchemistNotExpert = new OrchidDropConditions.EnableContentAlchemistNotExpert();
			IItemDropRuleCondition EnableContentShapeshifterNotExpert = new OrchidDropConditions.EnableContentShapeshifterNotExpert();
			IItemDropRuleCondition EnableContentAlchemistHitAlchemist = new OrchidDropConditions.EnableContentAlchemistHitAlchemist();
			IItemDropRuleCondition ThoriumDisabled = new OrchidDropConditions.ThoriumDisabled();
			IItemDropRuleCondition ThoriumDisabledGambler = new	OrchidDropConditions.ThoriumDisabledEnableContentGambler();
			IItemDropRuleCondition NotExpert = new Conditions.NotExpert();
			IItemDropRuleCondition IsExpert = new Conditions.IsExpert();

			// GENERIC NPCs

			switch (npc.type)
			{
				case NPCID.Vulture:
					{
						npcLoot.Add(ItemDropRule.ByCondition(ThoriumDisabledGambler, ItemType<VultureTalon>(), 1, 1, 2));
					}
					break;
				case NPCID.SpikedJungleSlime:
					{
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentGambler, ItemType<JungleSlimeCard>(), 25));
					}
					break;
				case NPCID.LavaSlime:
					{
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentGambler, ItemType<LavaSlimeCard>(), 25));
					}
					break;
				case NPCID.Demon:
					{
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentGambler, ItemType<DemonicPocketMirror>(), 20));
					}
					break;
				case NPCID.DarkCaster:
					{
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentGambler, ItemType<DungeonCard>(), 25));
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentShapeshifter, ItemType<DeepwaterLocket>(), 20));
					}
					break;
				case NPCID.RuneWizard:
					{
						npcLoot.Add(ItemDropRule.Common(ItemType<RuneRune>()));
					}
					break;
				case NPCID.ElfCopter:
					{
						npcLoot.Add(ItemDropRule.Common(ItemType<RCRemote>(), 50));
					}
					break;
				case NPCID.Hornet:
				case NPCID.HornetFatty:
				case NPCID.HornetHoney:
				case NPCID.HornetLeafy:
				case NPCID.HornetSpikey:
				case NPCID.HornetStingy:
					{
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentAlchemist, ItemType<PoisonVial>(), 20));
					}
					break;
				case NPCID.GoblinWarrior:
					{
						npcLoot.Add(ItemDropRule.Common(ItemType<GoblinSpike>(), 15));
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentGambler, ItemType<GoblinArmyCard>(), 50));
					}
					break;
				case NPCID.GoblinThief:
					{
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentShapeshifter, ItemType<GoblinDagger>(), 15));
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentAlchemist, ItemType<GoblinArmyFlask>(), 50));
						break;
					}
				case NPCID.GoblinPeon:
				case NPCID.GoblinArcher:
					{
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentAlchemist, ItemType<GoblinArmyFlask>(), 50));
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentGambler, ItemType<GoblinArmyCard>(), 50));
					}
					break;
				case NPCID.GoblinSorcerer:
					{
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentShapeshifter, ItemType<PredatorGoblin>(), 15));
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentGambler, ItemType<GoblinArmyCard>(), 50));
						npcLoot.Add(ItemDropRule.Common(ItemType<GoblinRune>(), 10));
					}
					break;
				case NPCID.Drippler:
				case NPCID.BloodZombie:
					{
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentAlchemist, ItemType<BloodMoonFlask>(), 40));
					}
					break;
				case NPCID.PirateCorsair:
				case NPCID.PirateDeadeye:
				case NPCID.PirateDeckhand:
				case NPCID.PirateCrossbower:
					npcLoot.Add(ItemDropRule.Common(ItemType<PirateWarhammer>(), 100));
					break;
				case NPCID.PirateCaptain:
					{
						npcLoot.Add(ItemDropRule.Common(ItemType<PirateWarhammer>(), 20));
						npcLoot.Add(ItemDropRule.Common(ItemType<PirateStandard>(), 10));
					}
					break;
				case NPCID.MartianSaucerCore:
					{
						npcLoot.Add(ItemDropRule.Common(ItemType<MartianWarhammer>(), 4));
					}
					break;
				case NPCID.Paladin:
					{
						npcLoot.Add(ItemDropRule.ExpertGetsRerolls(ItemType<PaladinGauntlet>(), 10, 2));
					}
					break;
				case NPCID.DevourerHead:
					{
						npcLoot.Add(ItemDropRule.Common(ItemType<ColossalWormTooth>(), 10));
					}
					break;
				case NPCID.FaceMonster:
					{
						npcLoot.Add(ItemDropRule.Common(ItemType<TerrifyingMonsterFang>(), 10));
					}
					break;
				case NPCID.ShortBones:
				case NPCID.AngryBones:
				case NPCID.BigBoned:
				case NPCID.AngryBonesBig:
				case NPCID.AngryBonesBigHelmet:
				case NPCID.AngryBonesBigMuscle:
					{
						npcLoot.Add(ItemDropRule.Common(ItemType<BadgeBattlesPast>(), 50));
					}
					break;
				case NPCID.WallCreeper:
				case NPCID.WallCreeperWall:
					{
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentShapeshifter, ItemType<WardenSpider>(), 15));
					}
					break;
				case NPCID.CaveBat:
				case NPCID.IceBat:
				case NPCID.JungleBat:
				case NPCID.Hellbat:
				case NPCID.SporeBat:
					{
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentShapeshifter, ItemType<SageBat>(), 100));
					}
					break;
				case NPCID.SporeSkeleton:
					{
						npcLoot.Add(ItemDropRule.Common(ItemType<GlowingMushroomGauntlet>(), 40));
					}
					break;
				case NPCID.ManEater:
				case NPCID.Snatcher:
					{
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentShapeshifter, ItemType<PlantEnzymes>(), 20));
						break;
					}
				case NPCID.GraniteFlyer:
					{
						npcLoot.Add(new DropBasedOnExpertMode(ItemDropRule.DropNothing(), ItemDropRule.Common(ItemType<SturdySlab>(), 40)));
						break;
					}
				case NPCID.GraniteGolem:
					{
						npcLoot.Add(ItemDropRule.NormalvsExpert(ItemType<SturdySlab>(), 40, 20));
						break;
					}
				case NPCID.GreekSkeleton:
					{
						npcLoot.Add(ItemDropRule.NormalvsExpert(ItemType<BadgeHoplite>(), 40, 20));
						break;
					}
				case NPCID.UndeadViking:
					{
						npcLoot.Add(ItemDropRule.Common(ItemType<IceAxeHammer>(), 40));
					}
					break;
				default:
					break;
			}

			// BOSSES & MINIBOSSES

			switch (npc.type)
			{
				case NPCID.LunarTowerSolar:
				case NPCID.LunarTowerStardust:
				case NPCID.LunarTowerNebula:
				case NPCID.LunarTowerVortex:
					{
						DropOneByOne.Parameters classicParameters = new()
						{
							ChanceNumerator = 1,
							ChanceDenominator = 1,
							MinimumStackPerChunkBase = 1,
							MaximumStackPerChunkBase = 3,
							MinimumItemDropsCount = 3, //one quarter of vanilla fragments, 12
							MaximumItemDropsCount = 5, //one quarter of vanilla fragments, 20
						};
						DropOneByOne.Parameters expertParameters = classicParameters with { MaximumStackPerChunkBase = 4, BonusMinDropsPerChunkPerPlayer = 1, BonusMaxDropsPerChunkPerPlayer = 1 };
						npcLoot.Add(new DropBasedOnExpertMode(new DropOneByOne(ItemType<HorizonFragment>(), classicParameters), new DropOneByOne(ItemType<HorizonFragment>(), expertParameters)));
					}
					break;
				case NPCID.KingSlime:
					{
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentShapeshifterNotExpert, ItemType<WardenSlime>(), 3));
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentGamblerNotExpert, ItemType<KingSlimeCard>(), 3));
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentAlchemistNotExpert, ItemType<KingSlimeFlask>(), 3));
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentAlchemistHitAlchemist, ItemType<ScrollTier1>()));
					}
					break;
				case NPCID.EyeofCthulhu:
					{
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentGamblerNotExpert, ItemType<EyeCard>(), 3));
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentAlchemistHitAlchemist, ItemType<ScrollTier2>()));
					}
					break;
				case NPCID.BrainofCthulhu:
					{
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentGamblerNotExpert, ItemType<BrainCard>(), 3));
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentAlchemistNotExpert, ItemType<PreservedCrimson>(), 3));
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentAlchemistHitAlchemist, ItemType<ScrollTier1>()));
					}
					break;
				case NPCID.SkeletronHead:
					{
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentGamblerNotExpert, ItemType<SkeletronCard>(), 3));
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentAlchemistHitAlchemist, ItemType<ScrollTier3>()));
					}
					break;
				case NPCID.QueenBee:
					{
						// 25% chance of obtaining either the card or die
						LeadingConditionRule leadingConditionRule = new LeadingConditionRule(NotExpert);
						leadingConditionRule.OnSuccess(ItemDropRule.OneFromOptionsNotScalingWithLuck(2, ItemType<QueenBeeCard>(), ItemType<HoneyDie>()));
						npcLoot.Add(leadingConditionRule);

						npcLoot.Add(ItemDropRule.ByCondition(EnableContentAlchemistNotExpert, ItemType<QueenBeeFlask>(), 3));
						npcLoot.Add(ItemDropRule.ByCondition(NotExpert, ItemType<BeeRune>(), 3));
					}
					break;
				case NPCID.MoonLordCore:
					{
						// 50% chance of obtaining either, one is guaranteed
						LeadingConditionRule leadingConditionRule = new LeadingConditionRule(NotExpert);
						leadingConditionRule.OnSuccess(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemType<MoonLordRune>(), ItemType<MoonLordShield>()));
						npcLoot.Add(leadingConditionRule);
					}
					break;
				case NPCID.WallofFlesh:
					{
						LeadingConditionRule leadingConditionRule = new LeadingConditionRule(NotExpert);
						leadingConditionRule.OnSuccess(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemType<GuardianEmblem>(), ItemType<ShapeshifterEmblem>()));
						npcLoot.Add(leadingConditionRule);
					}
					break;
				case NPCID.PirateShip:
					{
						npcLoot.Add(ItemDropRule.ByCondition(EnableContentGambler, ItemType<DeckDog>(), 200).OnFailedRoll(ItemDropRule.ByCondition(EnableContentGambler, ItemType<DeckPirate>(), 10)));
						npcLoot.Add(ItemDropRule.OneFromOptions(2, ItemType<PirateWarhammer>(), ItemType<PirateStandard>()));
					}
					break;
				case NPCID.BigMimicHallow:
					{
						npcLoot.Add(ItemDropRule.Common(ItemType<CrystalGauntlet>(), 2));
					}
					break;
				case NPCID.QueenSlimeBoss:
					{
						npcLoot.Add(ItemDropRule.ByCondition(NotExpert, ItemType<GuardianCrystalNinjaHelm>(), 2));
					}
					break;
				case NPCID.Plantera:
					{
						npcLoot.Add(ItemDropRule.ByCondition(NotExpert, ItemType<PlanteraStandard>(), 3));
					}
					break;
				case NPCID.Golem:
					{
						npcLoot.Add(ItemDropRule.ByCondition(NotExpert, ItemType<TempleWarhammer>(), 3));
					}
					break;
				case NPCID.HallowBoss:
					{
						npcLoot.Add(ItemDropRule.ByCondition(NotExpert, ItemType<GuardianEmpressMaterial>(), 1, 16, 27)); 
					}
					break;
				case NPCID.Everscream:
					{
						npcLoot.Add(ItemDropRule.Common(ItemType<FrostRune>(), 5));
					}
					break;
				case NPCID.Pumpking:
					{
						npcLoot.Add(ItemDropRule.Common(ItemType<PumpkingWarhammer>(), 5));
					}
					break;
				default:
					break;
			}

			// SPECIAL CASES

			if (System.Array.IndexOf(new int[] { NPCID.EaterofWorldsBody, NPCID.EaterofWorldsHead, NPCID.EaterofWorldsTail }, npc.type) > -1)
			{ // Eater of worlds
				LeadingConditionRule leadingConditionRule = new(new Conditions.LegacyHack_IsABoss());
				leadingConditionRule.OnSuccess(ItemDropRule.ByCondition(EnableContentGamblerNotExpert, ItemType<EaterCard>(), 3));
				leadingConditionRule.OnSuccess(ItemDropRule.ByCondition(EnableContentAlchemistNotExpert, ItemType<PreservedCorruption>(), 3));
				leadingConditionRule.OnSuccess(ItemDropRule.ByCondition(EnableContentAlchemistHitAlchemist, ItemType<ScrollTier2>()));
				npcLoot.Add(leadingConditionRule);
			}

			/*
			if (npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism)
			{ // The Twins
				LeadingConditionRule leadingConditionRule = new LeadingConditionRule(new Conditions.MissingTwin());
				leadingConditionRule.OnSuccess(XXX);
				npcLoot.Add(leadingConditionRule);
			}
			*/

			//THORIUM

			Mod thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				if (thoriumMod.IsNPCTypeEquals("TheGrandThunderBirdv2", npc.type))
				{
					npcLoot.Add(ItemDropRule.ByCondition(NotExpert, ItemType<ThoriumGrandThunderBirdWarhammer>(), 4));
				}

				if (thoriumMod.IsNPCTypeEquals("ThePrimeScouter", npc.type))
				{
					npcLoot.Add(ItemDropRule.ByCondition(NotExpert, ItemType<ThoriumStarScouterStandard>(), 6)); // 1 in 7 is brutal damn
				}

				if (thoriumMod.IsNPCTypeEquals("Viscount", npc.type))
				{
					npcLoot.Add(ItemDropRule.ByCondition(NotExpert, ItemType<ThoriumViscountQuarterstaff>(), 6)); // 1 in 7 is brutal damn
				}

				if (thoriumMod.IsNPCTypeEquals("GraniteEnergyStorm", npc.type))
				{
					npcLoot.Add(ItemDropRule.ByCondition(NotExpert, ItemType<ThoriumGraniteKatar>(), 6)); // 1 in 7 is brutal damn
				}

				if (thoriumMod.IsNPCTypeEquals("Illusionist", npc.type))
				{
					npcLoot.Add(ItemDropRule.Common(ItemType<ThoriumIllusionistPavise>()));
				}

				// TheGrandThunderBirdv2 QueenJelly GraniteEnergyStorm Viscount FallenDeathBeholder BoreanStriderPopped Lich Abyssion PatchWerk
			}
		}

		public override void OnKill(NPC npc)
		{
			OrchidGlobalNPC globalNPC = npc.GetGlobalNPC<OrchidGlobalNPC>();

			if (npc.type == NPCID.BlueSlime || npc.type == -3 || npc.type == -8 || npc.type == -9 || npc.type == -6 || npc.type == NPCID.IceSlime || npc.type == -10)
			{ // Most Surface Slimes
				if (Main.rand.NextBool(!OrchidWorld.foundSlimeCard ? 5 : 1000) && ModContent.GetInstance<OrchidServerConfig>().EnableContentGambler)
				{
					Item.NewItem(npc.GetSource_Loot(), (int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemType<SlimeCard>());
					OrchidWorld.foundSlimeCard = true;
				}
			}
		}
	}
}