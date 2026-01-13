namespace PathOfTerraria.Common.Tiles.FramingKinds;

/// <summary>
/// Defines a <see cref="ModTile"/> as a "lily pad" for framing purposes. Also containers helper methods.
/// </summary>
internal interface ILilyPadTile : ILoadable
{
	public class LilyPadFraming : GlobalTile
	{
		public override bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak)
		{
			if (ModContent.GetModTile(type) is ILilyPadTile lily && lily.HasDefaultFraming)
			{
				return DefaultFraming(i, j);
			}

			return true;
		}
	}

	ushort Type { get; }
	bool HasDefaultFraming => true;

	/// <summary>
	/// Places a <see cref="ILilyPadTile"/> tile.
	/// </summary>
	public static void PlacePad<T>(int x, int y, bool overRide) where T : ModTile, ILilyPadTile
	{
		((ILilyPadTile)ModContent.GetInstance<T>()).PlacePad(x, y, overRide);
	}

	public void PlacePad(int x, int y, bool overRide)
	{
		int width = WorldGen.genRand.Next(9, 21);

		for (int i = x - width + 1; i < x + width; ++i)
		{
			Tile tile = Main.tile[i, y];

			if (tile.HasTile && !overRide)
			{
				continue;
			}

			tile.HasTile = true;
			tile.TileType = Type;

			if (i == x)
			{
				for (int j = 1; j < width / 3; ++j)
				{
					Tile stem = Main.tile[i, y + j];
					stem.HasTile = true;
					stem.TileType = Type;
				}
			}
		}

		for (int i = x - width; i < x + width; ++i)
		{
			WorldGen.TileFrame(i, y);

			if (i == x)
			{
				for (int j = 1; j < width / 3; ++j)
				{
					WorldGen.TileFrame(i, y + j);
				}
			}
		}
	}

	public static bool DefaultFraming(int i, int j)
	{
		if (!WorldGen.InWorld(i, j, 30))
		{
			return false;
		}

		Tile tile = Main.tile[i, j];
		bool left = HasTile(i - 1, j);
		bool right = HasTile(i + 1, j);
		bool below = HasTile(i, j + 1);
		bool above = HasTile(i, j - 1);

		if (left && right)
		{
			if (below)
			{
				tile.TileFrameX = 54;
			}
			else
			{
				tile.TileFrameX = 18;
			}
		}
		else if (left && !right)
		{
			tile.TileFrameX = 36;
		}
		else if (!left && right)
		{
			tile.TileFrameX = 0;
		}
		else if (!left && !right)
		{
			if (above && below)
			{
				tile.TileFrameX = 72;
			}
			else if (above)
			{
				tile.TileFrameX = 90;
			}
			else
			{
				WorldGen.KillTile(i, j);
			}
		}
		else
		{
			WorldGen.KillTile(i, j);
		}

		tile.TileFrameY = (short)(Main.rand.Next(3) * 18);

		return false;

		static bool HasTile(int x, int y)
		{
			return Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType];
		}
	}
}
