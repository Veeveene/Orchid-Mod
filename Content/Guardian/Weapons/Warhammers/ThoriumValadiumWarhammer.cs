using System;
using Microsoft.Xna.Framework;
using OrchidMod;
using Terraria;
using OrchidMod.Common.Attributes;
using OrchidMod.Content.Guardian;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers {

    [CrossmodContent("ThoriumMod")]
    public class ValadiumWarhammer : OrchidModGuardianHammer
    {
    
        private static Mod ThoriumMod => ModLoader.GetMod("ThoriumMod");
        private static ModBuff ThoriumLightCurseDebuff => ThoriumMod.Find<ModBuff>("LightCurse");

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
            Item.knockBack = 8f;
            Item.shootSpeed = 10f;
            Item.damage = 212;
            Item.useTime = 18;
            Range = 60;
            TileBounce = true;
            GuardStacks = 1;
            SlamStacks = 1;
            ReturnSpeed = 1.5f;
            BlockDuration = 210;
            HoldOffset = -1f;
        }

        public override void OnThrow(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak)
        {
            // Attempt to reset the throw pull cooldown
            projectile.ai[2] = 0;
        }

        public override void ExtraAI(Player player, OrchidGuardian guardian, Projectile projectile)
        {
            if (projectile.ModProjectile is GuardianHammerAnchor anchor)
            {
                // Generic dust particles
                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.PurpleTorch);
                    dust.noGravity = true;
                }
                
                // Throwing behavior: grants the ability to consume a slam to pull enemies to the center
                // Has a 30t cooldown between freebie pulls, but if you're impatient you can spend a slam to pull earlier
                if (projectile.ai[1] > 0 && anchor.range < Range - 10)
                {
                    projectile.ai[2] += 1f;
                    
                    if ((int)projectile.ai[2] <= 5)
                    {
                    }
                    if ((int)projectile.ai[2] > 5)
                    {
                        if ((int)projectile.ai[2] < 30) guardian.SlamCostUI = 1;
                        if ((int) projectile.ai[2] == 30)
                        {
                            SoundEngine.PlaySound(SoundID.MaxMana);
                            for (int i = 0; i < 2; i++) Dust.NewDust(projectile.Center, projectile.width, projectile.height, DustID.ShadowbeamStaff);
                        }
                
                        // Check if right click, and if the cooldown is at least 5 (least common denominator)
                        if (Main.mouseRight && Main.mouseRightRelease && anchor.Ding && (int)projectile.ai[2] > 5)
                        {
                            GravitateNPCs(projectile.Center);
                            // If the player is impatient (i.e. less than 30t) consume a slam
                            if ((int)projectile.ai[2] < 30) guardian.UseSlam(1);
                            projectile.ai[2] = 0f;
                        } 
                    }
                    
                   
                }
                else if (anchor.BlockDuration > 0 && anchor.BlockDuration <= BlockDuration - 30)
                {
                    // Gravitational pulse every 60 ticks while blocking, after 30 ticks
                    if (anchor.BlockDuration % 60 == 0) GravitateNPCs(projectile.Center, 60, 500, dustRingRadius: 120);
                }
            }
        }

        public override void OnSwing(Player player, OrchidGuardian guardian, Projectile projectile, bool FullyCharged)
        {
            for (int i = 0; i < 2; i++)
            {
                Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.PurpleTorch);
                dust.noGravity = true;
            }
        }

        public override void OnThrowHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak)
        {
            target.AddBuff(ThoriumLightCurseDebuff.Type, 180);
        }

        public override void OnMeleeHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged)
        {
            target.AddBuff(ThoriumLightCurseDebuff.Type, 150);
        }

        public override void OnBlockHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit)
        {
            target.AddBuff(ThoriumLightCurseDebuff.Type, 90);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddTile(TileID.MythrilAnvil)
                .AddIngredient(ThoriumMod, "ValadiumIngot", 12)
                .Register();
        }
        
        private static void GravitateNPCs(Vector2 target, float power = 30f, float maxDistance = 250f, float dustRingRadius = 60f, NPC excluded = null)
        {
            SoundEngine.PlaySound(SoundID.Item43);
            
            for (int i = 0; i < 30; i++)
            {
                Vector2 dustPosition = target + Vector2.UnitX.RotatedBy(i * MathHelper.TwoPi / 30) * dustRingRadius;
                Vector2 dustVelocity = Vector2.UnitX.RotatedBy(i * MathHelper.TwoPi / 30) * -(0.15f * dustRingRadius);
                Dust gravDust = Dust.NewDustPerfect(dustPosition, DustID.ShadowbeamStaff, dustVelocity, 240, default, 1.75f);
                gravDust.noGravity = true; 
            }
            
            float sqrMaxDistance = maxDistance * maxDistance;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                float sqrDistance = Vector2.DistanceSquared(target, npc.Center);
                if (npc.CanBeChasedBy() && !NPCID.Sets.BelongsToInvasionOldOnesArmy[npc.type] && !npc.boss && sqrDistance < sqrMaxDistance && !(excluded != null && npc.whoAmI == excluded.whoAmI))
                {
                    Vector2 centerDiff = target - npc.Center;
                    float pullMagnitude = power / centerDiff.Length();
                    Vector2 pullForce = (4f * npc.velocity + centerDiff * pullMagnitude) / 5f;
                            
                    npc.velocity = pullForce;
                    if (Main.netMode == NetmodeID.MultiplayerClient) NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
                }
            }
        }

    }
    
}
