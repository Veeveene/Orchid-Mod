using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod;
using Terraria;
using OrchidMod.Common.Attributes;
using OrchidMod.Content.Guardian;
using OrchidMod.Utilities;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using static Terraria.Framing;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers {

    [CrossmodContent("ThoriumMod")]
    public class ThoriumGeodeVeinhammer : OrchidModGuardianHammer
    {
        public override void SafeSetDefaults()
        {
            Item.width = 48;
            Item.height = 44;
            Item.value = Item.sellPrice(0, 2);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.DD2_MonkStaffSwing;
            Item.knockBack = 8f;
            Item.shootSpeed = 10f;
            Item.damage = 112;
            Item.useTime = 8;
            Range = 50;
            ReturnSpeed = 2f;
            SwingSpeed = 0.2f;
            SwingChargeGain = 0.1f;
            SlamStacks = 1;
            BlockDuration = 120;
            
        }

        public override void OnSwing(Player player, OrchidGuardian guardian, Projectile projectile, bool FullyCharged)
        {
            for (int i = 0; i < 3; i++)
            {
                Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.BoneTorch, Alpha: 150, Scale: 0.75f);
                dust.fadeIn = 1f;
                dust.noGravity = true;
            }
        }

        public override void ExtraAI(Player player, OrchidGuardian guardian, Projectile projectile)
        {
            // guardian.SlamCostUI = 1;
            
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.BoneTorch, Alpha: 150, Scale: 0.75f);
                dust.fadeIn = 1f;
                dust.noGravity = true;
                
                if (projectile.ModProjectile is GuardianHammerAnchor anchor && anchor.Ding)
                {
                    Lighting.AddLight(projectile.Center, Color.Lavender.ToVector3());
                    for (int t = 0; t < 8; t++)
                    {
                        if (Main.rand.NextBool(3))
                        {
                            var chargedDust = Dust.NewDustPerfect(projectile.Center + Vector2.UnitX.RotatedBy(t * MathHelper.TwoPi / 8) * projectile.Size * 2, Main.rand.Next(68, 71), null, 200, Color.LavenderBlush, Main.rand.NextFloat(0.75f, 1.25f));
                            chargedDust.fadeIn = 1f;
                            chargedDust.noGravity = true;
                        }
                    }
                } 
            }
        }

        public override void OnThrow(Player player, OrchidGuardian guardian, Projectile projectile, bool Weak)
        {
            if (!Weak && projectile.ModProjectile is GuardianHammerAnchor anchor) 
            {
                projectile.Center = player.Center;
                anchor.range = 50 + (25 * player.blockRange);
            }
        }
        
        public override void OnThrowTileCollide(Player player, OrchidGuardian guardian, Projectile projectile, Vector2 oldVelocity)
        {
            if (projectile.ModProjectile is GuardianHammerAnchor anchor && !anchor.WeakThrow)
            {
                Point collisionPoint = new((int)((projectile.Center.X + oldVelocity.X) / 16f), (int)((projectile.Center.Y + oldVelocity.Y) / 16f));
                int blastRadius = 2;
            
                int minTileX = collisionPoint.X - blastRadius;
                int maxTileX = collisionPoint.X + blastRadius;
                int minTileY = collisionPoint.Y - blastRadius;
                int maxTileY = collisionPoint.Y + blastRadius;
            
                Utils.ClampWithinWorld(ref minTileX, ref minTileY, ref maxTileX, ref maxTileY);

                for (int i = minTileX; i <= maxTileX; i++ )
                for (int j = minTileY; j <= maxTileY; j++ )
                {
                    if (!((i == minTileX || i == maxTileX) && (j == minTileY || j == maxTileY)))
                    {
                        Tile tile = GetTileSafely(i,j);
                        if (tile.HasTile && tile != null)
                        {
                            ushort? tileUp = GetTileSafely(i, j - 1).TileType;
                            if (player.HasEnoughPickPowerToHurtTile(i, j) && tileUp is not (5 or 72 or 80 or 323 or > 583 and < 589 or 596 or 616 or 634)) // Test if tile above is a tree) 
                            {    
                                WorldGen.KillTile(i, j);
                                if (Main.netMode == NetmodeID.MultiplayerClient) NetMessage.SendData(MessageID.TileManipulation, number: 0, number2: i, number3: j);
                            }
                            else
                            {
                                WorldGen.KillTile(i, j, true, true);
                                SoundEngine.PlaySound(SoundID.Item150);
                            }
                        }
                    }
                }
            }
        }

        public override void AddRecipes()
        {
            if (ModLoader.TryGetMod("ThoriumMod", out Mod thoriumMod))
                CreateRecipe()
                .AddTile(TileID.Anvils)
                .AddIngredient(thoriumMod, "CrystalGeode", 10)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);
            
            int index = tooltips.FindIndex(ttip => ttip.Mod == "Terraria" && ttip.Name == "Tooltip1");
            
            Item bestPick = Main.LocalPlayer?.GetBestPickaxe();
            string TooltipToGet = Mod.GetLocalizationKey("Items.ThoriumGeodeVeinhammer.PickaxePowerNone");
            if (bestPick != null && bestPick.pick > 0) TooltipToGet = Mod.GetLocalizationKey("Items.ThoriumGeodeVeinhammer.PickaxePower");

            string text = Language.GetText(TooltipToGet).Format(bestPick.pick);
            if (Main.keyState.PressingShift()) text += string.Format(" ({0} [i:{1}])", bestPick.Name, bestPick.type);

            tooltips.Insert(index + 1, new TooltipLine(Mod, "VeinhammerPickPower", text));
        }
    }
}
