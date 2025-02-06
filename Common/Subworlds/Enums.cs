﻿namespace PathOfTerraria.Common.Subworlds.BossDomains;

[Flags]
public enum OpenFlags
{
	None = 0,
	Above = 1,
	Below = 2,
	Left = 4,
	Right = 8,
	UpLeft = 16,
	UpRight = 32,
	DownLeft = 64,
	DownRight = 128
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

	public static OpenFlags GetUnsolidOpenings(int i, int j, bool onlyVertical = true, bool noDiagonals = true)
	{
		OpenFlags flags = OpenFlags.None;

		if (!WorldGen.SolidTile(i, j - 1))
		{
			flags |= OpenFlags.Above;
		}

		if (!WorldGen.SolidTile(i, j + 1))
		{
			flags |= OpenFlags.Below;
		}

		if (onlyVertical)
		{
			return flags;
		}

		if (!WorldGen.SolidTile(i - 1, j))
		{
			flags |= OpenFlags.Left;
		}

		if (!WorldGen.SolidTile(i + 1, j))
		{
			flags |= OpenFlags.Right;
		}

		if (noDiagonals)
		{
			return flags;
		}

		if (!WorldGen.SolidTile(i + 1, j - 1))
		{
			flags |= OpenFlags.UpRight;
		}

		if (!WorldGen.SolidTile(i - 1, j - 1))
		{
			flags |= OpenFlags.UpLeft;
		}

		if (!WorldGen.SolidTile(i - 1, j + 1))
		{
			flags |= OpenFlags.DownLeft;
		}

		if (!WorldGen.SolidTile(i + 1, j + 1))
		{
			flags |= OpenFlags.DownRight;
		}

		return flags;
	}
}