using System;
using System.IO;
using Microsoft.Xna.Framework;
using OrchidMod;
using OrchidMod.Common.Attributes;
using OrchidMod.Content.Guardian;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	[CrossmodContent("ThoriumMod")]
    public class ThoriumBoreanStriderQuarterstaff : OrchidModGuardianQuarterstaff
    {
        public int SnowflakeStacks;    

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
            writer.Write((byte)SnowflakeStacks);
        }

        public override void NetReceive(BinaryReader reader)
        {
            SnowflakeStacks = reader.ReadByte();
        }

        public override void OnAttack(Player player, OrchidGuardian guardian, Projectile projectile, bool jabAttack, bool counterAttack)
        {
            Dust swingDust = Dust.NewDustDirect(projectile.Center, projectile.width * 2, projectile.height * 2, DustID.HallowSpray, Scale: 0.75f);
            swingDust.noGravity = true;
            
            if (projectile.ModProjectile is GuardianQuarterstaffAnchor anchor && anchor.Ding && jabAttack)
            {
                for (int i = -2; i < 3; i++)
                {
                    Vector2 velocity = Vector2.Normalize(Main.MouseWorld - player.MountedCenter) * Item.shootSpeed * (4+i)/4;
                    int damage = guardian.GetGuardianDamage(Item.damage * 0.5f);
                    Vector2 tipPosition = projectile.Center - Vector2.UnitY.RotatedBy(projectile.rotation + MathHelper.PiOver4) * projectile.width * 0.1f;
                    
                    Projectile snowflakeProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromAI(), tipPosition, velocity, ProjectileID.NorthPoleSnowflake, damage, Item.knockBack, projectile.owner);
                    snowflakeProjectile.CritChance = guardian.GetGuardianCrit(Item.crit);
                    snowflakeProjectile.rotation = snowflakeProjectile.velocity.ToRotation();
                    snowflakeProjectile.netUpdate = true;
                }
                SoundEngine.PlaySound(SoundID.Item28);
                SnowflakeStacks = 0;
                Item.NetStateChanged();
            }
        }

        public override void OnHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool jabAttack, bool counterAttack)
        {
            if (OrchidMod.ThoriumMod != null) {
                GuardianQuarterstaffAnchor anchor = projectile.ModProjectile as GuardianQuarterstaffAnchor;
                if (anchor.FirstHit)
                {
                    SnowflakeStacks++;
                    if (SnowflakeStacks > 5) SnowflakeStacks = 5;
                    if (SnowflakeStacks <= 5) CombatText.NewText(projectile.getRect(), SnowflakeStacks == 5 ? Color.DodgerBlue : Color.LightSkyBlue, SnowflakeStacks.ToString(), SnowflakeStacks == 5);
                    Item.NetStateChanged();
                }
                int debuffType = OrchidMod.ThoriumMod.Find<ModBuff>("Freezing").Type;
                target.AddBuff(debuffType, 120);
            }
            
        }
    }
}

