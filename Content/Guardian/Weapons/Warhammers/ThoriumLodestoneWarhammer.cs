using Microsoft.Xna.Framework;
using Terraria;
using OrchidMod.Common.Attributes;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

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
            Item.damage = 198;
            Item.useTime = 30;
            Range = 300; // really high so that hammer can potentially hit the ground if you do something silly like launch it from 200ft in the air
            SlamStacks = 2;
            ReturnSpeed = 0.5f;
            BlockDuration = 240;
            HoldOffset = -2;
            CannotMagnet = true;
            CannotExplode = true;
			Penetrate = true;
		}
        

        public override void ExtraAI(Player player, OrchidGuardian guardian, Projectile projectile)
        {
            
            // Generic dust particles
            if (Main.rand.NextBool(3))
			{ // From Lodestone Staff source
				Main.dust[Dust.NewDust(new Vector2(projectile.Center.X - 16f, projectile.Center.Y - 16f), 32, 32, DustID.Dirt, projectile.velocity.X / 2f, projectile.velocity.Y / 2f, 150, new Color(), 1.5f)].noGravity = true;
			}

            if (projectile.ModProjectile is GuardianHammerAnchor anchor)
            {
                // Behavior while throwing (affected by gravity, can be slammed)
                if (projectile.ai[1] > 0 && anchor.range > 0)
                {
                    // Behavior for Ultra Smash effect
                    if ((int)projectile.ai[2] == 1)
                    {
						for (int i = 0; i < 6; i++)
						{
							Dust dust;
							if (Main.rand.NextBool()) dust = Dust.NewDustDirect(new Vector2(projectile.Center.X - 16f, projectile.Center.Y - 16f), 32, 32, DustID.Dirt);
							else dust = Dust.NewDustDirect(new Vector2(projectile.Center.X - 16f, projectile.Center.Y - 16f), 32, 32, DustID.Torch);
							dust.velocity = projectile.velocity *= 0.5f;
							dust.noGravity = true;
							dust.scale *= Main.rand.NextFloat(1.25f, 2f);
						}

                        if (anchor.range % 4 == 0) SoundEngine.PlaySound(SoundID.Item34);
                    
                        projectile.velocity.Y = 30f;
                        projectile.velocity.X = 2f * projectile.direction;
                    }

                    if (anchor.range <= 285)
                    {
                        // Blip sound to 
                       //  if (anchor.range == 285) SoundEngine.PlaySound(SoundID.MaxMana); // I don't like the bleep
                        guardian.SlamCostUI = 3;
                    
                        projectile.velocity.Y += 0.75f;
                        if (projectile.velocity.Y > 20f) projectile.velocity.Y = 20f;
                        projectile.velocity.X *= 0.95f;
                    
                        if (Main.mouseLeft && Main.mouseLeftRelease && anchor.Ding && IsLocalPlayer(player) && (int)projectile.ai[2] != 1 && guardian.UseSlam(3, true))
						{
							guardian.UseSlam(3);
							SoundEngine.PlaySound(SoundID.Item88);
							projectile.damage = (int)(projectile.damage * 1.5f);
							projectile.ai[2] = 1f;
							projectile.netUpdate = true;
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
				target.AddBuff(debuffType, 300);
            }
        }

        public override void OnMeleeHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged)
        {
            if (OrchidMod.ThoriumMod != null) {
                int debuffType = OrchidMod.ThoriumMod.Find<ModBuff>("Sundered").Type;
                target.AddBuff(debuffType, 90);
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

			int dustType1 = -1;
			int dustType2 = -1;

			Point point = new Point((int)((int)projectile.position.X / 16f), (int)(((int)projectile.position.Y + projectile.height) / 16f));

			int count = 0;
			Tile tile = Framing.GetTileSafely(point);
			while (!tile.HasTile && count < 2)
			{ // sometimes it selects a few tiles too high, so pick the ones below
				count++;
				tile = Framing.GetTileSafely(point);
			}

			if (tile.HasTile)
			{ // pick the correct dusts from the tiles landed on
				dustType1 = Main.dust[WorldGen.KillTile_MakeTileDust(point.X, point.Y, tile)].type;
			}

			point.X++;
			tile = Framing.GetTileSafely(point);
			if (tile.HasTile)
			{
				dustType2 = Main.dust[WorldGen.KillTile_MakeTileDust(point.X, point.Y, tile)].type;
			}

			// From my Slime wildshape code
			int dustAmount = projectile.ai[2] == 1 ? 5 : 2;
			float velocityMult = projectile.ai[2] == 1 ? 1f : 0.6f;

			if (dustType1 != -1)
			{
				for (int i = 0; i < dustAmount * 2; i++)
				{
					Dust dust = Dust.NewDustDirect(projectile.position + new Vector2(0f, projectile.height), projectile.width, 0, dustType1);
					dust.scale *= Main.rand.NextFloat(0.8f, 1.2f);
					dust.velocity *= 0.5f * velocityMult;
					dust.velocity.Y -= 0.85f;
				}

				for (int i = 0; i < dustAmount; i++)
				{
					Dust dust = Dust.NewDustDirect(projectile.position + new Vector2(0f, projectile.height), projectile.width, 0, dustType1);
					dust.scale *= Main.rand.NextFloat(0.8f, 1.2f);
					dust.velocity *= 1.25f * velocityMult;
					dust.velocity.Y -= 1.25f;
				}
			}

			if (dustType2 != -1)
			{
				for (int i = 0; i < dustAmount * 2; i++)
				{
					Dust dust = Dust.NewDustDirect(projectile.position + new Vector2(0f, projectile.height), projectile.width, 0, dustType2);
					dust.scale *= Main.rand.NextFloat(0.8f, 1.2f);
					dust.velocity *= 1.5f * velocityMult;
					dust.velocity.Y -= 1.5f;
				}

				for (int i = 0; i < dustAmount; i++)
				{
					Dust dust = Dust.NewDustDirect(projectile.position + new Vector2(0f, projectile.height), projectile.width, 0, dustType2);
					dust.scale *= Main.rand.NextFloat(0.8f, 1.2f);
					dust.velocity *= 0.75f * velocityMult;
					dust.velocity.Y -= 1f;
				}
			}

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
				bool BigBlast = (uberCharged && anchor.Ding);
				Vector2 position;
				if (hitTarget != null)
				{
					position = hitTarget.Center;
				}
				else
				{
					position = projectile.Center;
					position.Y -= 16f;

					Vector2 offSet = new Vector2(0f, 5f);
					for (int i = 0; i < 10; i++)
					{ // for some reason, Collision.TileCollision does not work properly if the offset vector is too big, so we do a small loop.
						offSet = Collision.TileCollision(position, offSet, 2, 2, true);
						position += offSet;
						if (offSet.Y < 5f)
						{
							break;
						}
					}

					position.Y -= BigBlast ? 38f : 28f; // LodestoneStaffPro2 and LodestoneStaffPro4 require different offsets.
				}
                
                // We only want the big explosion if the hammer was fully charged before Ultra Smashing
                // (this may or may not already be covered for in ExtraAI() but it's good to be sure)
                int blastProjType = BigBlast ? ThoriumMod.Find<ModProjectile>("LodestoneStaffPro4").Type : ThoriumMod.Find<ModProjectile>("LodestoneStaffPro2").Type;
                // Boom projectile
                Projectile blastProj = Projectile.NewProjectileDirect(
                    projectile.GetSource_FromAI(),
					position, 
                    Vector2.Zero, blastProjType, 
                    (int)(projectile.damage * (BigBlast ? 2f : 0.5f)), 
                    projectile.knockBack, 
                    projectile.owner
                );
                blastProj.DamageType = ModContent.GetInstance<GuardianDamageClass>();
                
                // We only want to make the rocks fly out if the hammer was fully charged: otherwise it just wouldn't have the "oomf" to it
                if (anchor.Ding)
                    for (int rock = 0; rock < 3 + (uberCharged ? 4 : 0); rock++) 
                    {
                        Projectile rockProj = Projectile.NewProjectileDirect(
                            projectile.GetSource_FromAI(),
							position, 
                            new Vector2(Main.rand.NextFloat(-0.5f, 0.5f) * 3.5f, -Main.rand.NextFloat(1.25f,2.5f)*(uberCharged ? 4 : 2)),
                            ThoriumMod.Find<ModProjectile>("LodestoneStaffPro5").Type, 
                            (int)(projectile.damage * 0.2f), 
                            projectile.knockBack, 
                            projectile.owner,
                            Main.rand.Next(6),
                            projectile.Center.Y
                        );
                        rockProj.DamageType = ModContent.GetInstance<GuardianDamageClass>();
                    }
                SoundEngine.PlaySound(SoundID.Item14); 
                // Kill the projectile so it can only go "boom" once
                projectile.Kill();
            }
        }
    }
    
}
