using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common.ModObjects;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace OrchidMod.Content.Shapeshifter.Armors.Ashwood
{
	[AutoloadEquip(EquipType.Head)]
	public class ShapeshifterAshwoodHead : OrchidModShapeshifterItem
	{
		public static Texture2D TextureGlow;
		public static LocalizedText SetBonusText { get; private set; }

		public override void SetStaticDefaults()
		{
			SetBonusText = this.GetLocalization("SetBonus");
		}

		public override void SafeSetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.value = Item.sellPrice(0, 0, 50, 0);
			Item.rare = ItemRarityID.Orange;
			Item.defense = 6;
			TextureGlow ??= ModContent.Request<Texture2D>(Item.ModItem.Texture + "_Head_Glow", AssetRequestMode.ImmediateLoad).Value;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetCritChance<ShapeshifterDamageClass>() += 10;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ItemType<ShapeshifterAshwoodChest>() && legs.type == ItemType<ShapeshifterAshwoodLegs>();
		}

		public override void UpdateArmorSet(Player player)
		{
			OrchidShapeshifter shapeshifter = Shapeshifter(player);
			shapeshifter.ShapeshifterSetPyre = true;
			player.setBonus = SetBonusText.Value;
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.AshWood, 20);
			recipe.AddIngredient(ItemID.HellstoneBar, 8);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class ShapeshifterAshwoodHeadGlowmask : PlayerDrawLayer
	{
		public override Position GetDefaultPosition()
		{
			return new AfterParent(PlayerDrawLayers.Head);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			Player drawPlayer = drawInfo.drawPlayer;
			if (drawInfo.drawPlayer.dead)
			{
				return;
			}

			if (drawPlayer.armor[10].type == ItemType<ShapeshifterAshwoodHead>() || (drawPlayer.armor[10].type == ItemID.None && drawPlayer.armor[0].type == ItemType<ShapeshifterAshwoodHead>()))
			{
				OrchidPlayer modPlayer = drawPlayer.GetModPlayer<OrchidPlayer>();
				float colorFlicker = (0.4f + (float)(Math.Sin(modPlayer.Timer * 0.04851f) * 0.1f) + (float)(Math.Sin(modPlayer.Timer * 0.123f) * 0.05f) + (float)(Math.Sin(modPlayer.Timer * 0.31461f) * 0.02f) + (float)(Math.Sin(modPlayer.Timer * 0.07124f) * 0.03f));
				Color color = drawPlayer.GetImmuneAlphaPure(Color.White * colorFlicker, drawInfo.shadow);

				Vector2 drawPos = drawInfo.Position - Main.screenPosition + new Vector2(drawPlayer.width / 2 - drawPlayer.bodyFrame.Width / 2, drawPlayer.height - drawPlayer.bodyFrame.Height + 4f) + drawPlayer.headPosition;
				Vector2 headVect = drawInfo.headVect;
				DrawData drawData = new DrawData(ShapeshifterAshwoodHead.TextureGlow, drawPos.Floor() + headVect, drawPlayer.bodyFrame, color, drawPlayer.headRotation, headVect, 1f, drawInfo.playerEffect, 0);

				drawInfo.DrawDataCache.Add(drawData);
			}
		}
	}
}
