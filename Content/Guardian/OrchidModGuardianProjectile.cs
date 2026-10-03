using OrchidMod.Common;
using OrchidMod.Common.Global.NPCs;
using OrchidMod.Common.ModObjects;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian
{
	public abstract class OrchidModGuardianProjectile : OrchidModProjectile
	{
		public virtual void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone, Player player, OrchidGuardian guardian) { }
		public virtual void SafeModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers, Player player, OrchidGuardian guardian) { }
		/// <summary>Set to false in ModifyHitNPC when this projectile or specific anchor attack hits an enemy a second time. Flagged before most hit functions.</summary>
		/// <remarks>As the name suggests, this will be true on only the first time hit functions are called. Useful for effects that are only supposed to trigger once per projectile, such as resource generation.</remarks>
		public bool FirstHit;
		/// <summary>Set to false in ModifyHitNPC when this projectile or specific anchor attack hits an enemy for the first time. Flagged before most hit functions.</summary>
		public bool NotHitYet;
		/// <summary>Whether this projectile is the main attack from either a fully charged attack, slam, or a quarterstaff counterattack. Defaults to <c>false</c> and is set otherwise by Anchor AI or specific projectiles in their <c>SafeSetDefaults</c>.</summary>
		/// <remarks>Used for limiting hit effects to only important attacks.
		/// A charged attack or slam being consumed should generally only result in one <c>Strong</c> projectile hitting, including the anchor itself if it can hit.
		/// Most anchors will flag themselves as <c>Strong</c> by default, but it should be set manually if overriding their normal functions.</remarks>
		public bool Strong;

		/// <summary>Resets <c>FirstHit</c> and <c>NotHitYet</c> to true, and sets <c>Strong</c> to the input bool.</summary>
		/// <remarks>See also <see cref="Strong"/></remarks>
		public void ResetHitStatus(bool isStrong)
		{
			FirstHit = NotHitYet = true;
			Strong = isStrong;
		}

		public sealed override void AltSetDefaults()
		{
			Projectile.DamageType = ModContent.GetInstance<GuardianDamageClass>();
			ResetHitStatus(false);
			SafeSetDefaults();
		}

		public sealed override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Player player = Main.player[Projectile.owner];
			OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();

			SafeOnHitNPC(target, hit, damageDone, player, guardian);
		}

		public sealed override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			FirstHit = NotHitYet;
			NotHitYet = false;
			Player player = Main.player[Projectile.owner];
			OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
			SafeModifyHitNPC(target, ref modifiers, player, guardian);
			OrchidGlobalNPC modTarget = target.GetGlobalNPC<OrchidGlobalNPC>();
			if (!modTarget.GuardianHit)
			{
				modTarget.GuardianHit = true;

				if (Main.netMode == NetmodeID.MultiplayerClient)
				{
					var packet = Mod.GetPacket();
					packet.Write((byte)OrchidModMessageType.NPCHITBYCLASS);
					packet.Write((byte)target.whoAmI);
					packet.Write((byte)2);
					packet.Send();
				}
			}
		}
	}

	public abstract class OrchidModGuardianAnchor : OrchidModGuardianProjectile {} // Only used to "fake" true melee hits when hitting with those projectiles
	public abstract class OrchidModGuardianParryAnchor : OrchidModGuardianAnchor { } // used to identify achors that can "parry" attacks
}