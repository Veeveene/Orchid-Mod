using OrchidMod.Utilities;
using Terraria;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Buffs
{
	public class GuardianRegenPotionBuff : ModBuff
	{
		public override void Update(Player player, ref int buffIndex)
		{
			OrchidGuardian guardian = player.Guardian();
			guardian.GuardianRegenThreshold = 1;
		}
	}
}