using Terraria;

namespace OrchidMod.Content.Guardian.Accessories
{
	public class GuardianTest : OrchidModGuardianEquipable
	{
		public override void SafeSetDefaults()
		{
			Item.width = 24;
			Item.height = 28;
			Item.value = Item.sellPrice(0, 0, 30, 0);
			Item.rare = -11;
			Item.accessory = true;
			Item.useStyle = 1;
			Item.useTime = 60;
			Item.useAnimation = 60;
		}

		public override bool? UseItem(Player player)
		{
			OrchidGuardian modPlayer = player.GetModPlayer<OrchidGuardian>();
			Main.NewText("Generating all attack type names.");
			for (byte i = 0; i < 128; i++)
			{
				Main.NewText(AttackTypeString(new GuardianAttackInfo(i)) + " (" + i + ", 0b"+ i.ToString("B8") +")");
			}
			return true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			OrchidGuardian modPlayer = player.GetModPlayer<OrchidGuardian>();
			modPlayer.GuardianInfiniteResources = true;
			if (!hideVisual)
			{
				int mode = 6;
				for (int i = 3; i < 8; i++)
				{
					if (player.armor[i].type == Type)
						mode = i - 2;
				}
				modPlayer.GuardianDebugVisuals = mode;
			}
		}

		public override void UpdateVanity(Player player)
		{
			OrchidGuardian modPlayer = player.GetModPlayer<OrchidGuardian>();
			int mode = 6;
			for (int i = 13; i < 18; i++)
			{
				if (player.armor[i].type == Type)
				{
					mode = i - 12;
					break;
				}
			}
			modPlayer.GuardianDebugVisuals = mode;
		}

		public static string AttackTypeString(GuardianAttackInfo info)
		{
			//this is purely for debug, subject to change, and will be a nightmare to translate. don't worry about converting this to localization
			string s = "";
			//determine noun, by order of importance
			if (info.Jab) s = "jab";
			else if (info.Offense && info.Counter) s = "counterattack";
			else if (info.Slam) s = "slam";
			else if (info.Offense && info.Charged) s = "attack";
			else if (info.Guard)
			{
				if (info.Defense) s = "defend";
				else s = "guard";
			}
			else if (info.Offense) s = "bash";
			else s = "buff";
			//apply adjectives
			if (info.Counter && (info.Jab || !info.Offense)) s = "counter-" + s; //if the noun isn't counterattack, specify counter
			if (info.Slam && (info.Jab || (info.Offense && info.Counter))) s = "slam " + s; // if the noun is jab or counterattack, specify slam
			if (info.Reinforced) s = "reinforced " + s;
			else if (info.Charged) s = "charged " + s;
			if (info.Jab || info.Slam || (info.Offense && (info.Counter || info.Charged)) || !info.Guard) // if the noun is not defend or guard
			{
				if (info.Guard) //specify defensive, guard or defending
				{
					if (info.Defense) s = "defending " + s;
					else s = "guarding " + s;
				}
				else if (info.Defense) s = "defensive " + s;
				if (!info.Offense && (info.Jab || info.Slam)) s = "nonoffensive " + s; //if the noun is not buff, specify nonoffensive
			}
			else if (info.Offense) s = "offensive " + s; //if the noun is defend or guard, specify offensive
			return s;
		}
	}
}