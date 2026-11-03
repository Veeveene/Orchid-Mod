using Microsoft.Xna.Framework;
using OrchidMod.Content.Shapeshifter.Weapons.Warden;
using Terraria;

namespace OrchidMod.Content.Shapeshifter.Projectiles.Warden
{
	public class WardenSlimeProj : OrchidModShapeshifterProjectile
	{
		public override void SafeSetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 2;
			Projectile.scale = 1f;
			Projectile.alpha = 96;
			Projectile.penetrate = -1;
			Projectile.alpha = 255;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 60;
			Projectile.friendly = false;
			MeleeHit = true;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (IsLocalOwner)
			{  // Can't hit an enemy already hit by the falling player
				OrchidShapeshifter shapeshifter = Shapeshifter();
				if (shapeshifter.IsShapeshifted)
				{
					if (shapeshifter.ShapeshiftAnchor.ShapeshifterItem.ModItem is WardenSlime slime)
					{
						if (slime.HitNPCs.Contains(target.whoAmI))
						{
							return false;
						}
					}
				}

				if (!Collision.CanHitLine(Projectile.Center, 2, 2, target.position, target.width, target.height))
				{
					return false;
				}
			}

			return base.CanHitNPC(target);
		}

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidShapeshifter shapeshifter)
		{
			if (shapeshifter.IsShapeshifted)
			{
				if (shapeshifter.ShapeshiftAnchor.ShapeshifterItem.ModItem is WardenSlime slime)
				{
					slime.TrySpawnHealingGoo(shapeshifter.ShapeshiftAnchor.Projectile, player, shapeshifter);
				}
			}
		}

		public override void AI()
		{
			if (Projectile.ai[0] >= 2f)
			{ // full charge slam, big hitbox
				Projectile.width = 112;
				Projectile.height = 112;
				Projectile.ai[1] = 15f;
			} 
			else
			{ // normal hitbox
				Projectile.width = 80;
				Projectile.height = 80;
				Projectile.ai[1] = 4f * Projectile.ai[0];
			}

			Projectile.friendly = true;
			Projectile.position = Main.player[Projectile.owner].Center - new Vector2(Projectile.width, Projectile.height) * 0.5f;

			foreach (NPC npc in Main.npc)
			{ // yeet
				if (IsValidTarget(npc) && npc.Hitbox.Intersects(Projectile.Hitbox) && npc.knockBackResist > 0f)
				{
					npc.velocity = new Vector2(Projectile.ai[1] * (npc.Center.X > Projectile.Center.X ? 1 : -1), - Projectile.ai[1]) * npc.knockBackResist;
					npc.netUpdate = true;
				}
			}
		}
	}
}