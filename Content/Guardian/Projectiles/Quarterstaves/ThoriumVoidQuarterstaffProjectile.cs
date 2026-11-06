using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Assets;
using OrchidMod.Content.Guardian.Weapons.Quarterstaves;
using OrchidMod.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Projectiles.Quarterstaves;

public class ThoriumVoidQuarterstaffProjectile : OrchidModGuardianProjectile
{
	private static Texture2D TextureMain;
	private static Texture2D TextureGlow;
	
	public Vector2 OriginalVelocity;
	public Vector2 OriginalPosition;
	public List<Vector2> OldPosition;
	public List<float> OldRotation;
	public int TimeSpent = 0;
	
	public Projectile Quarterstaff;

	public override string Texture => OrchidAssets.MiscPath + "Extra_8";

	public override void SetStaticDefaults()
	{
	}

	public override void SafeSetDefaults()
	{
		Projectile.width = 24;
		Projectile.height = 24;
		Projectile.friendly = false;
		Projectile.aiStyle = -1;
		Projectile.timeLeft = 130;
		Projectile.tileCollide = false;
		Projectile.scale = 0.25f;
		Projectile.penetrate = 1;
		Projectile.stopsDealingDamageAfterPenetrateHits = true;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = 10;
		
		TextureMain ??= OrchidAssets.GetExtraTexture(8, AssetRequestMode.ImmediateLoad).Value;
		TextureGlow ??= OrchidAssets.GetExtraTexture(11, AssetRequestMode.ImmediateLoad).Value;

		OldPosition = [];
		OldRotation = [];
	}

	public override void AI()
	{
		if (!Initialized)
		{
			OriginalVelocity = Projectile.velocity;
			Projectile.velocity *= 2f / Projectile.velocity.Length();
			Initialized = true;
		}

		if (Projectile.ai[0] == 1)
		{
			if (Projectile.timeLeft > 20)
			{
				if (Projectile.ai[1] > 0)
				{
					NPC target = Main.npc[(int)Projectile.ai[1]];
					if (target != null && IsValidTarget(target) && target.DistanceSQ(Projectile.Center) <= 102400f)
					{
						Vector2 newVelocity = Vector2.Normalize(target.Center - Projectile.Center) * 0.5f;
						Projectile.velocity += newVelocity;
					}
					else Projectile.ai[1] = -1;
				}
				else
				{
					float maxDistance = 57600f;
					NPC target = null;
					foreach (NPC npc in Main.ActiveNPCs)
					{
						float distance = npc.DistanceSQ(Projectile.Center);
						if (npc.type == NPCID.TargetDummy) distance *= 1.5f;
						else if (npc.boss) distance *= 0.5f;
					
						if (IsValidTarget(npc) && distance <= maxDistance)
						{
							target = npc;
							maxDistance = npc.DistanceSQ(Projectile.Center);
						}
					}
				
					if (target != null)
					{
						Projectile.ai[1] = target.whoAmI;
					}
				}
			}
			else
			{
				Projectile.velocity *= 0.95f;
				Projectile.scale -= 0.02f;
			}
			
			if (OldPosition.Count > 0 && Projectile.Center.Distance(OldPosition[^1]) >= 6f)
			{
				OldPosition.Add(Projectile.Center);
				OldRotation.Add(Projectile.rotation);
			}
				
			if ((OldPosition.Count > 1 && TimeSpent % 6 == 0) || OldPosition.Count > 10)
			{
				OldPosition.RemoveAt(0);
				OldRotation.RemoveAt(0);
			}
		}
		else
		{
			Projectile.timeLeft++;
			if (Projectile.scale < 1)
			{
				Vector2 oldCenter = Projectile.Center;
				Projectile.scale += 0.05f;
				Projectile.width = Projectile.height = (int)(24 * Projectile.scale);
				Projectile.Center = oldCenter;
			}
			else
			{
				Projectile.scale = 1;
			}
			
			if (TimeSpent % 2 == 0)
			{
				OldPosition.Add(Projectile.Center);
				OldRotation.Add(Projectile.rotation);
			}
			
			if (Projectile.ai[1] < 0 || !IsValidTarget(Main.npc[(int)Projectile.ai[1]]))
			{
				float maxDistance = 518400f;
				NPC target = null;
				foreach (NPC npc in Main.ActiveNPCs)
				{
					float distance = npc.DistanceSQ(Projectile.Center);
					if (npc.type == NPCID.TargetDummy) distance *= 1.5f;
					else if (npc.boss) distance *= 0.5f;
					
					if (IsValidTarget(npc) && distance <= maxDistance)
					{
						target = npc;
						maxDistance = npc.DistanceSQ(Projectile.Center);
					}
				}
				
				if (target != null)
				{
					Projectile.ai[1] = target.whoAmI;
				}
			}
			
			if (TimeSpent > 20)
			{
				Projectile.ai[0] = 1;
				Projectile.friendly = true;
				Projectile.netUpdate = true;
				
				OldPosition.Add(Projectile.Center);
				OldRotation.Add(Projectile.rotation);
			}
		}
		
		if (OrchidMod.ThoriumMod is {} thoriumMod)
		{
			int dustType = thoriumMod.Find<ModDust>("VoidHeartDust").Type;
			if (Main.rand.NextBool(6))
			{
				Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(12), 24, 24, dustType);
				dust.velocity = -Projectile.velocity * 0.5f * (Main.rand.NextFloat() + 0.5f);
				dust.scale = Main.rand.NextFloat(2f) + 0.5f;
				dust.fadeIn = 1f;
			}
		}
		Lighting.AddLight(Projectile.Center, new Color(57, 0, 86).ToVector3());
		
		if (Projectile.velocity.Length() > OriginalVelocity.Length())
			Projectile.velocity *= OriginalVelocity.Length() / Projectile.velocity.Length();
		
		Projectile.rotation = Projectile.velocity.ToRotation();
		TimeSpent++;
	}

	public override bool? CanHitNPC(NPC target) => target.whoAmI == (int)Projectile.ai[1] ? base.CanHitNPC(target) : false;

	public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidGuardian guardian)
	{
		Projectile.timeLeft = 20;
		if (OrchidMod.ThoriumMod is {} thoriumMod)
		{
			int debuffType = OrchidMod.ThoriumMod.Find<ModBuff>("LightCurse").Type;
			target.AddBuff(debuffType, 60);
			
			int dustType = thoriumMod.Find<ModDust>("VoidHeartDust").Type;
			for (int i = 0; i < 4; i++)
			{
				Dust dust = Dust.NewDustDirect(target.Center, 0, 0, dustType);
				dust.velocity *= Main.rand.NextFloat() * 2.0f + 0.5f;
				dust.scale = Main.rand.NextFloat() + 0.5f;
				dust.fadeIn = 1f;
			}
		}
	}

	public override void OnKill(int timeLeft)
	{
		if (OrchidMod.ThoriumMod is {} thoriumMod)
		{
			int dustType = thoriumMod.Find<ModDust>("VoidHeartDust").Type;
			for (int i = 0; i < 10; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, dustType);
				dust.velocity *= Main.rand.NextFloat(2f);
				dust.scale = Main.rand.NextFloat() + 0.5f;
				dust.fadeIn = 1f;
			}
		}
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.WriteVector2(OriginalVelocity);
		writer.WriteVector2(OriginalPosition);
		writer.Write(TimeSpent);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		OriginalVelocity = reader.ReadVector2();
		OriginalPosition = reader.ReadVector2();
		TimeSpent = reader.ReadInt32();
	}

	public override bool OrchidPreDraw(SpriteBatch spriteBatch, ref Color lightColor)
	{
		spriteBatch.End(out SpriteBatchSnapshot snapshot);
		SpriteBatchSnapshot glowSnapshot = snapshot with { BlendState = BlendState.Additive, RasterizerState = RasterizerState.CullNone };
		spriteBatch.Begin(glowSnapshot);
		
		float colorMult = 1f;
		if (Projectile.timeLeft < 20) colorMult *= Projectile.timeLeft / 20f;
		
		for (int i = 0; i < OldPosition.Count; i++)
		{
			Color color = Lighting.GetColor(Projectile.Center.ToTileCoordinates(), Color.Lerp(new Color(57, 0, 86), new Color(188, 0, 163), i / (float)OldPosition.Count));
			float scale = MathHelper.Lerp(0.4f, 1f, i / (float)OldPosition.Count) * Projectile.scale;
			
			spriteBatch.Draw(TextureGlow, OldPosition[i] - Vector2.Normalize(Projectile.velocity) * 0.5f - Main.screenPosition, null, color * 0.8f, OldRotation[i], TextureGlow.Size() * 0.5f, scale * 0.75f, SpriteEffects.None, 0f);
			
			spriteBatch.End();
			spriteBatch.Begin(snapshot);
			spriteBatch.Draw(TextureMain, OldPosition[i] - Main.screenPosition, null, color, OldRotation[i], TextureMain.Size() * 0.5f, scale * 0.5f, SpriteEffects.None, 0f);
			
			spriteBatch.End();
			spriteBatch.Begin(glowSnapshot);
		}

		
		Vector2 drawPos = Projectile.Center - Main.screenPosition;
		Color colorPink = Lighting.GetColor(Projectile.Center.ToTileCoordinates(), new Color(188, 0, 163));
		Vector2 offset = Main.rand.NextVector2Circular(1f, 1f);
		spriteBatch.End();
		spriteBatch.Begin(glowSnapshot);
		spriteBatch.Draw(TextureGlow, drawPos + offset * 5f, null, colorPink * colorMult * 0.8f, Projectile.rotation, TextureGlow.Size() * 0.5f, Projectile.scale * 0.75f, SpriteEffects.None, 0f);	
		
		spriteBatch.End();
		spriteBatch.Begin(snapshot);
		spriteBatch.Draw(TextureMain, drawPos + offset * 5f, null, colorPink * colorMult, Projectile.rotation, TextureMain.Size() * 0.5f, Projectile.scale * 0.5f, SpriteEffects.None, 0f);
		
		// spriteBatch.End();
		// spriteBatch.Begin(snapshot with {BlendState = BlendState.Additive});
		Color colorPurple = Lighting.GetColor(Projectile.Center.ToTileCoordinates(), new Color(57, 0, 86));
		spriteBatch.Draw(TextureGlow, drawPos + offset * 2.5f, null, colorPurple * colorMult * 0.8f, Projectile.rotation, TextureGlow.Size() * 0.5f, Projectile.scale * 0.4f, SpriteEffects.None, 0f);		
		
		spriteBatch.Draw(TextureMain, drawPos + offset * 2.5f, null, colorPurple * colorMult, Projectile.rotation, TextureMain.Size() * 0.5f, Projectile.scale * 0.25f, SpriteEffects.None, 0f);	
		
		return false;
	}
}