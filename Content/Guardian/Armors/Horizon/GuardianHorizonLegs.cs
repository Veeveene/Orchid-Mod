using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Content.Guardian.Misc;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace OrchidMod.Content.Guardian.Armors.Horizon
{
	[AutoloadEquip(EquipType.Legs)]
	public class GuardianHorizonLegs : OrchidModGuardianEquipable
	{
		public override void SafeSetDefaults()
		{
			Item.width = 22;
			Item.height = 18;
			Item.value = Item.sellPrice(0, 10, 50, 0);
			Item.rare = ItemRarityID.Red;
			Item.defense = 24;
		}

		public override void UpdateEquip(Player player)
		{
			OrchidGuardian modPlayer = Guardian(player);
			modPlayer.modPlayer.OrchidDamageResistance += 0.2f;
			player.moveSpeed += 0.2f;
			player.GetAttackSpeed(DamageClass.Melee) += 0.2f;
			modPlayer.GuardianGuardMax += 3;
			modPlayer.GuardianSlamMax += 3;
		}

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
			recipe.AddIngredient<HorizonFragment>(15);
			recipe.AddIngredient(ItemID.LunarBar, 12);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.Register();
		}
	}

	public class GuardianHorizonLegsGlowmask : PlayerDrawLayer
	{
		public override Position GetDefaultPosition()
		{
			return new AfterParent(PlayerDrawLayers.Leggings);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			Player drawPlayer = drawInfo.drawPlayer;
			if (drawInfo.drawPlayer.dead)
			{
				return;
			}

			if (drawPlayer.armor[12].type == ItemType<GuardianHorizonLegs>() || (drawPlayer.armor[12].type == ItemID.None && drawPlayer.armor[2].type == ItemType<GuardianHorizonLegs>()))
			{
				Color color = drawPlayer.GetImmuneAlphaPure(Color.White, drawInfo.shadow);
				float mult = (((float)drawPlayer.statLifeMax2 - (float)drawPlayer.statLife) / (float)drawPlayer.statLifeMax2) * 0.66f;
				if (mult > 0.5f) mult = 0.5f;
				color *= mult;

				Texture2D texture = Request<Texture2D>("OrchidMod/Content/Guardian/Armors/Horizon/GuardianHorizonLegs_Legs_Glow").Value;
				Vector2 drawPos = drawInfo.Position - Main.screenPosition + new Vector2(drawPlayer.width / 2 - drawPlayer.legFrame.Width / 2, drawPlayer.height - drawPlayer.legFrame.Height + 4f) + drawPlayer.legPosition;
				Vector2 legsOffset = drawInfo.legsOffset;
				DrawData drawData = new DrawData(texture, drawPos.Floor() + legsOffset, drawPlayer.legFrame, color, drawPlayer.legRotation, legsOffset, 1f, drawInfo.playerEffect, 0)
				{
					shader = GameShaders.Armor.GetShaderIdFromItemId(ItemType<HorizonDye>())
				};
				drawInfo.DrawDataCache.Add(drawData);
			}
		}
	}
}
