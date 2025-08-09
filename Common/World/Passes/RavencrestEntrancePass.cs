using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.World.Generation;
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

		StructureTools.PlaceByOrigin("Assets/Structures/RavencrestEntrance", new Point16(origin.X, origin.Y), Vector2.Zero);
		GenVars.structures.AddProtectedStructure(new Rectangle(origin.X, origin.Y, size.X, size.Y));
		ModContent.GetInstance<RavencrestSystem>().EntrancePosition = new Point16(origin.X + size.X / 2, origin.Y + size.Y / 2 - 8);

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

		int worldW = Main.maxTilesX;
		int spawnX = Main.spawnTileX;

		int preferredRange = Math.Max(400, worldW / 10); 
		int hardRange = Math.Max(1000, worldW / 4); 
		int minFromSpawn = 120; //Not toooooo close

		// Local helper to pick an x biased toward center (spawnX)
		int PickBiasedX(int a, int b, int center)
		{
			int u1 = WorldGen.genRand.Next(a, b);
			int u2 = WorldGen.genRand.Next(a, b);
			int tri = (u1 + u2) / 2; // triangular distribution

			// Soft pull toward center (50%)
			int pulled = tri + (int)((center - tri) * 0.5);

			// Clamp to beach-safe bounds
			return Utils.Clamp(pulled, a, b);
		}

		while (true)
		{
			attempts++;

			int x;
			int y;

			// Expand the search window as attempts increase
			int expand = (attempts / 100) * 200; // +200 tiles half-width every 100 attempts
			int currentRange = Math.Min(hardRange, preferredRange + expand);

			// Clamp the candidate window inside the beaches
			int coastalBuffer = Math.Max(300, worldW / 20); // ~5% of world width, at least 300 tiles
			int left  = Math.Max(WorldGen.beachDistance + coastalBuffer, spawnX - currentRange);
			int right = Math.Min(worldW - WorldGen.beachDistance - coastalBuffer, spawnX + currentRange);

			// If the window collapses (tiny worlds), fall back to the original clamps
			if (left >= right) {
				left  = Math.Max(WorldGen.beachDistance, spawnX - currentRange);
				right = Math.Min(worldW - WorldGen.beachDistance, spawnX + currentRange);
			}

			// Find the first suitable surface tile, biased toward spawn and not too close to it
			while (true)
			{
				do
				{
					x = PickBiasedX(left, right, spawnX);
					y = 21;
				} while (Math.Abs(x - spawnX) < minFromSpawn);
				
				while (!WorldGen.SolidTile(x, y++) && WorldGen.InWorld(x, y, 20)) { }

				Tile tile = Framing.GetTileSafely(x, y);

				if (tile.TileType == TileID.Dirt)
				{
					break;
				}
			}

			// Keep your evil-path avoidance with a failsafe for very long searches
			if (attempts < 400 && !AvoidsEvilLocal((short)x, 200))
			{
				continue;
			}

			y += 4;

			int[] invalidTiles = [TileID.Cloud, TileID.RainCloud, TileID.Ebonstone, TileID.Crimstone];
			int[] validTiles =
			[
				TileID.Grass, TileID.ClayBlock, TileID.Dirt, TileID.Iron, TileID.Copper, TileID.Lead, TileID.Tin,
				TileID.Stone
			];

			int averageHeight = StructureTools.AverageHeights(x, y, 76, 4, 30, out bool valid, invalidTiles, validTiles);
			if (averageHeight == -1 || Math.Abs(averageHeight) >= 4)
			{
				continue;
			}

			Point origin = new(x, y);

			{
				int distFromSpawn = Math.Abs(origin.X - spawnX);
				
				double rejectChance = (distFromSpawn - minFromSpawn) / (double)preferredRange;
				rejectChance = Math.Clamp(rejectChance, 0, 0.9);

				if (rejectChance > 0.8)
				{
					rejectChance = 0.8;
				}

				double easing = 1.0 - (attempts / 600.0); // after ~600 attempts, much more lenient
				if (easing < 0.2)
				{
					easing = 0.2; // never fully disable the bias
				}

				if (easing > 1.0)
				{
					easing = 1.0;
				}

				if ((double)WorldGen.genRand.NextFloat() < rejectChance * easing)
				{
					continue;
				}
			}

			Dictionary<ushort, int> whitelistLookup = [];
			Dictionary<ushort, int> blacklistLookup = [];

			// Check if the terrain is mostly dirt, and if there's no invalid tiles.
			WorldUtils.Gen(origin, new Shapes.Rectangle(size.X, size.Y),
				new Actions.TileScanner(TileID.Dirt).Output(whitelistLookup));
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

			// Check if the upper portion of the structure area is mostly empty.
			for (int i = 0; i < size.X; i++)
			{
				for (int j = 0; j < size.Y; j++)
				{
					Tile tile = Framing.GetTileSafely(origin.X + i, origin.Y + j - size.Y);
					if (!tile.HasTile && tile.LiquidAmount == 0)
					{
						count++;
					}
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

			for (int i = origin.X - 20; i < origin.X + size.X + 20; ++i)
			{
				for (int j = origin.Y; j < origin.Y + size.Y + 40; ++j)
				{
					Tile.SmoothSlope(i, j, true, false);
				}
			}

			break;
		}
	}


	public override int GenIndex(List<GenPass> tasks)
	{
		return tasks.FindIndex(x => x.Name == "Smooth World") + 1;
	}
	
	private static bool AvoidsEvilLocal(short x, int radiusTiles = 200)
	{
		int y = (int)(Main.worldSurface * 0.35f);
		int[] evilTiles = [ TileID.Ebonstone, TileID.Crimstone, TileID.CorruptGrass, TileID.CrimsonGrass, TileID.Crimsand, TileID.Ebonsand ];

		// Walk down to surface at the probe column (to keep consistency with your old method)
		while (WorldGen.InWorld(x, y, 20) && !WorldGen.SolidTile(x, y))
		{
			y++;
		}

		int left = Math.Max(0, x - radiusTiles);
		int right = Math.Min(Main.maxTilesX - 1, x + radiusTiles);

		for (int i = left; i <= right; i += 2) // step 2 for speed
		{
			int sy = y;
			while (WorldGen.InWorld(i, sy, 20) && !WorldGen.SolidTile(i, sy))
			{
				sy++;
			}

			int tileType = Framing.GetTileSafely(i, sy).TileType;
			if (evilTiles.Contains(tileType))
			{
				return false;
			}
		}
		
		return true;
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
			int[] evilTiles =
			[
				TileID.Ebonstone, TileID.Crimstone, TileID.CorruptGrass, TileID.CrimsonGrass, TileID.Crimsand,
				TileID.Ebonsand
			];

			while (!WorldGen.SolidTile(x, y++)) { } //Move down to the nearest surface

			int tileType = Framing.GetTileSafely(x, y).TileType;
			return evilTiles.Contains(tileType);
		}
	}
}