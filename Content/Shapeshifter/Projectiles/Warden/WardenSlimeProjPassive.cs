using Microsoft.Xna.Framework;
using OrchidMod.Content.Shapeshifter.Weapons.Warden;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace OrchidMod.Content.Shapeshifter.Projectiles.Warden
{
	public class WardenSlimeProjPassive : OrchidModShapeshifterProjectile
	{
		public override void SafeSetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 600;
			Projectile.scale = 1f;
			Projectile.alpha = 64;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
			Main.projFrames[Projectile.type] = 7;
			Projectile.frame = 0;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			fallThrough = false;
			return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.frame == 0 && (Projectile.velocity.Y > 0f || Projectile.timeLeft < 540))
			{
				Projectile.frame = 1 + Main.rand.Next(6);
				SoundEngine.PlaySound(SoundID.NPCHit1);
			}

			Projectile.velocity *= 0f;

			return false;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.X * 0.1f;
			Projectile.velocity.X *= 0.975f;
			Projectile.velocity.Y += 0.3f;

			if (Projectile.timeLeft < 63)
			{
				Projectile.alpha += 3;
			}

			if (Projectile.velocity.Y > 10f) 
			{
				Projectile.velocity.Y = 10f;
			}

			if (IsLocalOwner && Projectile.frame != 0)
			{ // only called on the owner to avoid weird desync stuff
				foreach (Player player in Main.player)
				{
					if (player.active && player.statLifeMax2 > player.statLife)
					{
						OrchidShapeshifter shapeshifter = Shapeshifter(player);
						if (shapeshifter.IsShapeshifted)
						{ // Heals any friendly player that touches the projectile
							if (shapeshifter.ShapeshiftAnchor.ShapeshifterItem.ModItem is WardenSlime && player.Hitbox.Intersects(Projectile.Hitbox) && player.team == Main.player[Projectile.owner].team)
							{
								shapeshifter.modPlayer.TryHeal(Projectile.damage);
								Projectile.Kill();

								SoundStyle sound = SoundID.Item3;
								sound.Volume *= 0.7f;
								sound.Pitch -= 0.2f;
								SoundEngine.PlaySound(sound, Projectile.Center);
							}
						}
					}
				}
			}
		}
	}
}