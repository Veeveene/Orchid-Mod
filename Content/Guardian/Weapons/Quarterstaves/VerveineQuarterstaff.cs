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
	public class VerveineQuarterstaff : OrchidModGuardianQuarterstaff
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<VerveineAltQuarterstaff>();
		}

		public override void SafeSetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(0, 0, 85, 0);
			Item.rare = ItemRarityID.Green;
			Item.useTime = 26;
			ParryDuration = 90;
			Item.knockBack = 8f;
			Item.damage = 63;
			CounterSpeed = 1.8f;
			CounterKnockback = 0.25f;
			CounterHits = 0;
		}

		public override void OnAttack(Player player, OrchidGuardian guardian, Projectile projectile, bool jabAttack, bool counterAttack)
		{
			if (counterAttack)
			{
				//"fart"
				//      -Verveine
				Projectile newProjectile = Projectile.NewProjectileDirect(Item.GetSource_FromAI(), player.Center, Vector2.Zero, ModContent.ProjectileType<VerveineFart>(), (int)(Item.damage * 0.75f), Item.knockBack * 0.25f, projectile.owner);
				newProjectile.CritChance = guardian.GetGuardianCrit(Item.crit);
			}
		}
	}

	public class VerveineQuarterstaffTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSpelunker[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileLighted[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.AnchorValidTiles = new int[] { TileID.JungleGrass };
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 18 };
			TileObjectData.addTile(Type);

			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(222, 124, 192), name);

			DustType = DustID.JunglePlants;
			HitSound = SoundID.Grass;
			RegisterItemDrop(ModContent.ItemType<VerveineQuarterstaff>());
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.045f + Main.DiscoR * 0.0002f + Main.DiscoG * 0.0001f;
			g = 0.03f;
			b = 0.06f + Main.DiscoR * 0.0002f + Main.DiscoG * 0.0002f;
		}
		public override void NearbyEffects(int i, int j, bool closer)
		{
			if (!Main.gamePaused && Main.rand.NextBool(3000))
			{
				Tile tile = Main.tile[i, j];
				SoundEngine.PlaySound(SoundID.Item16.WithVolumeScale(0.5f), new Vector2(i * 16, j * 16));
				for (int n = 1; n < Main.rand.Next(2, 4); n++)
				{
					Gore fartCloud = Gore.NewGoreDirect(new EntitySource_TileUpdate(i, j), new Vector2(i * 16 - tile.TileFrameX, j * 16 - tile.TileFrameY), Vector2.UnitY * 1.25f, GoreID.FartCloud1 + Main.rand.Next(3));
					fartCloud.velocity.X *= 0.3f;
					fartCloud.scale *= (n + Main.rand.NextFloat()) * 0.5f;
					fartCloud.alpha = 120;
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
			player.cursorItemIconID = ModContent.ItemType<VerveineQuarterstaff>();
		}
		public override bool RightClick(int i, int j)
		{
			WorldGen.KillTile(i, j);
			return true;
		}
	}

	/*public class FartingPlant : ModItem
	{
		//debug item
		public override string Texture => $"Terraria/Images/Gore_435";
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<VerveineQuarterstaffTile>());
		}
	}*/
}
