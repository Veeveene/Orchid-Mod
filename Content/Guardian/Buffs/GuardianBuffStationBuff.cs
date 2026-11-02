using OrchidMod.Utilities;
using Terraria;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Buffs
{
	public class GuardianBuffStationBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			OrchidGuardian guardian = player.Guardian();
			guardian.GuardianSlamMax++;
		}
	}
}