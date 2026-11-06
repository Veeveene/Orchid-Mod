using Microsoft.Xna.Framework;
using OrchidMod.Common.Global.NPCs;
using OrchidMod.Common.ModObjects;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Buffs.Debuffs
{
	public class HockeyQuarterstaffDebuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
			BuffID.Sets.CanBeRemovedByNetMessage[Type] = true;
		}

		public override void Update(NPC npc, ref int buffIndex)
		{
			OrchidGlobalNPC orchidNPC = npc.GetGlobalNPC<OrchidGlobalNPC>();

			foreach (NPC otherNPC in Main.npc)
			{
				if (npc.Center.Distance(otherNPC.Center) < (Math.Max(otherNPC.width, otherNPC.height) * 0.5f + Math.Max(otherNPC.width, otherNPC.height) * 0.5f + 16f) && npc != otherNPC && OrchidModProjectile.IsValidTarget(otherNPC))
				{
					Player player = Main.player[orchidNPC.StarKOOwner];
					int remainingDamage = npc.buffTime[buffIndex] / 15;
					if (otherNPC.immune[player.whoAmI] <= 0)
					{
						player.ApplyDamageToNPC(otherNPC, Main.remixWorld ? 100 : 500, npc.velocity.Length(), npc.velocity.X > 0 ? 1 : -1, false, ModContent.GetInstance<GuardianDamageClass>(), true);
						otherNPC.immune[player.whoAmI] = 10;
					}
					if (npc.immune[player.whoAmI] <= 0)
					{
						player.ApplyDamageToNPC(npc, Math.Clamp(npc.life - remainingDamage, 1, Main.remixWorld ? 100 : 500), 0f, 1, false, ModContent.GetInstance<GuardianDamageClass>());
						if (npc.life > 10)
							npc.immune[player.whoAmI] = 10;
					}
				}
			}

			npc.velocity = Vector2.Lerp(npc.velocity, orchidNPC.ForcedVelocity, npc.buffTime[buffIndex] * npc.knockBackResist * 0.01f);
			if (npc.velocity.Length() < 8f)
			{
				npc.lifeRegen -= 15 * npc.buffTime[buffIndex];
				npc.RequestBuffRemoval(ModContent.BuffType<HockeyQuarterstaffDebuff>());
			}
			else
			{
				npc.lifeRegen -= 15;
				Vector2 finalVelocity = Vector2.Zero;
				Vector2 intendedVelocity = npc.velocity / 10f;
				for (int i = 0; i < 10; i++)
				{
					finalVelocity += Collision.TileCollision(npc.position + finalVelocity, intendedVelocity, npc.width, npc.height);
				}

				if ((finalVelocity - npc.velocity).Length() > 8f)
				{
					Player player = Main.player[orchidNPC.StarKOOwner];

					npc.velocity *= 0f;
					player.ApplyDamageToNPC(npc, Main.remixWorld ? 100 : 500, 0f, 1, false, ModContent.GetInstance<GuardianDamageClass>(), true);
					npc.RequestBuffRemoval(ModContent.BuffType<HockeyQuarterstaffDebuff>());
				}
			}
		}
	}
}