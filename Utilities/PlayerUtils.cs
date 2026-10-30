using OrchidMod.Common.ModObjects;
using Terraria;
using OrchidMod.Content.Guardian;
using OrchidMod.Content.Shapeshifter;
using Terraria.ModLoader;

namespace OrchidMod.Utilities;

public static partial class OrchidUtils
{
	public static OrchidPlayer OrchidPlayer(this Player player) => player.GetModPlayer<OrchidPlayer>();
	public static OrchidGuardian Guardian(this Player player) => player.GetModPlayer<OrchidGuardian>();
	public static OrchidGuardian Guardian(this Player player, out OrchidGuardian guardian)
	{
		guardian = player.Guardian();
		return guardian;
	}
	
	public static OrchidShapeshifter Shapeshifter(this Player player) => player.GetModPlayer<OrchidShapeshifter>();
	public static OrchidShapeshifter Shapeshifter(this Player player, out OrchidShapeshifter shifter)
	{
		shifter = player.Shapeshifter();
		return shifter;
	}
	
	/*we're probably not going to do anything with alchemist or gambler, i originally added these as boilerplate*/
	// public static OrchidAlchemist Alchemist(this Player player) => player.GetModPlayer<OrchidAlchemist>();
	// public static OrchidGambler Gambler(this Player player) => player.GetModPlayer<OrchidGambler>();

	// No idea why there isn't already
	public static void AddBuff<T>(this Player player, int timeToAdd, bool quiet = false) where T : ModBuff => player.AddBuff(ModContent.BuffType<T>(), timeToAdd, quiet);
	public static void ClearBuff<T>(this Player player) where T : ModBuff => player.ClearBuff(ModContent.BuffType<T>());
}