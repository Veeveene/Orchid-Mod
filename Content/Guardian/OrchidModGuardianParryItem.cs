using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace OrchidMod.Content.Guardian
{
	public abstract class OrchidModGuardianParryItem : OrchidModGuardianItem
	{
		public int InvincibilityDuration = 40; // Iframes duration after a parry

		public virtual void OnParry(Player player, OrchidGuardian guardian, Entity aggressor, Projectile anchor) { }
		public virtual void OnParryNPC(Player player, OrchidGuardian guardian, NPC npc, Projectile anchor) { }
		public virtual void OnParryProjectile(Player player, OrchidGuardian guardian, Projectile projectile, Projectile anchor) { }
		/// <summary>Specifically called if the aggressor when initiating the parry is neither an NPC nor a Projectile.</summary>
		public virtual void OnParryOther(Player player, OrchidGuardian guardian, Projectile anchor) { }
		/// <summary> Responsible for playing the sound when the player parries with this weapon. Default behavior is <c>SoundEngine.PlaySound(SoundID.Item37.WithPitchOffset(Main.rand.NextFloat(0.4f, 0.6f)), player.Center);</c> </summary>
		public virtual void PlayParrySound(Player player, OrchidGuardian guardian, Projectile anchor) => SoundEngine.PlaySound(SoundID.Item37.WithPitchOffset(Main.rand.NextFloat(0.4f, 0.6f)), player.Center);
	}
}
