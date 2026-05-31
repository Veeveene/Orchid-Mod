using Microsoft.Xna.Framework;
using OrchidMod.Content.Guardian.Projectiles.Warhammers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
	public class TempleWarhammer : OrchidModGuardianHammer
	{
		public override void SafeSetDefaults()
		{
			Item.width = 46;
			Item.height = 46;
			Item.value = Item.sellPrice(0, 7, 0, 0);
			Item.rare = ItemRarityID.Lime;
			Item.UseSound = SoundID.DD2_MonkStaffSwing;
			Item.knockBack = 10f;
			Item.shootSpeed = 13f;
			Item.damage = 243;
			Item.useTime = 20;
			Range = 45;
			GuardStacks = 1;
			SlamStacks = 2;
			SwingSpeed = 1.5f;
			ReturnSpeed = 1.5f;
			BlockDuration = 270;
		}

		public override bool ThrowAI(Player player, OrchidGuardian guardian, Projectile projectile, bool weak, bool OffHand)
		{
			if (Main.rand.NextBool(5))
			{
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.GoldFlame, Scale: Main.rand.NextFloat(1f, 1.2f));
				dust.velocity = dust.velocity * 0.25f + projectile.velocity * 0.2f;
			}

			if (Main.rand.NextBool(15) && !weak && IsLocalPlayer(player))
			{
				Vector2 dir = Vector2.Normalize(projectile.velocity.RotatedByRandom(MathHelper.ToRadians(45f))) * (7f + Main.rand.NextFloat(8f) * (projectile.timeLeft > 600 - Range ? 1 : -1));
				Projectile.NewProjectile(Item.GetSource_FromThis(), projectile.Center, dir, ModContent.ProjectileType<TempleWarhammerProj>(), (int)(projectile.damage * 0.75f), Item.knockBack, player.whoAmI);
			}

			return true;
		}

		public override void OnMeleeHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged, bool OffHand)
		{
			if (IsLocalPlayer(player))
			{
				int type = ModContent.ProjectileType<TempleWarhammerProj>();
				Vector2 dir = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi) * (7f + Main.rand.NextFloat(8f));
				Projectile.NewProjectile(Item.GetSource_FromThis(), projectile.Center, dir, type, (int)(projectile.damage * 0.6f), Item.knockBack, player.whoAmI);
			}
		}

		public override void OnMeleeHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged, bool OffHand)
		{
			if (IsLocalPlayer(player))
			{
				int type = ModContent.ProjectileType<TempleWarhammerProj>();
				Vector2 dir = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi) * (7f + Main.rand.NextFloat(8f));
				Projectile.NewProjectile(Item.GetSource_FromThis(), projectile.Center, dir, type, (int)(projectile.damage * 0.6f), Item.knockBack, player.whoAmI);
			}

			for (int i = 0; i < 10; i++)
			{
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.GoldFlame, Scale: Main.rand.NextFloat(1.3f, 1.6f));
				dust.velocity *= 2f;
			}
		}

		public override void OnThrowHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak, bool OffHand)
		{
			if (IsLocalPlayer(player))
			{
				int nb = Main.rand.Next(5);
				if (!Weak) nb += 6;
				int type = ModContent.ProjectileType<TempleWarhammerProj>();
				for (int i = 0; i < nb; i++)
				{
					Vector2 dir = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 6f * i).RotatedByRandom(MathHelper.ToRadians(15f)) * (7f + Main.rand.NextFloat(8f));
					Projectile.NewProjectile(Item.GetSource_FromThis(), projectile.Center, dir, type, (int)(projectile.damage * 0.6f), Item.knockBack, player.whoAmI);
				}
			}

			for (int i = 0; i < 20; i++)
			{
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.GoldFlame, Scale: Main.rand.NextFloat(1.3f, 1.6f));
				dust.velocity *= 2f;
			}
		}
	}
}
