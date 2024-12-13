using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems.Networking.Handlers;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Generation.Tools;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.World.Passes;

public class RavencrestMicrobiome : MicroBiome
{
	public override bool Place(Point origin, StructureMap structures)
	{
		Mod mod = ModContent.GetInstance<PoTMod>();

		var size = new Point16();
		StructureHelper.Generator.GetDimensions("Assets/Structures/RavencrestEntrance", mod, ref size);
		StructureHelper.Generator.GenerateStructure("Assets/Structures/RavencrestEntrance", new Point16(origin.X, origin.Y), mod);
		GenVars.structures.AddProtectedStructure(new Rectangle(origin.X, origin.Y, size.X, size.Y));
		HashSet<Point16> tiles = FitBase((short)origin.X, origin.Y + size.Y - 1, size.X);
		CleanBase(tiles);
		return true;
	}

	/// <summary>
	/// Cleans up the base and adds in grass and decor.
	/// </summary>
	/// <param name="tiles">Tiles to update.</param>
	private static void CleanBase(HashSet<Point16> tiles)
	{
		foreach (Point16 pos in tiles)
		{
			Tile tile = Main.tile[pos];
			OpenFlags flag = OpenExtensions.GetOpenings(pos.X, pos.Y, false, false);

			if (flag != OpenFlags.None && tile.TileType is TileID.Dirt or TileID.Grass && tile.HasTile)
			{
				if (!WorldGen.genRand.NextBool(3) || !tiles.Contains(new(pos.X, pos.Y - 1)))
				{
					tile.WallType = WallID.None;

					if (tile.TileType == TileID.Dirt)
					{
						tile.TileType = TileID.Grass;

						Decoration.OnPurityGrass(new Point16(pos.X, pos.Y - 1));
					}
				}
				else
				{
					tile.HasTile = false;
					tile.WallType = WallID.None;

					Tile top = Main.tile[pos.X, pos.Y - 1];
					top.WallType = WallID.None;

					Tile bottom = Main.tile[pos.X, pos.Y + 1];
					bottom.WallType = WallID.None;

					Tile left = Main.tile[pos.X - 1, pos.Y];
					left.WallType = WallID.None;

					Tile right = Main.tile[pos.X + 1, pos.Y];
					right.WallType = WallID.None;
				}
			}
			else if (flag == OpenFlags.None)
			{
				tile.WallType = WallID.Dirt;
			}

			WorldGen.SquareTileFrame(pos.X, pos.Y, true);
		}
	}

	/// <summary>
	/// Adds ground beneath the structure.
	/// </summary>
	/// <param name="x">Left side of the structure.</param>
	/// <param name="y">Bottom of the structure.</param>
	/// <param name="width">Width of the structure.</param>
	private static HashSet<Point16> FitBase(short x, int y, short width)
	{
		HashSet<Point16> tiles = [];

		for (int i = x - 20; i < x + width + 20; ++i)
		{
			float baseY = y;

			if (i < x)
			{
				baseY += MathF.Pow(x - i, 1.1f);
			}
			else if (i > x + width)
			{
				int increase = i - (x + width);
				baseY += MathF.Pow(increase * 1.5f, 1.1f);
			}

			int newY = (int)baseY;

			while (!WorldGen.SolidTile(i, ++newY) || Main.tile[i, newY].TileType == TileID.Grass)
			{
				Tile tile = Main.tile[i, newY];

				tile.HasTile = true;
				tile.TileType = TileID.Dirt;

				WorldGen.SlopeTile(i, newY, 0, true);
				tiles.Add(new Point16(i, newY));
			}
		}

		return tiles;
	}
}

/// <summary>
/// Originally written for worldgen, this no longer runs during worldgen. Instead, it runs in <see cref="Systems.ModPlayers.ExpModPlayer"/> on first level up.
/// </summary>
internal class RavenPass : AutoGenStep
{
	public override void Generate(GenerationProgress progress, GameConfiguration config)
	{
		int x = Main.maxTilesX / 2 + WorldGen.genRand.Next(50, 80) * (WorldGen.genRand.NextBool() ? -1 : 1);
		int y = (int)(Main.worldSurface * 0.35f);

		// Move the NPC up if it's in tiles, and down if it's not.
		if (Collision.SolidCollision(new Vector2(x, y) * 16, 20, 20)) 
		{
			while (Collision.SolidCollision(new Vector2(x, y) * 16, 20, 20))
			{
				y--;
			}
		}
		else
		{
			while (!Collision.SolidCollision(new Vector2(x, y) * 16, 20, 20))
			{
				y++;
			}
		}

		if (!WorldGen.generatingWorld && Main.netMode == NetmodeID.MultiplayerClient)
		{
			SpawnNPCOnServerHandler.Send((short)ModContent.NPCType<RavenNPC>(), new Vector2(x, y) * 16);
		}
		else
		{
			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), x * 16, y * 16, ModContent.NPCType<RavenNPC>());
		}
	}

	public override int GenIndex(List<GenPass> tasks)
	{
		return -1;
	}
}

internal class RavencrestEntrancePass : AutoGenStep
{
	public override void Generate(GenerationProgress progress, GameConfiguration config)
	{
		progress.Message = GenTitle;

		Point16 size = StructureTools.GetSize("Assets/Structures/RavencrestEntrance");
		Point16 pos = FindPlacement(size);
		new RavencrestMicrobiome().Place(pos.ToPoint(), GenVars.structures);
		ModContent.GetInstance<RavencrestSystem>().EntrancePosition = new Point16(pos.X + size.X / 2, pos.Y + size.Y / 2);
	}

	private static Point16 FindPlacement(Point16 size)
	{
		while (true)
		{
			int x = WorldGen.genRand.Next(150, Main.maxTilesX - 150); // Anywhere in the world, but ocean

			while (Math.Abs(x - Main.maxTilesX / 2) < 300) // Place away from spawn
			{
				x = WorldGen.genRand.Next(150, Main.maxTilesX - 150);
			}

			int y = (int)(Main.worldSurface * 0.35f); // Start at bottom of space,

			while (!WorldGen.SolidTile(x, ++y)) // and dig down.
			{
			}

			// Place only if this overlaps very few tiles and overlaps no structure.
			int tileCount = CountTiles(x, y - size.Y, size.X, size.Y);
			if (tileCount > 8 || !GenVars.structures.CanPlace(new Rectangle(x, y, size.X, size.Y)))
			{
				continue;
			}

			Tile tile = Main.tile[x, y];

			if (tile.TileType != TileID.Dirt) // Place only on dirt.
			{
				continue;
			}

			// Get average height, with a whitelist and blacklist for nearby tiles and preferred average depth of ground.
			int averageHeight = AverageHeights(x, y, 76, 4, 30, out bool valid, [TileID.Cloud, TileID.RainCloud, TileID.Ebonstone, TileID.Crimstone], 
				TileID.Grass, TileID.ClayBlock, TileID.Dirt, TileID.Iron, TileID.Copper, TileID.Lead, TileID.Tin, TileID.Stone);

			// Only place if average height difference is less than 2.
			if (valid && Math.Abs(averageHeight) <= 2)
			{
				return new Point16(x, y - size.Y + Math.Abs(averageHeight));
			}
		}
	}

	private static bool IsSolidGround(int x, int y, Point16 size, int passableNonSolidTileCount)
	{
		for (int i = x; i < x + size.X; i++)
		{
			if (!WorldGen.SolidOrSlopedTile(i, y + size.Y))
			{
				passableNonSolidTileCount--;
			}
		}

		return passableNonSolidTileCount > 0;
	}

	/// <summary>
	/// Counts the number of tiles in an area.
	/// </summary>
	/// <param name="x">Left of the area.</param>
	/// <param name="y">Top of the area.</param>
	/// <param name="width">Width of the area.</param>
	/// <param name="height">Height of the area.</param>
	/// <returns>Number of solid tiles in the area.</returns>
	public static int CountTiles(int x, int y, int width, int height)
	{
		int count = 0;

		for (int i = x; i < x + width; ++i)
		{
			for (int j = y; j < y + height; ++j)
			{
				if (WorldGen.SolidOrSlopedTile(i, j))
				{
					count++;
				}
			}
		}

		return count;
	}

	/// <summary>
	/// Determines flatness & depth placement of an area. Returns average heights; use <paramref name="valid"/> to check if the space is valid.<br/>
	/// This needs a lot of tweaking to get perfect.
	/// </summary>
	/// <param name="x">Left of the area.</param>
	/// <param name="y">Bottom of the area.</param>
	/// <param name="width">Width of the area.</param>
	/// <param name="validSkips">How many times non-<paramref name="allowedIds"/> tiles can be scanned before invalidating the area.</param>
	/// <param name="depth">The desired average depth for the area.</param>
	/// <param name="valid">Whether the area could be valid.</param>
	/// <param name="hardAvoidIds">If a tile of any of these types are scanned, the area is automatically invalid.</param>
	/// <param name="allowedIds">Tiles that do not increment skips when scanned.</param>
	/// <returns>Average height.</returns>
	private static int AverageHeights(int x, int y, int width, int validSkips, int depth, out bool valid, int[] hardAvoidIds, params int[] allowedIds)
	{
		int heights = 0;
		int avgDepth = 0;
		int skips = 0;

		for (int i = x - width / 2; i < x + width / 2; i++)
		{
			int useY = y;

			if (WorldGen.SolidTile(i, useY))
			{
				while (WorldGen.SolidTile(i, --useY))
				{
				}

				useY++;
			}
			else
			{
				while (!WorldGen.SolidTile(i, ++useY))
				{
				}
			}

			int heightDif = useY - y;
			heights += heightDif;

			int digY = useY;

			while (WorldGen.SolidTile(i, ++digY) && digY < useY + depth * 1.1f)
			{
			}

			avgDepth += digY - useY + heightDif;

			if (hardAvoidIds.Contains(Main.tile[i, useY].TileType))
			{
				valid = false;
				return -1;
			}

			if (!allowedIds.Contains(Main.tile[i, useY].TileType) && ++skips > validSkips)
			{
				valid = false;
				return -1;
			}
		}

		int realWidth = width / 2 * 2;
		valid = avgDepth / realWidth > depth;
		return heights / realWidth;
	}

	public override int GenIndex(List<GenPass> tasks)
	{
		return tasks.FindIndex(x => x.Name == "Smooth World") + 1;
	}
}
