using Terraria;
using Terraria.ModLoader;
using OrchidMod.Content.Guardian.Weapons.Quarterstaves;

namespace OrchidMod.Content.Guardian.Buffs
{
	public class GuardianVoidQuarterstaffBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.HeldItem != null && player.HeldItem.ModItem is ThoriumVoidQuarterstaff) player.GetAttackSpeed(DamageClass.Melee) += 2f;
		}
	}
}