using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

internal class IronBrickUnsafe : ModWall
{
	public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = false;

		DustType = DustID.Iron;
		AddMapEntry(new Color(110, 90, 78));
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}
}

internal class TinBrickUnsafe : ModWall
{
	public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = false;

		DustType = DustID.Iron;
		AddMapEntry(new Color(41, 41, 41));
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}
}

internal class PlatinumBrickUnsafe : ModWall
{
	public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = false;

		DustType = DustID.Iron;
		AddMapEntry(new Color(48, 48, 48));
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}
}

internal class SilverBrickUnsafe : ModWall
{
	public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = false;

		DustType = DustID.Iron;
		AddMapEntry(new Color(46, 56, 59));
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}
}

internal class TinPlatingUnsafe : ModWall
{
	public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = false;
		Main.wallLargeFrames[Type] = 1;

		DustType = DustID.Iron;
		AddMapEntry(new Color(48, 48, 48));
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}
}

internal class LeadBrickUnsafe : ModWall
{
	public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = false;

		DustType = DustID.Lead;
		AddMapEntry(new Color(48, 48, 48));
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}
}

internal class TungstenBrickUnsafe : ModWall
{
	public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = false;

		DustType = DustID.Tungsten;
		AddMapEntry(new Color(48, 48, 48));
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}
}

internal class CopperPlatingUnsafe : ModWall
{
	public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = false;
		Main.wallLargeFrames[Type] = 1;

		DustType = DustID.Copper;
		AddMapEntry(new Color(48, 48, 48));
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}
}
