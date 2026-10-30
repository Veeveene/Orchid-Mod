using System.Linq;
using Microsoft.Xna.Framework;
using OrchidMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Gauntlets
{
	public class NanitesGauntlet : OrchidModGuardianGauntlet
	{
		public override void SafeSetDefaults()
		{
			Item.width = 42;
			Item.height = 38;
			Item.knockBack = 5f;
			Item.damage = 495;
			Item.value = Item.sellPrice(0, 7, 50, 0);
			Item.rare = ItemRarityID.Yellow;
			Item.useTime = 10;
			Item.crit = 10;
			StrikeVelocity = 30f;
			ParryDuration = 90;
			PunchSpeed = 1.2f;
			hasArm = true;
			hasShoulder = true;
		}

		public float AddedVelocityMult = 1f;

		public override Color GetColor(bool offHand)
		{
			return new Color(0, 255, 255);
		}

		public override void SafeHoldItem(Player player)
		{
			if (player.mount.Type == MountID.None)
			{
				Vector2 intendedVelocity = player.velocity * 0.05f;
				Vector2 addedVelocity = Vector2.Zero;

				for (int i = 0; i < 10; i++)
					addedVelocity += Collision.TileCollision(player.position + addedVelocity, intendedVelocity, player.width, player.height, false, false, (int)player.gravDir);

				if (addedVelocity.Length() > 0.1f)
				{
					player.position += addedVelocity;
					if (player.OrchidPlayer().Timer % 15 == 0) AddedVelocityMult = 1 + addedVelocity.Length() / player.velocity.Length();
				}
			}
		}

		public override void ExtraAIGauntlet(Player player, OrchidGuardian guardian, Projectile projectile, bool offHandGauntlet)
		{
			if (Main.player[projectile.owner].mount.Type == MountID.None) Main.player[projectile.owner].armorEffectDrawShadow = true;
		}

		public override void OnHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool charged)
		{
			guardian.modPlayer.TryHeal(charged ? 10 : 5);
		}

		public override void OnParryGauntlet(Player player, OrchidGuardian guardian, Entity aggressor, Projectile anchor)
		{
			guardian.modPlayer.TryHeal(20);
		}
	}

	public class NanitesGauntletInfoDisplay : GlobalInfoDisplay
	{
		public override void ModifyDisplayParameters(InfoDisplay currentDisplay, ref string displayValue, ref string displayName, ref Color displayColor, ref Color displayShadowColor)
		{
			if (currentDisplay == InfoDisplay.Stopwatch)
			{
				if (Main.LocalPlayer.HeldItem.ModItem is NanitesGauntlet gauntlet)
				{
					int speed = 0;
					if (int.TryParse(displayValue.Split(' ').First(), out int result)) speed = result;
					displayValue = Language.GetTextValue("GameUI.Speed", (int)(speed * gauntlet.AddedVelocityMult));
				}
			}
		}
	}
}
