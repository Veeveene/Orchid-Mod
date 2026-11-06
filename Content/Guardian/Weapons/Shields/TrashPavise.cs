using System;
using Microsoft.Xna.Framework;
using OrchidMod.Assets;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	public class TrashPavise : OrchidModGuardianShield
	{
		public bool FirstBlock = false;
		public Vector2 ShakeAmount = Vector2.Zero;
		private Vector2 OldShakeAmount = Vector2.Zero;
		
		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 0, 0, 0);
			Item.width = 38;
			Item.height = 38;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item1;
			Item.knockBack = 16f;
			Item.damage = 54;
			Item.rare = ItemRarityID.Gray;
			Item.useTime = 24;
			distance = 32f;
			slamDistance = 55f;
			blockDuration = 60;
			blockRotation = 5f;
		}

		public override void ExtraAIShield(Player player, OrchidGuardian guardian, Projectile shield)
		{
			if (shield.ModProjectile is GuardianShieldAnchor anchor)
			{
				if (shield.ai[0] > 0)
				{
					if (ShakeAmount.Length() > 0)
					{
						OldShakeAmount = ShakeAmount;
						shield.Center += OldShakeAmount;
						ShakeAmount -= Vector2.Normalize(ShakeAmount) * MathF.Max(ShakeAmount.Length() % 0.4f, 0);
						if (ShakeAmount.Length() > 8f)
							ShakeAmount *= 8f / ShakeAmount.Length();
						shield.Center -= ShakeAmount;
					}
				}
				else
				{
					if (ShakeAmount.Length() > 0f)
						ShakeAmount = OldShakeAmount = Vector2.Zero;
				}
				
			}
		}

		public override bool Block(Player player, OrchidGuardian guardian, Projectile shield, Projectile projectile)
		{
			FirstBlock = true;
			ShakeAmount -= Vector2.Normalize(projectile.velocity) * 2f;
			Dust.NewDustDirect(shield.Center + 0.5f * (projectile.Center - shield.Center), 2, 2, DustID.Lead, -projectile.velocity.X * 0.5f, -projectile.velocity.Y * 0.5f);
			SoundEngine.PlaySound(new SoundStyle(OrchidAssets.SoundsPath + "Doink") { PitchVariance = 0.4f, Volume = FirstBlock ? 1f : 0.5f, MaxInstances = 5 }, shield.Center);
			return true;
		}

		public override bool BlockEnd(Player player, OrchidGuardian guardian, Projectile shield)
		{
			FirstBlock = false;
			return true;
		}

		public override void SlamHit(Player player, OrchidGuardian guardian, Projectile shield, NPC npc, bool WeakSlam)
		{
			npc.AddBuff(BuffID.Stinky, WeakSlam ? 100 : 300);
		}
	}
}
