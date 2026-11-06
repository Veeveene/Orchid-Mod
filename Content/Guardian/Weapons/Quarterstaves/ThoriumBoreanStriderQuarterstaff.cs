using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod;
using OrchidMod.Common.Attributes;
using OrchidMod.Common.ModObjects;
using OrchidMod.Content.Guardian;
using OrchidMod.Content.Guardian.Buffs;
using OrchidMod.Content.Guardian.Projectiles.Gauntlets;
using OrchidMod.Content.Guardian.Projectiles.Quarterstaves;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	[CrossmodContent("ThoriumMod")]
    public class ThoriumBoreanStriderQuarterstaff : OrchidModGuardianQuarterstaff
    {
        public Vector2 Tip;

        public override void SafeSetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.value = Item.sellPrice(0, 2);
            Item.rare = ItemRarityID.Pink;
            Item.useTime = 60;
            ParryDuration = 60;
            Item.knockBack = 4f;
            Item.shootSpeed = 12.5f;
            Item.damage = 330;
            GuardStacks = 1;
            SlamStacks = 2;
            JabChargeGain = 1.4f;
            ChargeRate = 0.25f;
            JabSpeed = 0.75f;
        }

        public override bool SafeCanHitNPC(Player player, Projectile anchor, NPC target) => false;

        public override void HoldItemFrame(Player player)
        {
	        if (player.HasBuff<GuardianBoreanStriderQuarterstaffBuff>())
	        {
		        if (ChargeRate < 2f) ChargeRate = 2f;
		        Item.NetStateChanged();
	        }
	        else
	        {
		        if (ChargeRate > 0.25f) ChargeRate = 0.25f;
		        Item.NetStateChanged();
	        }
        }

        public override void ExtraAIQuarterstaff(Player player, OrchidGuardian guardian, Projectile projectile)
        {
	        Tip = projectile.Center - Vector2.UnitY.RotatedBy(projectile.rotation + MathHelper.PiOver4) * projectile.width * 0.25f;

	        if (Main.rand.NextBool(3))
	        {
		         Dust dust = Dust.NewDustDirect(Tip - new Vector2(16), 24, 24, DustID.HallowSpray, Scale: 0.75f);
		         dust.noGravity = true;
	        }
        }

        public override void OnAttack(Player player, OrchidGuardian guardian, Projectile projectile, bool jabAttack, bool counterAttack)
        {
            for (int i = 0; i < 15; i++)
            {
	            Dust swingDust = Dust.NewDustDirect(Tip - new Vector2(4, 4), 8, 8, DustID.HallowSpray, Scale: 0.75f);
	            swingDust.noGravity = true;
            }
            
            if (IsLocalPlayer(player))
            {
	            Vector2 velocity = Vector2.Normalize(Main.MouseWorld - player.MountedCenter) * Item.shootSpeed;
	            if (jabAttack)
		            velocity *= 0.4f;
				else if (counterAttack)
					velocity *= 0.25f;

	            int grantsRSS = (!counterAttack || jabAttack) ? 1 : 0;
	            
	            Projectile flake = Projectile.NewProjectileDirect(projectile.GetSource_FromAI(), player.MountedCenter + Vector2.UnitY.RotatedBy(projectile.ai[1]) * projectile.width * 0.6f, velocity + player.velocity * 0.1f, ModContent.ProjectileType<ThoriumBoreanStriderQuarterstaffProjectile>(), guardian.GetGuardianDamage(Item.damage), Item.knockBack * (!jabAttack && !counterAttack ? 0.5f : 1) , Main.myPlayer, projectile.whoAmI, grantsRSS);
	            if (!jabAttack || counterAttack) // Swings and counters
	            {
		            if (!jabAttack && !counterAttack) player.ClearBuff(ModContent.BuffType<GuardianBoreanStriderQuarterstaffBuff>());
		            ((ThoriumBoreanStriderQuarterstaffProjectile)flake.ModProjectile).Strong = true;
	            }
	            else 
		            flake.damage = (int)(flake.damage * 0.2f);
            }
        }

        public override void OnHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool jabAttack, bool counterAttack) { }
    }
}

