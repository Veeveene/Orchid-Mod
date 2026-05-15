using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using OrchidMod.Content.Guardian;
using OrchidMod.Content.Guardian.Weapons.Misc;
using System.IO;
using OrchidMod.Content.General.Prefixes;
using Terraria.Audio;
using OrchidMod.Common;
using OrchidMod.Common.ModObjects;
using Terraria.DataStructures;

namespace OrchidMod.Content.Guardian.Projectiles.Misc
{

    public enum DoubleHammerHitContext 
    {
        Generic,
        ///<summary>Melee hit</summary>
        Swing,
        ///<summary>Throw hit</summary>
        Throw,
        ///<summary>Block hit</summary>
        Block
    }

    public class GuardianDoubleHammerAnchor : OrchidModGuardianAnchor
    {
        public List<Vector2> OldPosition;
        public List<float> OldRotation;
        public List<int> BlockedNPCs;

		/*
		Double Hammers can exist while 
		*/
        public int SelectedItem { get; set; } = -1;
        public GuardianDoubleHammer HammerItem;
        public Texture2D HammerTexture;
        public Texture2D HammerTextureGlow;

        public int Range = 0;
        public bool OffHandHammer = false;
        public bool Ding = false;
        public int HitCount = 0;
        public bool Penetrate;
        public bool WeakHit = false;
        public bool NeedNetUpdate = false;
        public bool FirstBlock = false;
		public int BlockDuration = 0;
        // ref float BlockDuration => ref Projectile.ai[0];
        public int hitboxOffset;

        public int HammerAnimFrame = 0;

		public bool Thrown => Projectile.timeLeft < 600;
        public bool Returning => Thrown && Range < 0;
        public bool WeakThrow => Projectile.ai[0] == 1;

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) 
        { 
            if (!OffHandHammer) overPlayers.Add(index);
        }

        public override void SafeSetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = false;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.scale = 1f;
			Projectile.timeLeft = 600;
			Projectile.alpha = 255;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
            Projectile.netImportant = true;
			FirstBlock = false;

			HammerAnimFrame = 0;

			OldPosition = [];
			OldRotation = [];
			BlockedNPCs = [];
		}
        
		public void OnChangeSelectedItem(Player owner)
		{
			SelectedItem = owner.selectedItem;
			Projectile.ai[0] = 0f;
			Projectile.ai[1] = 0f;
			Projectile.ai[2] = 0f;
			Projectile.localAI[1] = 0;
			Projectile.netUpdate = true;
			owner.GetModPlayer<OrchidGuardian>().GuardianItemCharge = 0;
		}

		// public override void OnSpawn(IEntitySource source)
		// {
		// 	Player player = Main.player[Projectile.owner];
		// 	OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
		// 	Item item = player.inventory[player.selectedItem];

		// 	if (item == null || item.ModItem is not GuardianDoubleHammer hammerItem)
		// 	{
		// 		if (Projectile.owner == Main.myPlayer) Projectile.Kill();
		// 		return;
		// 	}
		// 	else
		// 	{
		// 		HammerItem = hammerItem;
		// 		HammerTexture = hammerItem.GetHammerTexture(player, Projectile, OffHandHammer, out _);
		// 		hitboxOffset = (int)(HammerTexture.Width * guardian.GuardianWeaponScale * hammerItem.Item.scale / 2f);
		// 		DrawOriginOffsetX = DrawOriginOffsetY = hitboxOffset;

		// 		Texture2D glowTexture = HammerItem.GetGlowmaskTexture(player, Projectile, OffHandHammer, out _);
		// 		if (glowTexture != null) HammerTextureGlow = glowTexture;


		// 		Range = HammerItem.Range;
		// 		Penetrate = HammerItem.Penetrate;
		// 		Projectile.netUpdate = true;
		// 		Projectile.localNPCHitCooldown = hammerItem.HitCooldown;
		// 	}
		// }

		// ai[0]: Throw power
		// ai[1]: 
        public override void AI()
		{
			Player owner = Main.player[Projectile.owner];
			OrchidGuardian guardian = owner.GetModPlayer<OrchidGuardian>();

			if (HammerItem != null)
			{
				Projectile.scale = HammerItem.Item.scale * guardian.GuardianWeaponScale;
				if (IsLocalOwner)
				{ // OnSpawn() is called too early, guardian.GuardianWeaponScale is always equal to 1f
					hitboxOffset = (int)(HammerTexture.Width * guardian.GuardianWeaponScale * HammerItem.Item.scale / 2f);
				}

				if (NeedNetUpdate)
				{
					NeedNetUpdate = false;
					Projectile.netUpdate = true;
				}

				if (BlockDuration != 0)
				{
					if (BlockDuration <= HammerItem.BlockDuration * HammerItem.Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianBlockDuration)
					{ // hammers only starts to slow down 10 frames after being thrown
						Projectile.rotation += 0.25f * (Projectile.velocity.X > 0 ? 1 : -1);

						if (BlockDuration > 0)
						{
							Projectile.velocity *= 0.9f;
							Projectile.timeLeft++;
						}
						else
						{
							Projectile.tileCollide = false;
							float dist = Projectile.Center.Distance(owner.Center);
							Vector2 vel = Vector2.Normalize(owner.Center - Projectile.Center) * HammerItem.ReturnSpeed * BlockDuration * 0.2f;
							if (vel.Length() > 48f)
								vel = Vector2.Normalize(vel) * 48f;

							Projectile.velocity = -vel;

							if (dist < 30f && owner.whoAmI == Main.myPlayer)
								Projectile.Kill();
						}

						Rectangle hitBox = Projectile.Hitbox; // larger hitbox for projectiles
						hitBox.X -= (int)(HammerTexture.Width / 2f) - 4;
						hitBox.Y -= (int)(HammerTexture.Width / 2f) - 4;
						hitBox.Width += HammerTexture.Width + 8;
						hitBox.Height += HammerTexture.Width + 8;

						for (int l = 0; l < Main.projectile.Length; l++)
						{
							Projectile proj = Main.projectile[l];
							if (proj.active && proj.hostile && proj.damage > 0 && !OrchidGuardian.ProjectilesBlockBlacklist.Contains(proj.type))
							{
								if (proj.Hitbox.Intersects(Projectile.Hitbox))
								{
									bool killProj = HammerItem.OnBlockProjectile(owner, guardian, Projectile, proj);
									guardian.OnBlockProjectile(Projectile, proj);
									if (!FirstBlock)
									{
										FirstBlock = true;
										guardian.OnBlockProjectileFirst(Projectile, proj);
										HammerItem.OnBlockFirstProjectile(owner, guardian, Projectile, proj);
										SoundEngine.PlaySound(SoundID.Item37.WithPitchOffset(Main.rand.NextFloat(0.4f, 0.6f)), owner.Center);
									}
									if (killProj) proj.Kill();
									SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
								}
							}
						}

						hitBox = Projectile.Hitbox;
						hitBox.X -= 2;
						hitBox.Y -= 2;
						hitBox.Width += 4;
						hitBox.Height += 4;

						for (int k = 0; k < Main.maxNPCs; k++)
						{
							NPC target = Main.npc[k];
							if (target.active && !target.dontTakeDamage && !target.friendly && target.Hitbox.Intersects(hitBox))
							{
								HammerItem.OnBlockContact(owner, guardian, target, Projectile);

								bool contained = false;
								foreach (BlockedEnemy blockedEnemy in guardian.GuardianBlockedEnemies)
								{
									if (blockedEnemy.npc == target)
									{ // Enemy already blocked, reset the timer
										blockedEnemy.time = 120;
										contained = true;
										break;
									}
								}

								if (!contained)
								{ // First time blocking an enemy
									guardian.GuardianBlockedEnemies.Add(new BlockedEnemy(target, 120));
									guardian.OnBlockNPCNew(Projectile, target);
									SoundEngine.PlaySound(SoundID.Dig, owner.Center);

									if (!BlockedNPCs.Contains(target.whoAmI))
									{
										HammerItem.OnBlockNPC(owner, guardian, target, Projectile);
										BlockedNPCs.Add(target.whoAmI);
									}
								}

								if (target.knockBackResist > 0f && BlockDuration > 0)
								{ // Push enemy if possible
									Vector2 push = target.Center - Projectile.Center;
									push.Normalize();
									target.velocity = push + Projectile.velocity;
								}

								guardian.OnBlockNPC(Projectile, target);
								if (!FirstBlock)
								{ // First block stuff
									FirstBlock = true;
									guardian.OnBlockNPCFirst(Projectile, target);
									HammerItem.OnBlockFirstNPC(owner, guardian, target, Projectile);
									SoundEngine.PlaySound(SoundID.Item37.WithPitchOffset(Main.rand.NextFloat(0.4f, 0.6f)), owner.Center);
								}
							}
						}

						OldPosition.Add(new Vector2(Projectile.Center.X, Projectile.Center.Y));
						OldRotation.Add(Projectile.rotation + MathHelper.PiOver2);
						if (OldPosition.Count > 10)
							OldPosition.RemoveAt(0);
						if (OldRotation.Count > 10)
							OldRotation.RemoveAt(0);

						BlockDuration--;

						if (BlockDuration == 0)
						{
							BlockDuration = -1;
						}
					}
				}
				else if (Projectile.ai[1] <= 0)
				{
					if (owner.dead || owner.HeldItem.ModItem is not GuardianDoubleHammer hammerItem)
					{
						if (Projectile.owner == Main.myPlayer)
							Projectile.Kill();
					}
					else
					{

					}
				}

				HammerItem.ExtraAI(owner, guardian, Projectile, OffHandHammer);
			}
		}


        public override void SendExtraAI(BinaryWriter writer)
		{
            writer.Write(SelectedItem);
            writer.Write(OffHandHammer);
			writer.Write(HammerItem.Item.type);
			writer.Write(Range);
			writer.Write(BlockDuration);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
            SelectedItem = reader.ReadInt32();
            OffHandHammer = reader.ReadBoolean();
			int itemType = reader.ReadInt32();
			Range = reader.ReadInt32();
			BlockDuration = reader.ReadInt32();

			if (HammerItem == null)
			{
				OrchidGuardian guardian = Main.player[Projectile.owner].GetModPlayer<OrchidGuardian>();
				guardian.GuardianItemCharge = 0f;

				Item item = new(itemType);
				if (item.ModItem is GuardianDoubleHammer hammerItem)
				{
					HammerItem = hammerItem;

					if (Main.netMode != NetmodeID.Server)
					{
						HammerTexture = HammerItem.GetHammerTexture(Main.player[Projectile.owner], Projectile, OffHandHammer, out Rectangle? _);
						hitboxOffset = (int)(HammerTexture.Width * hammerItem.Item.scale / 2f);
						DrawOriginOffsetX = DrawOriginOffsetY = hitboxOffset;
					}

					Projectile.scale = hammerItem.Item.scale * guardian.GuardianWeaponScale;

					Range = HammerItem.Range;
					Penetrate = HammerItem.Penetrate;
				}
			}
		}


    }
}