namespace OrchidMod.Content.General.Prefixes
{
	// Accessory

	public class BrewingPrefix() : AccessoryPrefix(1, 0, 0);
	public class LoadedPrefix() : AccessoryPrefix(0, 2, 0);
	public class BlockingPrefix() : AccessoryPrefix(0, 0, 1);

	// Shaman - Damage, Bond Loading, Bond Duration, Critical Strike Chance, Velocity

	/*
	public class CursedPrefix : ShamanPrefix { public CursedPrefix() : base(0.85f, 1.00f, -1, 0, 0.90f) { } }
	public class PossessedPrefix : ShamanPrefix { public PossessedPrefix() : base(1.00f, 0.85f, 0, 0, 0.90f) { } }
	public class JinxedPrefix : ShamanPrefix { public JinxedPrefix() : base(1.00f, 0.90f, -2, 0, 1.00f) { } }
	public class HexxedPrefix : ShamanPrefix { public HexxedPrefix() : base(1.15f, 0.90f, 0, 1, 1.00f) { } }
	public class BewitchedPrefix : ShamanPrefix { public BewitchedPrefix() : base(0.85f, 1.15f, 0, 0, 1.00f) { } }
	public class VoodooedPrefix : ShamanPrefix { public VoodooedPrefix() : base(1.10f, 1.00f, 0, 3, 1.00f) { } }
	public class OccultPrefix : ShamanPrefix { public OccultPrefix() : base(1.00f, 1.10f, 0, 0, 1.15f) { } }
	public class FocusedPrefix : ShamanPrefix { public FocusedPrefix() : base(1.00f, 1.15f, 0, 0, 1.10f) { } }
	public class FerventPrefix : ShamanPrefix { public FerventPrefix() : base(1.00f, 1.00f, 2, 0, 1.05f) { } }
	public class SpiritedPrefix : ShamanPrefix { public SpiritedPrefix() : base(1.10f, 1.05f, 1, 2, 1.05f) { } }
	public class EffervescentPrefix : ShamanPrefix { public EffervescentPrefix() : base(1.10f, 1.00f, 2, 0, 1.00f) { } }
	public class EtherealPrefix : ShamanPrefix { public EtherealPrefix() : base(1.15f, 1.10f, 2, 5, 1.10f) { } }
	*/

	// Guardian - Damage, Knockback, Block Duration, Critical Strike Chance, Speed

	public class HaidexPrefix() : GuardianPrefix(1.00f, 1.00f, 0.85f, 20, 0.85f); // Easter Egg
	public class AnnoyingPrefixGuardian() : GuardianPrefix(0.8f, 1f, 1f, 0, 0.85f) // Ostensibly bad; exclusive to Toy Warhammers (base Annoying isn't applicable due to speed mult restriction)
	{
		public override float obnoxiousness => 4f;
	}  
	public class FlimsyPrefix() : GuardianPrefix(0.85f, 1.00f, 0.90f, 0, 0.90f); // Bad
	public class FeeblePrefix() : GuardianPrefixNoBlockDuration(1.00f, 0.85f, 1.00f, 0, 0.90f);
	public class FragilePrefix() : GuardianPrefix(1.00f, 0.90f, 0.85f, 0, 1.00f);
	public class ResolutePrefix() : GuardianPrefixNoBlockDuration(1.15f, 0.90f, 1.00f, 1, 1.00f); // Mitigated
	public class StoutPrefix() : GuardianPrefixNoBlockDuration(0.85f, 1.15f, 1f, 0, 1.00f);
	public class UnyieldingPrefix() : GuardianPrefixNoBlockDuration(1.10f, 1.00f, 1.00f, 3, 1.00f); // Good
	public class SturdyPrefix() : GuardianPrefix(1.00f, 1.10f, 1.15f, 0, 1.00f);
	public class SteadfastPrefix() : GuardianPrefixNoBlockDuration(1.00f, 1.15f, 1.00f, 0, 1.10f);
	public class ImpregnablePrefix() : GuardianPrefix(1.00f, 1.15f, 1.10f, 0, 1.00f);
	public class ToweringPrefix() : GuardianPrefix(1.00f, 1.00f, 1.15f, 0, 1.05f);
	public class SpartanPrefix() : GuardianPrefix(1.10f, 1.05f, 1.1f, 2, 1.05f); // Very good
	public class AngelicPrefix() : GuardianPrefixNoBlockDuration(1.15f, 1.00f, 1.00f, 5, 1.10f);
	public class HulkingPrefix() : GuardianPrefix(1.15f, 1.05f, 1.15f, 0, 1.00f);
	public class EmpyreanPrefix() : GuardianPrefix(1.15f, 1.10f, 1.15f, 5, 1.10f);

	// Shapeshifter - Damage, Knockback, Attack Speed, Critical Strike Chance, Move Speed
	public class TimidPrefix() : ShapeshifterPrefix(0.85f, 1.00f, -0.15f, 0, -0.05f); // Bad
	public class BoarishPrefix() : ShapeshifterPrefix(1.00f, 0.85f, -0.10f, 0, -0f);
	public class MisshapenPrefix() : ShapeshifterPrefix(1.00f, 0.90f, -0.15f, 0, 0f);
	public class EnragedPrefix() : ShapeshifterPrefix(1.15f, 0.90f, 0f, 1, 0f); // Mitigated
	public class BestialPrefix() : ShapeshifterPrefix(0.85f, 1.15f, 0f, 0, 0f);
	public class VoraciousPrefix() : ShapeshifterPrefix(1.10f, 1.00f, 0f, 3, 0f); // Good
	public class UntamedPrefix() : ShapeshifterPrefix(1.00f, 1.10f, 0f, 0, 0.15f);
	public class FiercePrefix() : ShapeshifterPrefix(1.00f, 1.15f, 0f, 0, 0.10f);
	public class FeralPrefix() : ShapeshifterPrefix(1.00f, 1.00f, 0.15f, 0, 0.05f);
	public class MonstrousPrefix() : ShapeshifterPrefix(1.15f, 1.05f, 0.15f, 0, 0f); // Very good 
	public class PrimalPrefix() : ShapeshifterPrefix(1.10f, 1.05f, 0.1f, 2, 0.05f);
	public class DivinePrefix() : ShapeshifterPrefix(1.15f, 1.10f, 0.15f, 5, 0.10f);
}