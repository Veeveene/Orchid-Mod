using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common;
using OrchidMod.Content.General.NPCs.Town;
using OrchidMod.Content.Guardian;
using OrchidMod.Content.Guardian.Weapons.Warhammers;
using OrchidMod.Content.Shapeshifter;
using System;
using System.Collections.Generic;
using System.Reflection;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod
{
	public partial class OrchidMod
	{
		private static Action<Player> GuardianFocus = new (player =>
		{
			player.statDefense += 3;
			player.lifeRegen += 6;
			OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
			guardian.GuardianGuardMax += 3;
			guardian.GuardianSlamMax += 3;
		});

		private void ThoriumModCalls()
		{
			if (ThoriumMod == null || ThoriumMod.Version < new Version(1, 7, 2, 0)) return;
			ThoriumMod.Call("TerrariumArmorAddClassFocus", ModContent.GetInstance<GuardianDamageClass>(), GuardianFocus, OrchidColors.GuardianTag);
			ThoriumMod.Call("AddMartianItemID", ModContent.ItemType<MartianWarhammer>());
		}

		private void CensusModCalls()
		{
			if (!ModLoader.TryGetMod("Census", out Mod censusMod)) return;

			censusMod.Call
			(
				"TownNPCCondition",
				ModContent.NPCType<Croupier>(),
				ModContent.GetInstance<Croupier>().GetLocalization("Census.SpawnCondition")
			);

			censusMod.Call
			(
				"TownNPCCondition",
				ModContent.NPCType<Chemist>(),
				ModContent.GetInstance<Chemist>().GetLocalization("Census.SpawnCondition")
			);
		}

		private static void ColoredDamageTypeModCalls()
		{
			if (!Main.dedServ && ModLoader.TryGetMod("ColoredDamageTypes", out Mod coloreddamagetypes))
			{
				// Colors in order : Tooltip, Damage, Crit
				coloreddamagetypes.Call("AddDamageType", ModContent.GetInstance<GuardianDamageClass>(), (165, 130, 100), (198, 172, 146), (155, 109, 85));
				coloreddamagetypes.Call("AddDamageType", ModContent.GetInstance<ShapeshifterDamageClass>(), (100, 175, 150), (120, 195, 170), (43, 132, 101));
			}
		}

		private static void RecipeBrowserModCalls() 
		{
			if (ModLoader.TryGetMod("RecipeBrowser", out Mod recipeBrowser) && !Main.dedServ) 
			{
				Main.QueueMainThreadAction(() =>
					{
						var utilities = recipeBrowser.Code.GetType("RecipeBrowser.Utilities");
						var method = utilities?.GetMethod("ResizeImage", BindingFlags.Static | BindingFlags.NonPublic);
						if (method != null)
						{
							// Call RecipeBrowser's "ResizeImage" method to scale down Grond to a 24x24 asset
							Asset<Texture2D> classWeaponIcon = (Asset<Texture2D>)method?.Invoke(null, [ModContent.Request<Texture2D>("OrchidMod/Content/Guardian/Weapons/Warhammers/GoldWarhammer"), 24, 24]);
							Asset<Texture2D> classToolIcon = (Asset<Texture2D>)method?.Invoke(null, [ModContent.Request<Texture2D>("OrchidMod/Content/Guardian/Weapons/Standards/CopperStandard"), 24, 24]);

							
							// The item category creation call itself, using the resized Grond asset as the icon
							recipeBrowser.Call("AddItemCategory", "Guardian", "Weapons", classWeaponIcon, (Predicate<Item>)(item =>
								{
									if (!item.accessory && item.damage > 0 && item?.ModItem is not OrchidModGuardianStandard)
										return item.CountsAsClass<GuardianDamageClass>() || item.DamageType == ModContent.GetInstance<GuardianDamageClass>();
									return false;
								})
							);
							
							recipeBrowser.Call("AddItemCategory", "Guardian", "Tools", classToolIcon, (Predicate<Item>)(item =>
								{
									if (!item.accessory)
										return item?.ModItem is OrchidModGuardianStandard;
									return false;
								})
							);
						}
					}
				);
			}
		}

		private void WikiThisModCalls() 
		{
			if (!Main.dedServ && ModLoader.TryGetMod("WikiThis", out Mod wikiThis))			
				wikiThis.Call("AddModURL", this, "https://terrariamods.wiki.gg/wiki/Orchid_Mod");
		}
	}
}
