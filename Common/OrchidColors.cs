using Microsoft.Xna.Framework;
using System;

namespace OrchidMod.Common
{
	public static class OrchidColors
	{
		public static readonly Color AlchemistTag = new(155, 255, 55);
		public static readonly Color DancerTag = new(255, 185, 255);
		public static readonly Color GamblerTag = new(255, 200, 0);
		public static readonly Color GuardianTag = new(165, 130, 100);
		public static readonly Color ShamanTag = new(0, 192, 255);
		public static readonly Color ShapeshifterTag = new(100, 175, 150);

		public static readonly Color CrossmodContentWarning = new(255, 130, 110);
		public static readonly Color GuardianTooltip = new(175, 255, 175);

		public static Color GetClassTagColor(ClassTags tag)
		{
			return tag switch
			{
				ClassTags.Alchemist => AlchemistTag,
				ClassTags.Dancer => DancerTag,
				ClassTags.Gambler => GamblerTag,
				ClassTags.Guardian => GuardianTag,
				ClassTags.Shaman => ShamanTag,
				ClassTags.Shapeshifter => ShapeshifterTag,
				_ => throw new NotImplementedException()
			};
		}	
	}
}