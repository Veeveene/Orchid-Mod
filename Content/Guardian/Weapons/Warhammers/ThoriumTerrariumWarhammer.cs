using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
	public class ThoriumTerrariumWarhammer : OrchidModGuardianHammer
	{

		public static readonly int[] PotentialDusts =
		[
			DustID.GemRuby,
            DustID.InfernoFork,
            DustID.GemTopaz,
            DustID.GemEmerald,
            DustID.Frost,
            DustID.GemSapphire,
            DustID.GemAmethyst
        ];

		public override void SafeSetDefaults()
		{
			Item.width = 44;
			Item.height = 44;
			Item.value = Item.sellPrice(0, 12);
			Item.rare = OrchidMod.ThoriumMod != null ? OrchidMod.ThoriumMod.Find<ModRarity>("TerrariumRarity").Type : ItemRarityID.Expert;
			Item.UseSound = SoundID.Item1;
			Item.knockBack = 4f;
			Item.useTime = 15;
			Item.shootSpeed = 12f;
			Item.damage = 359;
			Range = 30;
			GuardStacks = 1;
			SlamStacks = 1;
			ReturnSpeed = 1.6f;
			SwingChargeGain = 1.5f;
			WaitChargeGain = 2f;
			SwingSpeed = 1.75f;
			HitCooldown = 15;
			BlockDamage = 0.2f;
			Penetrate = true;
			BlockDuration = 80;
			hasSpecialHammerTexture = true;
			HammerFrames = 2;
			DualWarhammers = true;
			CannotBlock = true;
		}

		public override void OnThrow(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak, bool OffHand)
		{
			projectile.extraUpdates = 1;
			if (OffHand)
			{
				projectile.velocity *= 1.1f;
			}
		}

		public override Color GetHammerGlowmaskColor(Player player, OrchidGuardian guardian, Projectile projectile, Color lightColor, bool OffHand)
		{
			return OffHand ? Main.DiscoColor : Color.White;
		}

		public override void OnThrowHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak, bool OffHand)
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				target.AddBuff(thoriumMod.Find<ModBuff>("TerrariumBacklash").Type, Weak ? 120 : 300, false);
			}
		}

		public override void OnMeleeHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged, bool OffHand)
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				target.AddBuff(thoriumMod.Find<ModBuff>("TerrariumBacklash").Type, 120, false);
			}
		}

		public override void ExtraAI(Player player, OrchidGuardian guardian, Projectile projectile, bool OffHand)
		{
			if (OffHand && projectile.ModProjectile is GuardianHammerAnchor anchor)
			{
				anchor.Frame = 1;
			}

			// Stolen from Amber. Thank you Amber.

			Dust dust = Dust.NewDustPerfect(projectile.Center, PotentialDusts[Main.rand.Next(7)], Alpha: 100, Scale: 0.8f);
			Vector2 offs = Main.rand.NextVector2Circular(10, 10);
			dust.noGravity = true;

			if (projectile.ai[1] <= 0)
			{
				//this line copied from ThoriumGrandThunderBirdWarhammer
				//todo: make projectile.rotation actually work on warhammers so i don't have to do this for visuals
				Vector2 gemPos = projectile.Center + new Vector2(8 * projectile.spriteDirection, -8).RotatedBy(projectile.ai[1] > 0 ? projectile.rotation : guardian.GuardianItemCharge * 0.0065f * player.gravDir * projectile.spriteDirection);
				if (Main.rand.NextBool())
				{
					dust.scale *= 0.5f;
					dust.fadeIn = Main.rand.NextFloat(1f);
					offs *= 0.5f;
				}
				dust.position = gemPos + offs * 1.5f;
				dust.velocity = offs * Main.rand.NextFloat(0.1f) + player.velocity * 0.33f;
			}
			else
			{
				dust.position += offs;
				dust.velocity = projectile.velocity * 0.5f;
				dust.scale *= Main.rand.NextFloat(0.6f, 1f);
				dust.fadeIn = Main.rand.NextFloat(0.6f, 1f);
			}
		}
	}
}
