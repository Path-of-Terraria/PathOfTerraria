using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class LihzahrdGuard : ModTile
{
	private static readonly HashSet<int> CanMerge = [];
	
	public override void SetStaticDefaults()
	{
		TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
		TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
		TileID.Sets.DrawsWalls[Type] = true;

		Main.tileSolid[Type] = false;
		Main.tileBlockLight[Type] = false;
		Main.tileMerge[Type][TileID.LihzahrdBrick] = true;
		Main.tileMerge[TileID.LihzahrdBrick][Type] = true;

		CanMerge.Add(Type);
		CanMerge.Add(TileID.LihzahrdBrick);

		AddMapEntry(new Color(141, 56, 0));

		DustType = DustID.Lihzahrd;
	}

	public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
	{
		Tile tile = Main.tile[i, j];
		Tile leftTile = Main.tile[i - 1, j];
		Tile rightTile = Main.tile[i + 1, j];

		bool left = leftTile.HasTile && CanMerge.Contains(leftTile.TileType);
		bool right = rightTile.HasTile && CanMerge.Contains(rightTile.TileType);

		if (left && right)
		{
			tile.TileFrameX = 54;
		}
		else if (left && !right)
		{
			tile.TileFrameX = 36;
		}
		else if (!left && right)
		{
			tile.TileFrameX = 18;
		}
		else
		{
			tile.TileFrameX = 0;
		}

		tile.TileFrameY = (short)(Main.rand.Next(3) * 18);

		return false;
	}
}

public class GuardProjectile : GlobalProjectile
{
	public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
	{
		return entity.type == ProjectileID.SpikyBallTrap;
	}

	public override void AI(Projectile projectile)
	{
		Tile tile = Main.tile[projectile.Center.ToTileCoordinates()];

		if (tile.HasTile && tile.TileType == ModContent.TileType<LihzahrdGuard>())
		{
			projectile.Kill();
		}
	}
}