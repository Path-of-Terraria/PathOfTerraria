using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems.Networking.Handlers;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Generation.Tools;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
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

		bool hasDimensions = StructureHelper.Generator.GetDimensions("Assets/Structures/RavencrestEntrance", mod, ref size);
		
		if (!hasDimensions)
		{
			// TODO: Maybe this should not throw at all, despite Ravencrest being a core piece of the mod. - Naka
			throw new InvalidOperationException("Could not retrieve structure's dimensions. Path: Assets/Structures/RavencrestEntrance");
		}

		bool hasProtection = !structures.CanPlace(new Rectangle(origin.X, origin.Y, size.X, size.Y));

		if (hasProtection)
		{
			return false;
		}
		
		bool hasGenerated = StructureHelper.Generator.GenerateStructure("Assets/Structures/RavencrestEntrance", new Point16(origin.X, origin.Y), mod);

		if (!hasGenerated)
		{
			// TODO: Maybe this should not throw at all, despite Ravencrest being a core piece of the mod. - Naka
			throw new InvalidOperationException("Could not generate structure. Path: Assets/Structures/RavencrestEntrance");
		}
		
		GenVars.structures.AddProtectedStructure(new Rectangle(origin.X, origin.Y, size.X, size.Y));
		
		return true;
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

		Mod mod = ModContent.GetInstance<PoTMod>();

		var size = new Point16();
		
		bool hasDimensions = StructureHelper.Generator.GetDimensions("Assets/Structures/RavencrestEntrance", mod, ref size);
		
		if (!hasDimensions)
		{
			// TODO: Maybe this should not throw at all, despite Ravencrest being a core piece of the mod. - Naka
			throw new InvalidOperationException("Could not retrieve structure's dimensions. Path: Assets/Structures/RavencrestEntrance");
		}

		RavencrestMicrobiome biome = GenVars.configuration.CreateBiome<RavencrestMicrobiome>();

		bool generated = false;

		while (!generated)
		{
			int x = WorldGen.genRand.Next(WorldGen.beachDistance, Main.maxTilesX - WorldGen.beachDistance);
			int y = (int)(Main.worldSurface * 0.35f);
			
			// Place the entrance at least 150 tiles away from the center of the world.
			if (Math.Abs(x - Main.spawnTileX) <= 150)
			{
				continue;
			}
			
			// Find the first suitable surface tile.
			while (true)
			{
				while (!WorldGen.SolidTile(x, y++)) { }

				Tile tile = Framing.GetTileSafely(x, y);
				
				if (tile.TileType == TileID.Dirt)
				{
					break;
				}

				x = WorldGen.genRand.Next(WorldGen.beachDistance, Main.maxTilesX - WorldGen.beachDistance);
				y = 0;
			}

			int[] invalidTiles = [TileID.Cloud, TileID.RainCloud, TileID.Ebonstone, TileID.Crimstone];
			int[] validTiles = [TileID.Grass, TileID.ClayBlock, TileID.Dirt, TileID.Iron, TileID.Copper, TileID.Lead, TileID.Tin, TileID.Stone];
			
			// TODO: Should this check for valid/invalid tiles, if it's meant for calculating average heights? - Naka
			int averageHeight = StructureTools.AverageHeights(x, y, 76, 4, 30, out bool valid, invalidTiles, validTiles);

			if (averageHeight == -1 || Math.Abs(averageHeight) >= 5) 
			{
				continue;
			}
			
			Point origin = new(x, y);

			Dictionary<ushort, int> whitelistLookup = new();
			Dictionary<ushort, int> blacklistLookup = new();

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
			if (whitelistLookup.Values.Sum() >= area || blacklistLookup.Values.Sum() <= area / 4)
			{
				continue;
			}

			int count = 0;
			
			// Check if the upper portion of the structure is mostly empty.
			for (var i = 0; i < size.X; i++)
			{
				for (var j = 0; j < size.Y; j++)
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
			for (var i = 0; i < size.X; i++)
			{
				int strength = WorldGen.genRand.Next(8, 12);
				int steps = WorldGen.genRand.Next(1, 4);

				int type = WorldGen.genRand.NextBool(10) ? TileID.Stone : TileID.Dirt;

				WorldGen.TileRunner(origin.X + i, origin.Y + size.Y + strength / 2, strength, steps, TileID.Dirt, true, overRide: false);
			
				WorldGen.TileRunner(origin.X - 5 + i, origin.Y + size.Y + strength, strength, steps, type, true);
				WorldGen.TileRunner(origin.X + size.X + 5 - i, origin.Y + size.Y + strength, strength, steps, type, true);
			
				WorldGen.TileRunner(origin.X - 10 + i, origin.Y + size.Y + strength * 2, strength, steps, type, true);
				WorldGen.TileRunner(origin.X + size.X + 10 - i, origin.Y + size.Y + strength * 2, strength, steps, type, true);
			}

			generated = true;

			break;
		}
	}
	
	public override int GenIndex(List<GenPass> tasks)
	{
		return tasks.FindIndex(x => x.Name == "Smooth World") + 1;
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
