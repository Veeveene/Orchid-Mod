using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using OrchidMod.Content.Guardian.Weapons.Quarterstaves;
using Terraria.DataStructures;

namespace OrchidMod.Content.Guardian.Buffs
{
	public class GuardianBoreanStriderQuarterstaffBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.buffNoSave[Type] = true;
		}
	}
}