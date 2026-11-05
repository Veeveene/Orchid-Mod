using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Assets;
using OrchidMod.Common.Attributes;
using Terraria;
using Terraria.ModLoader;

namespace OrchidMod.Common.Global.Projectiles;

[CrossmodContent("ThoriumMod")]
public class ThoriumCrossmodGlobalProjectile : GlobalProjectile 
{
	public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("ThoriumMod") && OrchidMod.ThoriumMod != null;
	
	public static int ThoriumUselessFriendID { get; private set; } = -1;
	
    public static bool _Initialized;

    private static Type UselessFriend;
    private static FieldInfo UselessFriend_SkinChoice;
    private static FieldInfo UselessFriend_ItemFlamePos;

	public override void SetStaticDefaults()
	{
		var thoriumMod = ModLoader.GetMod("ThoriumMod") ?? OrchidMod.ThoriumMod;
		ThoriumUselessFriendID = thoriumMod.Find<ModProjectile>("EmptyWormholePotionPro").Type;
		var uselessFriend = thoriumMod.Code.GetType("ThoriumMod.Projectiles.LightPets.EmptyWormholePotionPro");
        if (uselessFriend != null)
        {
            UselessFriend = uselessFriend;

            var skinChoice = UselessFriend.GetField("skinChoice", BindingFlags.Public | BindingFlags.Instance);
            if (skinChoice != null) UselessFriend_SkinChoice = skinChoice;

            var itemFlamePos = UselessFriend.GetField("itemFlamePos", BindingFlags.Public | BindingFlags.Instance);
            if (itemFlamePos != null) UselessFriend_ItemFlamePos = itemFlamePos;

            OrchidMod.Instance.Logger.Info("Useless Friend detected");
        }
	}

    public override void Unload()
    {
        UselessFriend = null;
        UselessFriend_SkinChoice = null;
        UselessFriend_ItemFlamePos = null;
    }


    public override bool PreAI(Projectile projectile)
    {
        var thoriumMod = OrchidMod.ThoriumMod;
        if (thoriumMod != null)
        {
            if (ThoriumUselessFriendID != -1 && projectile.type == ThoriumUselessFriendID && projectile.owner == Main.myPlayer && projectile.ModProjectile != null && projectile.ModProjectile.GetType() == UselessFriend)
            {
                // if (Main.rand.NextBool(15)) CombatText.NewText(Main.LocalPlayer.getRect(), Color.Aqua, "Applies!");

                // if (UselessFriend_ItemFlamePos != null && ((Vector2[])UselessFriend_ItemFlamePos.GetValue(projectile.ModProjectile)).Length > 0)
                //     CombatText.NewText(Main.LocalPlayer.getRect(), Color.Orange, "itemFlamePos detected");

                if (UselessFriend_SkinChoice != null && UselessFriend_SkinChoice.GetValue(projectile.ModProjectile) is (byte)0)
                {
                    UselessFriend_SkinChoice.SetValue(projectile.ModProjectile, (byte)(1 + Main.rand.Next(9)));
                    if (UselessFriend_SkinChoice.GetValue(projectile.ModProjectile) is byte newValue) CombatText.NewText(projectile.getRect(), Color.Magenta, newValue);
                    projectile.netUpdate = true;
                    return false;
                }
            }
        }
        return base.PreAI(projectile);
    }

	public override bool PreDraw(Projectile projectile, ref Color lightColor)
	{
        var thoriumMod = OrchidMod.ThoriumMod;
        if (thoriumMod != null)
        {
            if (projectile.type == thoriumMod.Find<ModProjectile>("EmptyWormholePotionPro").Type && projectile.owner == Main.myPlayer && projectile.ModProjectile != null && projectile.ModProjectile.GetType() == UselessFriend)
            {                
                if (UselessFriend_SkinChoice != null && UselessFriend_SkinChoice.GetValue(projectile.ModProjectile) is byte choice)
                {
	                switch (choice)
	                {
		                case 0:
			                return true;
		                case 9:
		                {
			                // Draw code paraphrased from ThoriumMod.Projectiles.LightPets.EmptyWormholePotionPro

			                SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			                string path = OrchidAssets.ProjectilesPath + "ThoriumUselessFriend";
			                string origPath = "ThoriumMod/Projectiles/LightPets/EmptyWormholePotionPro";

			                int verticalFrames = Main.projFrames[projectile.type];

			                Texture2D wingTexture = ModContent.Request<Texture2D>(path + "_Wings").Value;
			                Vector2 drawPos = projectile.Center + new Vector2(projectile.ModProjectile.DrawOffsetX, projectile.gfxOffY) - Main.screenPosition;

			                Vector2 drawOffset = Vector2.UnitX * projectile.spriteDirection * 10f;

			                Vector2 drawOriginOffset = new(projectile.ModProjectile.DrawOriginOffsetX, projectile.ModProjectile.DrawOriginOffsetY);
			                if (projectile.frame == 7) Main.EntitySpriteDraw(wingTexture, drawPos + drawOffset, null, lightColor, projectile.rotation, wingTexture.Size() * 0.5f - drawOriginOffset, projectile.scale, effects);

			                Texture2D skinTexture = ModContent.Request<Texture2D>(path + "_Skin").Value;
			                Rectangle frame = skinTexture.Frame(1, verticalFrames, 0, projectile.frame);
			                Main.EntitySpriteDraw(skinTexture, drawPos, frame, lightColor, projectile.rotation, frame.Size() * 0.5f - drawOriginOffset, projectile.scale, effects);
                        
			                Texture2D accTexture;
			                if (!projectile.wet)
			                {
				                accTexture = ModContent.Request<Texture2D>(origPath + "_TorchGlow").Value;
				                Main.EntitySpriteDraw(accTexture, drawPos, frame, new Color(255, 255, 255, 50) * 0.8f, projectile.rotation, frame.Size() * 0.5f - drawOriginOffset, projectile.scale, effects);

				                if (UselessFriend_ItemFlamePos != null && UselessFriend_ItemFlamePos.GetValue(projectile.ModProjectile) is Vector2[] itemFlamePos)
				                {
					                accTexture = ModContent.Request<Texture2D>(origPath + "_TorchFlame").Value;
					                for (int i = 0; i < itemFlamePos.Length; i++)
					                {
						                Color color = new(100, 100, 100, 0);
						                Vector2 position = drawPos + itemFlamePos[i];
						                Main.EntitySpriteDraw(accTexture, position, frame, color, projectile.rotation, frame.Size() * 0.5f - drawOriginOffset, projectile.scale, effects);
					                }
				                }
			                }
			                else
			                {
				                accTexture = ModContent.Request<Texture2D>(origPath + "_GlowstickGlow").Value;
				                Main.EntitySpriteDraw(accTexture, drawPos, frame, Color.White, projectile.rotation, frame.Size() * 0.5f - drawOriginOffset, projectile.scale, effects);
			                }

			                return false;
		                }
	                }
                }
            }
        }
		return base.PreDraw(projectile, ref lightColor);
	}
}