using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.ID;
using OrchidMod.Content.Guardian;

namespace OrchidMod.Content.Guardian.Accessories;

public class HammerMagnet : OrchidModGuardianEquipable
{
	public override void SafeSetDefaults()
	{
		Item.width = 30;
		Item.height = 30;
		Item.value = Item.buyPrice(0, 15);
		Item.rare = ItemRarityID.LightRed;
		Item.accessory = true;
	}

	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		OrchidGuardian modPlayer = player.GetModPlayer<OrchidGuardian>();
		modPlayer.GuardianHammerMagnet = true;
	}
}