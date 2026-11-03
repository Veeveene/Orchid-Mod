using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace OrchidMod.Content.Shapeshifter
{
	public class ShapeshifterMapLayer : ModMapLayer
	{
		public override void Draw(ref MapOverlayDrawContext context, ref string text) {
			const float scaleIfNotSelected = 1f;
			const float scaleIfSelected = scaleIfNotSelected * 1.33f;

			foreach (Player player in Main.player)
			{
				if (player.active)
				{
					ShapeshifterShapeshiftAnchor anchor = player.Shapeshifter().ShapeshiftAnchor;
					if (anchor != null && (player.team == Main.LocalPlayer.team || player.team == 0))
					{ // Don't draw if the player is not transformer or in another team
						if (player.whoAmI != Main.myPlayer)
						{
							if (context.Draw(anchor.TextureShapeshiftIcon, anchor.Projectile.position / 16f, Color.White, new SpriteFrame(1, 1, 0, 0), scaleIfNotSelected, scaleIfSelected, Alignment.Center).IsMouseOver)
							{ // Display a yellow border when hovering other clients icons
								text = player.name;
								context.Draw(anchor.TextureShapeshiftIconBorder, anchor.Projectile.position / 16f, Color.Yellow, new SpriteFrame(1, 1, 0, 0), scaleIfNotSelected, scaleIfSelected, Alignment.Center);
							}
							else
							{ // Else display team color
								context.Draw(anchor.TextureShapeshiftIconBorder, anchor.Projectile.position / 16f, Main.teamColor[player.team], new SpriteFrame(1, 1, 0, 0), scaleIfNotSelected, scaleIfSelected, Alignment.Center);
							}
						}
						else
						{
							if (context.Draw(anchor.TextureShapeshiftIcon, anchor.Projectile.position / 16f, Color.White, new SpriteFrame(1, 1, 0, 0), scaleIfNotSelected, scaleIfNotSelected, Alignment.Center).IsMouseOver)
							{ // never change the scale when hovering local client icon
								text = player.name;
							}
							context.Draw(anchor.TextureShapeshiftIconBorder, anchor.Projectile.position / 16f, Main.teamColor[player.team], new SpriteFrame(1, 1, 0, 0), scaleIfNotSelected, scaleIfNotSelected, Alignment.Center);
						}
					}
				}
			}
		}
	}
}