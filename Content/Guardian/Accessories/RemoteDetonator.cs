using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.ID;
using OrchidMod.Common.ModObjects;
using OrchidMod.Content.Guardian;

namespace OrchidMod.Content.Guardian.Accessories;

public class RemoteDetonator : OrchidModGuardianEquipable
{
	public override void SafeSetDefaults()
	{
		Item.width = 16;
		Item.height = 36;
		Item.value = Item.sellPrice(0, 2);
		Item.rare = ItemRarityID.LightRed;
		Item.accessory = true;
	}

	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		if (player.HeldItem.ModItem is OrchidModGuardianHammer hammer && !hammer.CannotExplode)
		{
			var projectile = Main.projectile.FirstOrDefault(proj => proj.whoAmI < Main.maxProjectiles && proj.active && proj.owner == Main.myPlayer && proj.type == ModContent.ProjectileType<GuardianHammerAnchor>());
			if (projectile != null && projectile.ModProjectile is GuardianHammerAnchor anchor)
			{
				OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
				if (projectile.timeLeft < 598 && anchor.range > 0 && anchor.BlockDuration == 0) {
					if (anchor.range <= 285)
                    {
                        // Blip sound to 
                        if (anchor.range == hammer.Range - 15) SoundEngine.PlaySound(SoundID.MaxMana);
                        
                        if (anchor.range <= hammer.Range - 30) Dust.NewDustPerfect(projectile.position, DustID.Torch, -(projectile.velocity*Main.rand.NextFloat(0.125f, 0.625f)).RotatedByRandom(MathHelper.Pi/18), Scale: 1.125f);
                    
                        
                        guardian.SlamCostUI = 1;
                    
                        if (Main.mouseLeft && anchor.Ding && guardian.UseSlam(1, true))
                        {
                            SoundEngine.PlaySound(SoundID.MenuTick, projectile.Center);
                            guardian.UseSlam(1);
							OrchidModProjectile.spawnGenericExplosion(projectile, (int)(projectile.damage), 10f, 250, 0, true, true);
							for (int i = 0; i < 10; i++) Dust.NewDustPerfect(projectile.Center, DustID.Smoke, Main.rand.NextVector2Circular(7.8125f, 7.8125f));
							for (int i = 0; i < 10; i++) Dust.NewDustPerfect(projectile.Center, DustID.Torch, Main.rand.NextVector2CircularEdge(7.8125f, 7.8125f));
							projectile.Kill();
                        }
                    }
				}
			
			}
		}
	}
}