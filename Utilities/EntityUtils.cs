using JetBrains.Annotations;
using OrchidMod.Common.ModObjects;
using Terraria;
using OrchidMod.Content.Guardian;
using OrchidMod.Content.Shapeshifter;
using Terraria.ModLoader;

namespace OrchidMod.Utilities;

// Player utils
public static partial class OrchidUtils
{
	/// <summary>Returns the <see cref="OrchidPlayer"/> instance for either the given player or the <c>LocalPlayer</c> if null.</summary>
	/// <remarks>
	/// For known <c>Player</c> instances, this method can be invoked as an extension; i.e <c>player.OrchidPlayer()</c>.
	/// <br/> If the <c>LocalPlayer</c> is needed (such as in <c>ModifyTooltip</c> hooks that lack a "player" parameter), invoke this as a static method and pass in <c>null</c>;
	/// i.e. <c>OrchidUtils.OrchidPlayer(null)</c>.
	/// </remarks>
	public static OrchidPlayer OrchidPlayer(this Player player) => (player ?? Main.LocalPlayer).GetModPlayer<OrchidPlayer>();
	
	/// <summary>Returns the <see cref="OrchidGuardian"/> instance for either the given player or the <c>LocalPlayer</c> if null.</summary>
	/// <remarks>
	/// <see cref="OrchidModGuardianItem"/> and <see cref="OrchidModGuardianEquipable"/> define shortcuts for this method that pass in an optional <c>Player</c> parameter rather than an extension parameter;
	/// in classes that derive from those types, simply call <c>Guardian(player)</c> for a known player and <c>Guardian()</c> for the <c>LocalPlayer</c>.
	/// <para/>Otherwise, the same syntax as <see cref="OrchidPlayer(Player)"/> applies.
	/// </remarks>
	/// <seealso cref="OrchidModGuardianItem.Guardian(Player)"/>
	/// <seealso cref="OrchidModGuardianEquipable.Guardian(Player)"/>
	public static OrchidGuardian Guardian(this Player player) => (player ?? Main.LocalPlayer).GetModPlayer<OrchidGuardian>();
	
	/// <summary>Returns the <see cref="OrchidShapeshifter"/> instance for either the given player or the <c>LocalPlayer</c> if null.</summary>
	/// <remarks>
	/// For known <c>Player</c> instances, this method can be invoked as an extension; i.e <c>player.Shapeshifter()</c>.
	/// <br/> If the <c>LocalPlayer</c> is needed (such as in <c>ModifyTooltip</c> hooks that lack a "player" parameter), invoke this as a static method and pass in <c>null</c>;
	/// i.e. <c>OrchidUtils.Shapeshifter(null)</c>.
	/// </remarks>
	public static OrchidShapeshifter Shapeshifter(this Player player) => (player ?? Main.LocalPlayer).GetModPlayer<OrchidShapeshifter>();
	
	/*we're probably not going to do anything with alchemist or gambler, i originally added these as boilerplate*/
	// public static OrchidAlchemist Alchemist(this Player player) => player.GetModPlayer<OrchidAlchemist>();
	// public static OrchidGambler Gambler(this Player player) => player.GetModPlayer<OrchidGambler>();
	
	/// <summary>
	/// Has the exact same functionality as <see cref="Player.AddBuff(int,int,bool,bool)">Player.AddBuff()</see>, only it passes a <c>ModBuff</c> directly instead of <c>ModContent.BuffType&lt;T&gt;()</c> to simplify code.
	/// <br/> Use <c>Player.AddBuff()</c> if you're working with a vanilla <c>BuffID</c> or another mod's <c>ModBuff.Type</c>.
	/// </summary>
	/// <typeparam name="T">The <see cref="ModBuff"/> to add</typeparam>
	public static void AddBuff<T>(this Player player, int timeToAdd, bool quiet = false) where T : ModBuff => player.AddBuff(ModContent.BuffType<T>(), timeToAdd, quiet);
	
	/// <summary>
	/// Has the exact same functionality as <c>Player.ClearBuff()</c>, only it passes a <c>ModBuff</c> directly instead of <c>ModContent.BuffType&lt;T&gt;()</c> to simplify code.
	/// <br/> Use <see cref="Player.ClearBuff(int)"/> if you're working with a vanilla <c>BuffID</c> or another mod's <c>ModBuff.Type</c>, or <see cref="Player.DelBuff(int)"/> if you only know the buff index.
	/// </summary>
	/// <typeparam name="T">The <see cref="ModBuff"/> to clear</typeparam>
	public static void ClearBuff<T>(this Player player) where T : ModBuff => player.ClearBuff(ModContent.BuffType<T>());
	
}

// NPC utils
public static partial class OrchidUtils
{
	/// <summary>
	/// Has the exact same functionality as <see cref="NPC.AddBuff(int,int,bool)">NPC.AddBuff()</see>, only it passes a <c>ModBuff</c> directly instead of <c>ModContent.BuffType&lt;T&gt;()</c> to simplify code.
	/// <br/> Use <c>NPC.AddBuff()</c> if you're working with a vanilla <c>BuffID</c> or another mod's <c>ModBuff.Type</c>.
	/// </summary>
	/// <typeparam name="T">The <see cref="ModBuff"/> to add</typeparam>
	public static void AddBuff<T>(this NPC npc, int timeToAdd, bool quiet = false) where T : ModBuff => npc.AddBuff(ModContent.BuffType<T>(), timeToAdd, quiet);

	/// <summary> Removes the provided buff type from the player and shuffles the remaining buff indexes down to fill the gap. Use <see cref="NPC.DelBuff(int)"/> if you only know the buff index.</summary>
	/// <remarks> Reimplementation of <see cref="Player.ClearBuff(int)"/>, but for <c>NPCs</c>.</remarks>
	public static void ClearBuff(this NPC npc, int type)
	{
		if (type == 0)
			return;

		for (int i = 0; i < NPC.maxBuffs; i++) {
			if (npc.buffType[i] == type)
				npc.DelBuff(i);
		}
	}

	/// <summary>
	/// Has the exact same functionality as <see cref="ClearBuff(NPC, int)"/>, only it passes a <c>ModBuff</c> directly instead of <c>ModContent.BuffType&lt;T&gt;()</c> to simplify code.
	/// <br/> Use <c>NPC.ClearBuff()</c> if you're working with a vanilla <c>BuffID</c>, or <see cref="NPC.DelBuff(int)"/> if you only know the buff index.
	/// </summary>
	/// <typeparam name="T">The <see cref="ModBuff"/> to clear</typeparam>
	/// <seealso cref="OrchidUtils.ClearBuff(NPC, int)"/>
	public static void ClearBuff<T>(this NPC npc) where T : ModBuff => npc.ClearBuff(ModContent.BuffType<T>());
}