using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using OrchidMod.Common.Attributes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using OrchidMod.Common;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
	[CrossmodContent("ThoriumMod")]
	public class ThoriumThorsHammerWarhammer : OrchidModGuardianHammer
	{
		public override void SafeSetDefaults()
		{
			Item.width = 34;
			Item.height = 34;
			Item.value = Item.sellPrice(0, 0, 54, 0);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item1;
			Item.knockBack = 4f;
			Item.useTime = 15;
			Item.shootSpeed = 12f;
			Item.damage = 134;
			Range = 26;
			GuardStacks = 1;
			ReturnSpeed = 1.6f;
			SwingChargeGain = 1.5f;
			HitCooldown = 15;
			BlockDamage = 0.2f;
			Penetrate = true;
			BlockDuration = 80;
		}

		public override bool CanRightClick() => ModContent.GetInstance<OrchidClientConfig>().GuardianThoriumThorsHammerConversion;

		
		public override void RightClick(Player player)
		{
			if (OrchidMod.ThoriumMod != null && ModContent.GetInstance<OrchidClientConfig>().GuardianThoriumThorsHammerConversion)
			{
				Item.ChangeItemType(OrchidMod.ThoriumMod.Find<ModItem>("MeleeThorHammer").Type);
			}
		}

		public override void OnThrow(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak, bool OffHand)
		{
			projectile.extraUpdates = 1;
		}

		/*public override bool ThrowAI(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak)
		{
			Dust dust = Dust.NewDustDirect(projectile.position - new Vector2(4), projectile.width + 8, projectile.height + 8, DustID.MagicMirror, Alpha: 100, Scale: 0.8f);
			dust.velocity = projectile.velocity * 0.1f + (dust.position - projectile.Center).RotatedBy(Math.Min(projectile.velocity.Length() * 0.1f, MathHelper.PiOver2) * projectile.velocity.X > 0 ? 1 : -1) * 0.2f;
			if (Main.rand.NextBool(3))
			{
				dust.noGravity = true;
				dust.velocity *= 2.5f;
				dust.scale *= 1.5f;
			}
			return true;
		}*/

		public override void ExtraAI(Player player, OrchidGuardian guardian, Projectile projectile, bool OffHand)
		{
			Dust dust = Dust.NewDustPerfect(projectile.Center, DustID.MagicMirror, Alpha: 100, Scale: 0.8f);
			Vector2 offs = Main.rand.NextVector2Circular(10, 10);
			dust.noGravity = true;
			if ((projectile.ModProjectile as GuardianHammerAnchor).BlockDuration != 0)
			{
				dust.position += offs * 2;
				dust.velocity = projectile.velocity * 0.2f + offs.RotatedBy(-MathHelper.PiOver2) * 0.5f;
				dust.scale *= 1.5f;
			}
			else if (projectile.ai[1] <= 0)
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
				//dust.velocity = projectile.velocity * 0.2f + offs.RotatedBy(projectile.velocity.X > 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2);
				dust.scale *= Main.rand.NextFloat(0.6f, 1.6f);
				dust.fadeIn = Main.rand.NextFloat(0.6f, 2.2f - dust.scale);
			}
		}

		public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				var recipe = CreateRecipe();
				recipe.AddTile(thoriumMod, "ThoriumAnvil");
				recipe.AddIngredient(thoriumMod, "ThoriumBar", 20);
				recipe.AddIngredient(ItemID.HellstoneBar, 4);
				recipe.Register();

				recipe = CreateRecipe();
				recipe.AddRecipeGroup("ThorsHammers", 1);
				recipe.Register();

				recipe = CreateRecipe();
				recipe.AddIngredient(Item.type, 1);
				recipe.ReplaceResult(thoriumMod, "MeleeThorHammer", 1);
				recipe.Register();
			}
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);
            
			if (ModContent.GetInstance<OrchidClientConfig>().GuardianThoriumThorsHammerConversion)
			{
				int index = tooltips.FindIndex(ttip => ttip.Mod == "Terraria" && ttip.Name == "Tooltip0");
				tooltips[index].Text = Language.GetTextValue(Mod.GetLocalizationKey("Items.ThoriumThorsHammerWarhammer.ChangeMode"));
			}
        }
	}
}
