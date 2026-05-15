using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using OrchidMod.Content.Guardian.Weapons.Misc;
using OrchidMod.Content.Guardian.Projectiles.Warhammers;

namespace OrchidMod.Content.Guardian.Weapons.Warhammers
{
	public class ThoriumTerrariumWarhammer : GuardianDoubleHammer
	{

		public static readonly int[] PotentialDusts =
		[
			DustID.GemRuby,
            DustID.InfernoFork,
            DustID.GemTopaz,
            DustID.GemEmerald,
            DustID.Frost,
            DustID.GemSapphire,
            DustID.GemAmethyst
        ];

		public override void SafeSetDefaults()
		{
			Item.width = 44;
			Item.height = 44;
			Item.value = Item.sellPrice(0, 12);
			Item.rare = OrchidMod.ThoriumMod != null ? OrchidMod.ThoriumMod.Find<ModRarity>("TerrariumRarity").Type : ItemRarityID.Expert;
			Item.UseSound = SoundID.Item1;
			Item.knockBack = 5f;
			Item.shootSpeed = 8f;
			Item.damage = 255;
			Item.useTime = 40;
			Range = 64;
			GuardStacks = 2;
			SlamStacks = 2;
			SwingSpeed = 1.5f;
			ReturnSpeed = 0.8f;
			BlockDuration = 270;
		}

		public override void ExtraAI(Player player, OrchidGuardian guardian, Projectile anchor, bool offHandHammer)
		{
			if (Main.rand.NextBool(6))
                Dust.NewDustDirect(anchor.position, anchor.width, anchor.height, PotentialDusts[Main.rand.Next(7)]);
		}

		// public override void OnMeleeHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Charged, bool firstHit) 
		// {
		// 	var thoriumMod = OrchidMod.ThoriumMod;
		// 	if (thoriumMod != null)
		// 	{
		// 		int debuffType = thoriumMod.Find<ModBuff>("TerrariumBacklash").Type;
		// 		target.AddBuff(debuffType, firstHit ? 180 : 120);
				
		// 		// if (!Weak) {
		// 			Vector2 point = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * 120f;
		// 			Projectile echoProj = Projectile.NewProjectileDirect(projectile.GetSource_FromAI(), target.Center + point, Vector2.Normalize(point) * -10f, ModContent.ProjectileType<ThoriumTerrariumWarhammerProjectile>(), guardian.GetGuardianDamage(Item.damage * 0.4f), 6f, Main.myPlayer, target.whoAmI);
		// 		// }
		// 	}
		// }

		// public override void OnThrowHit(Player player, OrchidGuardian guardian, NPC target, Projectile projectile, float knockback, bool crit, bool Charged, bool firstHit) 
		// {
		// 	var thoriumMod = OrchidMod.ThoriumMod;
		// 	if (thoriumMod != null)
		// 	{
		// 		int debuffType = thoriumMod.Find<ModBuff>("TerrariumBacklash").Type;
		// 		target.AddBuff(debuffType, firstHit ? 180 : 120);
		// 		if (Charged)
		// 			for (int i = 0; i < 3; i++)
		// 			{
		// 				Vector2 point = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * 120f;
		// 				Projectile echoProj = Projectile.NewProjectileDirect(projectile.GetSource_FromAI(), target.Center + point, Vector2.Normalize(point) * -10f, ModContent.ProjectileType<ThoriumTerrariumWarhammerProjectile>(), guardian.GetGuardianDamage(Item.damage * 0.4f), 6f, Main.myPlayer, target.whoAmI);
		// 			}
		// 	}
		// }
	}
}
