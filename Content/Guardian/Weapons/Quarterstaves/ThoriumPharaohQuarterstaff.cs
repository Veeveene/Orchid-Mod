using Microsoft.Xna.Framework;
using OrchidMod.Content.Guardian.Projectiles.Quarterstaves;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	internal class ThoriumPharaohQuarterstaff : OrchidModGuardianQuarterstaff
	{
		public override void SafeSetDefaults()
		{
			Item.width = 48;
			Item.height = 48;
			Item.value = Item.sellPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.LightRed;
			Item.useTime = 10;
			ParryDuration = 60;
			Item.knockBack = 5f;
			Item.damage = 131;
			Item.shootSpeed = 15;
			JabChargeGain = 2f;
			GuardStacks = 1;
			SwingStyle = 2;
		}

		public override void OnAttack(Player player, OrchidGuardian guardian, Projectile projectile, bool jabAttack, bool counterAttack)
		{
			if (!jabAttack && !counterAttack && IsLocalPlayer(player))
			{ // Charged attack
				float numberProjectiles = 4;
				int projectileType = ModContent.ProjectileType<ThoriumPharaohQuarterstaffProjectile>();
				SoundEngine.PlaySound(SoundID.Item34.WithPitchOffset(Main.rand.NextFloat(0.1f, 0.3f)), projectile.Center);

				for (int i = -4; i < numberProjectiles; i++)
				{
					int damage = guardian.GetGuardianDamage(Item.damage);
					Vector2 velocity = -Vector2.UnitX.RotatedBy((player.Center - Main.MouseWorld).ToRotation() + MathHelper.ToRadians(i * 10)) * Item.shootSpeed;
					Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromAI(), player.Center, velocity, projectileType, damage, Item.knockBack, projectile.owner);
					newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
					newProjectile.ai[1] = Main.rand.Next(2);
				}
			}
		}

		public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (OrchidMod.ThoriumMod != null)
			{
				var recipe = CreateRecipe();
				recipe.AddTile(TileID.MythrilAnvil);
				recipe.AddIngredient(ItemID.AncientBattleArmorMaterial, 2);
				recipe.AddIngredient(thoriumMod, "PharaohsBreath", 8);
				recipe.Register();
			}
		}
	}
}
