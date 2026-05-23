using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using OrchidMod.Common.Attributes;


namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
	[CrossmodContent("ThoriumMod")]
	public class ThoriumFeatherWarhammer : OrchidModGuardianHammer
	{

		public override void SafeSetDefaults()
		{
			Item.width = 38;
			Item.height = 38;
			Item.value = Item.sellPrice(0, 0, 30, 0);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Zombie78.WithPitchOffset(0.1f).WithVolumeScale(0.5f);
			Item.knockBack = 4f;
			Item.shootSpeed = 12f;
			Item.damage = 68;
			Item.useTime = 20;
			Range = 70;
			GuardStacks = 1;
			ReturnSpeed = 2f;
			SwingSpeed = 2f;
			SwingChargeGain = 0.75f;
			HitCooldown = 10;
			Penetrate = true;
			TileBounce = true;
			BlockDuration = 180;
		}

		public override void OnThrow(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak)
		{
			if (!Weak)
			{
				projectile.ai[1] = MathHelper.Pi + projectile.velocity.ToRotation();
				SoundEngine.PlaySound(SoundID.NPCHit28.WithPitchOffset(0.5f).WithVolumeScale(0.5f), projectile.Center);
				GuardianHammerAnchor anchor = projectile.ModProjectile as GuardianHammerAnchor;
			}
			else 
			{
				GuardianHammerAnchor anchor = projectile.ModProjectile as GuardianHammerAnchor;
				anchor.range = 30;
			}

			CannotMagnet = false;
		}

		public override bool ThrowAI(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak)
		{
			GuardianHammerAnchor anchor = projectile.ModProjectile as GuardianHammerAnchor;
			if (Weak)
			{
				return true;
			}
			if (anchor.range == 40) anchor.range = 1;
			else if (anchor.range > 0 && anchor.range < 40)
			{
				projectile.velocity += new Vector2(-1.5f, 0).RotatedBy(projectile.ai[1]);
				if (projectile.velocity.Length() > Item.shootSpeed) projectile.velocity *= Item.shootSpeed / projectile.velocity.Length();
				if (anchor.range == 20) anchor.range = 1;
			}
			return true;
		}

		public override void OnThrowHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak)
		{
			GuardianHammerAnchor anchor = projectile.ModProjectile as GuardianHammerAnchor;
			if (!Weak && anchor.range >= 20)
			{
				if (anchor.range >= 40) anchor.range = 39;
				else anchor.range = 19;
				var thoriumMod = OrchidMod.ThoriumMod;
				if (thoriumMod != null)
				{
					target.AddBuff(thoriumMod.Find<ModBuff>("Stunned").Type, 20);
				}
			}
			else anchor.range = -100;
			projectile.velocity = anchor.InitialVelocity * -1f * projectile.velocity.Length();
			projectile.rotation = projectile.ai[1] - MathHelper.PiOver2;
			SoundEngine.PlaySound(SoundID.NPCHit46.WithPitchOffset(0.1f).WithVolumeScale(0.5f), projectile.Center);
			CannotMagnet = true;
		}

		public override void OnThrowTileCollide(Player player, OrchidGuardian guardian, Projectile projectile, Vector2 oldVelocity)
		{
			GuardianHammerAnchor anchor = projectile.ModProjectile as GuardianHammerAnchor;
			if (projectile.ai[0] != 1)
			{
				if (anchor.range >= 20)
				{
					if (anchor.range >= 40) anchor.range = 39;
					else anchor.range = 19;
				}
				else anchor.range = -40;
			} 
			projectile.velocity = -oldVelocity;
			projectile.rotation = projectile.ai[1] - MathHelper.PiOver2;
			SoundEngine.PlaySound(SoundID.NPCHit46.WithPitchOffset(0.1f).WithVolumeScale(0.5f), projectile.Center);
		}

		public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				CreateRecipe()
				.AddTile(thoriumMod, "ArcaneArmorFabricator")
				.AddIngredient(ItemID.Feather, 7)
				.Register();
			}
		}
	}
}
