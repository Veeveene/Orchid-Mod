using Terraria;
using Terraria.ID;

namespace OrchidMod.Content.Guardian.Weapons.Shields
{
	public class TrashPavise : OrchidModGuardianShield
	{
		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 0, 0, 0);
			Item.width = 38;
			Item.height = 38;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item1;
			Item.knockBack = 16f;
			Item.damage = 54;
			Item.rare = ItemRarityID.Gray;
			Item.useTime = 24;
			distance = 32f;
			slamDistance = 55f;
			blockDuration = 60;
		}

		public override void SlamHit(Player player, Projectile shield, NPC npc, bool WeakSlam)
		{
			npc.AddBuff(BuffID.Stinky, WeakSlam ? 100 : 300);
		}
	}
}
