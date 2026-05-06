using Microsoft.Xna.Framework;
using OrchidMod.Common;
using OrchidMod.Content.Shapeshifter.Projectiles.Sage;
using OrchidMod.Content.Shapeshifter.Projectiles.Warden;
using OrchidMod.Content.Shapeshifter.Weapons.Sage;
using OrchidMod.Content.Shapeshifter.Weapons.Warden;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Shapeshifter
{
	internal class ShapeshifterGlobalNPC : GlobalNPC
	{
		public bool SageOwlDebuff;
		public bool SageBatDebuff;
		public bool WardenSpiderDebuff;

		public List<ShapeshifterBleed> BleedSpecific; // List of bleeds applied by specific wildshapes (such as the Fossil Raptor)
		public List<ShapeshifterBleed> BleedGeneral; // List of generic shapeshifter bleeds applied by equipment (such as the Goblin Dagger)

		public int ShapeshifterBleedWildshape = 0; // Used by to count Shapshifter wildshape-specific bleeds stacks. See PredatorFossil.ShapeshiftOnHitNPC() for sync Example
		public int ShapeshifterBleedTimerWildshape = 0; // Used to time Shapshifter bleeds
		public int ShapeshifterBleedPotencyWildshape = 0; // Damage dealt by the active shapeshifter bleed

		public override bool InstancePerEntity => true;

		public override void SetDefaults(NPC entity)
		{
			BleedSpecific = new List<ShapeshifterBleed>();
			BleedGeneral = new List<ShapeshifterBleed>();
		}

		public override void ResetEffects(NPC npc) {
			SageOwlDebuff = false;
			SageBatDebuff = false;
			WardenSpiderDebuff = false;
		}

		public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers) {
			if (SageOwlDebuff) {
				modifiers.FlatBonusDamage += 3;
			}
		}

		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			int totalDamage = 0;

			for (int i = BleedSpecific.Count - 1; i >= 0; i--)
			{
				ShapeshifterBleed bleed = BleedSpecific[i];
				totalDamage += bleed.BleedCount * bleed.BleedPotency;
				bleed.BleedTimer--;
				if (bleed.BleedTimer <= 0)
				{
					BleedSpecific.RemoveAt(i);
				}
			}

			for (int i = BleedGeneral.Count - 1; i >= 0; i--)
			{
				ShapeshifterBleed bleed = BleedGeneral[i];
				totalDamage += bleed.BleedCount * bleed.BleedPotency;
				bleed.BleedTimer--;
				if (bleed.BleedTimer <= 0)
				{
					BleedGeneral.RemoveAt(i);
				}
			}

			if (totalDamage > 0)
			{
				if (npc.lifeRegen > 0)
				{
					npc.lifeRegen = 0;
				}

				if (damage < 0)
				{
					damage = 0;
				}

				damage += totalDamage;
				npc.lifeRegen -= totalDamage * 2;
			}
		}

		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
		{
			if (WardenSpiderDebuff && !npc.SpawnedFromStatue)
			{ // Executes low health enemies
				OrchidShapeshifter shapeshifter = Main.player[projectile.owner].GetModPlayer<OrchidShapeshifter>();
				if (projectile.type != ModContent.ProjectileType<WardenSpiderWeb>() && projectile.DamageType == ModContent.GetInstance<ShapeshifterDamageClass>() && shapeshifter.IsShapeshifted)
				{ // Higher threshold for the spider attacks
					bool spiderMelee = projectile.type == ModContent.ProjectileType<WardenSpiderProj>();
					int threshold = (spiderMelee ? 100 : 75); // 100 dmg threshold as a spider, 75 else

					float percentThreshold = 0.66f; // enemies can't be executed above 66% health
					if (npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.EaterofWorldsBody || npc.type == NPCID.EaterofWorldsTail || npc.boss)
					{ // bosses and EoW segments can't be executed above 33% health
						percentThreshold = 0.33f;
					}

					float remainingLife = npc.life - (projectile.damage - npc.defense * 0.5f);
					if (remainingLife < threshold && remainingLife / npc.lifeMax <= percentThreshold)
					{ // 999 damage
						modifiers.FlatBonusDamage += 100;
						modifiers.SetCrit();

						if (!npc.SpawnedFromStatue)
						{
							int healValue = 0;
							int lifeMax = npc.lifeMax;
							while (lifeMax > 0)
							{
								healValue++;
								lifeMax -= 25;

								if (!spiderMelee)
								{
									lifeMax -= 25;
								}
							}

							if (healValue > 15)
							{
								healValue = 15;
							}

							shapeshifter.modPlayer.TryHeal(shapeshifter.GetShapeshifterHealing(healValue));
						}
					}
				}
			}
		}

		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			if (projectile.ModProjectile is OrchidModShapeshifterProjectile modProjectile && npc.ModNPC != null)
			{ // "Fakes" melee hits with some projectile for thorium compatibility
				if (modProjectile.MeleeHit)
				{
					npc.ModNPC.OnHitByItem(modProjectile.Owner, modProjectile.Owner.HeldItem, hit, damageDone);
				}
			}
		}

		public override void OnKill(NPC npc)
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				OnKillShapeshifterGlobalNPC(npc);
			}
			else
			{ // Sending a packet for effects that require the player to be aware of the NPC death
				var packet = OrchidMod.Instance.GetPacket();
				packet.Write((byte)OrchidModMessageType.SYNCONKILLNPC);
				packet.Write(npc.whoAmI);
				packet.Send();
			}
		}

		public void OnKillShapeshifterGlobalNPC(NPC npc)
		{
			if (!npc.friendly && !npc.CountsAsACritter && npc.aiStyle != NPCAIStyleID.Spell)
			{
				OrchidShapeshifter shapeshifter = Main.LocalPlayer.GetModPlayer<OrchidShapeshifter>();

				if (shapeshifter.IsShapeshifted)
				{
					if (shapeshifter.Shapeshift is SageImp impItem && npc.Center.Distance(shapeshifter.Player.Center) < 800f)
					{ // Imp attack speed buff when a nearby enemy dies
						impItem.FastAttackTimer = 300;
						impItem.FastAttack += 3;
						if (impItem.FastAttack > 9)
						{
							impItem.FastAttack = 9;
						}
					}

					if (shapeshifter.Shapeshift is WardenEater eaterItem)
					{ // Eater spawning a projectile going towards its closest fruit when a nearby enemy dies
						float distanceMax = 400f;
						int target = -1;
						foreach (Projectile projectile in Main.projectile)
						{
							float distance = npc.Center.Distance(projectile.Center);
							if (projectile.type == ModContent.ProjectileType<WardenEaterProjAlt>() && projectile.owner == shapeshifter.Player.whoAmI && distance < distanceMax && projectile.frame == 0)
							{
								distanceMax = distance;
								target = projectile.whoAmI;
							}
						}

						if (target >= 0)
						{
							int projectileType = ModContent.ProjectileType<WardenEaterProjAltDeath>();
							Projectile newProjectile = Projectile.NewProjectileDirect(shapeshifter.Player.GetSource_ItemUse(eaterItem.Item), npc.Center, Vector2.Zero, projectileType, 0, 0f, shapeshifter.Player.whoAmI, ai0: target);
						}
					}

					if (shapeshifter.Shapeshift is SageCorruption corruptionItem && npc.Center.Distance(shapeshifter.Player.Center) < 560f)
					{ // enemy "explodes" - 35 tiles range = 5 more than the wildshape max range on left click
						int projectileType = ModContent.ProjectileType<SageCorruptionProjAlt>();
						int damage = shapeshifter.GetShapeshifterDamage(corruptionItem.Item.damage);
						Projectile newProjectile = Projectile.NewProjectileDirect(shapeshifter.Player.GetSource_ItemUse(corruptionItem.Item), npc.Center, Vector2.Zero, projectileType, damage, 0f, shapeshifter.Player.whoAmI, 1f);
						newProjectile.CritChance = shapeshifter.ShapeshiftAnchor.Projectile.CritChance;
						newProjectile.netUpdate = true;
					}

					if (shapeshifter.Shapeshift is SageCrimson crimsonItem && npc.Center.Distance(shapeshifter.Player.Center) < 560f)
					{ // enemy "explodes" - 35 tiles range = 5 more than the wildshape max range on left click
						int projectileType = ModContent.ProjectileType<SageCorruptionProjAlt>();
						int damage = shapeshifter.GetShapeshifterDamage(crimsonItem.Item.damage);
						Projectile newProjectile = Projectile.NewProjectileDirect(shapeshifter.Player.GetSource_ItemUse(crimsonItem.Item), npc.Center, Vector2.Zero, projectileType, damage, 0f, shapeshifter.Player.whoAmI, 1f, ai2:1f);
						newProjectile.CritChance = shapeshifter.ShapeshiftAnchor.Projectile.CritChance;
						newProjectile.netUpdate = true;
					}
				}
			}
		}

		public override void DrawEffects(NPC npc, ref Color drawColor) {
			if (SageOwlDebuff) {
				drawColor.R = 0;
			}

			if (SageBatDebuff) {
				drawColor.B = 0;
				if (drawColor.R < 128) drawColor.R = 128;
				if (drawColor.G < 128) drawColor.G = 128;
			}

			if (WardenSpiderDebuff)
			{
				drawColor = Color.Gray.MultiplyRGBA(drawColor);
			}
		}

		public ShapeshifterBleed ApplyBleed(int owner, int timer, int potency, int maxCount, bool isGeneral = false)
		{ // returns true if the bleed was successfully updated, false if a new one was created
			List<ShapeshifterBleed> bleedList = isGeneral ? BleedGeneral : BleedSpecific;
			foreach (ShapeshifterBleed bleed in bleedList)
			{
				if (bleed.Owner == owner)
				{ // found an existing bleed for that player, update it
					bleed.NewBleed = false;
					bleed.BleedTimer = timer;

					if (bleed.BleedPotency < potency)
					{ // current bleed is weaker than the new one, set the new potency and reset the bleed count
						bleed.BleedPotency = potency;
						bleed.BleedCount++;
					}

					if (bleed.BleedCount < maxCount)
					{
						bleed.BleedCount++;
					}

					return bleed;
				}
			}

			// no bleed found for that player, create one
			ShapeshifterBleed newBleed = new ShapeshifterBleed(1, potency, timer, owner);
			bleedList.Add(newBleed);
			return newBleed;
		}
	}

	public class ShapeshifterBleed
	{
		public bool NewBleed = true; // whether the bleed has only been applied once
		public int Owner = -1; // WhoAmI of the bleed owner
		public int BleedCount = 0; // Used by to count Shapshifter general bleeds stacks (inflicted by equipment)
		public int BleedTimer = 0; // Used to time Shapshifter general bleeds
		public int BleedPotency = 0;// Damage dealt by the active general shapeshifter bleed

		public ShapeshifterBleed(int count, int potency, int timer, int whoami)
		{
			BleedCount = count;
			BleedPotency = potency;
			BleedTimer = timer;
			Owner = whoami;
			NewBleed = true;
		}
	}
}
