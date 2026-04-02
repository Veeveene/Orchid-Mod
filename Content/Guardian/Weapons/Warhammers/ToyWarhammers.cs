using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common.Global.Items;
using OrchidMod.Content.General.Prefixes;
using OrchidMod.Content.Guardian;
using OrchidMod.Content.Guardian.Projectiles.Warhammers;
using OrchidMod.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Audio;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
    public class ToyWarhammers : OrchidModGuardianHammer
    {

        private SoundStyle SqueakSound = new SoundStyle("OrchidMod/Assets/Sounds/Squeak") { PitchRange = (-0.2f, 0.2f), MaxInstances = 5 };

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
        
        public override bool? UseItem(Player player)
		{
			var guardian = player.GetModPlayer<OrchidGuardian>();
			int damage = guardian.GetGuardianDamage(Item.damage);

			int projTypeMain = ModContent.ProjectileType<GuardianHammerAnchor>();
			Projectile mainProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), player.Center, Vector2.Zero, projTypeMain, damage, Item.knockBack, player.whoAmI);
			mainProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);

			int projTypeAlt = ModContent.ProjectileType<ToyWarhammerProjectile>();
			Projectile altProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), player.Center, Vector2.Zero, projTypeAlt, damage, Item.knockBack, player.whoAmI);
			altProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);

			if (Main.mouseRight && Main.mouseRightRelease && mainProjectile.ModProjectile is GuardianHammerAnchor anchorMain && altProjectile.ModProjectile is ToyWarhammerProjectile anchorAlt && ((SlamBlockCost > 0 && guardian.UseSlam(SlamBlockCost, true)) || (GuardBlockCost > 0 && guardian.UseGuard(GuardBlockCost, true))))
			{
				if (SlamBlockCost > 0) guardian.UseSlam(SlamBlockCost);
				if (GuardBlockCost > 0) guardian.UseGuard(GuardBlockCost);

                if (anchorMain.BlockDuration != 0) {
                    altProjectile.velocity = Vector2.Normalize(Main.MouseWorld - player.Center) * (10f + (Item.shootSpeed - 10f) * 0.35f * BlockVelocityMult);
                    altProjectile.friendly = true;
                    altProjectile.knockBack = 0f;
                    altProjectile.tileCollide = true;

                    anchorAlt.BlockDuration = (int)(BlockDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianBlockDuration + 10);
                    anchorAlt.NeedNetUpdate = true;
                }
                else {
                    mainProjectile.velocity = Vector2.Normalize(Main.MouseWorld - player.Center) * (10f + (Item.shootSpeed - 10f) * 0.35f * BlockVelocityMult);
                    mainProjectile.friendly = true;
                    mainProjectile.knockBack = 0f;
                    mainProjectile.tileCollide = true;

                    anchorMain.BlockDuration = (int)(BlockDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianBlockDuration + 10);
                    anchorMain.NeedNetUpdate = true;
                }

				
			}

			guardian.GuardianItemCharge = 0f;
			return true;
		}

        public override bool CanUseItem(Player player)
		{
			int projTypeMain = ModContent.ProjectileType<GuardianHammerAnchor>();
			int projTypeAlt = ModContent.ProjectileType<GuardianHammerAnchor>();

			if (Main.mouseRight && Main.mouseRightRelease)
			{
				var projMain = Main.projectile.FirstOrDefault(i => i.active && i.owner == player.whoAmI && i.type == projTypeMain && i.ModProjectile is GuardianHammerAnchor warhammerMain && warhammerMain.BlockDuration > 0);
				var projAlt = Main.projectile.FirstOrDefault(i => i.active && i.owner == player.whoAmI && i.type == projTypeAlt && i.ModProjectile is ToyWarhammerProjectile warhammerAlt && warhammerAlt.BlockDuration > 0);
                if (projMain != null && projMain.ModProjectile is GuardianHammerAnchor warhammerMain)
				{ // recalls existing blocking warhammers when right clicking
                    if (projAlt != null && projAlt.ModProjectile is ToyWarhammerProjectile alt && alt.BlockDuration != 0) {
                        warhammerMain.BlockDuration = -30; // -30 instead of -1 so they return faster
                        projMain.netUpdate = true;
                    }
				}
                if (projAlt != null && projAlt.ModProjectile is ToyWarhammerProjectile warhammerAlt)
                { 
                    if (projMain == null || (projMain != null && projMain.ModProjectile is GuardianHammerAnchor main && main.BlockDuration == 0)) {
                        warhammerAlt.BlockDuration = -30; // -30 instead of -1 so they return faster
                        projAlt.netUpdate = true;
                    }
                }
			}

			if ((player.ownedProjectileCounts[projTypeMain] > 0 && player.ownedProjectileCounts[projTypeAlt] > 0) || (!(Main.mouseRight && Main.mouseRightRelease && player.GetModPlayer<OrchidGuardian>().UseGuard(1, true)) && !Main.mouseLeft)) return false;
			return base.CanUseItem(player);
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

