using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common.Attributes;
using OrchidMod.Common.ModObjects;
using OrchidMod.Content.Guardian.Buffs;
using OrchidMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	[CrossmodContent("ThoriumMod")]
	public class ThoriumBronzeShield : OrchidModGuardianShield
	{
		public static Texture2D TextureAura;
		public override void SetStaticDefaults()
		{
			TextureAura ??= ModContent.Request<Texture2D>("OrchidMod/Content/Guardian/StandardAuraProjectile", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 0, 50, 0);
			Item.width = 46;
			Item.height = 60;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item71.WithPitchOffset(-1f);
			Item.knockBack = 20f;
			Item.damage = 160;
			Item.rare = ItemRarityID.Orange;
			Item.useTime = 46;
			distance = 64f;
			slamDistance = 96f;
			blockDuration = 360;
		}

		public override void SlamHit(Player player, Projectile shield, NPC npc, bool WeakSlam)
		{
			int buff = player.FindBuffIndex(ModContent.BuffType<GuardianThoriumBronzeShieldBuff>());
			if (buff != -1) player.DelBuff(buff);

			/*var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				int buff = npc.FindBuffIndex(thoriumMod.Find<ModBuff>("Petrify").Type);
				if (buff != -1) npc.DelBuff(buff);
			}*/
		}

		float oldVelX;
		float oldVelY;

		public override void BlockStart(Player player, Projectile shield)
		{
			oldVelX = player.velocity.X;
			oldVelY = player.velocity.Y;
			//redundant behavior to counteract the first two frames not counting
			OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
			if (IsLocalPlayer(player)) Main.buffNoTimeDisplay[ModContent.BuffType<GuardianThoriumBronzeShieldBuff>()] = true;
			guardian.GuardianBronzeShieldBuff = true;
			guardian.GuardianBronzeShieldDamage = guardian.GuardianGuardRecharge / 100f;
		}

		public override void ExtraAIShield(Projectile projectile)
		{
			Player player = Main.player[projectile.owner];
			OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
			if (projectile.ai[0] > 0)
			{
				oldVelX = player.velocity.X = MathHelper.Lerp(player.velocity.X, oldVelX, 0.6f);
				if (Math.Abs(oldVelX) > 10) oldVelX *= 0.9f;
				if (player.velocity.Y != 0)
				{
					player.velocity.Y = MathHelper.Lerp(player.velocity.Y, oldVelY, player.velocity.Y < oldVelY ? 0.5f : 0.3f);
				}
				oldVelY = player.velocity.Y;
				if (player.jump > 0) player.jump--;
				if (guardian.GuardianBronzeShieldDamage < 9.99f)
				{
					guardian.GuardianBronzeShieldDamage = Math.Min(9.99f, guardian.GuardianBronzeShieldDamage + guardian.GuardianGuardRecharge / 200f);
				}
				guardian.GuardianGuardRecharge = 0;
				player.AddBuff(ModContent.BuffType<GuardianThoriumBronzeShieldBuff>(), 60);
				foreach (Player ally in Main.player)
				{
					if (ally.whoAmI == player.whoAmI) continue;
					if (ally.Center.Distance(player.Center) < distance * guardian.GuardianStandardRange)
					{
						ally.AddBuff(ModContent.BuffType<ThoriumBronzeShieldProtection>(), 60);
					}
				}
			}
		}

		public override bool PreDrawShield(SpriteBatch spriteBatch, Projectile projectile, Player player, ref Color lightColor)
		{
			spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
			spriteBatch.Begin(spriteBatchSnapshot with { BlendState = BlendState.Additive });
			float alphamult = projectile.ai[0] > 0
				? (float)(Math.Sin(projectile.ai[0] * 0.075f) * 0.075f + 1f)
				: 0.2f + Math.Abs((Main.player[Main.myPlayer].GetModPlayer<OrchidPlayer>().Timer120 - 60) / 240f);
			Vector2 drawPositionAura = Vector2.Transform(player.Center.Floor() - Main.screenPosition + Vector2.UnitY * player.gfxOffY, Main.GameViewMatrix.EffectMatrix);
			spriteBatch.Draw(TextureAura, drawPositionAura, null, new Color(181, 46, 47) * alphamult, 0f, TextureAura.Size() * 0.5f, 0.007f * distance * player.GetModPlayer<OrchidGuardian>().GuardianStandardRange, SpriteEffects.None, 0f);
			spriteBatch.End();
			spriteBatch.Begin(spriteBatchSnapshot);
			return true;
		}

		public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				var recipe = CreateRecipe();
				recipe.AddTile(TileID.Anvils);
				recipe.AddIngredient(thoriumMod, "BronzeAlloyFragments", 10);
				recipe.Register();
			}
		}
	}
}
