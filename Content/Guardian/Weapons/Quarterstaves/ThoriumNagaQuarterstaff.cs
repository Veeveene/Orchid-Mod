using Microsoft.Xna.Framework;
using Terraria;
using OrchidMod;
using OrchidMod.Common.Attributes;
using OrchidMod.Content.Guardian;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves 
{

	[CrossmodContent("ThoriumMod")]
    public class ThoriumNagaQuarterstaff : OrchidModGuardianQuarterstaff
    {        
        private bool underWater;
        
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
            Item.shootSpeed = 20f;
            JabStyle = 1;
            JabSpeed = 0.9f;
            JabDamage = 0.75f;
            JabChargeGain = 1.5f;
            SwingStyle = 0;
            SwingSpeed = 1.4f;
            CounterSpeed = 0.25f;
            GuardStacks = 1;
            SlamStacks = 1;
        }

        public override void OnHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool jabAttack,
            bool counterAttack)
        {
            base.OnHitFirst(player, guardian, target, projectile, hit, jabAttack, counterAttack);
        }

        public override void ExtraAIQuarterstaff(Player player, OrchidGuardian guardian, Projectile projectile)
        {
            underWater = Collision.DrownCollision(player.position, player.width, player.height, player.gravDir, true);
        }

        public override void ExtraAIQuarterstaffSwinging(Player player, OrchidGuardian guardian, Projectile projectile)
        {
            if (underWater)
            {
                int underwaterChargePhase = 1;
                    
                float targetDirection = Vector2.Normalize(Main.MouseWorld - player.Center).ToRotation();
                Dust.NewDust(player.Center, player.width, player.height, DustID.BreatheBubble);
                switch (underwaterChargePhase)
                {
                    case 1:
                    {
                        int ticksWhileRiptide = 0;
                        
                        // player.fullRotation = targetDirection;
                        // player.ChangeDir(-player.direction);
                        
                        Vector2 riptideVelocity = Vector2.UnitX.RotatedBy(targetDirection) * 4;
                        player.velocity = riptideVelocity;
                        Dust.NewDust(player.Center, player.width, player.height, DustID.BreatheBubble, Scale: Main.rand.NextFloat(1.5f, 3.5f));
                        
                        ticksWhileRiptide++;
                        
                        if (ticksWhileRiptide > 45 || (Main.mouseLeft && Main.mouseLeftRelease)) underwaterChargePhase++;
                        break;
                    }
                    case 2:
                    {
                        for (int i = 0; i < 5; i++) Projectile.NewProjectileDirect(Item.GetSource_FromAI(), player.Center, Vector2.UnitX.RotatedBy(targetDirection + MathHelper.ToRadians(Main.rand.NextFloat(-5f, 5f))) * 4, ProjectileID.FlaironBubble, (int)(Item.damage * 0.5f), Item.knockBack, player.whoAmI);
                        // player.fullRotation = 0;
                        break;
                    }
                }
            }
        }

        public override void ExtraAIQuarterstaffCounterattacking(Player player, OrchidGuardian guardian, Projectile projectile)
        {
            // Override the counter speed
            projectile.ai[2] += -(CounterSpeed*player.GetTotalAttackSpeed(DamageClass.Melee)) + 2;
            
            int bubbleProjID = ProjectileID.Bubble;
            float bubbleDamMult = 0.4f;
            if (underWater)
            {
                bubbleProjID = ProjAIStyleID.FlaironBubble;
                bubbleDamMult *= 0.5f;
                Gore.NewGore(projectile.GetSource_FromThis(), projectile.Center, Vector2.UnitY.RotatedByRandom(MathHelper.Pi/24)*Main.rand.NextFloat(0.25f, 0.75f), 411);
            }
            Projectile.NewProjectileDirect(projectile.GetSource_FromThis(), projectile.Center, Vector2.One.RotatedBy(projectile.rotation) * 2, bubbleProjID, (int)(projectile.damage * bubbleDamMult), projectile.knockBack);
            
        }

        public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (OrchidMod.ThoriumMod != null)
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
