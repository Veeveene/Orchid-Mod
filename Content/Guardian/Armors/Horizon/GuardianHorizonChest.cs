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
	[AutoloadEquip(EquipType.Body)]
	public class GuardianHorizonChest : OrchidModGuardianEquipable
	{
		public override void SafeSetDefaults()
		{
			Item.width = 34;
			Item.height = 24;
			Item.value = Item.sellPrice(0, 14, 0, 0);
			Item.rare = ItemRarityID.Red;
			Item.defense = 38;
		}

		public override void UpdateEquip(Player player)
		{
			OrchidGuardian modPlayer = Guardian(player);
			modPlayer.modPlayer.OrchidDamageResistance += 0.25f;
			player.GetDamage<GuardianDamageClass>() += 0.25f;
			player.GetAttackSpeed(DamageClass.Melee) += 0.25f;
			player.statLifeMax2 += 100;
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
			recipe.AddIngredient<HorizonFragment>(20);
			recipe.AddIngredient(ItemID.LunarBar, 16);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.Register();
		}
	}


	public class GuardianHorizonChestGlowmask : PlayerDrawLayer
	{
		public override Position GetDefaultPosition()
		{
			return new AfterParent(PlayerDrawLayers.Torso);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			Player drawPlayer = drawInfo.drawPlayer;
			if (drawInfo.drawPlayer.dead)
			{
				return;
			}

			if (drawPlayer.armor[11].type == ItemType<GuardianHorizonChest>() || (drawPlayer.armor[11].type == ItemID.None && drawPlayer.armor[1].type == ItemType<GuardianHorizonChest>()))
			{
				Color color = drawPlayer.GetImmuneAlphaPure(Color.White, drawInfo.shadow);
				float mult = (((float)drawPlayer.statLifeMax2 - (float)drawPlayer.statLife) / (float)drawPlayer.statLifeMax2) * 0.66f;
				if (mult > 0.5f) mult = 0.5f;
				color *= mult;

				Texture2D texture = Request<Texture2D>("OrchidMod/Content/Guardian/Armors/Horizon/GuardianHorizonChest_Body_Glow").Value;
				float drawX = (int)drawInfo.Position.X + drawPlayer.width / 2;
				float drawY = (int)drawInfo.Position.Y + drawPlayer.height - drawPlayer.bodyFrame.Height / 2 + 4f;
				Vector2 origin = drawInfo.bodyVect;
				Vector2 position = new Vector2(drawX, drawY) + drawPlayer.bodyPosition - Main.screenPosition;
				Rectangle frame = new(0, 0, 40, 56);
				if (drawPlayer.Male)
				{
					if (drawPlayer.compositeFrontArm.enabled || drawPlayer.bodyFrame == new Rectangle(0, 56 * 7, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 8, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 9, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 14, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 15, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 16, 40, 56))
					{
						frame = new(0, 2, 40, 56); //walking bop
					}
					if (drawPlayer.bodyFrame == new Rectangle(0, 56 * 5, 40, 56) && !drawPlayer.compositeFrontArm.enabled)
					{
						frame = new(40, 0, 40, 56); //jumping frame
					}
				}
				else
				{
					frame = new(0, 112, 40, 56);
					if (drawPlayer.compositeFrontArm.enabled || drawPlayer.bodyFrame == new Rectangle(0, 56 * 7, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 8, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 9, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 14, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 15, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 16, 40, 56))
					{
						frame = new(0, 114, 40, 56); //walking bop
					}
					if (drawPlayer.bodyFrame == new Rectangle(0, 56 * 5, 40, 56) && !drawPlayer.compositeFrontArm.enabled)
					{
						frame = new(40, 112, 40, 56); //jumping frame
					}
				}
				float rotation = drawPlayer.bodyRotation;
				SpriteEffects spriteEffects = drawInfo.playerEffect;

				DrawData drawData = new(texture, position, frame, color, rotation, origin, 1f, spriteEffects, 0)
				{
					shader = GameShaders.Armor.GetShaderIdFromItemId(ItemType<HorizonDye>())
				};
				drawInfo.DrawDataCache.Add(drawData);
			}
		}
	}

	public class GuardianHorizonChestGlowmaskShoulder : PlayerDrawLayer
	{
		public override Position GetDefaultPosition()
		{
			return new AfterParent(PlayerDrawLayers.ArmOverItem);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			Player drawPlayer = drawInfo.drawPlayer;
			if (drawInfo.drawPlayer.dead)
			{
				return;
			}

			if (drawPlayer.armor[11].type == ItemType<GuardianHorizonChest>() || (drawPlayer.armor[11].type == ItemID.None && drawPlayer.armor[1].type == ItemType<GuardianHorizonChest>()) && (drawPlayer.bodyFrame != new Rectangle(0, 56 * 5, 40, 56) || drawPlayer.compositeFrontArm.enabled))
			{
				Color color = drawPlayer.GetImmuneAlphaPure(Color.White, drawInfo.shadow);
				float mult = (((float)drawPlayer.statLifeMax2 - (float)drawPlayer.statLife) / (float)drawPlayer.statLifeMax2) * 0.66f;
				if (mult > 0.5f) mult = 0.5f;
				color *= mult;

				Texture2D texture = Request<Texture2D>("OrchidMod/Content/Guardian/Armors/Horizon/GuardianHorizonChest_Body_Glow").Value;
				float drawX = (int)drawInfo.Position.X + drawPlayer.width / 2;
				float drawY = (int)drawInfo.Position.Y + drawPlayer.height - drawPlayer.bodyFrame.Height / 2 + 4f;
				Vector2 origin = drawInfo.bodyVect;
				Vector2 position = new Vector2(drawX, drawY) + drawPlayer.bodyPosition - Main.screenPosition;
				Rectangle frame = new(0, 56, 40, 56);
				if (drawPlayer.bodyFrame == new Rectangle(0, 56 * 7, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 8, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 9, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 14, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 15, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 16, 40, 56))
				{
					frame = new(0, 58, 40, 56); //walking bop
				}
				if (!drawPlayer.Male)
				{
					frame = new(0, 168, 40, 56);
					if (drawPlayer.bodyFrame == new Rectangle(0, 56 * 7, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 8, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 9, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 14, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 15, 40, 56) || drawPlayer.bodyFrame == new Rectangle(0, 56 * 16, 40, 56))
					{
						frame = new(0, 170, 40, 56); //walking bop
					}
				}
				float rotation = drawPlayer.bodyRotation;
				SpriteEffects spriteEffects = drawInfo.playerEffect;

				DrawData drawData = new(texture, position, frame, color, rotation, origin, 1f, spriteEffects, 0)
				{
					shader = GameShaders.Armor.GetShaderIdFromItemId(ItemType<HorizonDye>())
				};
				drawInfo.DrawDataCache.Add(drawData);
			}
		}
	}

	/*
	public class GuardianHorizonChestGlowmaskArms : PlayerDrawLayer
	{
		public override Position GetDefaultPosition()
		{
			return new AfterParent(PlayerDrawLayers.ArmOverItem);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			Player drawPlayer = drawInfo.drawPlayer;
			if (drawInfo.drawPlayer.dead)
			{
				return;
			}

			if (drawPlayer.armor[11].type == ItemType<GuardianHorizonChest>() || (drawPlayer.armor[11].type == ItemID.None && drawPlayer.armor[1].type == ItemType<GuardianHorizonChest>()))
			{
				Color color = drawPlayer.GetImmuneAlphaPure(Color.White, drawInfo.shadow);
				Texture2D texture = Request<Texture2D>("OrchidMod/Content/Guardian/Armors/Horizon/GuardianHorizonChest_Body_Glow").Value;

				float drawX = (int)drawInfo.Position.X + drawPlayer.width / 2;
				float drawY = (int)drawInfo.Position.Y + drawPlayer.height - drawPlayer.bodyFrame.Height / 2 + 4f;
				Vector2 origin = drawInfo.bodyVect;
				Vector2 position = new Vector2(drawX, drawY) + drawPlayer.bodyPosition - Main.screenPosition;
				Rectangle frame;
				if (drawPlayer.bodyFrame == new Rectangle(0, 56 * 5, 40, 56))
				{
					frame = new(80, 56, 40, 56); //Jumping
				}
				else if (drawPlayer.bodyFrame == new Rectangle(0, 56 * 1, 40, 56))
				{
					frame = new(120, 0, 40, 56); //Use1
				}
				else if (drawPlayer.bodyFrame == new Rectangle(0, 56 * 2, 40, 56))
				{
					frame = new(160, 0, 40, 56); //Use2
				}
				else
				{
					frame = new(0, 0, 0, 0); //None
				}
				float rotation = drawPlayer.bodyRotation;
				SpriteEffects spriteEffects = drawInfo.playerEffect;

				DrawData drawData = new(texture, position, frame, color, rotation, origin, 1f, spriteEffects, 0)
				{
					shader = GameShaders.Armor.GetShaderIdFromItemId(ItemType<HorizonDye>())
				};
				drawInfo.DrawDataCache.Add(drawData);
			}
		}
	}
	*/
}
