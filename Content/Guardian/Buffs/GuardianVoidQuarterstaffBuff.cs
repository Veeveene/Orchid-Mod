using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using OrchidMod.Content.Guardian.Weapons.Quarterstaves;
using OrchidMod.Utilities;
using Terraria.DataStructures;

namespace OrchidMod.Content.Guardian.Buffs
{
	public class GuardianVoidQuarterstaffBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.HeldItem != null && player.HeldItem.ModItem is ThoriumVoidQuarterstaff)
			{
				player.GetAttackSpeed(DamageClass.Melee) += 0.5f;
				player.Guardian().GuardianChargeSpeed += 2f;
			}
		}

		public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
		{
			// if (Main.dedServ) return;
			// OrchidGuardian guardian = Main.LocalPlayer.Guardian();
			// if (guardian != null && guardian.GuardianVoidQuarterstaff > 0)
			// {
			// 	DynamicSpriteFont font = FontAssets.MouseText.Value;
			// 	Vector2 textSize = ChatManager.GetStringSize(font, guardian.GuardianVoidQuarterstaff.ToString(), Vector2.One);
			// 	Vector2 drawPos = drawParams.Position + drawParams.SourceRectangle.Size() * 0.5f + textSize * 0.5f;
			// 	ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, guardian.GuardianVoidQuarterstaff.ToString(), drawPos, Color.Magenta, Color.Black, 0f, textSize * 0.5f, Vector2.One * 0.8f);
			// }
		}
	}
}