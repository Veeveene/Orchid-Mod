using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common.Global.Items;
using OrchidMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian
{
	public abstract class OrchidModGuardianRune : OrchidModGuardianItem
	{
		public int RuneCost; // Cost in slam
		public int RuneDuration; // Duration in ticks
		public float RuneDistance; // Projectile distance to player on spawn
		public int RuneNumber; // How many projectiles are spawned by default
		public int RuneAmountScaling; // How many projectiles are added for each bonus projectile stat point
		public virtual void ExtraAIRune(Projectile projectile) { } // Called at the end of the anchor AI for extra effects
		public virtual void PostDrawRune(SpriteBatch spriteBatch, Projectile projectile, Player player, Color lightColor) { } // Called after drawing the rune anchor
		public virtual bool PreDrawRune(SpriteBatch spriteBatch, Projectile projectile, Player player, ref Color lightColor) { return true; } // Called before drawing the rune anchor, return false to prevent it
		public virtual void SafeHoldItem(Player player) { }
		public virtual Color GetGlowColor() => Color.White; // Used to draw the glow color when reinforced
		public int GetAmount(OrchidGuardian guardian) => RuneNumber + guardian.GuardianBonusRune * RuneAmountScaling;

		public virtual void Activate(Player player, OrchidGuardian guardian, int type, int damage, float knockback, int critChance, int duration, float distance, int amount)
		{
			NewRuneProjectiles(player, guardian, duration, type, damage, knockback, critChance, distance, amount);
		}

		public sealed override void SetDefaults()
		{
			Item.DamageType = ModContent.GetInstance<GuardianDamageClass>();
			Item.noMelee = true;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = false;
			Item.maxStack = 1;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.shootSpeed = 00f;
			RuneCost = 1;
			RuneDuration = 1800;
			RuneDistance = 100f;
			RuneNumber = 1;
			RuneAmountScaling = 1;

			OrchidGlobalItemPerEntity orchidItem = Item.GetGlobalItem<OrchidGlobalItemPerEntity>();
			orchidItem.guardianWeapon = true;

			SafeSetDefaults();
			Item.useAnimation = Item.useTime;
		}

		public override bool WeaponPrefix() => true;

		public sealed override void HoldItem(Player player)
		{
			var projectileType = ModContent.ProjectileType<GuardianRuneAnchor>();
			var guardian = player.GetModPlayer<OrchidGuardian>();
			guardian.GuardianDisplayUI = 300;

			if (player.ownedProjectileCounts[projectileType] != 1)
			{
				if (IsLocalPlayer(player))
				{
					foreach (Projectile projectile in Main.projectile)
					{
						if (projectile.active && projectile.owner == player.whoAmI && projectile.type == projectileType)
						{
							projectile.Kill();
						}
					}

					Projectile proj = SpawnAnchor(player, out GuardianRuneAnchor rune);
					if (rune is not null)
						rune.OnChangeSelectedItem(player);
					else proj.Kill();
				}
			}
			else
			{
				var proj = Main.projectile.First(i => i.active && i.owner == player.whoAmI && i.type == projectileType);
				if (proj != null && proj.ModProjectile is GuardianRuneAnchor rune)
				{
					if (rune.SelectedItem != player.selectedItem)
					{
						rune.OnChangeSelectedItem(player);
					}
				}
			}
			SafeHoldItem(player);
		}

		public override bool CanUseItem(Player player)
		{
			var guardian = player.GetModPlayer<OrchidGuardian>();
			if (player.whoAmI == Main.myPlayer && !player.cursed)
			{
				var projectileType = ModContent.ProjectileType<GuardianRuneAnchor>();
				if (player.ownedProjectileCounts[projectileType] > 0)
				{
					var proj = Main.projectile.First(i => i.active && i.owner == player.whoAmI && i.type == projectileType);
					if (proj != null && proj.ModProjectile is GuardianRuneAnchor anchor && guardian.GuardianItemCharge == 0f)
					{
						foreach (Projectile projectile in Main.projectile) 
						{
							if (projectile.active && projectile.owner == player.whoAmI && projectile.type == Item.shoot)
							{ // Sets the player rune charge to a value depending on the remaining time left of the projectile, ai[1] helps syncing it on other clients
								proj.ai[1] = 90f * (projectile.timeLeft / (float)(RuneDuration * guardian.GuardianRuneTimer));
								break;
							}
						}

						guardian.GuardianItemCharge++;
						proj.ai[0] = 1f;
						anchor.NeedNetUpdate = true;
						SoundEngine.PlaySound(SoundID.Item7, player.Center);
					}
				}
			}
			return false;
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				var guardian = player.GetModPlayer<OrchidGuardian>();
			}
			return true;
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			return false;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			TooltipLine tt = tooltips.FirstOrDefault(x => x.Name == "Damage" && x.Mod == "Terraria");
			if (tt != null)
			{
				string[] splitText = tt.Text.Split(' ');
				string damageValue = splitText.First();
				tt.Text = damageValue + " " + Language.GetTextValue(ModContent.GetInstance<OrchidMod>().GetLocalizationKey("DamageClasses.GuardianDamageClass.DisplayName"));
			}

			int index = tooltips.FindIndex(ttip => ttip.Mod.Equals("Terraria") && ttip.Name.Equals("Knockback"));
			tooltips.Insert(index + 1, new TooltipLine(Mod, "RuneDuration", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.RuneDuration", OrchidUtils.FramesToSeconds((int)(RuneDuration * Main.LocalPlayer.GetModPlayer<OrchidGuardian>().GuardianRuneTimer)))));

			index = tooltips.FindIndex(ttip => ttip.Mod.Equals("Terraria") && ttip.Name.Equals("Knockback"));
			tooltips.Insert(index + 1, new TooltipLine(Mod, "UseSlams", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.UsesUpTo", this.RuneCost)));
		}

		public Projectile NewRuneProjectile(Player player, OrchidGuardian guardian, int duration, int type, int damage, float knockback, int critChance, float distance = 0f, float angle = 0f, float ai2 = 0f)
		{
			Projectile projectile = Main.projectile[Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI, distance, angle, ai2)];
			projectile.timeLeft = duration;
			projectile.CritChance = critChance;
			projectile.netUpdate = true;
			return projectile;
		}

		public List<Projectile> NewRuneProjectiles(Player player, OrchidGuardian guardian, int duration, int type, int damage, float knockback, int critChance, float distance, int number, float angle = 0f, float ai2 = 0f)
		{
			List<Projectile> projectiles = new List<Projectile>();

			for (int i = 0; i < number; i++)
				projectiles.Add(NewRuneProjectile(player, guardian, duration, type, damage, knockback, critChance, distance, angle + (360 / number) * i, ai2));
			return projectiles;
		}
	}
}
