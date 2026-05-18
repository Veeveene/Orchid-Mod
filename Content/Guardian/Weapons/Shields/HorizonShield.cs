using Microsoft.Xna.Framework;
using OrchidMod.Content.Guardian.Projectiles.Shields;
using OrchidMod.Content.Guardian.Misc;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using OrchidMod.Utilities;
using Terraria.Audio;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	public class HorizonShield : OrchidModGuardianShield
	{
		public int TimeHeld;
		public bool StoredBlock;
		public Texture2D TextureGlowH;
		public Texture2D TextureGlowHAlt;
		public float ColorMult;
		public List<Vector2> OldPosition;
		public List<float> OldRotation;

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 10, 0, 0);
			Item.width = 42;
			Item.height = 50;
			Item.UseSound = SoundID.DD2_FlameburstTowerShot;
			Item.knockBack = 6f;
			Item.damage = 527;
			Item.rare = ItemRarityID.Red;
			Item.useTime = 24;
			distance = 70f;
			slamDistance = 150f;
			blockDuration = 300;
			TextureGlowH ??= ModContent.Request<Texture2D>(Texture + "_GlowH", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			TextureGlowHAlt ??= ModContent.Request<Texture2D>(Texture + "_GlowHAlt", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			OldPosition = new List<Vector2>();
			OldRotation = new List<float>();
			TimeHeld = 0;
		}

		public override void UpdateInventory(Player player)
		{
			if (player.HeldItem.type != Type && OldPosition.Count > 0)
			{
				TimeHeld = 0;
				StoredBlock = false;
				OldPosition = new List<Vector2>();
				OldRotation = new List<float>();
			}

			Item.UseSound = player.GetModPlayer<OrchidGuardian>().GuardianCounterTime > 0 ? SoundID.DD2_ExplosiveTrapExplode : SoundID.DD2_FlameburstTowerShot;
		}

		public override void HoldItemFrame(Player player)
		{
			OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
			guardian.GuardianCounter = true;
			guardian.GuardianCounterHorizon = true;
			TimeHeld ++;
		}

		public override void Slam(Player player, Projectile shield, bool WeakSlam)
		{
			if (IsLocalPlayer(player) && !WeakSlam)
			{
				Projectile anchor = GetAnchor(player).Projectile;
				int type = ModContent.ProjectileType<HorizonShieldProj>();
				Vector2 dir = Vector2.Normalize(Main.MouseWorld - player.Center) * 15f;
				Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), anchor.Center, dir, type, (int)(shield.damage * (player.GetModPlayer<OrchidGuardian>().GuardianCounterTime > 0 ? 3f : 1f)), Item.knockBack, player.whoAmI, StoredBlock ? 1f : 0f);
				newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
				StoredBlock = false;
			}
		}

		public override void ExtraAIShield(Projectile projectile)
		{
			if (projectile.ai[1] + projectile.ai[0] > 0)
			{
				ColorMult = (float)Math.Sin(TimeHeld * 0.1f) * 0.25f + 0.75f;

				if (OldPosition.Count > (StoredBlock ? 20 : 15))
				{
					OldPosition.RemoveAt(0);
					OldRotation.RemoveAt(0);
				}

				OldPosition.Add(new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)));
				OldRotation.Add(projectile.rotation);

				for (int i = 0; i < OldPosition.Count; i++)
				{
					Vector2 pos = OldPosition[i];
					pos.Y += Main.rand.NextFloat(6.4f);
					pos.X += Main.rand.NextFloat(3f) - 1.5f;
					OldPosition[i] = pos;
				}
			}
			else
			{
				StoredBlock = false;
				ColorMult = (float)Math.Sin(TimeHeld * 0.1f) * 0.125f + 0.35f;

				if (OldPosition.Count > 10)
				{
					OldPosition.RemoveAt(0);
					OldRotation.RemoveAt(0);
				}
				else
				{
					OldPosition.Add(new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)));
					OldRotation.Add(projectile.rotation);
				}

				for (int i = 0; i < OldPosition.Count; i++)
				{
					Vector2 pos = OldPosition[i];
					pos.Y += Main.rand.NextFloat(3.15f);
					pos.X += Main.rand.NextFloat(1f) - 0.5f;
					OldPosition[i] = pos;
				}
			}
		}

		public override void Protect(Player player, Projectile shield)
		{
			if (!StoredBlock && IsLocalPlayer(player))
			{
				StoredBlock = true;
				SoundEngine.PlaySound(SoundID.DD2_FlameburstTowerShot, player.Center);
			}
		}

		public override void PostDrawShield(SpriteBatch spriteBatch, Projectile projectile, Player player, Color lightColor)
		{
			var drawPosition = projectile.Center - Main.screenPosition + Vector2.UnitY * player.gfxOffY;
			var effect = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
			spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });

			spriteBatch.Draw(TextureGlowH, drawPosition, null, Color.White * ColorMult, projectile.rotation, TextureGlowH.Size() * 0.5f, projectile.scale, effect, 0f);

			Vector2 slamoffset = Vector2.Zero;
			for (int i = 0; i < OldPosition.Count - 1; i++)
			{
				if (projectile.ai[1] > 0)
				{
					slamoffset.X += i * 0.2f;
				}
				Vector2 drawPositionGlow = projectile.Center - (Vector2.UnitY * 6f + OldPosition[i] + slamoffset).RotatedBy(projectile.rotation) - Main.screenPosition;
				spriteBatch.Draw(TextureGlowHAlt, drawPositionGlow, null, new Color(216, 61, 30), OldRotation[i], TextureGlowHAlt.Size() * 0.5f, projectile.scale * (i + 1) * 0.08f, effect, 0f);

				SpriteEffects effectFlip = projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
				drawPositionGlow = projectile.Center + (Vector2.UnitY * 6f + OldPosition[i] - slamoffset).RotatedBy(projectile.rotation) - Main.screenPosition;
				spriteBatch.Draw(TextureGlowHAlt, drawPositionGlow, null, new Color(59, 88, 204), OldRotation[i] + MathHelper.Pi, TextureGlowHAlt.Size() * 0.5f, projectile.scale * (i + 1) * 0.08f, effectFlip, 0f);
			}

			spriteBatch.End();
			spriteBatch.Begin(spriteBatchSnapshot);
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddIngredient<HorizonFragment>(18);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.Register();
		}
	}
}
