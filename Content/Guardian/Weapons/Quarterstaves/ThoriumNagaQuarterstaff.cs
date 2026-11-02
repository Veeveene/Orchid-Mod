using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using OrchidMod.Common;
using OrchidMod.Common.ModObjects;
using OrchidMod.Common.Attributes;
using OrchidMod.Content.Guardian.Projectiles.Quarterstaves;
using OrchidMod.Utilities;
using Terraria.GameContent.Dyes;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves 
{

	[CrossmodContent("ThoriumMod")]
    public class ThoriumNagaQuarterstaff : OrchidModGuardianQuarterstaff
    {        
        public bool underWater;
        public bool wasUnderWater;
        public int waterAttack = 0;
        public int waterAttackSuper = 0;
        private int waterAttackCooldown = 0;

        public override bool IsLoadingEnabled(Mod mod) => false;

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
            Item.shootSpeed = 15f;
            JabStyle = 1;
            JabSpeed = 0.9f;
            JabDamage = 0.75f;
            JabChargeGain = 1.5f;
            SwingStyle = 0;
            SwingSpeed = 0.8f;
            GuardStacks = 1;
            SlamStacks = 1;

            waterAttack = 0;
        }

        public override void SafeHoldItem(Player player)
        {
            Projectile anchor = Main.projectile.FirstOrDefault(proj => proj.active && proj.whoAmI < Main.maxProjectiles && proj.owner == Main.myPlayer && proj.type == AnchorType);
            
            if (waterAttack > 0)
            {
	            if (anchor?.ai[0] > 20f) SwingSpeed = 1 / (5 * player.GetTotalAttackSpeed(DamageClass.Melee));
	            else SwingSpeed = 0.25f;
            }
            else SwingSpeed = 0.8f;

            JabStyle = 0;

            player.trident = true;
        }

        public override void OnAttack(Player player, OrchidGuardian guardian, Projectile projectile, bool jabAttack, bool counterAttack)
		{
            if (projectile.ModProjectile is GuardianQuarterstaffAnchor)
            {
                if (!jabAttack)
                {
                    if (!counterAttack && underWater) 
                    {
                        SoundEngine.PlaySound(SoundID.Item109, player.Center);
                        if (waterAttack == 0) 
                        {
	                        StartLunge(player, projectile, (Main.MouseWorld - player.Center).ToRotation());
                            waterAttack = 1;
                            waterAttackCooldown = 10;
                        }
                    }
                }
                else
                {
	                if (IsLocalPlayer(player))
	                {
		                Vector2 velocity = Vector2.UnitY.RotatedBy((player.Center - Main.MouseWorld).ToRotation() + MathHelper.PiOver2) * Item.shootSpeed + player.velocity;
		                Vector2 tipPosition = projectile.Center - Vector2.UnitY.RotatedBy(projectile.rotation + MathHelper.PiOver4) * projectile.width * 0.5f;
		                
		                int damage = guardian.GetGuardianDamage(Item.damage * 0.05f);
		                int projectileType = ModContent.ProjectileType<ThoriumNagaQuarterstaffProjectile>();
		                Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromAI(), tipPosition + player.velocity, velocity, projectileType, damage, Item.knockBack, projectile.owner);
		                newProjectile.CritChance = guardian.GetGuardianCrit(Item.crit);

		                for (int i = 0; i < 10; i++)
		                {
			                Dust dust = Dust.NewDustPerfect(tipPosition, DustID.GreenFairy, Main.rand.NextVector2CircularEdge(2.5f, 2.5f), Scale: 2f, newColor: Color.DarkCyan);
			                dust.noGravity = true;
		                }

		                SoundEngine.PlaySound(SoundID.Item111 with { PitchVariance = 0.8f, Volume = 0.5f }, tipPosition);
	                }
                }
            }
			
		}

        public override void ExtraAIQuarterstaff(Player player, OrchidGuardian guardian, Projectile projectile)
        {
	        underWater = Collision.DrownCollision(player.position, player.width, player.height, player.gravDir, true);

            waterAttackCooldown--;
            if (waterAttackCooldown <= 0) waterAttackCooldown = 0;
            
            OrchidPlayer orchidPlayer = player.GetModPlayer<OrchidPlayer>();
            if (projectile.ai[0] > 1)
            {
	            Vector2 tipPosition = projectile.Center - Vector2.UnitY.RotatedBy(projectile.rotation + MathHelper.PiOver4) * projectile.width * 0.5f;
	            Projectile bubble = Main.projectile.FirstOrDefault(proj => proj.active && proj.whoAmI < Main.maxProjectiles && proj.owner == Main.myPlayer && proj.type == ModContent.ProjectileType<ThoriumNagaQuarterstaffProjectile>() && OrchidUtils.CheckAABBvCircularCollision(proj.Hitbox, new Circle(tipPosition, 32f)));
	            if (waterAttackCooldown == 0 && bubble != null)
	            {
		            bubble.Kill();
		            projectile.ai[0] = 40;
		            StartLunge(player, projectile, (Main.MouseWorld - player.Center).ToRotation());
		            SoundEngine.PlaySound(SoundID.Item150);
		            waterAttackSuper++;
		            if (waterAttackSuper == 3)
		            {
			            SoundEngine.PlaySound(SoundID.MaxMana);
			            for (int i = 0; i < 10; i++)
			            {
				            Dust dust = Dust.NewDustPerfect(player.Center, DustID.GreenFairy, Main.rand.NextVector2CircularEdge(2.5f, 2.5f), Scale: 2f, newColor: Color.DarkCyan);
				            dust.noGravity = true;
			            }
		            }
		            waterAttackCooldown = 10;
		            if (waterAttack == 0) waterAttack = 1;
	            }
	            
	            if (underWater && Main.rand.NextBool(4)) Dust.NewDustDirect(projectile.Center, player.width, player.height, DustID.BreatheBubble, Scale: Main.rand.NextFloat(1.5f, 3.5f));
                    
                bool attackInput = Main.mouseLeft && Main.mouseLeftRelease;
                if (ModContent.GetInstance<OrchidClientConfig>().GuardianSwapGauntletInputs) attackInput = Main.mouseRight && Main.mouseRightRelease;
                
                if (waterAttack == 1) 
                {
                    player.armorEffectDrawShadowEOCShield = true;
                    if (underWater) ((WaterShaderData)Filters.Scene["WaterDistortion"].GetShader()).QueueRipple(projectile.Center, 2.5f, RippleShape.Circle);
                    
                    
                    if (projectile.ai[0] < 20 || attackInput)
                    {
                        if (waterAttackSuper >= 3)
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
                        else
	                        projectile.ai[0] = 1;
                    }
                }
            }
            else
            {
                waterAttack = 0;
                waterAttackSuper = 0;
                waterAttackCooldown = 0;
            }
            
            if (wasUnderWater && !underWater && waterAttack == 1) // Dolphin leap
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
            
                waterAttack = 0;
            }

            wasUnderWater = underWater;
        }

        public override bool PreSwingAI(Player player, OrchidGuardian guardian, Projectile projectile)
        {
            if (waterAttack == 2)
            {
                projectile.rotation = projectile.ai[1] - MathHelper.PiOver4 + MathHelper.TwoPi * MathF.Sin(projectile.ai[0] * MathHelper.Pi / 20) * -player.direction;
				projectile.Center = player.MountedCenter.Floor() + Vector2.UnitY.RotatedBy(MathHelper.TwoPi * MathF.Sin(projectile.ai[0] * MathHelper.Pi / 20) * -player.direction) * 60f;
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.PiOver4 * player.direction + projectile.ai[1] + 0.1f - (float)Math.Cos(0.3142f * (projectile.ai[0] - 9)) * player.direction);
				player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, projectile.ai[1] - 0.1f + (float)Math.Cos(0.3142f * (projectile.ai[0]- 9)) * 0.2f * player.direction);
                
                return false;
            }
            return true;
        }

        public override bool PreDrawQuarterstaff(SpriteBatch spriteBatch, Projectile projectile, Player player, ref Color lightColor, SpriteEffects effects)
        {
	        Texture2D lungeTexture = ModContent.Request<Texture2D>(Texture + "_Lunge").Value;
	        if (waterAttack == 1)
	        {
		        Vector2 tipPosition = projectile.Center - Vector2.UnitY.RotatedBy(projectile.rotation + MathHelper.PiOver4) * projectile.width * 0.5f + player.velocity;
		        
		        spriteBatch.End(out SpriteBatchSnapshot snapshot);
		        spriteBatch.Begin(snapshot with { BlendState = BlendState.Additive });
		        
		        spriteBatch.Draw(lungeTexture, tipPosition, lungeTexture.Frame(1, 2, 0, 0), Color.SeaGreen * 0.5f, projectile.rotation, lungeTexture.Frame(1, 2, 0, 0).Size() * 0.5f, projectile.scale * 1.5f, effects, 0f);
		        spriteBatch.Draw(lungeTexture, tipPosition, lungeTexture.Frame(1, 2, 0, 0), Color.SeaGreen * 0.9f, projectile.rotation, lungeTexture.Frame(1, 2, 0, 0).Size() * 0.5f, projectile.scale * 1.4f, effects, 0f);
		        spriteBatch.Draw(lungeTexture, tipPosition, lungeTexture.Frame(1, 2, 0, 1), Color.Gold * 0.9f, projectile.rotation, lungeTexture.Frame(1, 2, 0, 1).Size() * 0.5f, projectile.scale * 1.4f, effects, 0f);
		        
		        spriteBatch.End();
		        spriteBatch.Begin(snapshot);
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

		public void StartLunge(Player player, Projectile projectile, float direction)
		{
			if (player.velocity.Y == 0) player.position.Y -= 16;

			OrchidPlayer orchidPlayer = player.OrchidPlayer();
			Vector2 velocity = Vector2.UnitX.RotatedBy(direction) * 20f;
			orchidPlayer.ForcedVelocityVector = velocity;
			orchidPlayer.ForcedVelocityTimer = 60;
			orchidPlayer.PlayerImmunity = 20;
			orchidPlayer.ForcedVelocityUpkeep = 0;
			
			// int dashType = ModContent.ProjectileType<ThoriumNagaQuarterstaffProjectileDash>();
			// if (player.ownedProjectileCounts[dashType] != 1)
			// {
			// 	Projectile dash = Projectile.NewProjectileDirect(projectile.GetSource_FromAI(), projectile.Center, velocity, dashType, 0, 0);
			// 	((ThoriumNagaQuarterstaffProjectileDash)dash.ModProjectile).Anchor = projectile;
			// }
			
		}
    }    
}
