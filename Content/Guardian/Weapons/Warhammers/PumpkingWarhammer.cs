using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Content.General.Prefixes;
using OrchidMod.Content.Guardian.Projectiles.Warhammers;
using OrchidMod.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
	public class PumpkingWarhammer : OrchidModGuardianHammer
	{
		// public SpriteBatchSnapshot? LocalSpriteBatchSnapshot;
		public Projectile ScytheProjectile;
		/// <summary> 
		/// Whether the scythe is currently embedded into an enemy 
		/// </summary>
		public bool EmbeddedScythe;

		public override void SafeSetDefaults()
		{
			Item.width = 46;
			Item.height = 46;
			Item.value = Item.sellPrice(0, 10, 0, 0);
			Item.rare = ItemRarityID.Yellow;
			Item.UseSound = SoundID.NPCHit54.WithPitchOffset(0.75f);
			Item.knockBack = 8f;
			Item.shootSpeed = 16f;
			Item.damage = 519;
			Item.useTime = 8;
			Range = 25;
			SlamStacks = 2;
			ReturnSpeed = 2f;
			BlockDuration = 600;
			CannotSwing = true;
			HoldOffset = -32f;
			Penetrate = true;
			TileBounce = true;
			HitCooldown = 20;
			BlockVelocityMult = 2f;
			hasSpecialHammerTexture = true;
			HammerFrames = 2;
			SlamBlockCost = 3;
			GuardBlockCost = 0;

			EmbeddedScythe = false;
		}

		public override void ExtraAI(Player player, OrchidGuardian guardian, Projectile projectile, bool OffHand)
		{

			if (projectile.ModProjectile is GuardianHammerAnchor anchor) {
				
				anchor.HammerAnimFrame = 1;

				int projType = ModContent.ProjectileType<PumpkingWarhammerProjectile>();
				if (ScytheProjectile != null && ScytheProjectile.active && ScytheProjectile.type == projType && ScytheProjectile.owner == player.whoAmI)
				{
					EmbeddedScythe = true;
					if (Main.rand.NextBool())
					{
						Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Torch);
						dust.scale *= Main.rand.NextFloat(1.5f, 2.5f);
						dust.velocity *= 3f;
						dust.noGravity = true;
					}
					return;
				}
				

				anchor.HammerAnimFrame = 0;
				EmbeddedScythe = false;
				if (Main.rand.NextBool())
				{
					Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Wraith);
					dust.scale *= Main.rand.NextFloat(1f, 1.5f);
					dust.noGravity = true;

					if (Main.rand.NextBool(10))
					{
						Gore gore = Gore.NewGoreDirect(projectile.GetSource_FromAI(), projectile.Center + new Vector2(Main.rand.NextFloat(-8f, 0f), Main.rand.NextFloat(-8f, 0f)), Vector2.UnitY.RotatedByRandom(MathHelper.Pi), 99);
						gore.rotation = Main.rand.NextFloat(MathHelper.Pi);
						gore.scale *= Main.rand.NextFloat(0.4f, 0.7f);
					}
				}
			}
			
		}

		public override void OnBlockThrow(Player player, OrchidGuardian guardian, Projectile projectile)
		{
			if (EmbeddedScythe) 
			{
				int projType = ModContent.ProjectileType<PumpkingWarhammerProjectile>();
				if (ScytheProjectile != null && ScytheProjectile.active && ScytheProjectile.type == projType && ScytheProjectile.owner == player.whoAmI) {
					ScytheProjectile.friendly = false;
					ScytheProjectile.localAI[1] = 1f;
				}
			}
		}

		public override bool ThrowAI(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak, bool OffHand)
		{
			if (EmbeddedScythe && projectile.ModProjectile is GuardianHammerAnchor anchor && anchor.range <= -16 && anchor.BlockDuration <= 0)
			{
				for (int i = 0; i < 5; i++)
				{
					Gore gore = Gore.NewGoreDirect(projectile.GetSource_FromAI(), projectile.Center + new Vector2(Main.rand.NextFloat(-8f, 0f), Main.rand.NextFloat(-8f, 0f)), Vector2.UnitY.RotatedByRandom(MathHelper.Pi), 61 + Main.rand.Next(3));
					gore.rotation = Main.rand.NextFloat(MathHelper.Pi);
					gore.scale *= Main.rand.NextFloat(0.3f, 0.5f);
					gore.velocity *= 0.5f;
				}

				for (int i = 0; i < 15; i++)
				{
					Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Torch);
					dust.scale *= Main.rand.NextFloat(2.25f, 3f);
					dust.velocity *= Main.rand.NextFloat(1.5f, 3f);
					dust.noGravity = true;
				}

				SoundEngine.PlaySound(SoundID.LiquidsWaterLava.WithVolumeScale(1.5f), projectile.Center);
				projectile.Kill();
				return false;
			}
			return true;
		}

		public override void OnThrowHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak, bool OffHand)
		{

			if (EmbeddedScythe && ScytheProjectile != null && ScytheProjectile.active && ScytheProjectile.owner == player.whoAmI)
			{
				ScytheProjectile.localAI[2] += 2;
				SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy.WithPitchOffset(ScytheProjectile.localAI[2] * 0.2f));
				CombatText.NewText(player.Hitbox, Color.IndianRed, (int)(ScytheProjectile.localAI[2] / 2f), false, true);
			}
		}

		public override void OnThrow(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak, bool OffHand)
		{
			if (Weak && EmbeddedScythe)
			{
				for (int i = 0; i < 5; i++)
				{
					Gore gore = Gore.NewGoreDirect(projectile.GetSource_FromAI(), projectile.Center + new Vector2(Main.rand.NextFloat(-8f, 0f), Main.rand.NextFloat(-8f, 0f)), Vector2.UnitY.RotatedByRandom(MathHelper.Pi), 61 + Main.rand.Next(3));
					gore.rotation = Main.rand.NextFloat(MathHelper.Pi);
					gore.scale *= Main.rand.NextFloat(0.7f, 1f);
				}

				SoundEngine.PlaySound(SoundID.LiquidsWaterLava, projectile.Center);
				projectile.Kill();
			}
		}

		// public override bool PreDrawHammer(Player player, OrchidGuardian guardian, Projectile projectile, SpriteBatch spriteBatch, ref Color lightColor, ref Texture2D hammerTexture, ref Rectangle drawRectangle)
		// {
		// 	drawRectangle.Height /= 2;

		// 	if (EmbeddedScythe && projectile.ModProjectile is GuardianHammerAnchor anchor && anchor.BlockDuration <= 0)
		// 	{ // adjusts the draw rectangle to the next frame if the parry scythe is active
		// 		spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
		// 		spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });
		// 		LocalSpriteBatchSnapshot = spriteBatchSnapshot;
		// 		drawRectangle.Y += drawRectangle.Height;
		// 		return true;
		// 	}

		// 	LocalSpriteBatchSnapshot = null;

		// 	return true;
		// }

		// public override void PostDrawHammer(Player player, OrchidGuardian guardian, Projectile projectile, SpriteBatch spriteBatch, Color lightColor, Texture2D hammerTexture, Rectangle drawRectangle)
		// {
		// 	if (LocalSpriteBatchSnapshot != null)
		// 	{
		// 		spriteBatch.End();
		// 		spriteBatch.Begin((SpriteBatchSnapshot)LocalSpriteBatchSnapshot);
		// 	}
		// }

		// public override bool? UseItem(Player player)
		// { // Copied from the main Warhammer class to edit block guard cost - probably worth making the cost a field if this happens again
		// 	var guardian = player.GetModPlayer<OrchidGuardian>();
		// 	int projType = ModContent.ProjectileType<GuardianHammerAnchor>();
		// 	int damage = guardian.GetGuardianDamage(Item.damage);
		// 	Projectile projectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), player.Center, Vector2.Zero, projType, damage, Item.knockBack, player.whoAmI);
		// 	projectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);

		// 	if (Main.mouseRight && Main.mouseRightRelease && projectile.ModProjectile is GuardianHammerAnchor anchor && guardian.UseSlam(3, true))
		// 	{
		// 		projectile.velocity = Vector2.Normalize(Main.MouseWorld - player.Center) * (10f + (Item.shootSpeed - 10f) * 0.35f * BlockVelocityMult);
		// 		projectile.friendly = true;
		// 		projectile.knockBack = 0f;
		// 		projectile.tileCollide = true;

		// 		anchor.BlockDuration = (int)(BlockDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianBlockDuration + 10);
		// 		anchor.NeedNetUpdate = true;
		// 		guardian.UseSlam(3);
		// 	}

		// 	guardian.GuardianItemCharge = 0f;
		// 	return true;
		// }

		public override void OnBlockHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit)
		{
			int latchDamage = guardian.GetGuardianDamage(Item.damage * 0.2f);
			int projectileType = ModContent.ProjectileType<PumpkingWarhammerProjectile>();
			Vector2 position = target.Center - Vector2.Normalize(projectile.Center - target.Center) * Math.Min(target.width, target.height) * 0.5f;
			Vector2 offSet = target.Center - position;
			Projectile newProjectile = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), position, Vector2.Zero, projectileType, latchDamage, 0f, player.whoAmI, target.whoAmI, offSet.X, offSet.Y);
			newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
			EmbeddedScythe = true;
			ScytheProjectile = newProjectile;
			projectile.Kill();
		}
	}
}
