using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Utilities;
using Terraria;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian;

public interface IGuardianCustomUIItem
{
	[CanBeNull]
	string TextureName { get; }
	
	[CanBeNull] public string TextureOffPath(bool fancy) => null;
	[CanBeNull] public string TextureOnPath(bool fancy) => null;
	[CanBeNull] public string TextureReadyPath(bool fancy) => null;
	
	public float ChargeThreshold(Player player) => (70 * player.GetTotalAttackSpeed(DamageClass.Melee) - player.HeldItem.useTime) / 2.5f;
	
	public bool PreDrawUI(SpriteBatch spriteBatch, Vector2 drawPos, ref Texture2D textureOn, ref Texture2D textureOff, ref Texture2D textureReady) => false;
}