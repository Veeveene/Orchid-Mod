using System;
using System.Globalization;
using Terraria.GameInput;
using Terraria.Localization;

namespace OrchidMod.Utilities
{
	public static partial class OrchidUtils
	{
		public static string FirstCharToUpper(this string input, bool toLower = false)
		{
			var str = toLower ? input.ToLower() : input;
			return str switch
			{
				null => throw new ArgumentNullException(nameof(input)),
				"" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
				_ => string.Concat(str[0].ToString().ToUpper(), str.AsSpan(1))
			};
		}

		public static string FramesToSeconds(int frames)
		{
			string seconds = Math.DivRem(frames, 60, out int centiseconds).ToString();
			if (centiseconds == 0) return seconds;
			centiseconds = (int)(centiseconds / 0.6f + 0.5f);
			if (centiseconds % 10 == 0) centiseconds /= 10;
			string separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			return seconds + separator + centiseconds;
		}
		
		public static string GetClickInfoTooltip(bool configSwap = false, bool rightClick = true)
		{
			string button = "";
			
			KeyConfiguration keyConfig = PlayerInput.CurrentProfile.InputModes[PlayerInput.UsingGamepad ? InputMode.XBoxGamepad : InputMode.Keyboard];
			string keyStatus = PlayerInput.BuildCommand("", true, keyConfig.KeyStatus[configSwap ^ rightClick ? "MouseRight" : "MouseLeft"]).Replace(": ", "");
			if (keyStatus == "Mouse1") button = Language.GetTextValue("Controls.LeftClick");
			else if (keyStatus == "Mouse2") button = Language.GetTextValue("Controls.RightClick");
			else button = Language.GetTextValue("Mods.OrchidMod.Misc.MiscInput", keyStatus);
			
			// if (PlayerInput.UsingGamepad)
			// {
			// 	KeyConfiguration keyConfig = PlayerInput.CurrentProfile.InputModes[InputMode.XBoxGamepad]
			// 	button = PlayerInput.BuildCommand("", true, keyConfig.KeyStatus[configSwap ^ rightClick ? "MouseRight" : "MouseLeft"]).Replace(": ", "");
			// }
			// else
			// {
			// 	if (configSwap ^ rightClick) button = Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.RightClick");
			// 	else button = Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.LeftClick");
			// }
			
			return button;
		}
	}
}