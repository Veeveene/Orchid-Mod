using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrchidMod.Common;
using OrchidMod.Common.Global.Items;
using OrchidMod.Content.General.Prefixes;
using OrchidMod.Content.Guardian.Buffs.Debuffs;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Terraria.NPC;

namespace OrchidMod.Content.Guardian
{
	public abstract class OrchidModGuardianGreathammer : OrchidModGuardianDeflectItem
	{
		/// <summary>Amount of slams (resource) to grant the owner on a fully charged throw hit. SlamStacks or GuardStacks should always be >0. Defaults to 0</summary>
		public int SlamStacks;
		/// <summary>Amount of guards (resource) to grant the owner on a fully charged throw hit. GuardStacks or SlamStacks should always be >0. Defaults to 0</summary>
		public int GuardStacks;
		/// <summary>Number of guards consumed on a successful deflect. The player will be granted an equal number of slams.</summary>
		public int GuardCost;
		/// <summary>Duration of deflect stuns applied to hit enemies. Defaults to 120 (2 seconds). Use OrchidModGuardianGreathammer.GetStunDuration() when attempting to get the stun duration to apply.</summary>
		public int StunDuration;
		/// <summary>Multiplier for the speed of the greathammer when returning after a throw. Multiplier is relative to ShootSpeed. Defaults to 1f.</summary>
		public float ReturnSpeed;
		/// <summary>Multiplier for the speed of the slam animation. Defaults to 1f.</summary>
		public float SlamSpeed;
		/// <summary>Multiplier for the speed of the deflect animation. Defaults to 1f.</summary>
		public float DeflectSpeed;
		/// <summary>Multiplier for the amount of bonus charge gained from hitting with a slam. Defaults to 1f.</summary>
		public float SlamChargeGain;
		/// <summary>Multiplier for the baseline charge speed of the weapon. Defaults to 1f.</summary>
		public float WaitChargeGain;
		/// <summary>Multiplier for the item damage dealt to enemies hit by an uncharged bash. Defaults to 0.25f</summary>
		public float BashDamage;
		/// <summary>Multiplier for the item damage dealt to enemies hit by a slam. Defaults to 1f</summary>
		public float SlamDamage;
		/// <summary>Multiplier for the item damage dealt to enemies hit by a throw. Defaults to 1.5f</summary>
		public float ThrowDamage;
		/// <summary>Time in frames before throw velocity reaches 0 and the hammer begins returning. Throw velocity loss is calculated dynamically, so it will slow faster with a lower time, and vice versa. Use `Item.ShootSpeed` to control base throw velocity. Defaults to 20.</summary>
		public int Range;
		/// <summary>Multiplier for the size of the default Greathammer aura and its hitbox. Defaults to 1f</summary>
		public float AuraScale;
		/// <summary>Projectile.localNPCHitCooldown for the hammer anchor while thrown. Defaults to -1.</summary>
		public int ThrowHitCooldown;
		/// <summary>Amount of frames for the HammerTexture to be split into. </summary>
		public int HammerFrames = 1;
		/// <summary>Offsets the draw origin of the hammer sprite from its center. Defaults to <c>new(-8, 8)</c> to make the point the player holds it at and the point it pivots on closer to the center of the shaft rather than near the head.</summary>
		public Point DrawOffset;
		/// <summary>Color of the main trail of the aura. Recommended to leave alpha at a low value.</summary>
		public Color AuraPrimary;
		/// <summary>Color of the outer trail of the aura. Recommended to leave alpha at a low value.</summary>
		public Color AuraSecondary;
		public virtual string HammerTexture => Texture + "_Hammer";


		/// <summary>Called on the first frame of a swing, FullyCharged is true if the guardian's weapon charge is full.</summary>
		public virtual void OnSlam(Player player, OrchidGuardian guardian, Projectile projectile, GuardianGreathammerAnchor anchor, bool FullyCharged, ref bool Weak) { } // implemented
		/// <summary>Called at the end of a slam, right after switching back to a charging state.</summary>
		public virtual void OnSlamEnd(Player player, OrchidGuardian guardian, Projectile projectile, GuardianGreathammerAnchor anchor, bool FullyCharged, bool Weak) { } // implemented

		//GRRRAAAA THESE HOOKS ARE USELESS I STILL NEED TO IMPLEMENT THEM!!! GRRRAAAAAA

		/// <summary>Called before default ModifyHit functionality. Return false to prevent normal multipliers like SwingDamage or reduced damage after penetration from applying.</summary>
		public virtual bool ModifyHit(Player player, OrchidGuardian guardian, Projectile projectile, GuardianGreathammerAnchor anchor, NPC target, ref HitModifiers modifiers) => true;
		/// <summary>Called at the end of the anchor's TileCollide(), after applying normal warhammer tilecollide behavior.</summary>
		public virtual void OnThrowTileCollide(Player player, OrchidGuardian guardian, Projectile projectile, Vector2 oldVelocity) { }
		/// <summary>Called on the first frame of a throw</summary>
		public virtual void OnThrow(Player player, OrchidGuardian guardian, Projectile projectile) { }
		/// <summary>Called at the end of the anchor's Projectile AI()</summary>
		public virtual void ExtraAI(Player player, OrchidGuardian guardian, Projectile projectile, GuardianGreathammerAnchor anchor) { }
		/// <summary>Called before default throw AI. Return false to prevent the default AI from running.</summary>
		/// <remarks>If returning false, use <c>anchor.ReturnFromThrow()</c> to return to held from thrown.</remarks>
		public virtual bool ThrowAI(Player player, OrchidGuardian guardian, Projectile projectile, GuardianGreathammerAnchor anchor) => true;
		/// <summary>Called before drawing the hammer. Return false to prevent the default draw code from running.</summary>
		/// <remarks>The default draw code will use hammerTexture and drawRectangle (which defaults to null)</remarks>
		public virtual bool PreDrawHammer(Player player, OrchidGuardian guardian, Projectile projectile, SpriteBatch spriteBatch, ref Color lightColor, ref Texture2D hammerTexture, ref Rectangle drawRectangle) => true;
		/// <summary>Called after drawing the hammer.</summary>
		public virtual void PostDrawHammer(Player player, OrchidGuardian guardian, Projectile projectile, SpriteBatch spriteBatch, Color lightColor, Texture2D hammerTexture, Rectangle drawRectangle) { }
		/// <summary>Color applied to the hammer's glowmask when drawn. To automatically draw a glowmask for the hammer, add a HammerName_Glow texture file where the hammer texture is located.</summary>
		public virtual Color GetHammerGlowmaskColor(Player player, Projectile projectile, Color lightColor) => Color.White;
		/// <summary>Called before the base Greathammer anchor plays any sound other than the full charge ding. Return false to prevent the sound, if playing your own.</summary>
		/// <remarks>Use <c>projectile.ai[0]</c> to determine the context of the sound. <c>2</c> is slamming, <c>3</c> and <c>4</c> are successful and failed deflects, <c>5</c> is throwing or after being thrown, and <c>-1</c> is catching. <c>justThrown</c> will be flagged for the hammer's initial throw sound.</remarks>
		public virtual bool PreSound(Player player, OrchidGuardian guardian, Projectile projectile, bool justThrown) => true;
		/// <summary>Gets the stun duration for this specific warhammer, taking external modifiers into account (such as reforges).</summary>
		public virtual int GetStunDuration(Player player, OrchidGuardian guardian) => (int)(StunDuration * Item.GetGlobalItem<GuardianPrefixItem>().GetBlockDuration() * guardian.GuardianDeflectStunDuration);
		
		/// <summary>Draws extra UI elements on the GuardianUIState while held.</summary>
		public virtual void WarhammerPostDrawUI(SpriteBatch spriteBatch, Player player, ref Color lightColor, Projectile projectile) { }
		/// <summary>Called at the end of HoldItem if an anchor exists.</summary>
		public virtual void SafeHoldItem(Player player, OrchidGuardian guardian, Projectile anchor) { }

		public override int? AnchorType => ModContent.ProjectileType<GuardianHammerAnchor>();
		public sealed override void SetDefaults()
		{
			Item.DamageType = GetInstance<GuardianDamageClass>();
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = false;
			Item.maxStack = 1;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 10f;
			Item.shootSpeed = 24f;
			Range = 20;
			ThrowHitCooldown = -1;
			GuardStacks = 1;
			SlamStacks = 0;
			GuardCost = 2;
			BashDamage = 0.25f;
			SlamDamage = 1f;
			ThrowDamage = 1.5f;
			DeflectDamage = 1f;
			SlamSpeed = 1f;
			ReturnSpeed = 1f;
			DeflectSpeed = 1f;
			WaitChargeGain = 1f;
			SlamChargeGain = 1f;
			AuraScale = 1f;
			DrawOffset = new(-8, 8);
			HammerFrames = 1;
			StunDuration = 120;

			OrchidGlobalItemPerEntity orchidItem = Item.GetGlobalItem<OrchidGlobalItemPerEntity>();
			orchidItem.guardianWeapon = true;

			SafeSetDefaults();

			Item.useAnimation = Item.useTime;
		}

		public override bool AltFunctionUse(Player player) => true;

		public override bool WeaponPrefix() => true;

		public sealed override void HoldItem(Player player)
		{
			OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
			guardian.GuardianDisplayUI = 300;
			
			Projectile projectile;
			int greathammerID = ProjectileType<GuardianGreathammerAnchor>();
			if (player.ownedProjectileCounts[greathammerID] == 0)
				projectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), player.Center, Vector2.Zero, greathammerID, 0, 0f, player.whoAmI);
			else projectile = Main.projectile.FirstOrDefault(i => i.active && i.owner == player.whoAmI && i.type == greathammerID);
			if (projectile.ModProjectile is GuardianGreathammerAnchor anchor)
			{
				if (anchor.Projectile.ai[0] != 4 && anchor.HammerItem != this)
				{
					anchor.ResetHammerItem();
				}

				SafeHoldItem(player, guardian, projectile);
			}
		}

		public override bool? UseItem(Player player)
		{
			if (!IsLocalPlayer(player)) return false;
			int greathammerID = ProjectileType<GuardianGreathammerAnchor>();
			Projectile projectile = Main.projectile.FirstOrDefault(i => i.active && i.owner == player.whoAmI && i.type == greathammerID);
			if (projectile.ModProjectile is GuardianGreathammerAnchor anchor)
			{
				OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
				bool chargeInput = Main.mouseLeft;
				bool swingInput = Main.mouseRight;
				if (GetInstance<OrchidClientConfig>().GuardianSwapGauntletImputs)
				{
					chargeInput = Main.mouseRight;
					swingInput = Main.mouseLeft;
				}
				if (anchor.IsIdle)
				{
					if (chargeInput)
					{ //do charge
						anchor.ChangeState(1, 5, updateDelay: true);
					}
					else if (swingInput)
					{ //do deflect
						int size = 120;
						Rectangle hitbox = new((int)player.Center.X, (int)player.Center.Y - size / 2, size / 3 * 2, size);
						if (projectile.spriteDirection == -1) hitbox.X -= hitbox.Width;
						if (guardian.UseGuard(GuardCost, true) && guardian.DoDeflect(hitbox, out bool caughtProjectile, out bool killedNPC, this, anchor, lenience: 5f))
						{
							guardian.UseGuard(GuardCost);
							int toAdd = GuardCost;

							// Meteorite armor set. It's a bit unclean, but i can't put this anywhere else (?)
							if (guardian.GuardianMeteorite)
							{
								for (int i = 0; i < GuardCost; i++)
								{
									if (Main.rand.NextBool())
									{
										toAdd++;
									}
								}
							}

							guardian.AddSlam(toAdd);

							anchor.ChangeState(3, 30, updateDelay: true);
							SoundEngine.PlaySound(SoundID.Item52, player.Center);
							guardian.OnAttack(AttackID.GreathammerDeflect);
							if (guardian.DeflectedEnemies.Count() > 0) SoundEngine.PlaySound(SoundID.Item110.WithPitchOffset(-0.3f), player.Center);
							if (GreathammerPostDeflect(player, guardian, projectile, anchor, caughtProjectile, killedNPC))
							{
								foreach (NPC npc in guardian.DeflectedEnemies)
								{
									npc.AddBuff(BuffType<GreathammerStunDebuff>(), GetStunDuration(player, guardian));
								}
							}
						}
						else
						{
							if (!guardian.UseGuard(GuardCost, true)) guardian.WarnMissingResources(GuardCost, 0);
							anchor.ChangeState(4, 30, updateDelay: true);
							guardian.OnAttack(AttackID.GreathammerWeakDeflect);
						}
					}
				}
			}
			return false;
		}

		/// <summary>Handles behavior after a deflect. Projectiles caught have been killed and the player's <c>OrchidGuardian.DeflectedEnemies</c> has been populated. Return true to stun enemies hit by the deflect. Remember if overriding to spawn a projectile for caughtProjectile. Returns true by default.</summary>
		public bool GreathammerPostDeflect(Player player, OrchidGuardian guardian, Projectile projectile, GuardianGreathammerAnchor anchor, bool caughtProjectile, bool killedNPC)
		{
			if (caughtProjectile || killedNPC)
			{
				Vector2 vel = player.Center.DirectionTo(Main.MouseWorld) * 10f;
				Projectile deflectProj = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), player.Center + vel * 4, vel, ProjectileType<GreathammerDeflectedProjectile>(), guardian.GetGuardianDamage(Item.damage), Item.knockBack, player.whoAmI);
				deflectProj.CritChance = guardian.GetGuardianCrit(Item.crit);
				if (PreSound(player, guardian, projectile, false)) SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot, player.Center);

				if (deflectProj.ModProjectile is GreathammerDeflectedProjectile greathammerDeflectedProjectile)
				{
					greathammerDeflectedProjectile.Greathammer = this;
				}
			}
			return true;
		}

		public override bool CanUseItem(Player player)
		{
			int greathammerID = ProjectileType<GuardianGreathammerAnchor>();
			Projectile projectile = Main.projectile.FirstOrDefault(i => i.active && i.owner == player.whoAmI && i.type == greathammerID);
			if (projectile != null && projectile.ModProjectile is GuardianGreathammerAnchor anchor && anchor.IsIdle)
				return true;
			return false;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			var guardian = Main.LocalPlayer.GetModPlayer<OrchidGuardian>();
			TooltipLine tt = tooltips.FirstOrDefault(x => x.Name == "Damage" && x.Mod == "Terraria");
			if (tt != null)
			{
				string[] splitText = tt.Text.Split(' ');
				string damageValue = splitText.First();
				tt.Text = damageValue + " " + Language.GetTextValue(GetInstance<OrchidMod>().GetLocalizationKey("DamageClasses.GuardianDamageClass.DisplayName"));
			}

			int index = tooltips.FindIndex(ttip => ttip.Mod.Equals("Terraria") && ttip.Name.Equals("Knockback"));
			string click = GetInstance<OrchidClientConfig>().GuardianSwapGauntletImputs ? Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.LeftClick") : Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.RightClick");

			tooltips.Insert(index + 1, new TooltipLine(Mod, "Deflect", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.GreathammerDeflect", click)));

			tooltips.Insert(index + 2, new TooltipLine(Mod, "DeflectGrants", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.DeflectGrants", GuardCost))
			{
				OverrideColor = new Color(175, 255, 175)
			});

			tooltips.Insert(index + 3, new TooltipLine(Mod, "Charge", Language.GetTextValue("Mods.OrchidMod.UI.GuardianItem.SlamChargeToThrow", click))
			{
				OverrideColor = new Color(175, 255, 175)
			});

			if (GuardStacks > 0 || SlamStacks > 0)
			{
				string TooltipToGet = GetInstance<OrchidMod>().GetLocalizationKey("Misc.GuardianGrants");
				switch(GuardStacks)
				{
					case > 0: TooltipToGet += "Guard"; break;
				}
				switch (SlamStacks)
				{
					case > 0: TooltipToGet += "Slam"; break;
				}
				if (GuardStacks == SlamStacks) TooltipToGet += "Same";

				tooltips.Insert(index + 1, new TooltipLine(Mod, "GuardianGrants", Language.GetText(TooltipToGet).Format(GuardStacks, SlamStacks))
				{
					OverrideColor = new Color(175, 255, 175)
				});
			}
		}
	}
}