using PathOfTerraria.Content.Items.Quest;
using PathOfTerraria.Content.Projectiles.Utility;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.Furniture;

internal class EvilBook : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileSolid[Type] = false;
		Main.tileBlockLight[Type] = false;

		TileObjectData.newTile.CopyFrom(TileObjectData.StyleOnTable1x1);
		TileObjectData.newTile.RandomStyleRange = 3;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile | AnchorType.Table 
			| AnchorType.Platform, 1, 0);
		TileObjectData.newTile.AnchorValidTiles = [TileID.MetalBars]; 
		TileObjectData.addTile(Type);

		DustType = DustID.UnusedBrown;

		RegisterItemDrop(ModContent.ItemType<AncientEvilBook>());
	}

	public override void NearbyEffects(int i, int j, bool closer)
	{
		foreach (Projectile proj in Main.ActiveProjectiles)
		{
			if (proj.ModProjectile is SkeletronRitualProj)
			{
				return;
			}
		}

		Tile leftCandle = Main.tile[i - 1, j];
		Tile rightCandle = Main.tile[i + 1, j];
		Tile bar = Main.tile[i, j + 1];

		if (!leftCandle.HasTile || !rightCandle.HasTile || !bar.HasTile)
		{
			return;
		}

		if (!IsValidCandle(leftCandle) || !IsValidCandle(rightCandle) || bar.TileType != TileID.MetalBars || 
			bar.TileFrameX != 144 && bar.TileFrameX != 342)
		{
			return;
		}

		Vector2 worldPos = new Vector2(i, j).ToWorldCoordinates();
		IEntitySource source = new EntitySource_TileInteraction(Main.player[Player.FindClosest(worldPos - new Vector2(8), 16, 16)], i, j);
		Projectile.NewProjectileDirect(source, worldPos, Vector2.Zero, ModContent.ProjectileType<SkeletronRitualProj>(), 0, 0, Main.myPlayer);
		return;

		static bool IsValidCandle(Tile tile)
		{
			return tile.TileType == TileID.Candles || tile.TileType == TileID.PlatinumCandle;
		}
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = 3;
	}

	public override void SetSpriteEffects(int i, int j, ref SpriteEffects effects)
	{
		effects = (i % 2 == 0) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
	}
}