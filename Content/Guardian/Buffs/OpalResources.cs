using OrchidMod.Content.Guardian.Weapons.Gauntlets;
using System.Reflection;
using OrchidMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Buffs
{
	public class OpalResources : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.manaRegenBonus += 2;
			player.manaRegenDelayBonus += 1;

			OrchidGuardian guardian = player.Guardian();
			guardian.GuardianGuardRecharge += 0.1f;
			guardian.GuardianSlamRecharge += 0.1f;

			if (OrchidMod.ThoriumMod != null)
			{
				foreach (ModPlayer thoriumPlayer in player.ModPlayers)
				{
					if (thoriumPlayer.Name == "ThoriumPlayer" && thoriumPlayer.Mod == OrchidMod.ThoriumMod)
					{
						FieldInfo field1 = thoriumPlayer.GetType().GetField("inspirationRegenBonus", BindingFlags.Public | BindingFlags.Instance);
						FieldInfo field2 = thoriumPlayer.GetType().GetField("throwerExhaustionRegenBonus", BindingFlags.Public | BindingFlags.Instance);

						if (field1 != null && field2 != null)
						{
							field1.SetValue(thoriumPlayer, (float)field1.GetValue(thoriumPlayer) + 0.1f);
							field2.SetValue(thoriumPlayer, (float)field2.GetValue(thoriumPlayer) + 0.1f);
						}
						break;
					}
				}

				//ModPlayer thoriumPlayer = player.GetModPlayer(OrchidMod.ThoriumMod, "ThoriumPlayer");
			}
		}
	}
}