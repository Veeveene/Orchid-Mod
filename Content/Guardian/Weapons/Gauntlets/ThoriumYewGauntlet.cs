using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common;
using OrchidMod.Common.Attributes;
using OrchidMod.Content.General.Prefixes;
using OrchidMod.Content.Guardian.Projectiles.Gauntlets;
using OrchidMod.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace OrchidMod.Content.Guardian.Weapons.Gauntlets
{
	[CrossmodContent("ThoriumMod")]
	public class ThoriumYewGauntlet : OrchidModGuardianGauntlet
	{
		public bool PullOnKill;

		public Projectile HookProjectile;

		public override void SafeSetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.knockBack = 3f;
			Item.damage = 94;
			Item.value = Item.sellPrice(0, 1, 30, 0);
			Item.rare = ItemRarityID.Green;
			Item.useTime = 30;
			Item.shootSpeed = 15f;
			StrikeVelocity = 15f;
			ParryDuration = 80;
			ChargeSpeedMultiplier = 2.5f;
			GauntletFrames = 2;
			PullOnKill = true;

		}

		public override Color GetColor(bool offHand)
		{
			return new Color(137, 175, 133);
		}

		public override void ExtraAIGauntlet(Player player, OrchidGuardian guardian, Projectile anchor, bool offHandGauntlet) 
		{
			if (anchor.ModProjectile is GuardianGauntletAnchor gauntlet) {	
				int projType = ModContent.ProjectileType<ThoriumYewGauntletProjectile>();
				if (HookProjectile != null && HookProjectile.type == projType && HookProjectile.active && HookProjectile.owner == player.whoAmI && ((HookProjectile.ai[0] == 1f && !offHandGauntlet) || (HookProjectile.ai[0] == 2f && offHandGauntlet)))
					gauntlet.GauntletAnimFrame = 1;
				else {
					gauntlet.GauntletAnimFrame = 0;
					// HookProjectile = null;
				}
			}		
		}


		// public override Texture2D GetGauntletTexture(Player player, Projectile anchor, bool OffHandGauntlet, out Rectangle? drawRectangle)
		// {
		// 	Texture2D texture = ModContent.Request<Texture2D>(GauntletTexture).Value;
		// 	Rectangle rectangle = texture.Bounds;
		// 	rectangle.Height = rectangle.Height / 2;

		// 	int projType = ModContent.ProjectileType<ThoriumYewGauntletProjectile>();
		// 	foreach(Projectile projectile in Main.projectile)
		// 	{
		// 		if (projectile.type == projType && projectile.active && projectile.owner == player.whoAmI && ((projectile.ai[0] == 1f && !OffHandGauntlet) || (projectile.ai[0] == 2f && OffHandGauntlet)))
		// 		{
		// 			rectangle.Y = rectangle.Height;
		// 		}
		// 	}

		// 	drawRectangle = rectangle;
		// 	return texture;
		// }

		public override bool OnPunch(Player player, OrchidGuardian guardian, Projectile projectile, bool offHandGauntlet, bool fullyManuallyCharged, ref bool charged, ref int damage)
		{
			if (fullyManuallyCharged)
			{
				int projType = ModContent.ProjectileType<ThoriumYewGauntletProjectile>();
				if (HookProjectile != null && HookProjectile.type == projType && HookProjectile.active && HookProjectile.owner == player.whoAmI && ((HookProjectile.ai[0] == 1f && !offHandGauntlet) || (HookProjectile.ai[0] == 2f && offHandGauntlet))) {
					HookProjectile.Kill();
					HookProjectile = null;
				}

				Vector2 velocity = Vector2.UnitY.RotatedBy((Main.MouseWorld - player.MountedCenter).ToRotation() - MathHelper.PiOver2) * Item.shootSpeed;
				Projectile newprojectile = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), projectile.Center + velocity, velocity, projType, damage, 5f, player.whoAmI, offHandGauntlet ? 2f : 1f, -1f, projectile.whoAmI);
				newprojectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
				HookProjectile = newprojectile;
				return false;
			}

			return true;
		}

		public override bool PreDrawGauntlet(SpriteBatch spriteBatch, Projectile projectile, Player player, bool offHandGauntlet, ref Color lightColor)
		{
			// Projectile hookProjectile = null;
			// foreach (Projectile proj in Main.projectile)
			// {
			// 	if (proj.type == projType && proj.active && proj.owner == player.whoAmI && ((proj.ai[0] == 1f && !offHandGauntlet) || (proj.ai[0] == 2f && offHandGauntlet)))
			// 	{
			// 		hookProjectile = proj;
			// 	}
			// }

			int projType = ModContent.ProjectileType<ThoriumYewGauntletProjectile>();
			if (HookProjectile != null && HookProjectile.type == projType && HookProjectile.active && HookProjectile.owner == player.whoAmI && ((HookProjectile.ai[0] == 1f && !offHandGauntlet) || (HookProjectile.ai[0] == 2f && offHandGauntlet)))
			{ // Draw chain between hook and gauntlet
				Texture2D chainTexture = ModContent.Request<Texture2D>(Texture + "_Chain", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Vector2 chainDirection = HookProjectile.Center - (projectile.Center + Vector2.UnitY * player.gfxOffY);
				Vector2 segment = Vector2.Normalize(chainDirection) * chainTexture.Height * 0.66f;

				int nbSegments = 0;

				while(chainDirection.Length() > (segment * nbSegments).Length())
				{
					nbSegments++;
				}

				while (nbSegments > 0)
				{
					nbSegments--;
					chainDirection -= segment;
					Vector2 chainPos = projectile.Center + chainDirection - Main.screenPosition;
					spriteBatch.Draw(chainTexture, chainPos, null, lightColor, 0f, chainTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
				}

			}

			return base.PreDrawGauntlet(spriteBatch, projectile, player, offHandGauntlet, ref lightColor);
		}

		public override bool CanRightClick() => true;

		public override bool ConsumeItem(Player player) => false;

		public override void RightClick(Player player)
		{
			PullOnKill = !PullOnKill;
		}

		public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				var recipe = CreateRecipe();
				recipe.AddTile(thoriumMod, "ArcaneArmorFabricator");
				recipe.AddIngredient(thoriumMod, "YewWood", 18);
				recipe.Register();
			}
		}

		public override void SaveData(TagCompound tag)
		{
			tag.Add("PullOnKill", PullOnKill);
		}

		public override void LoadData(TagCompound tag)
		{
			PullOnKill = tag.GetBool("PullOnKill");
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write(PullOnKill);
			int index = (HookProjectile != null && HookProjectile.active && HookProjectile.owner == Main.myPlayer && HookProjectile.type == ModContent.ProjectileType<ThoriumYewGauntletProjectile>() ? HookProjectile.whoAmI : 255); 
			writer.Write((byte)index);
		}

		public override void NetReceive(BinaryReader reader)
		{
			PullOnKill = reader.ReadBoolean();
			var index = reader.Read();
			if (index != -1 && index < Main.maxProjectiles) {
				var proj = Main.projectile[index];
				if (proj != null && proj.active && proj.owner == Main.myPlayer && proj.type == ModContent.ProjectileType<ThoriumYewGauntletProjectile>())
					HookProjectile = proj;
			}
			
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			base.ModifyTooltips(tooltips);
			var guardian = Main.LocalPlayer.GetModPlayer<OrchidGuardian>();
			// TooltipLine tt = tooltips.FirstOrDefault(x => x.Name == "Damage" && x.Mod == "Terraria");
			// if (tt != null)
			// {
			// 	string[] splitText = tt.Text.Split(' ');
			// 	string damageValue = splitText.First();
			// 	tt.Text = damageValue + " " + Language.GetTextValue(Mod.GetLocalizationKey("DamageClasses.GuardianDamageClass.DisplayName"));
			// }

			int index = tooltips.FindIndex(ttip => ttip.Mod.Equals("Terraria") && ttip.Name.Equals("Knockback"));
			// tooltips.Insert(index + 1, new TooltipLine(Mod, "ParryDuration", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.ParryDuration", OrchidUtils.FramesToSeconds((int)(ParryDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianParryDuration)))));
			//
			//
			// string click = ModContent.GetInstance<OrchidClientConfig>().GuardianSwapGauntletInputs ? Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.LeftClick") : Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.RightClick");
			// tooltips.Insert(index + 2, new TooltipLine(Mod, "ClickInfo", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.Parry", click))
			// {
			// 	OverrideColor = new Color(175, 255, 175)
			// });

			string pull = PullOnKill ? Language.GetTextValue("Mods.OrchidMod.Items.ThoriumYewGauntlet.PullOnKill") : Language.GetTextValue("Mods.OrchidMod.Items.ThoriumYewGauntlet.NoPullOnKill");
			tooltips.Insert(index + 4, new TooltipLine(Mod, "ClickInfo2", pull));

			SafeModifyTooltips(tooltips);
		}
	
	}
}
