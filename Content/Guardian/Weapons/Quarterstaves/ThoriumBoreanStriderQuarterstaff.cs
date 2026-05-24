using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod;
using OrchidMod.Common.Attributes;
using OrchidMod.Content.Guardian;
using OrchidMod.Content.Guardian.Projectiles.Gauntlets;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	[CrossmodContent("ThoriumMod")]
    public class ThoriumBoreanStriderQuarterstaff : OrchidModGuardianQuarterstaff
    {
        public float SnowflakeStacks;    

        public override void SafeSetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.value = Item.sellPrice(0, 2);
            Item.rare = ItemRarityID.Pink;
            Item.useTime = 25;
            ParryDuration = 60;
            Item.knockBack = 4f;
            Item.shootSpeed = 8f;
            Item.damage = 140;
            GuardStacks = 1;
            SlamStacks = 1;
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(SnowflakeStacks);
        }

        public override void NetReceive(BinaryReader reader)
        {
            SnowflakeStacks = reader.ReadSingle();
        }

        public override void HoldItemFrame(Player player)
        {
	        if (player.dead)
	        {
		        SnowflakeStacks = 0;
		        Item.NetStateChanged();
	        }
        }

        public override void ExtraAIQuarterstaff(Player player, OrchidGuardian guardian, Projectile projectile)
        {
	        if (projectile.ai[0] <= 1f && projectile.localAI[1] == 1)
	        {
		        projectile.localAI[1] = 0;
		        projectile.netUpdate = true;
	        }
        }

        public override void OnAttack(Player player, OrchidGuardian guardian, Projectile projectile, bool jabAttack, bool counterAttack)
        {
            Dust swingDust = Dust.NewDustDirect(projectile.Center, projectile.width * 2, projectile.height * 2, DustID.HallowSpray, Scale: 0.75f);
            swingDust.noGravity = true;
            
            if (projectile.ModProjectile is GuardianQuarterstaffAnchor anchor && anchor.Ding && !jabAttack && projectile.localAI[1] == 0 && SnowflakeStacks >= 6f)
            {
	            projectile.localAI[1] = 1;
	            projectile.netUpdate = true;
	            SnowflakeStacks = -1;
                for (int i = -2; i < 3; i++)
                {
                    Vector2 velocity = Vector2.Normalize(Main.MouseWorld - player.MountedCenter) * Item.shootSpeed * (4+i)/4;
                    int damage = guardian.GetGuardianDamage(Item.damage * 0.5f);
                    Vector2 tipPosition = projectile.Center - Vector2.UnitY.RotatedBy(projectile.rotation + MathHelper.PiOver4) * projectile.width * 0.1f;
                    
                    Projectile snowflakeProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromAI(), player.MountedCenter, velocity, ProjectileID.NorthPoleSnowflake, damage, Item.knockBack, projectile.owner);
                    snowflakeProjectile.CritChance = guardian.GetGuardianCrit(Item.crit);
                    snowflakeProjectile.rotation = snowflakeProjectile.velocity.ToRotation();
                    snowflakeProjectile.DamageType = ModContent.GetInstance<GuardianDamageClass>();
                    snowflakeProjectile.netUpdate = true;
                }
                SoundEngine.PlaySound(SoundID.Item28);
                SnowflakeStacks = 0;
                Item.NetStateChanged();
            }
        }

        public override void OnHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool jabAttack, bool counterAttack)
        {
            if (projectile.ModProjectile is GuardianQuarterstaffAnchor anchor) {
                if (anchor.FirstHit && projectile.localAI[1] == 0)
                { 
	                // if (SnowflakeStacks == 9) SoundEngine.PlaySound(SoundID.Item4);
	                // SnowflakeStacks++;
	                // if (SnowflakeStacks > 10) SnowflakeStacks = 10;
	                // if (SnowflakeStacks <= 10) CombatText.NewText(projectile.getRect(), SnowflakeStacks == 10 ? Color.DodgerBlue : Color.LightSkyBlue, SnowflakeStacks.ToString(), SnowflakeStacks == 10);

	                if (SnowflakeStacks < 6)
	                {
		                float toAdd = 0;
		                if (counterAttack) toAdd = 3;
		                else if (!jabAttack) toAdd = 2;
		                else toAdd = 1;

		                if ((SnowflakeStacks += toAdd) >= 6f)
		                {
			                SnowflakeStacks = 6f;
			                SoundEngine.PlaySound(SoundID.Item4);
			                for (int i = 0; i < 10; i++)
			                {
				                Dust dust = Dust.NewDustPerfect(player.Center, DustID.FrostHydra, Main.rand.NextVector2CircularEdge(2.5f, 2.5f), Scale: 3f);
				                dust.noGravity = true;
			                }
		                }
	                }

	                Item.NetStateChanged();
                }

                Mod thoriumMod = OrchidMod.ThoriumMod;
                if (thoriumMod != null)
                {
	                int debuffType = thoriumMod.Find<ModBuff>("Freezing").Type;
	                target.AddBuff(debuffType, 120);
                }
            }
        }

        public override void QuarterstaffPostDrawUI(SpriteBatch spriteBatch, Player player, ref Color lightColor, Projectile projectile)
        {
	        Vector2 position = (player.position + new Vector2(player.width * 0.5f, player.height + player.gfxOffY + 12)).Floor();
	        Vector2 drawpos = new Vector2(position.X + 22, position.Y - 94 * player.gravDir + 3f * (player.gravDir - 1)) - Main.screenPosition;

	        Texture2D snowflakeUIOff = ModContent.Request<Texture2D>(Texture + "_UIOff").Value;
	        Texture2D snowflakeUIOn = ModContent.Request<Texture2D>(Texture + "_UIOn").Value;
	        Texture2D snowflakeUIReady = ModContent.Request<Texture2D>(Texture + "_UIReady").Value;
	        
	        if (OrchidMod.OrchidClientConfig.GuardianThoriumBoreanStriderColorUI)
		        snowflakeUIOn = ModContent.Request<Texture2D>(ModContent.GetInstance<IceGauntletProjectile>().Texture).Value;

	        
	        
	        int val = 26;
	        float stacks = SnowflakeStacks / 6f; 
	        while (stacks < 1)
	        {
		        stacks += 0.0385f;
		        val--;
	        }
	        Rectangle rectangle = snowflakeUIOff.Bounds;
	        rectangle.Height = val;
	        rectangle.Y = snowflakeUIOff.Height - val;
	        
	        if (SnowflakeStacks >= 6)
				spriteBatch.Draw(snowflakeUIReady, drawpos - new Vector2(2f, 2f), null, Color.White * 0.8f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
	        
	        spriteBatch.Draw(snowflakeUIOff, drawpos, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
	        drawpos.Y += snowflakeUIOff.Height - val;
	        spriteBatch.Draw(snowflakeUIOn, drawpos, rectangle, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        }
    }
}

