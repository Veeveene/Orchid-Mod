using Microsoft.Xna.Framework;
using OrchidMod.Content.Guardian.Projectiles.Warhammers;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
	public class DesertWarhammer : OrchidModGuardianHammer
	{

		public override void SafeSetDefaults()
		{
			Item.width = 34;
			Item.height = 36;
			Item.value = Item.sellPrice(0, 0, 30, 0);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.DD2_MonkStaffSwing;
			Item.knockBack = 9f;
			Item.shootSpeed = 12f;
			Item.damage = 72;
			Item.useTime = 26;
			Range = 30;
			GuardStacks = 1;
			SlamStacks = 1;
			ReturnSpeed = 1f;
			SwingChargeGain = 1.5f;
			BlockDuration = -1;
			TileBounce = true;
			Warslammer = 1;
		}

		public override void OnMeleeHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool FullyCharged, bool OffHand)
		{
			BoneBurst(player, guardian, target, projectile, new Vector2(player.direction * 4, 0), 1);
		}

		public override void OnThrowHitFirst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Weak, bool OffHand)
		{
			if (projectile.ModProjectile is GuardianHammerAnchor anchor && anchor.range > -15)
				BoneBurst(player, guardian, target, projectile, projectile.velocity, Weak ? 1 : 3);
		}

		public override void OnThrowTileCollide(Player player, OrchidGuardian guardian, Projectile projectile, Vector2 oldVelocity, bool OffHand)
		{
			if (projectile.ModProjectile is GuardianHammerAnchor anchor)
			{
				anchor.range = -15;
				if (projectile.ai[0] != 1)
				{
					BoneBurst(player, guardian, null, projectile, projectile.velocity, 4);
				}
				projectile.velocity /= 2;
				projectile.damage /= 2;
			}
		}

		public bool JustSlammed = false;

		public override bool BlockAI(Player player, OrchidGuardian guardian, Projectile projectile, GuardianHammerAnchor anchor, ref bool shouldBlock)
		{
			anchor.GrantResources = false;
			projectile.ai[1] = 1;
			projectile.timeLeft = 600;
			anchor.BlockDuration = 0;
			JustSlammed = true;
			return false;
		}

		public override void ExtraAI(Player player, OrchidGuardian guardian, Projectile projectile, bool OffHand)
		{
			if (projectile.ModProjectile is GuardianHammerAnchor anchor && !anchor.GrantResources)
			{ //dumb fix to get the slam to stop clipping into things
				projectile.tileCollide = true;
			}
		}

		public override bool ModifyAttackInfo(ref GuardianAttackInfo info)
		{
			if (JustSlammed)
			{
				JustSlammed = false;
				info = AttackID.ShieldSlam;
			}
			return true;
		}

		void BoneBurst(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, Vector2 hitVelocity, int amount)
		{
			if (target != null) Projectile.perIDStaticNPCImmunity[ModContent.ProjectileType<DesertWarhammerProjectile>()][target.whoAmI] = Main.GameUpdateCount + 40u;
			int type = ModContent.ProjectileType<DesertWarhammerProjectile>();
			for (float i = 3f / amount; i <= 3f; i += 3f / amount)
			{
				Vector2 velocity = 
					hitVelocity * (0.15f * (i + Main.rand.NextFloat(1, 3)))
					* new Vector2(1, projectile.velocity.Y < 0 ? 1 : 0.5f)
					+ Main.rand.NextVector2Circular(0.5f, 0.5f) * amount
					- new Vector2(0, 0.2f * (Main.rand.NextFloat(2, 4) - i));
				
				if (IsLocalPlayer(player))
				{
					Projectile newProj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), projectile.Center, velocity, type, guardian.GetGuardianDamage(Item.damage * 0.25f), 1f, player.whoAmI);
					if (Main.rand.Next(100) < guardian.GetGuardianCrit(Item.crit))
					{
						newProj.ai[0] = 1;
					}
				}
				for (int j = 0; j < 2; j++)
				{
					Dust.NewDust(projectile.position, projectile.width, projectile.height, Main.rand.NextBool() ? DustID.Gold : DustID.Dirt, hitVelocity.X * 0.5f, -1);
				}
			}
			SoundEngine.PlaySound(SoundID.DD2_SkeletonHurt.WithPitchOffset(-0.2f), projectile.Center);
		}

		public override void AddRecipes()
		{
			var recipe = CreateRecipe();
			recipe.AddTile(TileID.Anvils);
			recipe.AddIngredient(ItemID.FossilOre, 15);
			recipe.Register();
		}
	}
}
