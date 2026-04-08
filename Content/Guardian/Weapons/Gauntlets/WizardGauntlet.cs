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
	public class WizardGauntlet : OrchidModGuardianGauntlet
	{
		public int MageMode;

		public override void SafeSetDefaults()
		{
			Item.width = 24;
			Item.height = 28;
			Item.knockBack = 5f;
			Item.damage = 124;
			Item.value = Item.buyPrice(0, 80);
			Item.rare = ItemRarityID.LightPurple;
			Item.useTime = 30;
			StrikeVelocity = 16f;
			ParryDuration = 60;
			GauntletFrames = 10;

			MageMode = 0;
		}

		public override void SafeHoldItem(Player player)
		{
			if (MageMode == 1)
				Item.DamageType = DamageClass.Magic;
			else Item.DamageType = ModContent.GetInstance<GuardianDamageClass>();

		}

		public override void GauntletModifyHitNPC(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, ref NPC.HitModifiers modifiers, bool charged)
		{
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			frame.Height /= 2;
			frame.Y += frame.Height * MageMode;
			spriteBatch.Draw(texture, position, frame, drawColor, 0f, frame.Size() * 0.5f, scale * 2f, SpriteEffects.None, 0f);
			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Rectangle drawRectangle = texture.Bounds;
			drawRectangle.Height /= 2;
			drawRectangle.Y += drawRectangle.Height * MageMode;
			spriteBatch.Draw(texture, Item.Center - Main.screenPosition, drawRectangle, lightColor, rotation, drawRectangle.Size() * 0.5f, scale, SpriteEffects.None, 0f);
			return false;
		}

		public override bool CanRightClick() => true;

		public override bool ConsumeItem(Player player) => false;

		public override void RightClick(Player player)
		{
			if (MageMode == 0) MageMode = 1;
			else MageMode = 0;

			Item.NetStateChanged();
			return;
		}

		public override void SaveData(TagCompound tag)
		{
			tag.Add("MageMode", MageMode);
		}

		public override void LoadData(TagCompound tag)
		{
			MageMode = tag.GetByte("MageMode");
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write(MageMode);
		}

		public override void NetReceive(BinaryReader reader)
		{
			MageMode = reader.ReadByte();
		}
	}
}
