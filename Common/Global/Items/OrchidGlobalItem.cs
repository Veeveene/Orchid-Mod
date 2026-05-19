using OrchidMod.Common.Attributes;
using OrchidMod.Content.Guardian;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Localization;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using MonoMod.RuntimeDetour;
using OrchidMod.Content.Guardian.Weapons.Warhammers;

namespace OrchidMod.Common.Global.Items
{
	public partial class OrchidGlobalItem : GlobalItem, IPostSetupContent
	{
		private static readonly Dictionary<int, ClassTags> tagsByItemType = new();

		// ...

		void IPostSetupContent.PostSetupContent(Mod mod)
		{
			foreach (var modItem in mod.GetContent<ModItem>())
			{
				var sacrificeCountDict = CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId;

				if (modItem.Mod.Equals(mod) && !sacrificeCountDict.ContainsKey(modItem.Type))
				{
					int sacrificeCount = GetAutoSacrificeCount(modItem);

					if (sacrificeCount > 0)
					{
						sacrificeCountDict.Add(modItem.Type, sacrificeCount);
					}
				}

				if (tagsByItemType.ContainsKey(modItem.Type)) continue;

				var atr = modItem.GetType().GetCustomAttribute<ClassTagAttribute>();

				if (atr is null || atr.Tag.Equals(ClassTags.Without)) continue; 

				tagsByItemType.Add(modItem.Type, atr.Tag);
			}
		}


		private static Hook _RightClickHook;
		
		internal delegate void orig_RightClick(Item item, Player player);


		public override void Load()
		{
			MethodInfo m_RightClick = typeof(ItemLoader).GetMethod("RightClick", BindingFlags.Static | BindingFlags.Public);
			if (m_RightClick != null)
				_RightClickHook = new Hook(m_RightClick, Detour_RightClick);

		}

		public override void Unload()
		{
			tagsByItemType.Clear();

			_RightClickHook?.Dispose();
			_RightClickHook = null;
		}

		private void Detour_RightClick(orig_RightClick orig, Item item, Player player)
		{
			if (OrchidMod.ThoriumMod != null && item.type == OrchidMod.ThoriumMod.Find<ModItem>("MagicThorHammer").Type && ModContent.GetInstance<OrchidClientConfig>().GuardianThoriumThorsHammerConversion)
			{
				item.ChangeItemType(ModContent.ItemType<ThoriumThorsHammerWarhammer>());
				return;
			}
			orig(item, player);
		}

		// ...

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (ModContent.GetInstance<OrchidClientConfig>().ShowClassTags && tagsByItemType.ContainsKey(item.type))
			{
				AddClassTagToTooltips(item, tooltips);
			}

			if (ModContent.GetInstance<OrchidServerConfig>().LoadCrossmodContentWithoutRequiredMods && ModContent.GetInstance<OrchidClientConfig>().ShowModTags)
			{
				var atr = item.ModItem?.GetType().GetCustomAttribute<CrossmodContentAttribute>();

				if (atr is not null)
				{
					AddCrossmodInfoToTooltips(item, tooltips, atr.Mods);
				}
			}

			if (OrchidMod.ThoriumMod != null && item.type == OrchidMod.ThoriumMod.Find<ModItem>("MagicThorHammer").Type && ModContent.GetInstance<OrchidClientConfig>().GuardianThoriumThorsHammerConversion)
			{
				var index = tooltips.FindIndex(i => i.Mod.Equals("Terraria") && i.Name.Equals("Tooltip0"));
				tooltips[index].Text = Language.GetTextValue("Mods.OrchidMod.Misc.ThoriumMagicThorHammer");
			}
		}

		// ...

		private void AddClassTagToTooltips(Item item, List<TooltipLine> tooltips)
		{
			var index = tooltips.FindIndex(i => i.Mod.Equals("Terraria") && i.Name.Equals("ItemName"));

			if (index < 0) return;

			var tagType = tagsByItemType[item.type];

			tooltips.Insert(index + 1, new TooltipLine(Mod, "ClassTag", GetClassTagText(tagType))
			{
				OverrideColor = OrchidColors.GetClassTagColor(tagType)
			});
		}

		private void AddCrossmodInfoToTooltips(Item item, List<TooltipLine> tooltips, string[] mods)
		{
			for (int i = 0; i < mods.Length; i++)
			{
				if (mods[i] == "ThoriumMod")
				{
					mods[i] = "Thorium Mod";
				}
			}

			var text = Language.GetTextValue("Mods.OrchidMod.Misc.Crossmod") + String.Join(", ", mods);

			tooltips.Add(new TooltipLine(Mod, "CrossmodInfo", text)
			{
				OverrideColor = OrchidColors.CrossmodContentWarning
			});

		}

		// ...

		private static string GetClassTagText(ClassTags tag)
			=> Language.GetTextValue("Mods.OrchidMod.DamageClasses." + tag);

		private static int GetAutoSacrificeCount(ModItem modItem)
		{
			// https://terraria.fandom.com/wiki/Journey_Mode#Research

			var item = modItem.Item;
			var itemType = item.type;

			// Heart, star, potency, ...
			if (ItemID.Sets.IsAPickup[itemType]) return 0;

			// How can I put more than 1 item in the slot?
			if (item.maxStack <= 1) return 1;

			// Consumables
			if (item.consumable)
			{
				// Placeables (Walls)
				if (item.createWall > WallID.None) return 400;

				// Placeables (Tiles)
				if (item.createTile > TileID.Dirt)
				{
					var tileType = item.createTile;

					// Platforms
					if (TileID.Sets.Platforms[tileType]) return 200;

					// Furniture
					if (!Main.tileSolid[tileType]) return 1;

					// Ores, blocks, torches, ropes, empty bullets, coins and other...
					return 100;
				}

				// Potions, food, and ...
				if (item.buffType > 0)
				{
					var buffType = item.buffType;

					// Food
					if (BuffID.Sets.IsWellFed[buffType]) return 10;

					// Potions and other...
					return 20;
				}
			}

			// Baits
			if (item.bait > 0) return 5;

			// Dyes
			if (item.dye > 0) return 3;

			// ...
			if (item.damage > 0 || item.accessory || item.useStyle > ItemUseStyleID.None ||
				item.defense > 0 || item.vanity || item.mountType > 0) return 1;

			// If nothing fits the condition, crafting materials?
			return 25;
		}
	}
}
