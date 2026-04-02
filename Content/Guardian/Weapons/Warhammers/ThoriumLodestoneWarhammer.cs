using System;
using Microsoft.Xna.Framework;
using OrchidMod;
using Terraria;
using OrchidMod.Common.Attributes;
using OrchidMod.Content.Guardian;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers {

    [CrossmodContent("ThoriumMod")]
    public class ThoriumLodestoneWarhammer : OrchidModGuardianHammer
    {
    
        private static Mod ThoriumMod => ModLoader.GetMod("ThoriumMod");
        private static ModBuff ThoriumSunderedDebuff => ThoriumMod.Find<ModBuff>("Sundered");

        public override bool IsLoadingEnabled(Mod mod)
        {
            return ModLoader.HasMod("ThoriumMod");
        }

        public override void SafeSetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.value = Item.sellPrice(0, 2);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.DD2_MonkStaffSwing;
            Item.knockBack = 10f;
            Item.shootSpeed = 16f;
            Item.damage = 162;
            Item.useTime = 30;
            Range = 300; // really high so that hammer can potentially hit the ground if you do something silly like launch it from 200ft in the air 
            GuardStacks = 1;
            SlamStacks = 1;
            ReturnSpeed = 0.5f;
            BlockDuration = 240;
            HoldOffset = -2;
        }
        

        public override void ExtraAI(Player player, OrchidGuardian guardian, Projectile projectile)
        {
            
            // Generic dust particles
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Copper, Scale: 0.75f);
                dust.noGravity = true;
            }

            if (projectile.ModProjectile is GuardianHammerAnchor anchor)
            {
                // Behavior while throwing (affected by gravity, can be slammed)
                if (projectile.ai[1] > 0 && anchor.range > 0)
                {
                    // Behavior for Ultra Smash effect
                    if ((int)projectile.ai[2] == 1)
                    {
                        Dust.NewDustPerfect(projectile.position + Vector2.UnitX.RotatedBy(MathHelper.TwoPi*Main.rand.NextFloat()) * projectile.Size*0.5f, DustID.Torch, (projectile.velocity*Main.rand.NextFloat(0.125f, 0.625f)).RotatedByRandom(MathHelper.Pi/18), Scale: Main.rand.NextFloat(0.75f, 2.25f));
                        for (int i = 0; i < 3; i++) Dust.NewDust(projectile.position, projectile.width*2, projectile.height*2, DustID.Torch, Scale: 0.75f);

                        if (anchor.range % 4 == 0) SoundEngine.PlaySound(SoundID.Item34);
                    
                        projectile.velocity.Y = 30f;
                        projectile.velocity.X = 2f * projectile.direction;
                    }
                    if (anchor.range <= 285)
                    {
                        // Blip sound to 
                        if (anchor.range == 285) SoundEngine.PlaySound(SoundID.MaxMana);
                        
                        if (anchor.range <= 270) Dust.NewDustPerfect(projectile.position, DustID.Copper, -(projectile.velocity*Main.rand.NextFloat(0.125f, 0.625f)).RotatedByRandom(MathHelper.Pi/18), Scale: 1.125f);
                    
                        
                        guardian.SlamCostUI = 3;
                    
                        projectile.velocity.Y += 0.75f;
                        if (projectile.velocity.Y > 20f) projectile.velocity.Y = 20f;
                        projectile.velocity.X *= 0.95f;
                    
                        if (Main.mouseRight && anchor.Ding && (int)projectile.ai[2] != 1 && guardian.UseSlam(3, true))
                        {
                            projectile.ai[2] = 1f;
                            SoundEngine.PlaySound(SoundID.Item88);
                            projectile.damage = (int)(projectile.damage * 1.5f);
                            guardian.UseSlam(3);
                        }
                    } 
                }
            }
        }

        public override void OnSwing(Player player, OrchidGuardian guardian, Projectile projectile, bool FullyCharged)
        {
            for (int i = 0; i < 2; i++)
            {
                Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Copper, Scale: 0.75f);
                dust.noGravity = true;
            }
        }

        public override void OnThrowHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak)
        {
            if (OrchidMod.ThoriumMod != null) {
                int debuffType = OrchidMod.ThoriumMod.Find<ModBuff>("Sundered").Type;
                target.AddBuff(debuffType, 180);
            }
        }

        public override void OnMeleeHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged)
        {
            if (OrchidMod.ThoriumMod != null) {
                int debuffType = OrchidMod.ThoriumMod.Find<ModBuff>("Sundered").Type;
                target.AddBuff(debuffType, 150);
            }
        }

        public override void OnBlockHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit)
        {
            if (OrchidMod.ThoriumMod != null) {
                int debuffType = OrchidMod.ThoriumMod.Find<ModBuff>("Sundered").Type;
                target.AddBuff(debuffType, 90);
            }
        }

        public override void OnThrowTileCollide(Player player, OrchidGuardian guardian, Projectile projectile, Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath43);
            for (int i = 0; i < Main.rand.Next(5,9); i++) Dust.NewDustPerfect(projectile.position, DustID.Copper, (-oldVelocity*Main.rand.NextFloat(0.125f, 0.625f)).RotatedByRandom(MathHelper.Pi/18), Scale: Main.rand.NextFloat(0.75f, 2.25f));
            DoBlastStuff(projectile, (int)projectile.ai[2] == 1);
        }

        
        public override void AddRecipes()
        {
            var thoriumMod = OrchidMod.ThoriumMod;
            if (thoriumMod != null) {
                CreateRecipe()
                .AddTile(TileID.MythrilAnvil)
                .AddIngredient(thoriumMod, "LodeStoneIngot", 12)
                .Register();
            }
        }

        private static void DoBlastStuff(Projectile projectile, bool uberCharged, NPC hitTarget = null)
        {
            if (projectile.active && projectile.ModProjectile is GuardianHammerAnchor anchor)
            {
                int verticalOffset = 40;
                if (hitTarget != null) verticalOffset += (int)(hitTarget.height * 0.5f);
                
                // We only want the big explosion if the hammer was fully charged before Ultra Smashing
                // (this may or may not already be covered for in ExtraAI() but it's good to be sure)
                int blastProjType = uberCharged && anchor.Ding ? ThoriumMod.Find<ModProjectile>("LodestoneStaffPro4").Type : ThoriumMod.Find<ModProjectile>("LodestoneStaffPro2").Type;
                // Boom projectile
                Projectile.NewProjectile(
                    projectile.GetSource_FromAI(), 
                    projectile.Center - Vector2.UnitY * verticalOffset, 
                    Vector2.Zero, blastProjType, 
                    (int)(projectile.damage * 0.2f), 
                    projectile.knockBack, 
                    projectile.owner
                );
                
                // We only want to make the rocks fly out if the hammer was fully charged: otherwise it just wouldn't have the "oomf" to it
                if (anchor.Ding)
                    for (int rock = 0; rock < 3 + (uberCharged ? 4 : 0); rock++)
                        Projectile.NewProjectile(
                            projectile.GetSource_FromAI(),
                            projectile.position - Vector2.UnitY * verticalOffset, 
                            new Vector2(Main.rand.NextFloat(-0.5f, 0.5f) * 3.5f, -Main.rand.NextFloat(1.25f,2.5f)*(uberCharged ? 4 : 2)),
                            ThoriumMod.Find<ModProjectile>("LodestoneStaffPro5").Type, 
                            (int)(projectile.damage * 0.25f), 
                            projectile.knockBack, 
                            projectile.owner
                        );
                SoundEngine.PlaySound(SoundID.Item14); 
                // Kill the projectile so it can only go "boom" once
                projectile.Kill();
            }
        }
    }
    
}
