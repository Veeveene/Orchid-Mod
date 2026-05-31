using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Audio;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
	public class PirateWarhammer : OrchidModGuardianHammer
	{
		public override void SafeSetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(0, 3, 40, 0);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.DD2_MonkStaffSwing;
			Item.knockBack = 10f;
			Item.shootSpeed = 16f;
			Item.damage = 220;
			Item.useTime = 20;
			Range = 40;
			SlamStacks = 3;
			ReturnSpeed = 1.8f;
			BlockDuration = 270;
		}

		public override void OnMeleeHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged, bool OffHand)
		{
			if (!((GuardianHammerAnchor)projectile.ModProjectile).WeakHit && !FullyCharged)
			{
				CombatText.NewText(player.Hitbox, new Color(175, 255, 175), Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.Charged"), false);
				SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot.WithPitchOffset(0.2f), player.Center);
				guardian.GuardianItemCharge = 210;
			}
			SoundEngine.PlaySound(SoundID.Coins.WithPitchOffset(-0.4f), player.Center);
			SoundEngine.PlaySound(SoundID.CoinPickup.WithPitchOffset(-0.4f), player.Center);
			for (int i = 0; i < 10; i++)
			{
				Dust dust = Dust.NewDustDirect(target.Center, 0, 0, DustID.Enchanted_Gold, Alpha: 100, Scale: 0.75f);
				if (Main.rand.NextBool(2))
				{
					dust.velocity *= 2f;
					dust.scale *= 2f;
					dust.noGravity = true;
					Dust.NewDustDirect(target.Center, 0, 0, DustID.Gold, 0, -2f, Scale: 0.8f).velocity *= 0.5f;
				}
				else dust.velocity *= 1.5f;
			}
		}

		public override void OnMeleeHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged, bool OffHand)
		{
			target.AddBuff(BuffID.Midas, 300);
		}
	}
}
