using System.IO;
using Microsoft.Xna.Framework;
using OrchidMod;
using OrchidMod.Content.Guardian;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
    public class SqueakyHammers : OrchidModGuardianHammer
    {

        private SoundStyle SqueakSound = new SoundStyle("CreepZoneGuardian/Assets/Sounds/Items/Weapons/Squeak") { PitchRange = (-0.2f, 0.2f), MaxInstances = 5 };

        private float OffhandHammerAI0, OffhandHammerAI1, OffhandHammerAI2;

        public override void SafeSetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.value = Item.sellPrice(0, 7, 50);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.DD2_MonkStaffSwing;
            Item.knockBack = 3;
            Item.shootSpeed = 20f;
            Item.damage = 60;
            Item.useTime = 10;
            Range = 60;
            TileBounce = true;
            GuardStacks = 1;
            ReturnSpeed = 1.8f;
            BlockDuration = 60;
            hasSpecialHammerTexture = true;
            HoldOffset = -2f;
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write((byte)OffhandHammerAI0);
            writer.Write((byte)OffhandHammerAI1);
            writer.Write((byte)OffhandHammerAI2);
        }

        public override void OnMeleeHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged)
        {
            SoundEngine.PlaySound(SqueakSound);
        }

        public override void OnThrowHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak)
        {
            SoundEngine.PlaySound(SqueakSound);
        }

        public override void OnThrowTileCollide(Player player, OrchidGuardian guardian, Projectile projectile, Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SqueakSound);
        }
    }
}

