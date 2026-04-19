using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Projectiles.Quarterstaves
{
	public class ThoriumConcentratedThoriumQuarterstaffProjectile : OrchidModGuardianProjectile
	{

		public List<int> ChainedEnemies;
		public bool DoneSearching;

		public NPC PastNode = null;
		public NPC CurrentNode = null;

		public override string Texture => $"Terraria/Images/Projectile_27";

		public override void SafeSetDefaults()
		{
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 900;
			Projectile.scale = 1f;
			Projectile.penetrate = 5;
			Projectile.alpha = 255;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			
			ChainedEnemies = [];
			DoneSearching = false;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (!DoneSearching && ChainedEnemies != null && !ChainedEnemies.Contains(target.whoAmI)) return base.CanHitNPC(target);
			return false;
		}

		// public override void OnSpawn(IEntitySource source)
		// {
		// 	ChainedEnemies = [];
		// }

		public override void AI()
		{
			if (!DoneSearching)
			{
				Projectile.timeLeft = 900;

				if (CurrentNode == null || !CurrentNode.active || CurrentNode.friendly) DoneSearching = true;

				if (ChainedEnemies != null && ChainedEnemies.Count == 10) DoneSearching = true;

				float distanceSq = 102400f;
				NPC nextTarget = null;
				foreach (NPC npc in Main.npc)
				{
					if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.Center.DistanceSQ(CurrentNode.Center) < distanceSq && !ChainedEnemies.Contains(npc.whoAmI)) 
					{
						distanceSq = npc.Center.DistanceSQ(CurrentNode.Center);
						nextTarget = npc;
					}
				}
				if (nextTarget != null) Projectile.velocity = Projectile.Center.DirectionTo(nextTarget.Center) * 60f;
				else DoneSearching = true;
				
			}
			if (ChainedEnemies.Count > 0)
			{
				for (int i = 0; i < ChainedEnemies.Count - 1; i++)
				{
					NPC node = Main.npc[i];
					if (node == null || !node.active) Projectile.Kill();
					if (ChainedEnemies.Count > 1 && i > 0)
					{
						Dust.QuickDustLine(Main.npc[i - 1].Center, node.Center, 10f, Color.Aqua);
					}
				}
			}
			
			if (Main.rand.NextBool(4)) Dust.NewDustDirect(Projectile.Center, 12, 12, DustID.UltraBrightTorch);
		}

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidGuardian guardian)
		{
			
			if (!DoneSearching)
			{
				if (target.active && !target.friendly) ChainedEnemies.Add(target.whoAmI);
				if (PastNode == null && CurrentNode == null) CurrentNode = target;
				else
				{
					PastNode = CurrentNode;
					CurrentNode = target;
				}
			}
			
			
		}
	}
}