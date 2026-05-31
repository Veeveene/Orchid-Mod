using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Audio;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
    public class ToyWarhammers : OrchidModGuardianHammer
    {

		private SoundStyle SqueakSound = new SoundStyle("OrchidMod/Assets/Sounds/Squeak") { PitchRange = (-0.2f, 0.2f), MaxInstances = 5, Volume = 0.05f };

        public override void SafeSetDefaults()
        {
            Item.width = 44;
            Item.height = 42;
            Item.value = Item.sellPrice(0, 3, 50, 0);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.DD2_MonkStaffSwing;
            Item.knockBack = 2;
            Item.shootSpeed = 15f;
            Item.damage = 60;
            Item.useTime = 30;
            Range = 40;
			SwingSpeed = 3f;
            TileBounce = true;
            GuardStacks = 1;
            ReturnSpeed = 1.8f;
            HoldOffset = -2f;
			SwingChargeGain = 0.25f;
			DualWarhammers = true;
			hasSpecialHammerTexture = true;
			CannotBlock = true;
		}

        public override void OnMeleeHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged, bool OffHand)
        {
            SoundEngine.PlaySound(SqueakSound);
        }

        public override void OnThrowHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak, bool OffHand)
        {
            SoundEngine.PlaySound(SqueakSound);
        }

        public override void OnThrowTileCollide(Player player, OrchidGuardian guardian, Projectile projectile, Vector2 oldVelocity, bool OffHand)
        {
            SoundEngine.PlaySound(SqueakSound);
        }
    }
}

