using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OrchidMod.Content.General.Projectiles;
using OrchidMod.Content.Guardian;
using OrchidMod.Content.Guardian.Projectiles.Quarterstaves;
using OrchidMod.Content.Guardian.Weapons.Quarterstaves;
using OrchidMod.Content.Guardian.Weapons.Shields;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Common.ModObjects
{
	public class OrchidPlayer : ModPlayer
	{
		public OrchidAlchemist modPlayerAlchemist;
		public OrchidGambler modPlayerGambler;
		public OrchidDancer modPlayerDancer;
		public OrchidGuardian modPlayerGuardian;
		public OrchidShapeshifter modPlayerShapeshifter;

		public int Timer120 = 0; // Used for various AIs. I'll eventually get rid of this

		// Gameplay Fields

		public int Timer = 0; // Used for various AIs. Increased by 1 every frame
		public int keepSelected = -1;
		public int originalSelectedItem;
		public bool autoRevertSelectedItem = false;
		public int PlayerImmunity = 0; // Player is immune if this is >0
		/// <summary>Vector the player will be moved every frame if ForcedVelocityTimer > 0, ignoring normal velocity.</summary>
		public Vector2 ForcedVelocityVector = Vector2.Zero;
		/// <summary>Multiplier of ForcedVelocityVector the player velocity is set to after ForcedVelocityTimer expires.</summary>
		public float ForcedVelocityUpkeep = 0f;
		/// <summary>If set to true, the player will phase through solid-top tiles. resets to false when ForcedVelocityTimer expires.</summary>
		public bool ForcedVelocityIgnoresPlatforms = false;
		/// <summary>How long should the forced velocity be kept.</summary>
		public int ForcedVelocityTimer = 0;
		public bool OrchidDoubleDash = false;
		public int OrchidDoubleDashCD = 0;
		/// <summary>The last NPC referenced in OnHitNPC() for this player</summary>
		public NPC LastHitNPC = null;
		/// <summary>Set to 15 after a tap, decremented every frame. Registers a double tap and resets to 0 if another tap is input while above 0.</summary>
		/// <remarks>Up = 0, Right = 1, Down = 2, Left = 3</remarks>
		public int[] DoubleTapping = new int[4]; 
		/// <summary>Set to 15 after a double tap, decremented every frame.</summary>
		/// <remarks>Up = 0, Right = 1, Down = 2, Left = 3</remarks>
		public int[] DoubleTapped = new int[4];
		/// <summary>The player has double tapped Up on this frame.</summary>
		public bool DoubleTapUp => DoubleTapped[0] == 15;
		/// <summary>The player has double tapped Down on this frame.</summary>
		public bool DoubleTapDown => DoubleTapped[1] == 15;
		/// <summary>The player has double tapped Down on this frame.</summary>
		public bool DoubleTapRight => DoubleTapped[2] == 15;
		/// <summary>The player has double tapped Right on this frame.</summary>
		public bool DoubleTapLeft => DoubleTapped[3] == 15;
		/// <summary>The player has double tapped their Set Bonus key on this frame.</summary>
		public bool DoubleTapSetBonus => (DoubleTapDown && !Main.ReversedUpDownArmorSetBonuses) || (DoubleTapUp && Main.ReversedUpDownArmorSetBonuses);
		/// <summary>The player has double tapped a direction on this frame.</summary>
		public bool DoubleTapAny => DoubleTapUp || DoubleTapDown || DoubleTapRight || DoubleTapLeft; 
		/// <summary>Set to 15 after a double tap. Decremented every frame.</summary>
		public ref int DoubleTappedUp => ref DoubleTapped[0];
		/// <summary>Set to 15 after a double tap. Decremented every frame.</summary>
		public ref int DoubleTappedDown => ref DoubleTapped[1];
		/// <summary>Set to 15 after a double tap. Decremented every frame.</summary>
		public ref int DoubleTappedRight => ref DoubleTapped[2];
		/// <summary>Set to 15 after a double tap. Decremented every frame.</summary>
		public ref int DoubleTappedLeft => ref DoubleTapped[3];
		/// <summary>List of current Orchid Titanium Shards owned by this player.</summary>
		public List<Projectile> TitaniumShards = new List<Projectile>();
		/// <summary>If true, all player Drawlayers will not render.</summary>
		public bool HideAllDrawLayers = false;
		/// <summary>If true, the players Smart Cursor will be enabled next time they un-shapeshift or stop holding a guardian weapon.</summary>
		public bool SmartCursorTrigger = false;
		/// <summary>If true, prevents SmartCursorTrigger from being modified again until the player un-shapeshifts or stops holding a guardian weapon.</summary>
		public bool SmartCursorCheck = false;

		// Equipment Fields (General)

		/// <summary>Divides damage taken by the player by the sum of all damage resistance bonuses.</summary>
		public float OrchidDamageResistance = 1f;
		/// <summary>Chance of taking damage. Defaults to 1. Multiply this value instead of reducing it! If this value is 0 or lower the player has a 100% chance of dodging damage. Displays Black Belt's dodge visual.</summary>
		public float OrchidDodgeChance = 1;

		// Equipment Fields (Individual Items)

		public bool remoteCopterPet = false;

		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
		{
			/*
			if (Player.ZoneSkyHeight && !attempt.inLava && !attempt.inHoney && Main.rand.NextBool(10) && Main.hardMode && attempt.rare)
			{
				itemDrop = ModContent.ItemType<Content.Shaman.Weapons.Hardmode.WyvernMoray>();
			}
			*/

			if (attempt.fishingLevel < 50 && Main.rand.NextBool(5 + (int)(attempt.fishingLevel / 2f))) 
			{
				itemDrop = ModContent.ItemType<TrashPavise>();
			}
		}

		public override void Initialize()
		{
			modPlayerAlchemist = Player.GetModPlayer<OrchidAlchemist>();
			modPlayerGambler = Player.GetModPlayer<OrchidGambler>();
			modPlayerDancer = Player.GetModPlayer<OrchidDancer>();
			modPlayerGuardian = Player.GetModPlayer<OrchidGuardian>();
			modPlayerShapeshifter = Player.GetModPlayer<OrchidShapeshifter>();
		}

		public override void PreUpdate()
		{
			if (Player.whoAmI == Main.myPlayer)
			{
				if (autoRevertSelectedItem)
				{
					if (Player.itemTime == 0 && Player.itemAnimation == 0)
					{
						Player.selectedItem = originalSelectedItem;
						autoRevertSelectedItem = false;
					}
				}
			}
		}

		public override void PostUpdate()
		{
			if (Main.cSmartCursorModeIsToggleAndNotHold && ModContent.GetInstance<OrchidClientConfig>().SmartSmartCursor && Player.whoAmI == Main.myPlayer)
			{
				if (Player.HeldItem.ModItem != null && (Player.HeldItem.ModItem is OrchidModGuardianItem && Player.HeldItem.damage > 0 && !Player.HeldItem.accessory) || Player.GetModPlayer<OrchidShapeshifter>().IsShapeshifted)
				{ // if the player is holding a guardian weapon or shapeshifted
					if (!SmartCursorCheck)
					{ // disable smart cursor automatically and remember it
						SmartCursorCheck = true;

						if (Main.SmartCursorWanted || Main.SmartCursorShowing)
						{
							SmartCursorTrigger = true;
							Main.SmartCursorWanted_Mouse = false;
							Main.SmartCursorWanted_GamePad = false;
							Main.SmartCursorShowing = false;
						}
					}
					else if (Main.SmartCursorWanted || Main.SmartCursorShowing)
					{ // disable the override if the player manually toggles smart cursor after it has been disabled
						SmartCursorTrigger = false;
					}
				}
				else
				{ // re-enable smart cursor if it was disabled above
					if (SmartCursorTrigger)
					{ // from tmodloader code : Main.cs L2769
						if (PlayerInput.SteamDeckIsUsed && PlayerInput.SettingsForUI.CurrentCursorMode == CursorMode.Mouse)
						{
							Main.SmartCursorWanted_Mouse = true;
						}
						else if (PlayerInput.UsingGamepad)
						{
							Main.SmartCursorWanted_GamePad = true;
						}
						else Main.SmartCursorWanted_Mouse = true;
					}

					SmartCursorCheck = false;
					SmartCursorTrigger = false;
				}
			}

			if (Main.myPlayer == Player.whoAmI)
			{
				Point pos = new((int)(Player.Center.X / 16f), (int)(Player.position.Y + Player.height) / 16 - 1);
				if (Main.tile[pos].TileType == ModContent.TileType<VerveineQuarterstaffTile>() && !Player.HasBuff(BuffID.Stinky) && Main.rand.NextBool(30))
				{
					Tile tile = Main.tile[pos];
					Vector2 origin = new(pos.X * 16 + 16 - tile.TileFrameX, pos.Y * 16 + 16 - tile.TileFrameY);
					Projectile proj = Projectile.NewProjectileDirect(Player.GetSource_TileInteraction(pos.X, pos.Y), origin, Vector2.Zero, ModContent.ProjectileType<VerveineFart>(), 40, 2, Main.myPlayer, 0, Main.getGoodWorld && Main.rand.NextBool(2) ? 1 : 0);
					proj.hostile = true;
					proj.trap = true;
				}
				if (Main.tile[pos].TileType == ModContent.TileType<VerveineAltQuarterstaffTile>() && !Player.HasBuff(BuffID.Slimed) && Main.rand.NextBool(30))
				{
					Tile tile = Main.tile[pos];
					Vector2 origin = new(pos.X * 16 + 16 - tile.TileFrameX, pos.Y * 16 + 16 - tile.TileFrameY);
					Projectile proj = Projectile.NewProjectileDirect(Player.GetSource_TileInteraction(pos.X, pos.Y), origin, Vector2.Zero, ModContent.ProjectileType<VerveineFart>(), 40, 2, Main.myPlayer, 1, Main.getGoodWorld && Main.rand.NextBool(2) ? 1 : 0);
					proj.hostile = true;
					proj.trap = true;
				}
			}
		}

		public override void PreUpdateMovement()
		{
			if ((DoubleTapLeft || DoubleTapRight) && OrchidDoubleDashCD <= 0 && OrchidDoubleDash)
			{
				OrchidDoubleDashCD = 60;
				Player.dashDelay = 60;
				SoundEngine.PlaySound(SoundID.Item19, Player.Center);
				Player.velocity.X = DoubleTapRight ? 15f : -15f;
			}
		}

		public override void HideDrawLayers(PlayerDrawSet drawInfo)
		{
			if (HideAllDrawLayers)
			{
				foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.DrawOrder)
				{
					layer.Hide();
				}
			}
		}

		public override void ResetEffects()
		{
			Timer++;
			Timer120++;
			if (Timer120 == 120)
				Timer120 = 0;

			remoteCopterPet = false;
			OrchidDoubleDash = false;
			OrchidDodgeChance = 1f;
			HideAllDrawLayers = false;

			if (OrchidDoubleDashCD > 0)
			{
				OrchidDoubleDashCD--;
				if (OrchidDoubleDashCD > 30)
				{
					Player.velocity.X *= 0.95f;
				}
			}

			if (keepSelected != -1)
			{
				Player.selectedItem = keepSelected;
				keepSelected = -1;
			}

			if (PlayerImmunity > 0) PlayerImmunity--;

			if (ForcedVelocityTimer > 0)
			{
				Player.fallStart = (int)(Player.position.Y / 16);
				Player.maxFallSpeed = ForcedVelocityVector.Length();
				ForcedVelocityTimer--;

				if (ForcedVelocityTimer <= 0)
				{ // resets fields after the forcedvelocity ends
					Player.velocity = ForcedVelocityVector * ForcedVelocityUpkeep;
					ForcedVelocityVector = Vector2.Zero;
					ForcedVelocityUpkeep = 0f;
					ForcedVelocityIgnoresPlatforms = false;
					Player.direction = Player.velocity.X > 0 ? 1 : -1;
				}
				else
				{
					Vector2 addedVelocity = Vector2.Zero;
					for (int i = 0; i < 10; i++)
					{
						addedVelocity += Collision.TileCollision(Player.position + addedVelocity, ForcedVelocityVector * 0.1f, Player.width, Player.height, ForcedVelocityIgnoresPlatforms, false, (int)Player.gravDir);
					}

					Player.velocity = addedVelocity * 0.001f; // if set to 0, this causes weird issues with slopes & solid top tiles
					Player.position += addedVelocity;
					Player.direction = addedVelocity.X > 0 ? 1 : -1;
				}
			}

			OrchidDamageResistance = 1f;
			
			for (int i = 0; i < 4; i++)
			{
				bool tapKey = false;
				switch(i)
				{
					case 0:
						tapKey = Player.controlUp && Player.releaseUp;
						break;
					case 1:
						tapKey = Player.controlDown && Player.releaseDown;
						break;
					case 2:
						tapKey = Player.controlRight && Player.releaseRight;
						break;
					case 3:
						tapKey = Player.controlLeft && Player.releaseLeft;
						break;
				}
				if (DoubleTapped[i] > 0) DoubleTapped[i]--;
				if (tapKey)
				{
					if (DoubleTapping[i] > 0)
					{
						DoubleTapping[i] = 0;
						DoubleTapped[i] = 15;
					}
					else DoubleTapping[i] = 15;
				}
				else if (DoubleTapping[i] > 0) DoubleTapping[i]--;
			}
			TitaniumShards.RemoveAll(p => !p.active || p.type != ModContent.ProjectileType<OrchidTitaniumShard>());
			int index = 0;
			foreach (Projectile shard in TitaniumShards)
			{
				shard.ai[0] = index / (float)TitaniumShards.Count;
				index++;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			LastHitNPC = target;
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			if (OrchidDamageResistance > 0) modifiers.FinalDamage /= OrchidDamageResistance;
			else modifiers.FinalDamage *= 9999;
			//idk if we'd ever have a situation where it's possible to hit -100% damage resistance but this makes it kill the player instead of throwing an exception
			//seems fitting anyway
		}

		public override bool FreeDodge(Player.HurtInfo info)
		{
			if (PlayerImmunity > 0)
			{
				Player.SetImmuneTimeForAllTypes(PlayerImmunity > 40 ? PlayerImmunity : 40);
				SoundEngine.PlaySound(SoundID.Item1, Player.Center);
				return true;
			}

			if (Main.rand.NextFloat() > OrchidDodgeChance)
			{
				Player.NinjaDodge();
				return true;
			}

			return false;
		}

		public void SetDodgeImmuneTime(int time = 40, bool ignoreOrchidPlayerImmunity = false)
		{
			if (!ignoreOrchidPlayerImmunity)
			{
				PlayerImmunity = time;
			}

			Player.SetImmuneTimeForAllTypes(time);
		}

		public void TryHeal(int amount)
		{
			if (!Player.moonLeech && Player.whoAmI == Main.myPlayer)
			{
				int damage = Player.statLifeMax2 - Player.statLife;
				if (amount > damage)
				{
					amount = damage;
				}
				if (amount > 0)
				{
					Player.HealEffect(amount, true);
					Player.statLife += amount;
				}
			}
		}

		///<summary>Spawns custom Orchid Titanium Shards, and refreshes the player's Titanium Barrier buff.</summary>
		public void SpawnTitaniumShards(IEntitySource source, int count = 1, int maxCount = 8)
		{
			Player.AddBuff(BuffID.TitaniumStorm, 600);
			if (TitaniumShards.Count + count > maxCount) count = maxCount - TitaniumShards.Count;
			if (count < 1) return;
			for (int i = 0; i < count; i++)
			{
				Projectile newProjectile = Projectile.NewProjectileDirect(source, Player.Center, Vector2.Zero, ModContent.ProjectileType<OrchidTitaniumShard>(), 50, 15f, Player.whoAmI);
				newProjectile.CritChance = (int)(Player.GetCritChance<GuardianDamageClass>() + Player.GetCritChance<GenericDamageClass>() + 4);
				TitaniumShards.Add(newProjectile);
			}
		}
	}
}
