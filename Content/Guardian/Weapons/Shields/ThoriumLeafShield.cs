using System;
using Microsoft.Xna.Framework;
using OrchidMod.Common.Attributes;
using OrchidMod.Content.Guardian.Projectiles.Shields;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	[CrossmodContent("ThoriumMod")]
	public class ThoriumLeafShield : OrchidModGuardianShield
	{

		public override void SafeSetDefaults()
		{
			Item.width = 30;
			Item.height = 34;
			Item.rare = ItemRarityID.White;
			Item.value = Item.sellPrice(0, 0, 5, 0);
			Item.knockBack = 2.5f;
			Item.damage = 22;
			Item.useTime = 60;
			distance = 30f;
			slamDistance = 120f;
			blockDuration = 80;
			slamAutoReuse = false;
			shouldFlip = true;
		}

		void LeafBurst(Player player, Projectile shield, int time, int spinTime = 30, float baseSpeed = 0, int quantity = 3, bool fanOut = true)
		{
            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Grass, shield.position);
			float dir = (Main.MouseWorld - player.Center).ToRotation();
			Vector2 spread = new Vector2(0, 1).RotatedBy(dir);
			for (int i = 0; i < quantity; i++)
			{
				float side = (i - ((quantity - 1) / 2f)) * -shield.direction;
				Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromThis(), shield.Center, new Vector2(baseSpeed, 0).RotatedBy(dir) + spread.RotatedByRandom(Main.rand.NextFloat()) * side, ModContent.ProjectileType<ThoriumLeafShieldProj>(), (int)(shield.damage * 0.8f), Item.knockBack, player.whoAmI);
				if (!fanOut) side = 0;
				newProjectile.CritChance = (int)(player.GetCritChance<GuardianDamageClass>() + player.GetCritChance<GenericDamageClass>() + Item.crit);
				newProjectile.position += newProjectile.velocity * 4;
				newProjectile.ai[0] = dir + (side + Main.rand.NextFloat() - 0.5f) * 0.1f;
				newProjectile.ai[1] = spinTime + Math.Abs(side) * 4;
				newProjectile.timeLeft = time + (int)Math.Abs(side) * 2;
                int dust = Dust.NewDust(shield.Center - new Vector2(4, 4), 0, 0, DustID.Grass);
			}
		}

		static void ResetState(Projectile shield)
		{
			GuardianShieldAnchor anchor = shield.ModProjectile as GuardianShieldAnchor;
			shield.friendly = false;
			shield.ai[1] = shield.ai[0] = anchor.isSlamming = 0;
			anchor.NeedNetUpdate = true;
		}

		public override void Protect(Player player, Projectile shield)
		{
			if (IsLocalPlayer(player))
			{
				LeafBurst(player, shield, 45, 50, fanOut: false);
				ResetState(shield);
			}
		}

		public override void SlamHitFirst(Player player, Projectile shield, NPC npc, bool WeakSlam)
		{
			if (IsLocalPlayer(player) && !WeakSlam)
			{
				LeafBurst(player, shield, 62 - (int)((player.Center - shield.Center).Length() / 120), 25, (shield.ai[1] * 0.15f) - 4f);
				ResetState(shield);
			}
		}

		public override void AddRecipes()
		{
			var thoriumMod = OrchidMod.ThoriumMod;
			if (thoriumMod != null)
			{
				var recipe = CreateRecipe();
				recipe.AddTile(TileID.WorkBenches);
				recipe.AddIngredient(thoriumMod, "LivingLeaf", 10);
				recipe.AddIngredient(ItemID.Wood, 10);
				recipe.Register();
			}
		}
	}
}
