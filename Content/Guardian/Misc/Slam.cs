using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Guardian.Misc
{
	public class Slam : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 16;
			Item.maxStack = 1;
			Item.rare = ItemRarityID.White;
		}

		public override void SetStaticDefaults()
		{
			ItemID.Sets.IsAPickup[Type] = true;
			ItemID.Sets.IgnoresEncumberingStone[Type] = true;
			Item.ResearchUnlockCount = -1;
		}

		public override void PostUpdate()
		{
			Lighting.AddLight(Item.Center, Color.Brown.ToVector3() * 0.4f * Main.essScale);
		}

		public override Color? GetAlpha(Color lightColor)
		{
			return Color.White;
		}

		public override bool CanPickup(Player player)
		{
			OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
			if (guardian.GuardianSlam >= guardian.GuardianSlamMax && Main.netMode == NetmodeID.SinglePlayer) return false; // CBA to sync slam stacks in mp
			return base.CanPickup(player);
		}

		public override bool OnPickup(Player player)
		{
			OrchidGuardian guardian = player.GetModPlayer<OrchidGuardian>();
			guardian.AddSlam(1);
			SoundEngine.PlaySound(SoundID.Item53, player.Center);
			return false;
		}
	}
}
