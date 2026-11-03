using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Content.Guardian.Buffs;
using OrchidMod.Content.Guardian.Weapons.Quarterstaves;
using OrchidMod.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using SpriteBatchSnapshot = OrchidMod.Utilities.SpriteBatchSnapshot;

namespace OrchidMod.Content.Guardian.Projectiles.Quarterstaves;

public class ThoriumBoreanStriderQuarterstaffProjectile : OrchidModGuardianProjectile
{
	// public List<BoreanStriderProjectileMist> Mist;

	public Asset<Texture2D> TextureSmall;
	public Asset<Texture2D> TextureGlow;
	public Asset<Texture2D> TextureSmallGlow;
	// public Asset<Texture2D> TextureMist;

	public List<Vector2> OldPosition;
	public List<float> OldRotation;
	public List<float> OldScale;

	public Projectile QuarterstaffProj;
	public Item QuarterstaffItem;
	public Vector2 OrigVelocity;
	public float Scale = 0f;
	public float Spin;
	public int HitCount;

	/// <summary></summary>
	public bool Ready;

	public override void SafeSetDefaults()
	{
		Projectile.width = 36;
		Projectile.height = 36;
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.aiStyle = -1;
		Projectile.timeLeft = 270;
		Projectile.scale = 1f;
		Projectile.penetrate = -1;
		Projectile.extraUpdates = 1;
		Projectile.alpha = 255;
		Projectile.tileCollide = false;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = -1;
		OldPosition = [];
		OldRotation = [];
		OldScale = [];
		HitCount = 0;
		// Mist = [];

		TextureSmall ??= ModContent.Request<Texture2D>(Texture + "_Small", AssetRequestMode.ImmediateLoad);
		TextureGlow ??= ModContent.Request<Texture2D>(Texture + "_Glow", AssetRequestMode.ImmediateLoad);
		TextureSmallGlow ??= ModContent.Request<Texture2D>(Texture + "_Small_Glow", AssetRequestMode.ImmediateLoad);
		// TextureMist ??= ModContent.Request<Texture2D>(Texture + "_Mist", AssetRequestMode.ImmediateLoad);
	}

	public override void OnSpawn(IEntitySource source)
	{
		Projectile parent = Main.projectile[(int)Projectile.ai[0]];
		if (parent.active && parent.owner == Projectile.owner && parent.ModProjectile is GuardianQuarterstaffAnchor anchor && anchor.QuarterstaffItem.ModItem is ThoriumBoreanStriderQuarterstaff)
		{
			QuarterstaffItem = anchor.QuarterstaffItem;
			QuarterstaffProj = parent;
		}
	}

	public override void AI()
	{
		Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
		Projectile.spriteDirection = Projectile.direction;

		if (!Ready)
		{
			/* Always called on first tick */
			if (!Initialized)
			{
				Initialized = true;
				Ready = false;

				OrigVelocity = Projectile.velocity;
				Projectile.velocity *= float.Epsilon;

				SoundEngine.PlaySound(SoundID.Item30, Projectile.Center);

				if (Strong)
				{
					Projectile.penetrate = -1;
					Projectile.localNPCHitCooldown = 20;
					if (Projectile.ai[1] != 1)
					{
						HitCount = 10;
						Projectile.timeLeft = (int)Guardian.GetParryDuration(QuarterstaffItem, 120) + 1;
					}

					Projectile.Name = Language.GetTextValue("Mods.OrchidMod.Projectiles.ThoriumBoreanStriderQuarterstaffProjectile.StrongName");
				}
				else
				{
					Projectile.penetrate = 5;
					Projectile.timeLeft = 91;
				}
			}

			if (Strong)
			{
				Projectile.alpha -= 5;

				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
				Projectile.Center = Owner.MountedCenter;
				if (QuarterstaffProj != null && Projectile.ai[1] == 1)
					Projectile.Center += Vector2.UnitY.RotatedBy(QuarterstaffProj.ai[1]) * QuarterstaffProj.width * 0.1f;


				Scale = Projectile.Opacity * 2.5f;
				if (Projectile.ai[1] != 1) Scale *= 0.6f;

				Projectile.timeLeft++;
			}
			else
			{
				Scale = 1f;
				Projectile.alpha = 0;
			}

			SetSize(Scale);

			if (Projectile.alpha <= 0)
			{
				Ready = true;
				Projectile.velocity = OrigVelocity;
			}
		}
		else
		{
			if (Main.rand.NextBool(Strong ? 3 : 6))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.HallowSpray);
				dust.velocity = -Projectile.velocity * 0.2f;
			}

			if (Strong)
			{
				Projectile.velocity *= 0.98f;
				if (Projectile.velocity.Length() is <= 0.02f and > float.Epsilon)
					Projectile.velocity *= float.Epsilon;
			}
			else
			{
				if (QuarterstaffProj?.ai[0] == 0 || Guardian.GuardianItemCharge == 0)
					Projectile.ai[1] = 0;

				Projectile.velocity *= (Projectile.timeLeft < 60 ? 0.9f : 1.04f);
			}
		}

		Lighting.AddLight(Projectile.Center, Color.LightBlue.ToVector3() * Projectile.Opacity * Utils.Remap(Projectile.Opacity, 0f, 1f, 0f, 0.5f) * (Strong ? 1 : 0.2f));

		if (Projectile.timeLeft < 18)
		{
			Projectile.alpha += 15;
			if (Projectile.alpha > 255) Projectile.alpha = 255;
		}

		Projectile.rotation += 0.03f * Projectile.direction;

		if (Projectile.timeLeft % (Strong ? 4 : 2) == 0)
		{
			OldPosition.Add(Projectile.Center);
			OldRotation.Add(Projectile.rotation);
			OldScale.Add(Projectile.scale);
		}

		if (OldPosition.Count > 10)
		{
			OldPosition.RemoveAt(0);
			OldRotation.RemoveAt(0);
			OldScale.RemoveAt(0);
		}
	}

	public override bool? CanHitNPC(NPC target) => Ready ? base.CanHitNPC(target) : false;

	public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidGuardian guardian)
	{
		Mod thoriumMod = OrchidMod.ThoriumMod;
		if (thoriumMod != null)
		{
			int debuffType = thoriumMod.Find<ModBuff>("Freezing").Type;
			target.AddBuff(debuffType, 120);
		}

		if (Strong)
		{
			int toAdd = 1;
			if (target.aiStyle is NPCAIStyleID.Worm or NPCAIStyleID.TheDestroyer && target.type != NPCID.SolarCrawltipedeTail && target.type != NPCID.StardustWormHead) toAdd = 2;
			HitCount += toAdd;

			if (HitCount < 20)
				SetSize(2.5f * (1 - 0.04f * HitCount));
			else Projectile.Kill();

			SoundEngine.PlaySound(SoundID.Item50, Projectile.Center);

			if (FirstHit)
			{
				if (Projectile.ai[1] == 1)
				{
					int guardsToAdd = 2;
					int slamsToAdd = 2;

					if (QuarterstaffItem?.ModItem is ThoriumBoreanStriderQuarterstaff frostWalker)
					{
						guardsToAdd = frostWalker.GuardStacks;
						slamsToAdd = frostWalker.SlamStacks;
					}

					guardian.AddGuard(guardsToAdd);
					guardian.AddSlam(slamsToAdd);
				}
				else if (IsLocalOwner) player.AddBuff<GuardianBoreanStriderQuarterstaffBuff>(600);


				SoundEngine.PlaySound(SoundID.Item4);
				for (int i = 0; i < 15; i++)
				{
					Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.FrostHydra, Main.rand.NextVector2CircularEdge(3f, 3f), Scale: 2f);
					dust.noGravity = true;
				}
			}
		}
		else
		{
			if (FirstHit && Projectile.ai[1] != 0 && guardian.GuardianItemCharge > 0)
			{
				float toAdd = 20f;
				if (player.HasBuff<GuardianBoreanStriderQuarterstaffBuff>()) toAdd *= 3f;
				guardian.GuardianItemCharge += toAdd;
				if (guardian.GuardianItemCharge >= 180f)
					guardian.GuardianItemCharge = 180f;
				Projectile.ai[1] = 0;
			}
		}

		for (int i = 0; i < 15; i++)
		{
			Dust dust = Dust.NewDustDirect(target.Center, 10, 10, DustID.MagicMirror, Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f), 175, Scale: 1.5f);
			dust.noGravity = true;
		}

		if (FirstHit)
			FirstHit = false;
	}

	public override void SafeModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers, Player player, OrchidGuardian guardian)
	{
		modifiers.FinalDamage *= 1 - 0.0225f * HitCount;
		if (Strong && Projectile.ai[1] != 1)
			modifiers.FinalDamage *= 0.75f;

		if (target.aiStyle == NPCAIStyleID.Worm && target.type != NPCID.SolarCrawltipedeTail && target.type != NPCID.StardustWormHead)
		{
			// attacking a worm, exception for crawltipedes and milkyway weavers
			modifiers.FinalDamage *= 0.5f;
		}

		modifiers.DamageVariationScale *= 0.25f;
	}

	public override void OnKill(int timeLeft)
	{
		for (int i = 0; i < 6; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				if (Main.rand.NextBool())
				{
					Dust cloudDust = Dust.NewDustDirect(Projectile.Center, 10, 10, DustID.MagicMirror, Main.rand.NextFloat(-8f, 8f) * Scale, Main.rand.NextFloat(-8f, 8f) * Scale, 175, Scale: Scale);
					cloudDust.noGravity = true;
				}

				Dust hexDust = Dust.NewDustPerfect(Projectile.Center, DustID.HallowSpray, Vector2.UnitX.RotatedBy(branchRotation(i) + Main.rand.NextFloat(-0.15f, 0.15f)) * Projectile.width * Main.rand.NextFloat(0.01f, 0.1f), 175, Scale: Scale);
				hexDust.noGravity = true;
			}
		}

		SoundEngine.PlaySound(SoundID.Item51, Projectile.Center);
		if (Strong && OrchidMod.ThoriumMod is { } thoriumMod)
		{
			for (int i = 0; i < 6; i++)
			{
				int icebreakerGore = thoriumMod.Find<ModGore>("IceBreakerGore" + Main.rand.Next(1, 4)).Type;
				Vector2 offset = Vector2.UnitX.RotatedBy(branchRotation(i));
				Gore mainBranch = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center + offset * 10f * Scale, offset * 2 * Scale, thoriumMod.Find<ModGore>("IceBreakerGore1").Type, Scale * 0.6f);
				mainBranch.position -= mainBranch.AABBRectangle.Size() * 0.5f;
				mainBranch.rotation = branchRotation(i) + MathHelper.PiOver2;

				if (Scale > 1.5f)
				{
					for (int j = -1; j < 2; j += 2)
					{
						Vector2 subOffset = offset.RotatedBy(0.349f * j);
						Gore subBranch1 = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center + (offset * 15f + subOffset * 2f) * Scale, subOffset * Scale, icebreakerGore, Scale * 0.4f);
						subBranch1.position -= subBranch1.AABBRectangle.Size() * 0.5f;
						subBranch1.rotation = branchRotation(i) + 0.35f * j - MathHelper.PiOver2;
						if (Scale > 0.5f)
						{
							Gore subBranch2 = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center + (offset * 19f + subOffset) * Scale, subOffset * 0.5f * Scale, icebreakerGore, Scale * 0.2f);
							subBranch2.position -= subBranch2.AABBRectangle.Size() * 0.5f;
							subBranch2.rotation = subBranch1.rotation;
						}
					}
				}
			}
		}

		return;

		float branchRotation(int i) => Projectile.rotation + MathHelper.Pi / 12 + MathHelper.TwoPi * i / 6;
	}

	public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
	{
		Texture2D projTexture = TextureAssets.Projectile[Projectile.type].Value;
		Texture2D glowTexture = TextureGlow.Value;
		if (!Strong)
		{
			projTexture = TextureSmall.Value;
			glowTexture = TextureSmallGlow.Value;
		}

		SpriteEffects effects = Projectile.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		Vector2 drawPosition = Projectile.Center - Main.screenPosition;
		
		spriteBatch.End(out SpriteBatchSnapshot snapshot);
		spriteBatch.Begin(snapshot with { BlendState = BlendState.Additive, SamplerState = SamplerState.PointClamp });
		
		for (int i = 0; i < OldRotation.Count; i++)
		{
			Vector2 oldPosition = OldPosition[i] - Main.screenPosition;
			Color trailColor = Color.Lerp(Color.MidnightBlue, Color.RoyalBlue, i / 10f);
			spriteBatch.Draw(projTexture, oldPosition, null, trailColor * Utils.Remap(i, 0, 9, 0.5f, 1f) * Projectile.Opacity, OldRotation[i], glowTexture.Size() * 0.5f, OldScale[i] * Utils.Remap(i, 0, 9, 0.8f, 1.05f), effects, 0f);
		}

		spriteBatch.Draw(glowTexture, drawPosition, null, Color.RoyalBlue * Projectile.Opacity, Projectile.rotation, glowTexture.Size() * 0.5f, Projectile.scale * 1.05f, effects, 0f);

		spriteBatch.End();
		spriteBatch.Begin(snapshot);

		spriteBatch.Draw(projTexture, drawPosition, null, Color.White with { A = 191 } * Projectile.Opacity, Projectile.rotation, projTexture.Size() * 0.5f, Projectile.scale, effects, 0f);

		return false;
	}

	public void SetSize(float scale)
	{
		Vector2 oldCenter = Projectile.Center;
		Scale = scale;
		Projectile.scale = scale / 2;
		Projectile.width = (int)(36 * scale);
		Projectile.height = (int)(36 * scale);
		Projectile.Center = oldCenter;
	}
}