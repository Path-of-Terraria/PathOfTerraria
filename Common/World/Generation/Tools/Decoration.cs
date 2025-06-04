using PathOfTerraria.Common.Subworlds.BossDomains;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.World.Generation.Tools;

internal static class Decoration
{
	internal static void GrowOnJungleGrass(short x, short y, OpenFlags flags)
	{
		if (flags.HasFlag(OpenFlags.Above))
		{
			new CheckChain((int x, int y, ref int? checkType) =>
			{
				if (WorldGen.genRand.NextBool(2))
				{
					return;
				}

				checkType = TileID.PlantDetritus;
				WorldGen.PlaceJunglePlant(x, y, TileID.PlantDetritus, WorldGen.genRand.Next(8), 0);
			}).Chain((int x, int y, ref int? checkType) =>
			{
				if (WorldGen.genRand.NextBool(2))
				{
					return;
				}

				checkType = TileID.PlantDetritus;
				WorldGen.PlaceJunglePlant(x, y, TileID.PlantDetritus, WorldGen.genRand.Next(12), 1);
			}).Chain((int x, int y, ref int? checkType) =>
			{
				checkType = TileID.JunglePlants;
				WorldGen.PlaceTile(x, y, checkType.Value, true, false, style: WorldGen.genRand.Next(24));

				Tile tile = Main.tile[x, y];
				tile.TileFrameX = (short)((WorldGen.genRand.NextBool(5) ? 8 : WorldGen.genRand.Next(24)) * 18);
			}).Run(x, y - 1);
		}

		if (flags.HasFlag(OpenFlags.Below))
		{
			if (!WorldGen.genRand.NextBool(3))
			{
				int length = WorldGen.genRand.Next(5, 12);

				for (int k = 1; k < length; ++k)
				{
					if (Main.tile[x, y + k].HasTile)
					{
						break;
					}

					WorldGen.PlaceTile(x, y + k, TileID.JungleVines, true);
				}
			}
		}
	}

	public static void OnPurityGrass(Point16 position)
	{
		if (!WorldGen.genRand.NextBool(3))
		{
			WorldGen.PlaceTile(position.X, position.Y - 1, TileID.Plants);
		}
		else if (WorldGen.genRand.NextBool(6))
		{
			WorldGen.PlaceTile(position.X, position.Y - 1, TileID.Saplings);

			if (!WorldGen.GrowTree(position.X, position.Y - 1))
			{
				WorldGen.KillTile(position.X, position.Y - 1);
			}
		}
		else if (WorldGen.genRand.NextBool(4))
		{
			WorldGen.PlaceSmallPile(position.X, position.Y - 1, WorldGen.genRand.Next(10), 0);
		}
	}
}
