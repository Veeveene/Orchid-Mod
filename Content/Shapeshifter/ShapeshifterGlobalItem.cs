using OrchidMod.Content.Guardian;
using Terraria;
using Terraria.ModLoader;

namespace OrchidMod.Common.Global.Items
{
	public partial class ShapeshifterGlobalItem : GlobalItem
	{
		public override bool CanUseItem(Item item, Player player)
		{
			OrchidShapeshifter shapeshifter = player.Shapeshifter();
			if (shapeshifter.IsShapeshifted && shapeshifter.ShapeshifterHookDash > 0f && Main.projHook[item.shoot])
			{ // blocks hooks if the player has a shawl
				return false;
			}

			return base.CanUseItem(item, player);
		}
	}
}
