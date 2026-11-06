using OrchidMod.Content.Alchemist;
using OrchidMod.Content.Gambler;
using OrchidMod.Content.Shapeshifter;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System.Linq;
using System;

namespace OrchidMod.Common.ModSystems
{
	public class OrchidRecipes : ModSystem
	{
		public static int AnyCopperBarGroup { get; private set; }

		public static int AnySilverBarGroup { get; private set; }

		public static int AnyGoldBarGroup { get; private set; }

		public static int AnyDemoniteBarGroup { get; private set; }

		public static int AnyCobaltBarGroup { get; private set; }

		public static int AnyMythrilBarGroup { get; private set; }

		public static int AnyAdamantiteBarGroup { get; private set; }

		public static LocalizedText RecipeGroupAnyText { get; private set; }

		public override void OnModLoad()
		{
			string str = "RecipeGroups.";
			if (RecipeGroupAnyText != null)
				return;
			RecipeGroupAnyText = Mod.GetLocalization(str + "Any");
		}


		public override void AddRecipeGroups()
		{
			string any = Language.GetTextValue("LegacyMisc.37");

			var thoriumMod = OrchidMod.ThoriumMod;
			if (OrchidMod.ThoriumMod != null)
			{
				thoriumMod.TryFind("MeleeThorHammer", out ModItem thorsHammerMelee);
				thoriumMod.TryFind("RangedThorHammer", out ModItem thorsHammerRanged);
				thoriumMod.TryFind("MagicThorHammer", out ModItem thorsHammerMagic);
				
				RecipeGroup group = new(() => $"{any} " + thorsHammerMelee.DisplayName.ToString().Split(':').First(), thorsHammerMelee.Type, thorsHammerRanged.Type, thorsHammerMagic.Type);
				RecipeGroup.RegisterGroup("ThorsHammers", group);
			}

			// Taken from the Thorium Mod
			AnyCopperBarGroup = RecipeGroup.RegisterGroup("CopperBar", new RecipeGroup(()
				=> RecipeGroupAnyText.Format(any, Lang.GetItemNameValue(ItemID.CopperBar)), ItemID.CopperBar, ItemID.TinBar));

			AnySilverBarGroup = RecipeGroup.RegisterGroup("SilverBar", new RecipeGroup(()
				=> RecipeGroupAnyText.Format(any, Lang.GetItemNameValue(ItemID.SilverBar)), ItemID.SilverBar, ItemID.TungstenBar));

			AnyGoldBarGroup = RecipeGroup.RegisterGroup("GoldBar", new RecipeGroup(()
				=> RecipeGroupAnyText.Format(any, Lang.GetItemNameValue(ItemID.GoldBar)), ItemID.GoldBar, ItemID.PlatinumBar));

			AnyDemoniteBarGroup = RecipeGroup.RegisterGroup("DemoniteBar", new RecipeGroup(()
				=> RecipeGroupAnyText.Format(any, Lang.GetItemNameValue(ItemID.DemoniteBar)), ItemID.DemoniteBar, ItemID.CrimtaneBar));

			AnyCobaltBarGroup = RecipeGroup.RegisterGroup("CobaltBar", new RecipeGroup(()
				=> RecipeGroupAnyText.Format(any, Lang.GetItemNameValue(ItemID.CobaltBar)), ItemID.CobaltBar, ItemID.PalladiumBar));

			AnyMythrilBarGroup = RecipeGroup.RegisterGroup("MythrilBar", new RecipeGroup(()
				=> RecipeGroupAnyText.Format(any, Lang.GetItemNameValue(ItemID.MythrilBar)), ItemID.MythrilBar, ItemID.OrichalcumBar));

			AnyAdamantiteBarGroup = RecipeGroup.RegisterGroup("AdamantiteBar", new RecipeGroup(()
				=> RecipeGroupAnyText.Format(any, Lang.GetItemNameValue(ItemID.AdamantiteBar)), ItemID.AdamantiteBar, ItemID.TitaniumBar));
		}

		public override void PostAddRecipes()
		{
			bool ContentAlchemist = ModContent.GetInstance<OrchidServerConfig>().EnableContentAlchemist;
			bool ContentGambler = ModContent.GetInstance<OrchidServerConfig>().EnableContentGambler;
			bool ContentShapeshifter = ModContent.GetInstance<OrchidServerConfig>().EnableContentShapeshifter;

			if (!ContentAlchemist || !ContentGambler || !ContentShapeshifter)
			{
				for (int i = 0; i < Recipe.numRecipes; i++)
				{
					Recipe recipe = Main.recipe[i];
					if (!ContentAlchemist && recipe.createItem.ModItem is OrchidModAlchemistItem or OrchidModAlchemistMisc or OrchidModAlchemistEquipable)
					{
						recipe.DisableRecipe();
					}

					if (!ContentGambler && recipe.createItem.ModItem is OrchidModGamblerEquipable or OrchidModGamblerDie or OrchidModGamblerChipItem or GamblerDeck)
					{
						recipe.DisableRecipe();
					}

					if (!ContentShapeshifter && recipe.createItem.ModItem is OrchidModShapeshifterItem)
					{
						recipe.DisableRecipe();
					}
				}
			}
		}
	}
}