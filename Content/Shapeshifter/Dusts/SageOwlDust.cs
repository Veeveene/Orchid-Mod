using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace OrchidMod.Content.Shapeshifter.Dusts
{
	public class SageOwlDust : ModDust
	{
		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, Main.rand.Next(0, 3) * 10, 10, 10);
			dust.fadeIn = 120 + Main.rand.Next(60);
		}

		public override bool Update(Dust dust)
		{
			if (dust.fadeIn > 0)
			{
				dust.fadeIn--;
			}
			else
			{
				dust.alpha += 10;
				if (dust.alpha > 240f) dust.active = false;
			}

			int style = 0;
			int styleData = 0;
			if (dust.customData is int customData)
			{
				styleData = customData;
				style = styleData % 4 == 0 ? 1 : 0;
			}
			dust.frame = new Rectangle(10 * style, dust.frame.Y, 10, 10);

			dust.position += dust.velocity;
			dust.velocity *= 0.95f;
			dust.velocity.X += (float)Math.Sin(Main.GlobalTimeWrappedHourly * (514f / (styleData + 200f))) * 0.075f;
			dust.velocity.Y = 1.5f - Math.Abs(dust.velocity.X);
			dust.rotation = MathHelper.PiOver4 - dust.velocity.X * 0.75f + (styleData % 2 == 0 ? MathHelper.Pi : 0);
			return false;
		}
	}
}
