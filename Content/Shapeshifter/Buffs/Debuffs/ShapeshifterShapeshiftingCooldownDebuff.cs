using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace OrchidMod.Content.Shapeshifter.Buffs.Debuffs
{
	public class ShapeshifterShapeshiftingCooldownDebuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			OrchidShapeshifter shapeshifter = player.Shapeshifter();
			shapeshifter.ShapeshifterMeleeSpeedBonus -= 0.25f;
			shapeshifter.ShapeshifterMoveSpeedBonusFinal *= 0.75f;

			if (player.buffTime[buffIndex] == 1)
			{
				for (int i = 0; i < 5; i++)
				{
					Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Smoke);
					dust.scale *= Main.rand.NextFloat(1f, 1.5f);
					dust.velocity *= Main.rand.NextFloat(0.5f, 0.75f);
				}

				for (int i = 0; i < 2; i++)
				{
					Gore gore = Gore.NewGoreDirect(player.GetSource_FromAI(), player.Center + new Vector2(Main.rand.NextFloat(-24f, 0f), Main.rand.NextFloat(-24f, 0f)), Vector2.UnitY.RotatedByRandom(MathHelper.Pi), 61 + Main.rand.Next(3));
					gore.rotation = Main.rand.NextFloat(MathHelper.Pi);
					gore.velocity += player.velocity * Main.rand.NextFloat(0.5f, 1f);
				}

				SoundEngine.PlaySound(SoundID.LiquidsWaterLava, player.Center); 
			}
		}
	}
}