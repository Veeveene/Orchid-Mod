namespace OrchidMod.Content.Guardian
{
	public struct GuardianAttackInfo
	{
		/// <summary> Whether this action is responsible for dealing damage or spawning a projectile intended to cause direct damage. Defaults to true when a blank AttackInfo is instanced. </summary>
		public bool Offense = true;
		/// <summary> Whether this action takes direct action to defend the player, including granting invulnerability, destroying projectiles, or spawning a blocking hitbox. </summary>
		public bool Defense = false;
		/// <summary> Whether this action is involved in the initiation of a defensive action, including initiating a parry, block or deflect. </summary>
		public bool Guard = false;
		/// <summary> Whether this action is a slamming attack, including attacks that can consume either a slam or another resource. </summary>
		public bool Slam = false;
		/// <summary> Internal bool not intended for direct use. See <c>Charged</c>. </summary>
		bool charge = false;
		/// <summary> Internal bool not intended for direct use. See <c>Jab</c> and <c>Reinforced</c>. </summary>
		bool chargeContext = false;
		/// <summary> Whether this action should be treated as a counterattack. This will automatically be set as part of <c>OrchidGuardian.OnAttack</c> if <c>OrchidGuardian.GuardianCounterTime > 0</c> and <c>Slam</c> is <c>true</c>. </summary>
		public bool Counter = false;
		// verveine said to call this unused bit meow
		//public bool meow = false;

		/// <summary> Whether this action consumes a full charge to perform. This is mutually exclusive with <c>Jab</c>, and <c>Reinforced</c> is dependent on this. Setting this to <c>true</c> will disable <c>Jab</c>, and setting this to <c>false</c> will disable <c>Reinforced</c>.</summary>
		/// <remarks> See <c>SetCharged()</c>, <c>SetJab()</c>, <c>SetReinforced()</c>, and <c>SetNonCharge()</c> for helper methods for setting these fields. </remarks>
		public bool Charged
		{
			get => charge;
			set
			{
				if (value != charge)
				{
					charge = value;
					chargeContext = false;
				}
			}
		}
		/// <summary> Whether this action is a jab, a non-charged attack performed in relation to charging, including during charging or with the primary purpose of accumulating charge or an equivalent resource. </summary>
		/// <inheritdoc cref="Charged"/>
		public bool Jab => !charge && chargeContext;
		/// <summary> Whether this action is reinforced, consuming more than a full charge to perform. This also counts as <c>Charged</c>. </summary>
		/// <inheritdoc cref="Charged"/>
		public bool Reinforced => charge && chargeContext;
		/// <summary> Whether this action is unrelated to charging. This will only be true if <c>Charged</c> and <c>Jab</c> are false. </summary>
		/// <inheritdoc cref="Charged"/>
		public bool NonCharge => !charge && !chargeContext;

		public GuardianAttackInfo() {}

		public GuardianAttackInfo(byte input)
		{
			Offense = (input & 0b_0000_0001) != 0;
			Defense = (input & 0b_0000_0010) != 0;
			Guard = (input & 0b_0000_0100) != 0;
			Slam = (input & 0b_0000_1000) != 0;
			charge = (input & 0b_0001_0000) != 0;
			chargeContext = (input & 0b_0010_0000) != 0;
			Counter = (input & 0b_0100_0000) != 0;
			//meow = (input & 0b_1000_0000) != 0;
		}

		public byte ToByte()
		{
			byte output = 0b_0000_0000;
			if (Offense) output |= 0b_0000_0001;
			if (Defense) output |= 0b_0000_0010;
			if (Guard) output |= 0b_0000_0100;
			if (Slam) output |= 0b_0000_1000;
			if (charge) output |= 0b_0001_0000;
			if (chargeContext) output |= 0b_0010_0000;
			if (Counter) output |= 0b_0100_0000;
			//if (meow) output |= 0b_1000_0000;
			return output;
		}

		public static implicit operator byte(GuardianAttackInfo input) => input.ToByte();
		public static implicit operator GuardianAttackInfo(byte input) => new(input);

		/// <summary> Use to manually set charge state. </summary>
		/// <inheritdoc cref="Charged"/>
		public void SetChargeState(bool chargeValue, bool contextValue)
		{
			charge = chargeValue;
			chargeContext = contextValue;
		}


		/// <summary> Helper method that flags <c>this.Offense</c> then returns <c>this</c>. </summary>
		/// <remarks> Useful for daisy-chaining to quickly set multiple flags. To make a gauntlet slam that also counts as guarding, defending and counterattacking, you can do <c>AttackID.GauntletSlam.SetDefense().SetGuard().SetCounter()</c>.</remarks>
		public GuardianAttackInfo SetOffense() { Offense = true; return this; }
		/// <summary> Helper method that disables <c>this.Offense</c> then returns <c>this</c>. </summary>
		/// <inheritdoc cref="SetOffense()"/>
		public GuardianAttackInfo ResetOffense() { Offense = false; return this; }
		/// <summary> Helper method that flags <c>this.Defense</c> then returns <c>this</c>. </summary>
		/// <inheritdoc cref="SetOffense()"/>
		public GuardianAttackInfo SetDefense() { Defense = true; return this; }
		/// <summary> Helper method that flags <c>this.Guard</c> then returns <c>this</c>. </summary>
		/// <inheritdoc cref="SetOffense()"/>
		public GuardianAttackInfo SetGuard() { Guard = true; return this; }
		/// <summary> Helper method that flags <c>this.Slam</c> then returns <c>this</c>. </summary>
		/// <inheritdoc cref="SetOffense()"/>
		public GuardianAttackInfo SetSlam() { Slam = true; return this; }
		/// <summary> Helper method that flags <c>this.Charged</c>, disables <c>this.Reinforced</c> and <c>this.Jab</c>, then returns <c>this</c>. Note that <c>Charged</c> is mutually exclusive with <c>Jab</c>. Use <c>SetReinforced</c> instead for reinforced attacks. </summary>
		/// <inheritdoc cref="SetOffense()"/>
		public GuardianAttackInfo SetCharged() { Charged = true; chargeContext = false; return this; }
		/// <summary> Helper method that flags <c>this.Jab</c>, disables <c>this.Charged</c> and <c>this.Reinforced</c>, then returns <c>this</c>. Note that <c>Jab</c> is mutually exclusive with <c>Charged</c> and <c>Reinforced</c>. </summary>
		/// <inheritdoc cref="SetOffense()"/>
		public GuardianAttackInfo SetJab() { Charged = false; chargeContext = true; return this; }
		/// <summary> Helper method that flags <c>this.Charged</c> and <c>this.Reinforced</c>, disables <c>this.Jab</c>, then returns <c>this</c>. Note that <c>Charged</c> is mutually exclusive with <c>Jab</c>. </summary>
		/// <inheritdoc cref="SetOffense()"/>
		public GuardianAttackInfo SetReinforced() { Charged = true; chargeContext = true; return this; }
		/// <summary> Helper method that disables <c>this.Charged</c>, <c>this.Reinforced</c>, and <c>this.Jab</c>, then returns <c>this</c>.</summary>
		/// <inheritdoc cref="SetOffense()"/>
		public GuardianAttackInfo SetNonCharge() { Charged = false; chargeContext = false; return this; }
		/// <summary> Helper method that flags <c>this.Counter</c> then returns <c>this</c>. </summary>
		/// <inheritdoc cref="SetOffense()"/>
		public GuardianAttackInfo SetCounter() { Counter = true; return this; }
	}

	public static class AttackID
	{
		/// <summary> AttackInfo with Offense. </summary>
		static readonly GuardianAttackInfo Bash = new(1); // free shield bash, uncharged hammer throw, greathammer failed deflect
		/// <summary> AttackInfo with Defense. </summary>
		static readonly GuardianAttackInfo Defend = new(2); // parry resolve
		/// <summary> AttackInfo with Guard. </summary>
		static readonly GuardianAttackInfo Guard = new(4); // parry prepare
		/// <summary> AttackInfo with Offense and Guard. </summary>
		static readonly GuardianAttackInfo GuardAttack = new(5); // katar dash
		/// <summary> AttackInfo with Defense and Guard. </summary>
		static readonly GuardianAttackInfo Block = new(6); // shield block
		/// <summary> AttackInfo with Offense, Defense, and Guard. </summary>
		static readonly GuardianAttackInfo BlockAttack = new(7); // hammer block, greathammer successful deflect
		/// <summary> Attackinfo with Offense and Slam. </summary>
		static readonly GuardianAttackInfo Slam = new(9); // shield slam, gauntlet slam
		/// <summary> Attackinfo with Charged. </summary>
		static readonly GuardianAttackInfo ChargedBuff = new(16); // rune charge, standard charge
		/// <summary> Attackinfo with Offense and Charged. </summary>
		static readonly GuardianAttackInfo Charged = new(17); // charged hammer throw, quarterstaff swing, charged greathammer throw
		/// <summary> Attackinfo with Defense, Guard, and Charged. </summary>
		static readonly GuardianAttackInfo ChargedBlock = new(22); // charged shield block
		/// <summary> Attackinfo with Offense, Slam and Charged. </summary>
		static readonly GuardianAttackInfo ChargedSlam = new(25); // charged gauntlet slam
		/// <summary> Attackinfo with Offense and Jab. </summary>
		static readonly GuardianAttackInfo Jab = new(33); // hammer swing, gauntlet jab, quarterstaff jab
		/// <summary> Attackinfo with Offense, Slam, and Jab. </summary>
		static readonly GuardianAttackInfo SlamJab = new(41); // katar slam, greathammer slam
		/// <summary> Attackinfo with Reinforced. </summary>
		static readonly GuardianAttackInfo ReinforcedBuff = new(48); // rune reinforce, standard reinforce
		/// <summary> Attackinfo with Offense, Defense and Counter. </summary>
		static readonly GuardianAttackInfo CounterDefend = new(67); // quarterstaff parry resolve

		/// <inheritdoc cref="Block"/>
		public static GuardianAttackInfo ShieldBlock => Block;
		/// <inheritdoc cref="ChargedBlock"/>
		public static GuardianAttackInfo ShieldCharge => ChargedBlock;
		/// <inheritdoc cref="Bash"/>
		public static GuardianAttackInfo ShieldBash => Bash;
		/// <inheritdoc cref="Slam"/>
		public static GuardianAttackInfo ShieldSlam => Slam;
		/// <inheritdoc cref="Jab"/>
		public static GuardianAttackInfo HammerSwing => Jab;
		/// <inheritdoc cref="Bash"/>
		public static GuardianAttackInfo HammerBash => Bash;
		/// <inheritdoc cref="Charged"/>
		public static GuardianAttackInfo HammerCharge => Charged;
		/// <inheritdoc cref="BlockAttack"/>
		public static GuardianAttackInfo HammerBlock => BlockAttack;
		/// <inheritdoc cref="ChargedBuff"/>
		public static GuardianAttackInfo RuneCharge => ChargedBuff;
		/// <inheritdoc cref="ReinforcedBuff"/>
		public static GuardianAttackInfo RuneReinforce => ReinforcedBuff;
		/// <inheritdoc cref="Jab"/>
		public static GuardianAttackInfo GauntletJab => Jab;
		/// <inheritdoc cref="Slam"/>
		public static GuardianAttackInfo GauntletSlam => Slam;
		/// <inheritdoc cref="ChargedSlam"/>
		public static GuardianAttackInfo GauntletCharge => ChargedSlam;
		/// <inheritdoc cref="Guard"/>
		public static GuardianAttackInfo GauntletGuard => Guard;
		/// <inheritdoc cref="Defend"/>
		public static GuardianAttackInfo Parry => Defend;
		/// <inheritdoc cref="ChargedBuff"/>
		public static GuardianAttackInfo StandardCharge => ChargedBuff;
		/// <inheritdoc cref="ReinforcedBuff"/>
		public static GuardianAttackInfo StandardReinforce => ReinforcedBuff;
		/// <inheritdoc cref="Jab"/>
		public static GuardianAttackInfo QuarterstaffJab => Jab;
		/// <inheritdoc cref="Charged"/>
		public static GuardianAttackInfo QuarterstaffCharge => Charged;
		/// <inheritdoc cref="Guard"/>
		public static GuardianAttackInfo QuarterstaffGuard => Guard;
		/// <inheritdoc cref="CounterDefend"/>
		public static GuardianAttackInfo QuarterstaffCounter => CounterDefend;
		/// <inheritdoc cref="SlamJab"/>
		public static GuardianAttackInfo KatarSlam => SlamJab;
		/// <inheritdoc cref="Charged"/>
		public static GuardianAttackInfo KatarCharge => Charged;
		/// <inheritdoc cref="GuardAttack"/>
		public static GuardianAttackInfo KatarDash => GuardAttack;
		/// <inheritdoc cref="Charged"/>
		public static GuardianAttackInfo GreathammerCharge => Charged;
		/// <inheritdoc cref="SlamJab"/>
		public static GuardianAttackInfo GreathammerSlam => SlamJab;
		/// <inheritdoc cref="Jab"/>
		public static GuardianAttackInfo GreathammerSlamWeak => Jab;
		/// <inheritdoc cref="BlockAttack"/>
		public static GuardianAttackInfo GreathammerDeflect => BlockAttack;
		/// <inheritdoc cref="Bash"/>
		public static GuardianAttackInfo GreathammerWeakDeflect => Bash;
	}
}
