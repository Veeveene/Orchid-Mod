using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Content.Guardian.Buffs;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace OrchidMod.Content.Guardian.Weapons.Gauntlets
{
	public enum JewelerGauntletGem : byte
	{
		NONE = 0,
		AMETHYST = 1,
		TOPAZ = 2,
		SAPPHIRE = 3,
		EMERALD = 4,
		RUBY = 5,
		DIAMOND = 6,
		AMBER = 7,
		AQUAMARINE = 8,
		OPAL = 9
	}

	public class JewelerGauntlet : OrchidModGuardianGauntlet
	{
		public JewelerGauntletGem GemType;

		public override void SafeSetDefaults()
		{
			Item.width = 32;
			Item.height = 34;
			Item.knockBack = 4f;
			Item.damage = 74;
			Item.value = Item.sellPrice(0, 1, 15, 0);
			Item.rare = ItemRarityID.Blue;
			Item.useTime = 35;
			StrikeVelocity = 16f;
			ParryDuration = 60;
			GauntletFrames = 10;
			GemType = 0;
		}

		public override Color GetColor(bool offHand)
		{
			switch(GemType)
			{
				default:
					return new Color(180, 180, 180);
				case JewelerGauntletGem.AMETHYST:
					return new Color(219, 97, 255);
				case JewelerGauntletGem.TOPAZ:
					return new Color(255, 226, 133);
				case JewelerGauntletGem.SAPPHIRE:
					return new Color(23, 147, 234);
				case JewelerGauntletGem.EMERALD:
					return new Color(81, 207, 160);
				case JewelerGauntletGem.RUBY:
					return new Color(243, 114, 113);
				case JewelerGauntletGem.DIAMOND:
					return new Color(218, 185, 210);
				case JewelerGauntletGem.AMBER:
					return new Color(233, 168, 48);
				case JewelerGauntletGem.AQUAMARINE:
					return new Color(109, 255, 216);
				case JewelerGauntletGem.OPAL:
					return new Color(255, 146, 163);
			}
		}

		public override void SafeModifyTooltips(List<TooltipLine> tooltips, OrchidGuardian guardian)
		{
			string tooltip;
			Color? color = null;

			switch (GemType)
			{
				default:
					tooltip = Language.GetTextValue(Mod.GetLocalizationKey("Items." + GetType().Name + ".None"));
					color = Color.Gray;
					break;
				case JewelerGauntletGem.AMETHYST:
					tooltip = Language.GetTextValue(Mod.GetLocalizationKey("Items." + GetType().Name + ".Amethyst"));
					break;
				case JewelerGauntletGem.TOPAZ:
					tooltip = Language.GetTextValue(Mod.GetLocalizationKey("Items." + GetType().Name + ".Topaz"));
					break;
				case JewelerGauntletGem.SAPPHIRE:
					tooltip = Language.GetTextValue(Mod.GetLocalizationKey("Items." + GetType().Name + ".Sapphire"));
					break;
				case JewelerGauntletGem.EMERALD:
					tooltip = Language.GetTextValue(Mod.GetLocalizationKey("Items." + GetType().Name + ".Emerald"));
					break;
				case JewelerGauntletGem.RUBY:
					tooltip = Language.GetTextValue(Mod.GetLocalizationKey("Items." + GetType().Name + ".Ruby"));
					break;
				case JewelerGauntletGem.DIAMOND:
					tooltip = Language.GetTextValue(Mod.GetLocalizationKey("Items." + GetType().Name + ".Diamond"));
					break;
				case JewelerGauntletGem.AMBER:
					tooltip = Language.GetTextValue(Mod.GetLocalizationKey("Items." + GetType().Name + ".Amber"));
					break;
				case JewelerGauntletGem.AQUAMARINE:
					tooltip = Language.GetTextValue(Mod.GetLocalizationKey("Items." + GetType().Name + ".Aquamarine"));
					break;
				case JewelerGauntletGem.OPAL:
					tooltip = Language.GetTextValue(Mod.GetLocalizationKey("Items." + GetType().Name + ".Opal"));
					break;
			}

			int index = tooltips.FindIndex(ttip => ttip.Mod.Equals("Terraria") && ttip.Name.Equals("Tooltip0"));
			if (index < 0) index = tooltips.Count - 1;

			TooltipLine line = new(Mod, "Tooltip", tooltip);

			if (color.HasValue)
				line.OverrideColor = color.Value;

			tooltips.Insert(index + 1, line);
		}

		public override void SafeHoldItem(Player player)
		{
			if (GemType == JewelerGauntletGem.DIAMOND)
			{
				Lighting.AddLight(player.Center, new Color(218, 185, 210).ToVector3() * 1f * Main.essScale);
			}

			if (GemType == JewelerGauntletGem.SAPPHIRE || GemType == JewelerGauntletGem.EMERALD || GemType == JewelerGauntletGem.AQUAMARINE || GemType == JewelerGauntletGem.TOPAZ)
			{ // Updated too late (after movement), needs a buffer
				player.GetModPlayer<OrchidGuardian>().GuardianJewelerGauntlet = (byte)GemType;
			}

			if (GemType == JewelerGauntletGem.OPAL && player.Center.Distance(Main.LocalPlayer.Center) < 480f)
			{
				Main.LocalPlayer.AddBuff(ModContent.BuffType<OpalResources>(), 5);
			}
		}

		public override void ExtraAIGauntlet(Player player, OrchidGuardian guardian, Projectile anchor, bool offHandGauntlet)
		{
			if (anchor.ModProjectile is GuardianGauntletAnchor gauntlet) gauntlet.GauntletAnimFrame = (byte)GemType;
		}

		public override void OnParryGauntlet(Player player, OrchidGuardian guardian, Entity aggressor, Projectile anchor)
		{
			if (GemType == JewelerGauntletGem.RUBY)
			{
				player.AddBuff(ModContent.BuffType<RubyHealing>(), 300);
			}
		}

		public override void GauntletModifyHitNPC(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, ref NPC.HitModifiers modifiers, bool charged)
		{
			if (GemType == JewelerGauntletGem.AMETHYST)
			{
				modifiers.ArmorPenetration += 50;
			}
		}

		public override void OnHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, NPC.HitInfo hit, bool charged)
		{
			if (GemType == JewelerGauntletGem.AMBER && charged)
			{
				//guardian.GuardianGuardRecharging += 0.25f;
			}
		}

		// public override Texture2D GetGauntletTexture(Player player, Projectile anchor, bool OffHandGauntlet, out Rectangle? drawRectangle)
		// {
		// 	Texture2D texture = ModContent.Request<Texture2D>(GauntletTexture).Value;
		// 	Rectangle rectangle = texture.Bounds;
		// 	rectangle.Height = rectangle.Height / 10;
		// 	rectangle.Y += (byte)GemType * rectangle.Height;
		// 	drawRectangle = rectangle;
		// 	return texture;
		// }

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			frame.Height /= 10;
			frame.Y += frame.Height * (byte)GemType;
			spriteBatch.Draw(texture, position, frame, drawColor, 0f, frame.Size() * 0.5f, scale * 10f, SpriteEffects.None, 0f);
			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Rectangle drawRectangle = texture.Bounds;
			drawRectangle.Height /= 10;
			drawRectangle.Y += drawRectangle.Height * (byte)GemType;
			spriteBatch.Draw(texture, Item.Center - Main.screenPosition, drawRectangle, lightColor, rotation, drawRectangle.Size() * 0.5f, scale, SpriteEffects.None, 0f);
			return false;
		}

		public override bool CanRightClick() => true;

		public override bool ConsumeItem(Player player) => false;

		public override void RightClick(Player player)
		{
			if (GemType == JewelerGauntletGem.AMETHYST) player.QuickSpawnItem(Item.GetSource_Misc("Jeweler Gauntlet Socket"), ItemID.Amethyst);
			if (GemType == JewelerGauntletGem.TOPAZ) player.QuickSpawnItem(Item.GetSource_Misc("Jeweler Gauntlet Socket"), ItemID.Topaz);
			if (GemType == JewelerGauntletGem.SAPPHIRE) player.QuickSpawnItem(Item.GetSource_Misc("Jeweler Gauntlet Socket"), ItemID.Sapphire);
			if (GemType == JewelerGauntletGem.EMERALD) player.QuickSpawnItem(Item.GetSource_Misc("Jeweler Gauntlet Socket"), ItemID.Emerald);
			if (GemType == JewelerGauntletGem.RUBY) player.QuickSpawnItem(Item.GetSource_Misc("Jeweler Gauntlet Socket"), ItemID.Ruby);
			if (GemType == JewelerGauntletGem.DIAMOND) player.QuickSpawnItem(Item.GetSource_Misc("Jeweler Gauntlet Socket"), ItemID.Diamond);
			if (GemType == JewelerGauntletGem.AMBER) player.QuickSpawnItem(Item.GetSource_Misc("Jeweler Gauntlet Socket"), ItemID.Amber);

			if (OrchidMod.ThoriumMod != null)
			{
				if (GemType == JewelerGauntletGem.AQUAMARINE) player.QuickSpawnItem(Item.GetSource_Misc("Jeweler Gauntlet Socket"), OrchidMod.ThoriumMod.Find<ModItem>("Aquamarine").Type);
				if (GemType == JewelerGauntletGem.OPAL) player.QuickSpawnItem(Item.GetSource_Misc("Jeweler Gauntlet Socket"), OrchidMod.ThoriumMod.Find<ModItem>("Opal").Type);
			}

			GemType = JewelerGauntletGem.NONE;

			if (Main.mouseItem.type == ItemID.Amethyst) GemType = JewelerGauntletGem.AMETHYST;
			if (Main.mouseItem.type == ItemID.Topaz) GemType = JewelerGauntletGem.TOPAZ;
			if (Main.mouseItem.type == ItemID.Sapphire) GemType = JewelerGauntletGem.SAPPHIRE;
			if (Main.mouseItem.type == ItemID.Emerald) GemType = JewelerGauntletGem.EMERALD;
			if (Main.mouseItem.type == ItemID.Ruby) GemType = JewelerGauntletGem.RUBY;
			if (Main.mouseItem.type == ItemID.Diamond) GemType = JewelerGauntletGem.DIAMOND;
			if (Main.mouseItem.type == ItemID.Amber) GemType = JewelerGauntletGem.AMBER;

			if (OrchidMod.ThoriumMod != null)
			{
				if (Main.mouseItem.type == OrchidMod.ThoriumMod.Find<ModItem>("Aquamarine").Type) GemType = JewelerGauntletGem.AQUAMARINE;
				if (Main.mouseItem.type == OrchidMod.ThoriumMod.Find<ModItem>("Opal").Type) GemType = JewelerGauntletGem.OPAL;
			}

			if (GemType != JewelerGauntletGem.NONE)
			{ // Type changed, remove a gem from the player cursor
				if (Main.mouseItem.stack > 1) Main.mouseItem.stack--;
				else Main.mouseItem.TurnToAir();
			}

			Item.NetStateChanged();
			return;
		}

		public override void SaveData(TagCompound tag)
		{
			tag.Add("Gem", (byte)GemType);
		}

		public override void LoadData(TagCompound tag)
		{
			GemType = (JewelerGauntletGem)tag.GetByte("Gem");
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write((byte)GemType);
		}

		public override void NetReceive(BinaryReader reader)
		{
			GemType = (JewelerGauntletGem)reader.ReadByte();
		}
	}
}
