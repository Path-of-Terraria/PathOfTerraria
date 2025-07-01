using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria.ID;
using Terraria.Utilities;

namespace PathOfTerraria.Common.Subworlds.BossDomains;

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

	public static OpenFlags GetUnsolidAndWallOpenings(int i, int j, bool onlyVertical = true, bool noDiagonals = true)
	{
		OpenFlags flags = OpenFlags.None;

		if (!SolidOrWalled(i, j - 1))
		{
			flags |= OpenFlags.Above;
		}

		if (!SolidOrWalled(i, j + 1))
		{
			flags |= OpenFlags.Below;
		}

		if (onlyVertical)
		{
			return flags;
		}

		if (!SolidOrWalled(i - 1, j))
		{
			flags |= OpenFlags.Left;
		}

		if (!SolidOrWalled(i + 1, j))
		{
			flags |= OpenFlags.Right;
		}

		if (noDiagonals)
		{
			return flags;
		}

		if (!SolidOrWalled(i + 1, j - 1))
		{
			flags |= OpenFlags.UpRight;
		}

		if (!SolidOrWalled(i - 1, j - 1))
		{
			flags |= OpenFlags.UpLeft;
		}

		if (!SolidOrWalled(i - 1, j + 1))
		{
			flags |= OpenFlags.DownLeft;
		}

		if (!SolidOrWalled(i + 1, j + 1))
		{
			flags |= OpenFlags.DownRight;
		}

		return flags;

		static bool SolidOrWalled(int i, int j)
		{
			return WorldGen.SolidTile(i, j) || Main.tile[i, j].WallType > WallID.None;
		}
	}

	public static Point GetDirectionAbsolute(this OpenFlags flag)
	{
		return flag switch
		{
			OpenFlags.Above => new Point(0, -1),
			OpenFlags.Below => new Point(0, 1),
			OpenFlags.Right => new Point(1, 0),
			_ => new Point(-1, 0)
		};
	}

	public static Point GetDirectionRandom(this OpenFlags flags, UnifiedRandom random = null)
	{
		PriorityQueue<OpenFlags, float> directions = new();
		random ??= Main.rand;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void AddIfTrue(OpenFlags flag)
		{
			if (flags.HasFlag(flag))
			{
				directions.Enqueue(flag, random.NextFloat());
			}
		}

		AddIfTrue(OpenFlags.Above);
		AddIfTrue(OpenFlags.Below);
		AddIfTrue(OpenFlags.Left);
		AddIfTrue(OpenFlags.Right);

		return GetDirectionAbsolute(directions.Dequeue());
	}

	public static bool Cardinal(this OpenFlags flags)
	{
		return flags.HasFlag(OpenFlags.Right) || flags.HasFlag(OpenFlags.Above) || flags.HasFlag(OpenFlags.Left) || flags.HasFlag(OpenFlags.Below);
	}
}