namespace PathOfTerraria.Common.Subworlds.BossDomains;

[Flags]
public enum OpenFlags
{
	None = 0,
	Above,
	Below,
	Left,
	Right,
	UpLeft,
	UpRight,
	DownLeft,
	DownRight
}

public static class OpenExtensions
{
	public static OpenFlags GetOpenings(int i, int j, bool onlyVertical = true, bool noDiagonals = true)
	{
		OpenFlags flags = OpenFlags.None;

		if (!Main.tile[i, j - 1].HasTile)
		{
			flags |= OpenFlags.Above;
		}

		if (!Main.tile[i, j + 1].HasTile)
		{
			flags |= OpenFlags.Below;
		}

		if (onlyVertical)
		{
			return flags;
		}

		if (!Main.tile[i - 1, j].HasTile)
		{
			flags |= OpenFlags.Left;
		}

		if (!Main.tile[i + 1, j].HasTile)
		{
			flags |= OpenFlags.Right;
		}

		if (noDiagonals)
		{
			return flags;
		}

		if (!Main.tile[i + 1, j - 1].HasTile)
		{
			flags |= OpenFlags.UpRight;
		}

		if (!Main.tile[i - 1, j - 1].HasTile)
		{
			flags |= OpenFlags.UpLeft;
		}

		if (!Main.tile[i - 1, j + 1].HasTile)
		{
			flags |= OpenFlags.DownLeft;
		}

		if (!Main.tile[i + 1, j + 1].HasTile)
		{
			flags |= OpenFlags.DownRight;
		}

		return flags;
	}
}