using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Greathammers
{
	public class CoznixGreathammer : OrchidModGuardianGreathammer
	{
		public int Timer;

		public override void SafeSetDefaults()
		{
			Item.width = 38;
			Item.height = 36;
			Item.value = Item.sellPrice(0, 0, 27, 0);
			Item.rare = ItemRarityID.Expert;
			Item.knockBack = 5f;
			Item.shootSpeed = 14f;
			Item.useTime = 40;
			Item.damage = 96;
			GuardStacks = 1;
			SlamSpeed = 0.8f;
			Range = 25;
			AuraScale = 1.05f;
			AuraPrimary = new(232, 70, 0, 60);
			AuraSecondary = new(107, 36, 27, 60);
		}

		public override void OnSlam(Player player, OrchidGuardian guardian, Projectile projectile, GuardianGreathammerAnchor anchor, bool FullyCharged)
		{
			// Stupid windfury proc effect https://www.youtube.com/watch?v=yFB-pQ65-Kk
			// yes, we will remove it later
			// i am sorry
			// :(

			if (Main.rand.Next(6) != 0)
			{
				SlamSpeed += 0.8f;
			}
			else
			{
				SlamSpeed = 0.8f;
			}
		}

		public override void OnSlamEnd(Player player, OrchidGuardian guardian, Projectile projectile, GuardianGreathammerAnchor anchor, bool FullyCharged)
		{
			if (SlamSpeed > 0.8f)
			{
				anchor.DoSlam(guardian);
			}
		}

		public override void SafeHoldItem(Player player, OrchidGuardian guardian, Projectile anchor)
		{
			Timer++; // for glowmask
		}

		public override Color GetHammerGlowmaskColor(Player player, Projectile projectile, Color lightColor)
		{
			Color color = Color.White * (0.45f + ((float)Math.Sin(Timer * 0.0785f) * 0.05f) + ((float)Math.Sin(Timer * 0.1605f) * 0.05f) + ((float)Math.Sin(Timer * 0.341f) * 0.05f));
			return color;
		}
	}
}
