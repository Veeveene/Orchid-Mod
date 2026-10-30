using Microsoft.Xna.Framework;
using OrchidMod.Common;
using OrchidMod.Common.Global.Items;
using OrchidMod.Content.General.Prefixes;
using OrchidMod.Content.Guardian.Projectiles.Misc;
using OrchidMod.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Localization;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using OrchidMod.Common.Attributes;

namespace OrchidMod.Content.Guardian.Weapons.Misc
{
	[CrossmodContent("ThoriumMod")]
	public class GuardianNeedle : OrchidModGuardianParryItem, IGuardianCustomUIItem
	{
		string IGuardianCustomUIItem.TextureName => "GuardianNeedle";
		public override int AnchorType => ModContent.ProjectileType<GuardianNeedleAnchor>();

		public int ParryDuration;

		public virtual void SafeHoldItem(Player player) { }

		public override void SetStaticDefaults()
		{
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			SafeSetStaticDefaults();
		}
		public sealed override void SetDefaults()
		{
			Item.DamageType = ModContent.GetInstance<GuardianDamageClass>();
			Item.noMelee = true;
			Item.autoReuse = true;
			Item.maxStack = 1;
			Item.noUseGraphic = true;
			Item.useStyle = ItemUseStyleID.Thrust;
			Item.UseSound = SoundID.Item50;

			Item.width = 46;
			Item.height = 46;
			Item.value = Item.sellPrice(0, 1, 60, 0);
			Item.rare = ItemRarityID.LightRed;
			Item.useTime = Item.useAnimation = 20;
			Item.knockBack = 6f;
			Item.damage = 78;
			ParryDuration = 20;

			OrchidGlobalItemPerEntity orchidItem = Item.GetGlobalItem<OrchidGlobalItemPerEntity>();
			orchidItem.guardianWeapon = true;
		}

		public override void AddRecipes()
		{
			/*
			var recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.SpiderFang);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.Register();
			*/
		}

		public override void OnParry(Player player, OrchidGuardian guardian, Entity aggressor, Projectile anchor) 
		{
			Vector2 targetPosition = Main.MouseWorld;
			if (aggressor != null)
			{
				targetPosition = aggressor.Center;

				player.velocity = new Vector2(-player.velocity.X * 0.33f, -10f);

				if ((player.velocity.X > 0 && player.controlLeft) || (player.velocity.X < 0 && player.controlRight))
				{
					player.velocity.X *= 0.25f;
				}
			}

			anchor.ai[2] = -41f;
			anchor.ai[1] = Vector2.Normalize(targetPosition - player.MountedCenter).ToRotation() - MathHelper.PiOver2;
			guardian.GuardianItemCharge = 0;
			anchor.netUpdate = true;
		}

		public override bool WeaponPrefix() => true;

		public override bool AltFunctionUse(Player player) => true;

		public override bool CanUseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer && !player.cursed)
			{
				if (player.ownedProjectileCounts[AnchorType] > 0)
				{

					var guardian = player.GetModPlayer<OrchidGuardian>();
					var proj = Main.projectile.FirstOrDefault(i => i.active && i.owner == player.whoAmI && i.type == AnchorType);
					if (proj != null && proj.ModProjectile is GuardianNeedleAnchor anchor && proj.ai[0] >= 0f)
					{
						bool shouldBlock = guardian.GetGauntletBlockInput(true);
						bool shouldCharge = guardian.GetGauntletPunchInput(false);

						if (shouldBlock && !shouldCharge && guardian.UseGuard(1, true, warn:true) && proj.ai[0] <= 0f && proj.ai[2] == 0f)
						{
							player.immuneTime = 0;
							player.immune = false;
							guardian.modPlayer.PlayerImmunity = 0;
							guardian.GuardianItemCharge = 0f;
							guardian.UseGuard(1);
							proj.ai[0] = 0f;
							proj.ai[2] = ParryDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianParryDuration;

							Vector2 targetPosition = Main.MouseWorld;
							if (targetPosition.Y < player.MountedCenter.Y + 16)
							{
								targetPosition.Y = player.MountedCenter.Y + 16;
							}
							proj.ai[1] = Vector2.Normalize(targetPosition - player.MountedCenter).ToRotation() - MathHelper.PiOver2;

							anchor.NeedNetUpdate = true;
							SoundEngine.PlaySound(SoundID.Item37, player.Center);
							SoundEngine.PlaySound(SoundID.DoubleJump, player.Center);

							for (int i = 0; i < 7; i++)
							{
								Dust dust = Dust.NewDustDirect(player.Center, 0, 0, DustID.Smoke);
								dust.scale *= Main.rand.NextFloat(1f, 1.5f);
								dust.velocity *= Main.rand.NextFloat(0.5f, 0.75f);
							}

							for (int i = 0; i < 5; i++)
							{
								Gore gore = Gore.NewGoreDirect(player.GetSource_FromAI(), player.Center + new Vector2(Main.rand.NextFloat(-24f, 0f), Main.rand.NextFloat(-24f, 0f)), Vector2.UnitY.RotatedByRandom(MathHelper.Pi), 61 + Main.rand.Next(3));
								gore.rotation = Main.rand.NextFloat(MathHelper.Pi);
							}

							guardian.OnAttack(AttackID.KatarDash, this);
						}

						if (shouldCharge && guardian.GuardianItemCharge == 0f && proj.ai[0] == 0f && proj.ai[2] >= -10f)
						{
							proj.ai[0] = 1f;
							proj.ai[2] = 0f;
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
			var projectileType = ModContent.ProjectileType<GuardianNeedleAnchor>();
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

				Projectile proj = SpawnAnchor(player, out GuardianNeedleAnchor anchor);
				if (anchor is not null)
					anchor.OnChangeSelectedItem(player);
				else proj.Kill();
			}
			else
			{
				var proj = Main.projectile.First(i => i.active && i.owner == player.whoAmI && i.type == projectileType);
				if (proj != null && proj.ModProjectile is GuardianNeedleAnchor anchor)
				{
					if (anchor.SelectedItem != player.selectedItem)
					{
						anchor.OnChangeSelectedItem(player);
					}
				}
			}
			SafeHoldItem(player);
		}

		public override bool ModifyAttackInfo(ref GuardianAttackInfo info)
		{
			if (info.Defense) info.SetOffense().SetCounter();
			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			var guardian = Main.LocalPlayer.GetModPlayer<OrchidGuardian>();
			TooltipLine tt = tooltips.FirstOrDefault(x => x.Name == "Damage" && x.Mod == "Terraria");
			if (tt != null)
			{
				string[] splitText = tt.Text.Split(' ');
				string damageValue = splitText.First();
				tt.Text = damageValue + " " + Language.GetTextValue(ModContent.GetInstance<OrchidMod>().GetLocalizationKey("DamageClasses.GuardianDamageClass.DisplayName"));
			}

			int index = tooltips.FindIndex(ttip => ttip.Mod.Equals("Terraria") && ttip.Name.Equals("Knockback"));
			tooltips.Insert(index + 1, new TooltipLine(Mod, "ParryDuration", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.ParryDuration", OrchidUtils.FramesToSeconds((int)guardian.GetParryDuration(Item, ParryDuration)))));

			string click = OrchidUtils.GetClickInfoTooltip(guardian.GauntletInputSwap);
			tooltips.Insert(index + 2, new TooltipLine(Mod, "ClickInfo", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.ParryDashNeedle", click))
			{
				OverrideColor = new Color(175, 255, 175)
			});

			tooltips.Insert(index + 3, new TooltipLine(Mod, "Swing", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.ChargeToDash", click))
			{
				OverrideColor = new Color(175, 255, 175)
			});
		}
	}
}
