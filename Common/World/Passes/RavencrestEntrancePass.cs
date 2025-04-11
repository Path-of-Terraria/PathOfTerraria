using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Generation.Tools;
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

		Point16 size = StructureHelper.API.Generator.GetStructureDimensions("Assets/Structures/RavencrestEntrance", mod);
		bool hasProtection = !structures.CanPlace(new Rectangle(origin.X, origin.Y, size.X, size.Y));

		if (hasProtection)
		{
			return false;
		}
		
		StructureHelper.API.Generator.GenerateStructure("Assets/Structures/RavencrestEntrance", new Point16(origin.X, origin.Y), mod);
		GenVars.structures.AddProtectedStructure(new Rectangle(origin.X, origin.Y, size.X, size.Y));
		ModContent.GetInstance<RavencrestSystem>().EntrancePosition = new Point16(origin.X + size.X / 2, origin.Y + size.Y / 2);

		return true;
	}
}

internal class RavencrestEntrancePass : AutoGenStep
{
	public override void Generate(GenerationProgress progress, GameConfiguration config)
	{
		progress.Message = GenTitle;

		Mod mod = ModContent.GetInstance<PoTMod>();

		Point16 size = StructureHelper.API.Generator.GetStructureDimensions("Assets/Structures/RavencrestEntrance", mod);
		RavencrestMicrobiome biome = GenVars.configuration.CreateBiome<RavencrestMicrobiome>();
		int attempts = 0;

		while (true)
		{
			attempts++;

			int x = WorldGen.genRand.Next(Main.maxTilesX / 4, Main.maxTilesX / 4 * 3);
			int y = (int)(Main.worldSurface * 0.35f);
			
			// Place the entrance at least 180 tiles away from spawn.
			if (Math.Abs(x - Main.spawnTileX) <= 180)
			{
				continue;
			}
			
			// Find the first suitable surface tile.
			while (true)
			{
				while (!WorldGen.SolidTile(x, y++) && WorldGen.InWorld(x, y, 20)) 
				{ 
				}

				Tile tile = Framing.GetTileSafely(x, y);
				
				if (tile.TileType == TileID.Dirt)
				{
					break;
				}

				do
				{
					x = WorldGen.genRand.Next(WorldGen.beachDistance, Main.maxTilesX - WorldGen.beachDistance);
					y = 0;
				} while (Math.Abs(x - Main.spawnTileX) <= 180);
			}

			if (attempts < 2000 && !AvoidsEvilPath((short)x)) //Include an additional 'attempts' failsafe
			{
				continue;
			}

			int[] invalidTiles = [TileID.Cloud, TileID.RainCloud, TileID.Ebonstone, TileID.Crimstone];
			int[] validTiles = [TileID.Grass, TileID.ClayBlock, TileID.Dirt, TileID.Iron, TileID.Copper, TileID.Lead, TileID.Tin, TileID.Stone];

			// TODO: Should this check for valid/invalid tiles, if it's meant for calculating average heights? - Naka
			int averageHeight = StructureTools.AverageHeights(x, y, 76, 4, 30, out bool valid, invalidTiles, validTiles);

			if (averageHeight == -1 || Math.Abs(averageHeight) >= 4) 
			{
				continue;
			}
			
			Point origin = new(x, y);

			Dictionary<ushort, int> whitelistLookup = [];
			Dictionary<ushort, int> blacklistLookup = [];

			// Check if the terrain is mostly dirt, and if there's no invalid tiles.
			WorldUtils.Gen(origin, new Shapes.Rectangle(size.X, size.Y), new Actions.TileScanner(TileID.Dirt).Output(whitelistLookup));
			WorldUtils.Gen
			(
				origin,
				new Shapes.Rectangle(size.X, size.Y), 
				new Actions.TileScanner
				(
					TileID.Sand,
					TileID.Sandstone,
					TileID.HardenedSand,
					TileID.Ebonsand,
					TileID.Ebonstone,
					TileID.CorruptGrass,
					TileID.Crimsand,
					TileID.Crimstone,
					TileID.CrimsonGrass,
					TileID.SandstoneBrick,
					TileID.IceBlock,
					TileID.SnowBlock,
					TileID.BlueDungeonBrick,
					TileID.GreenDungeonBrick,
					TileID.PinkDungeonBrick,
					TileID.Mud,
					TileID.JungleGrass,
					TileID.LeafBlock,
					TileID.LivingWood
				).Output(blacklistLookup)
			);
			
			int area = size.X * size.Y;

			// Check if the scanned area is full of valid tiles for a solid base, and if there's less than a quarter of invalid tiles.
			if (blacklistLookup.Values.Sum() >= area || whitelistLookup.Values.Sum() <= area / 4)
			{
				continue;
			}

			int count = 0;
			
			// Check if the upper portion of the structure is mostly empty.
			for (int i = 0; i < size.X; i++)
			{
				for (int j = 0; j < size.Y; j++)
				{
					Tile tile = Framing.GetTileSafely(origin.X + i, origin.Y + j - size.Y);

					if (tile.HasTile || tile.LiquidAmount > 0)
					{
						continue;
					}
					
					count++;
				}
			}

			bool empty = count >= area / 2;

			if (!empty)
			{
				continue;
			}

			origin.Y -= size.Y + averageHeight / 2;

			if (!biome.Place(origin, GenVars.structures))
			{
				continue;
			}

			HashSet<Point16> tiles = FitBase((short)origin.X, origin.Y + size.Y - 1, size.X);

			CleanBase(tiles);

			// Fills up small dirt blotches to make the structure naturally blend in, alongside the previously generated base.
			for (int i = 0; i < size.X; i++)
			{
				int strength = WorldGen.genRand.Next(8, 12);
				int steps = WorldGen.genRand.Next(1, 4);

				int type = WorldGen.genRand.NextBool(10) ? TileID.Stone : TileID.Dirt;
				int bottom = origin.Y + size.Y;

				WorldGen.TileRunner(origin.X + i, bottom + strength / 2, strength, steps, TileID.Dirt, true, overRide: false);
			
				WorldGen.TileRunner(origin.X - 5 + i, bottom + strength, strength, steps, type, true);
				WorldGen.TileRunner(origin.X + size.X + 5 - i, bottom + strength, strength, steps, type, true);
			
				WorldGen.TileRunner(origin.X - 10 + i, bottom + strength * 2, strength, steps, type, true);
				WorldGen.TileRunner(origin.X + size.X + 10 - i, bottom + strength * 2, strength, steps, type, true);
			}

			break;
		}
	}
	
	public override int GenIndex(List<GenPass> tasks)
	{
		return tasks.FindIndex(x => x.Name == "Smooth World") + 1;
	}

	/// <summary> Avoids plotting on intercept course with world evil biomes. </summary>
	private static bool AvoidsEvilPath(short x)
	{
		int center = Main.maxTilesX / 2;
		int y = (int)(Main.worldSurface * 0.35f);

		if (x < center)
		{
			for (short i = x; i < center; i++)
			{
				if (UnsafeCoords(i, (short)y))
				{
					return false;
				}
			}
		}
		else
		{
			for (short i = x; i > center; i--)
			{
				if (UnsafeCoords(i, (short)y))
				{
					return false;
				}
			}
		}

		return true;

		static bool UnsafeCoords(short x, short y)
		{
			int[] evilTiles = [TileID.Ebonstone, TileID.Crimstone, TileID.CorruptGrass, TileID.CrimsonGrass, TileID.Crimsand, TileID.Ebonsand];

			while (!WorldGen.SolidTile(x, y++)) { } //Move down to the nearest surface

			int tileType = Framing.GetTileSafely(x, y).TileType;
			return evilTiles.Contains(tileType);
		}
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

			tile.Slope = SlopeType.Solid;
			//Tile.SmoothSlope(pos.X, pos.Y, true);
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
				tiles.Add(new Point16(i, newY + 1));
			}
		}

		return tiles;
	}
}
