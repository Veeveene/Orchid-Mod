using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using OrchidMod.Common;
using OrchidMod.Common.ModObjects;
using OrchidMod.Common.Attributes;
using OrchidMod.Content.Guardian.Projectiles.Quarterstaves;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves 
{

	[CrossmodContent("ThoriumMod")]
    public class ThoriumNagaQuarterstaff : OrchidModGuardianQuarterstaff
    {        
        public bool bonusChargeHit;
        private bool underWater;
        private int waterAttack = 0;
        
        public override void SafeSetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.value = Item.sellPrice(0, 2);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item71.WithPitchOffset(0.5f).WithVolumeScale(0.5f);
            Item.useTime = 20;
            ParryDuration = 90;
            Item.knockBack = 6f;
            Item.damage = 160;
            Item.shootSpeed = 42f;
            JabStyle = 1;
            JabSpeed = 0.9f;
            JabDamage = 0.75f;
            JabChargeGain = 1.5f;
            SwingStyle = 0;
            SwingSpeed = 1.4f;
            GuardStacks = 1;
            SlamStacks = 1;

            waterAttack = 0;
        }

        public override void SafeHoldItem(Player player)
        {
            Projectile anchor = Main.projectile.FirstOrDefault(proj => proj.active && proj.whoAmI < Main.maxProjectiles && proj.owner == Main.myPlayer && proj.type == ModContent.ProjectileType<GuardianQuarterstaffAnchor>());
            if (underWater) 
            {
                if (anchor.ai[0] > 20f) SwingSpeed = 1 / (3 * player.GetTotalAttackSpeed(DamageClass.Melee));
                else SwingSpeed = 0.6f;

                JabStyle = 0;
            }
            else 
            {
                SwingSpeed = 1.4f;
                JabStyle = 1;
            }
        }

        public override void OnHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool jabAttack, bool counterAttack)
        {
            if (jabAttack)
			{
				GuardianQuarterstaffAnchor anchor = projectile.ModProjectile as GuardianQuarterstaffAnchor;
				if (!underWater && anchor.DamageReset == 0) bonusChargeHit = true;
			}
			else if (!counterAttack)
			{
				if (!underWater) Projectile.perIDStaticNPCImmunity[ModContent.ProjectileType<ThoriumNagaQuarterstaffProjectile>()][target.whoAmI] = Main.GameUpdateCount + 20;
			}
        }

        public override void OnAttack(Player player, OrchidGuardian guardian, Projectile projectile, bool jabAttack, bool counterAttack)
		{
            if (projectile.ModProjectile is GuardianQuarterstaffAnchor anchor)
            {
                if (jabAttack)
                {
                    if (underWater)
                    {

                    }
                    else bonusChargeHit = false;
                }
                else if (!counterAttack)
                {
                    if (underWater) 
                    {
                        SoundEngine.PlaySound(SoundID.Item109, player.Center);
                        OrchidPlayer orchidPlayer = player.GetModPlayer<OrchidPlayer>();
                        if (waterAttack == 0) 
                        {
                            player.position.Y -= 4;
                            orchidPlayer.ForcedVelocityVector = Vector2.UnitX.RotatedBy((Main.MouseWorld - player.Center).ToRotation()) * 10f;
                            orchidPlayer.ForcedVelocityTimer = 60;
                            orchidPlayer.ForcedVelocityUpkeep = 0;

                            waterAttack = 1;
                        }
                    }
                    else 
                    {
                        SoundEngine.PlaySound(SoundID.Item66, player.Center);
                        Vector2 vel = -Vector2.UnitX.RotatedBy((player.Center - Main.MouseWorld).ToRotation()) * Item.shootSpeed;
                        Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromAI(), player.Center, vel, ModContent.ProjectileType<ThoriumNagaQuarterstaffProjectile>(), (int)(Item.damage * 2.5f), Item.knockBack * 2, projectile.owner);
                        newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
                    }
                    
                }
            }
			
		}

        public override void ExtraAIQuarterstaff(Player player, OrchidGuardian guardian, Projectile projectile)
        {
            underWater = Collision.DrownCollision(player.position, player.width, player.height, player.gravDir, true);
            
            OrchidPlayer orchidPlayer = player.GetModPlayer<OrchidPlayer>();
            if (underWater) 
            {
                player.trident = true;
                if (projectile.ai[0] > 1) 
                {
                    if (Main.rand.NextBool(4)) Dust.NewDustDirect(projectile.Center, player.width, player.height, DustID.BreatheBubble, Scale: Main.rand.NextFloat(1.5f, 3.5f));
                    
                    bool attackInput = Main.mouseLeft && Main.mouseLeftRelease;
                    if (ModContent.GetInstance<OrchidClientConfig>().GuardianSwapGauntletImputs) attackInput = Main.mouseRight && Main.mouseRightRelease;

                    if (waterAttack == 1) 
                    {
                        player.armorEffectDrawShadowEOCShield = true;
                        
                        if (IsLocalPlayer(player))
                        {
                            ref Vector2 forcedVelocity = ref orchidPlayer.ForcedVelocityVector;
                            forcedVelocity = Vector2.UnitX.RotatedBy(forcedVelocity.ToRotation().AngleTowards(projectile.AngleTo(Main.MouseWorld), MathHelper.Pi/60)) * forcedVelocity.Length();
                            projectile.ai[1] = Vector2.Normalize(forcedVelocity).ToRotation() - MathHelper.PiOver2;
						    projectile.Center = player.MountedCenter.Floor() + Vector2.UnitY.RotatedBy(projectile.ai[1]) * (38f - (float)Math.Sin(0.0523f * (30 - projectile.ai[0])) * 24f);

                        }
                        
                        if (projectile.ai[0] < 20 || attackInput)
                        {
                            if (projectile.ai[0] > 20) projectile.ai[0] = 20;
                            orchidPlayer.ForcedVelocityVector = Vector2.Zero;
                            orchidPlayer.ForcedVelocityTimer = 0;
                            orchidPlayer.ForcedVelocityUpkeep = 0;

                            projectile.scale *= 1.5f;
                            projectile.width = (int)(projectile.width * 1.5f);
                            projectile.height = (int)(projectile.height * 1.5f);

                            SoundEngine.PlaySound(SoundID.Item66, player.Center);
                            SoundEngine.PlaySound(SoundID.Splash, player.Center);

                            waterAttack = 2;
                        }
                    }
                }
                else waterAttack = 0;
            }
            else 
            {
                if (waterAttack == 1)
                {
                    orchidPlayer.ForcedVelocityVector *= 2;
                    orchidPlayer.ForcedVelocityTimer = 1;
                    orchidPlayer.ForcedVelocityUpkeep = 1;

                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 direction = orchidPlayer.ForcedVelocityVector.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi/12, MathHelper.Pi/12)) * Main.rand.NextFloat(0.6f, 1.2f);
                        Dust.NewDustPerfect(player.Center, Dust.dustWater(), direction, Scale: Main.rand.NextFloat(1f, 3f));
                        if (Main.rand.NextBool(4)) {
                            Gore gore = Gore.NewGorePerfect(projectile.GetSource_FromAI(), player.Center, direction * 0.1f, 412);
                            gore.type = 412;
                        }
                    }
                }
                waterAttack = 0;
            }
        }

        public override bool PreSwingAI(Player player, OrchidGuardian guardian, Projectile projectile)
        {
            if (underWater && waterAttack == 2)
            {
                projectile.rotation = projectile.ai[1] - MathHelper.PiOver4 + 0.3142f * (projectile.ai[0]) * -player.direction + MathHelper.Pi;
				projectile.Center = player.MountedCenter.Floor() + Vector2.UnitY.RotatedBy(projectile.ai[1] + 0.3142f * (projectile.ai[0]) * -player.direction) * 60f;
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.PiOver4 * player.direction + projectile.ai[1] + 0.1f - (float)Math.Cos(0.3142f * (projectile.ai[0] - 9)) * player.direction);
				player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, projectile.ai[1] - 0.1f + (float)Math.Cos(0.3142f * (projectile.ai[0]- 9)) * 0.2f * player.direction);
                
                return false;
            }
            return true;
        }

        public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				CreateRecipe()
				.AddTile(TileID.MythrilAnvil)
                .AddIngredient<ThoriumAquaiteQuarterstaff>()
				.AddIngredient(thoriumMod, "AbyssalChitin", 8)
				.Register();
			}
		}
    }    
}
