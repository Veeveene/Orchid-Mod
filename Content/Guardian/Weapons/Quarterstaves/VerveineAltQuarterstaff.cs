using Microsoft.Xna.Framework;
using OrchidMod.Content.Guardian.Projectiles.Quarterstaves;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Localization;
using Terraria.Audio;
using Terraria.DataStructures;

namespace OrchidMod.Content.Guardian.Weapons.Quarterstaves
{
	public class VerveineAltQuarterstaff : OrchidModGuardianQuarterstaff
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<VerveineQuarterstaff>();
		}

		public override void SafeSetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(0, 0, 85, 0);
			Item.rare = ItemRarityID.Green;
			Item.useTime = 26;
			ParryDuration = 90;
			Item.knockBack = 7f;
			Item.damage = 48;
			CounterSpeed = 1.8f;
			CounterKnockback = 0.25f;
			CounterHits = 0;
			GuardStacks = 1;
			SlamStacks = 1;
		}

		public override void OnAttack(Player player, OrchidGuardian guardian, Projectile projectile, bool jabAttack, bool counterAttack)
		{
			if (counterAttack)
			{
				Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromAI(), player.Center, Vector2.Zero, ModContent.ProjectileType<VerveineFart>(), Item.damage, Item.knockBack * 0.25f, projectile.owner, 1);
				newProjectile.CritChance = guardian.GetGuardianCrit(Item.crit);
			}
		}
	}

	public class VerveineAltQuarterstaffTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSpelunker[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileLighted[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.AnchorValidTiles = new int[] { TileID.MushroomGrass };
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 18 };
			TileObjectData.addTile(Type);

			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(182, 175, 130), name);

			DustType = DustID.GlowingMushroom;
			HitSound = SoundID.Grass;
			RegisterItemDrop(ModContent.ItemType<VerveineAltQuarterstaff>());
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.05f;
			g = 0.05f;
			b = 0.1f + Main.DiscoB * 0.0001f;
		}
		public override void NearbyEffects(int i, int j, bool closer)
		{
			if (!Main.gamePaused && Main.rand.NextBool(3000))
			{
				Tile tile = Main.tile[i, j];
				SoundEngine.PlaySound(SoundID.Item16.WithVolumeScale(0.5f), new Vector2(i * 16, j * 16));
				for (int n = 1; n < Main.rand.Next(2, 4); n++)
				{
					Gore fartCloud = Gore.NewGoreDirect(new EntitySource_TileUpdate(i, j), new Vector2(i * 16 - tile.TileFrameX, j * 16 - tile.TileFrameY), Vector2.UnitY * 1.25f, 375 + Main.rand.Next(3));
					fartCloud.velocity.X *= 0.3f;
					fartCloud.scale *= (n + Main.rand.NextFloat()) * 0.5f;
					fartCloud.rotation += Main.rand.NextFloat(MathHelper.TwoPi);
				}
			}
		}

		public override bool IsTileDangerous(int i, int j, Player player) => true;
		public override bool CanDrop(int i, int j) => true;
		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<VerveineAltQuarterstaff>();
		}
		public override bool RightClick(int i, int j)
		{
			WorldGen.KillTile(i, j);
			return true;
		}
	}
}
