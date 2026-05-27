using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common;
using OrchidMod.Common.Global.Items;
using OrchidMod.Content.General.Prefixes;
using OrchidMod.Content.Guardian.Misc;
using OrchidMod.Content.Guardian.Projectiles.Misc;
using OrchidMod.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Localization;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Misc
{
	public class HorizonLance : OrchidModGuardianParryItem
	{
		public int StandardDuration; // Effect duration in ticks
		public int ParryDuration;
		public virtual string LanceTexture => Texture;
		public virtual string LanceTextureGlow => Texture + "_Glow";

		public virtual void SafeHoldItem(Player player) { }

		public sealed override void SetDefaults()
		{
			Item.DamageType = ModContent.GetInstance<GuardianDamageClass>();
			Item.noMelee = true;
			Item.autoReuse = true;
			Item.maxStack = 1;
			Item.noUseGraphic = true;
			Item.useStyle = ItemUseStyleID.Thrust;
			Item.UseSound = SoundID.DD2_GhastlyGlaiveImpactGhost;


			Item.width = 52;
			Item.height = 52;
			Item.value = Item.sellPrice(0, 10, 0, 0);
			Item.rare = ItemRarityID.Red;
			Item.useTime = Item.useAnimation = 40;
			Item.knockBack = 8f;
			Item.damage = 618;
			StandardDuration = 1800; // 30 sec
			ParryDuration = 90;

			OrchidGlobalItemPerEntity orchidItem = Item.GetGlobalItem<OrchidGlobalItemPerEntity>();
			orchidItem.guardianWeapon = true;
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddIngredient<HorizonFragment>(18);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.Register();
		}

		public override void OnParry(Player player, OrchidGuardian guardian, Entity aggressor, Projectile anchor) 
		{
			anchor.ai[0] = 0;

			if (player.statDefense > 10) guardian.modPlayer.TryHeal((int)(player.statDefense * 0.1f));
			SoundEngine.PlaySound(SoundID.Item68, player.Center);

			Vector2 offset = Vector2.UnitY;

			if (aggressor != null)
			{
				if (aggressor is NPC npc)
				{
					offset = offset.RotatedBy((npc.Center - player.Center).ToRotation() - MathHelper.PiOver2);
				}

				if (aggressor is Projectile projectile)
				{
					//offset = offset.RotatedBy((projectile.Center - player.Center).ToRotation() - MathHelper.PiOver2);
					offset = Vector2.Normalize(-projectile.velocity);
				}
			}
			else 
			{
				offset = offset.RotatedByRandom(MathHelper.Pi);
			}

			var projectileType = ModContent.ProjectileType<GuardianHorizonLanceCounter>();
			int damage = guardian.GetGuardianDamage(Item.damage);
			Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromAI(), player.Center, offset, projectileType, damage, Item.knockBack, player.whoAmI);
			newProjectile.CritChance = guardian.GetGuardianCrit(Item.crit);
		}

		public override bool WeaponPrefix() => true;

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override bool CanUseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer && !player.cursed)
			{
				var projectileType = ModContent.ProjectileType<GuardianHorizonLanceAnchor>();
				if (player.ownedProjectileCounts[projectileType] > 0)
				{

					var guardian = player.GetModPlayer<OrchidGuardian>();
					var proj = Main.projectile.First(i => i.active && i.owner == player.whoAmI && i.type == projectileType);
					if (proj != null && proj.ModProjectile is GuardianHorizonLanceAnchor anchor && proj.ai[0] >= 0f)
					{
						bool shouldBlock = Main.mouseRight && Main.mouseRightRelease;
						bool shouldCharge = Main.mouseLeft && Main.mouseLeftRelease;

						if (ModContent.GetInstance<OrchidClientConfig>().GuardianSwapGauntletImputs)
						{
							shouldBlock = Main.mouseLeft && Main.mouseLeftRelease;
							shouldCharge = Main.mouseRight && Main.mouseRightRelease;
						}

						if (shouldBlock && guardian.UseGuard(1, true) && proj.ai[0] <= 1f)
						{
							player.immuneTime = 0;
							player.immune = false;
							guardian.modPlayer.PlayerImmunity = 0;
							guardian.GuardianItemCharge = 0f;
							guardian.UseGuard(1);
							proj.ai[0] = ParryDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianParryDuration + 1f;
							anchor.NeedNetUpdate = true;
							SoundEngine.PlaySound(SoundID.Item37, player.Center);
						}

						if (shouldCharge && guardian.GuardianItemCharge == 0f)
						{
							proj.ai[0] = 1f;
							anchor.NeedNetUpdate = true;
							guardian.GuardianItemCharge++;
							SoundEngine.PlaySound(SoundID.Item7, player.Center);
						}
					}
				}
			}
			return false;
		}

		public sealed override void HoldItem(Player player)
		{
			var projectileType = ModContent.ProjectileType<GuardianHorizonLanceAnchor>();
			var guardian = player.GetModPlayer<OrchidGuardian>();
			guardian.GuardianDisplayUI = 300;

			if (player.ownedProjectileCounts[projectileType] != 1)
			{
				foreach (Projectile projectile in Main.projectile)
				{
					if (projectile.active && projectile.owner == player.whoAmI && projectile.type == projectileType)
					{
						projectile.Kill();
					}
				}

				var index = Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center.X, player.Center.Y, 0f, 0f, projectileType, 0, 0f, player.whoAmI);

				var proj = Main.projectile[index];
				if (proj.ModProjectile is not GuardianHorizonLanceAnchor standard)
				{
					proj.Kill();
				}
				else
				{
					standard.OnChangeSelectedItem(player);
				}
			}
			else
			{
				var proj = Main.projectile.First(i => i.active && i.owner == player.whoAmI && i.type == projectileType);
				if (proj != null && proj.ModProjectile is GuardianHorizonLanceAnchor standard)
				{
					if (standard.SelectedItem != player.selectedItem)
					{
						standard.OnChangeSelectedItem(player);
					}
				}
			}
			SafeHoldItem(player);
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			//Projectile proj = Main.player[Main.myPlayer].GetModPlayer<OrchidGuardian>().GuardianCurrentStandardAnchor;
			//if (proj != null && proj.ModProjectile is GuardianHorizonLanceAnchor standard)
			//{
				spriteBatch.Draw(ModContent.Request<Texture2D>(LanceTextureGlow).Value, position, frame, Color.White, 0, origin, scale, SpriteEffects.None, 0);
			//}
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			var guardian = Main.LocalPlayer.GetModPlayer<OrchidGuardian>();
			int index = tooltips.FindIndex(ttip => ttip.Mod.Equals("Terraria") && ttip.Name.Equals("Knockback"));

			tooltips.Insert(index + 1, new TooltipLine(Mod, "ParryDuration", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.ParryDuration", OrchidUtils.FramesToSeconds((int)(ParryDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianParryDuration)))));


			tooltips.Insert(index + 2, new TooltipLine(Mod, "RuneDuration", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.BuffDuration", OrchidUtils.FramesToSeconds((int)(StandardDuration * Main.LocalPlayer.GetModPlayer<OrchidGuardian>().GuardianRuneTimer)))));

			tooltips.Insert(index + 3, new TooltipLine(Mod, "ShieldStacks", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.HorizonLaceShieldStacks"))
			{
				OverrideColor = new Color(175, 255, 175)
			});

			string click = ModContent.GetInstance<OrchidClientConfig>().GuardianSwapGauntletImputs ? Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.LeftClick") : Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.RightClick");
			tooltips.Insert(index + 4, new TooltipLine(Mod, "ClickInfo", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.Parry", click))
			{
				OverrideColor = new Color(175, 255, 175)
			});
		}
	}
}
