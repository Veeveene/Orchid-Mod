using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Content.Guardian.Misc;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace OrchidMod.Content.Guardian.Armors.Horizon
{
	[AutoloadEquip(EquipType.Head)]
	public class GuardianHorizonHead : OrchidModGuardianEquipable
	{
		public static LocalizedText SetBonusText { get; private set; }

		public override void SetStaticDefaults()
		{
			SetBonusText = this.GetLocalization("SetBonus");
		}

		public override void SafeSetDefaults()
		{
			Item.width = 26;
			Item.height = 28;
			Item.value = Item.sellPrice(0, 7, 0, 0);
			Item.rare = ItemRarityID.Red;
			Item.defense = 28;
		}

		public override void UpdateEquip(Player player)
		{
			OrchidGuardian modPlayer = Guardian(player);
			modPlayer.modPlayer.OrchidDamageResistance += 0.15f;
			player.GetDamage<GuardianDamageClass>() += 0.15f;
			player.GetCritChance<GuardianDamageClass>() += 15;
			player.aggro += 1500;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ItemType<GuardianHorizonChest>() && legs.type == ItemType<GuardianHorizonLegs>();
		}

		public override void UpdateArmorSet(Player player)
		{
			OrchidGuardian modPlayer = Guardian(player);
			player.setBonus = SetBonusText.Value;
			modPlayer.GuardianHorizon = true;
		}

		/*
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
		{
			spriteBatch.DrawSimpleItemGlowmaskInWorld(Item, new Color(250, 250, 250, 200) * OrchidWorld.alchemistMushroomArmorProgress, rotation, scale);
		}
		*/

		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			Color color = new Color(229, 181, 142) * 0.5f;
			Lighting.AddLight(Item.Center, color.R / 255f, color.G / 255f, color.B / 255f);
		}

		public override void EquipFrameEffects(Player player, EquipType type)
		{
			Color color = new Color(229, 181, 142) * 0.75f;
			Lighting.AddLight(player.Center, color.R / 255f, color.G / 255f, color.B / 255f);
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddIngredient<HorizonFragment>(10);
			recipe.AddIngredient(ItemID.LunarBar, 8);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.Register();
		}
	}

	public class GuardianHorizonHeadGlowmask : PlayerDrawLayer
	{
		public override Position GetDefaultPosition()
		{
			return new AfterParent(PlayerDrawLayers.Head);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			/*
			SpriteBatch spriteBatch = Main.spriteBatch;
			spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
			*/

			Player drawPlayer = drawInfo.drawPlayer;
			if (drawInfo.drawPlayer.dead)
			{
				return;
			}

			if (drawPlayer.armor[10].type == ItemType<GuardianHorizonHead>() || (drawPlayer.armor[10].type == ItemID.None && drawPlayer.armor[0].type == ItemType<GuardianHorizonHead>()))
			{
				Color color = drawPlayer.GetImmuneAlphaPure(Color.White, drawInfo.shadow);
				float mult = (((float)drawPlayer.statLifeMax2 - (float)drawPlayer.statLife) / (float)drawPlayer.statLifeMax2) * 0.66f;
				if (mult > 0.5f) mult = 0.5f;
				color *= mult;

				Texture2D texture = Request<Texture2D>("OrchidMod/Content/Guardian/Armors/Horizon/GuardianHorizonHead_Head_Glow").Value;
				Vector2 drawPos = drawInfo.Position - Main.screenPosition + new Vector2(drawPlayer.width / 2 - drawPlayer.bodyFrame.Width / 2, drawPlayer.height - drawPlayer.bodyFrame.Height + 4f) + drawPlayer.headPosition;
				Vector2 headVect = drawInfo.headVect;
				DrawData drawData = new DrawData(texture, drawPos.Floor() + headVect, drawPlayer.bodyFrame, color, drawPlayer.headRotation, headVect, 1f, drawInfo.playerEffect, 0)
				{
					shader = GameShaders.Armor.GetShaderIdFromItemId(ItemType<HorizonDye>())
				};
				//GameShaders.Misc["OrchidMod:HorizonGlow"].Apply(drawData);
				drawInfo.DrawDataCache.Add(drawData);
			}
			/*
			spriteBatch.End();
			spriteBatch.Begin(spriteBatchSnapshot);
			*/
		}
	}

	public class GuardianHorizonPonytailGlowmask : PlayerDrawLayer
	{
		public override Position GetDefaultPosition()
		{
			return new AfterParent(PlayerDrawLayers.Head);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			/*
			SpriteBatch spriteBatch = Main.spriteBatch;
			spriteBatch.End(out SpriteBatchSnapshot spriteBatchSnapshot);
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
			*/

			Player drawPlayer = drawInfo.drawPlayer;
			if (drawInfo.drawPlayer.dead)
			{
				return;
			}

			if (drawPlayer.armor[10].type == ItemType<GuardianHorizonHead>() || (drawPlayer.armor[10].type == ItemID.None && drawPlayer.armor[0].type == ItemType<GuardianHorizonHead>()))
			{
				Color color = drawPlayer.GetImmuneAlphaPure(Color.White, drawInfo.shadow);
				color *= 0.5f;

				Texture2D texture = Request<Texture2D>("OrchidMod/Content/Guardian/Armors/Horizon/GuardianHorizonHead_Head_Glow_Ponytail").Value;
				Vector2 drawPos = drawInfo.Position - Main.screenPosition + new Vector2(drawPlayer.width / 2 - drawPlayer.bodyFrame.Width / 2, drawPlayer.height - drawPlayer.bodyFrame.Height + 4f) + drawPlayer.headPosition;
				Vector2 headVect = drawInfo.headVect;
				DrawData drawData = new DrawData(texture, drawPos.Floor() + headVect, drawPlayer.bodyFrame, color, drawPlayer.headRotation, headVect, 1f, drawInfo.playerEffect, 0)
				{
					shader = GameShaders.Armor.GetShaderIdFromItemId(ItemType<HorizonDye>())
				};
				//GameShaders.Misc["OrchidMod:HorizonGlow"].Apply(drawData);
				drawInfo.DrawDataCache.Add(drawData);
			}
			/*
			spriteBatch.End();
			spriteBatch.Begin(spriteBatchSnapshot);
			*/
		}
	}
}
