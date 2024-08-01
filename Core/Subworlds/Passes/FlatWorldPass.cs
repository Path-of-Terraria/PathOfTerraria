using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Core.Subworlds.Passes;

public class FlatWorldPass(int floorY = 500, bool spawnWalls = false) : GenPass("Terrain", 1)
{
	public readonly int FloorY = floorY;
	public readonly bool SpawnWalls = spawnWalls;

	protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = "ENTERING TIER X MAP"; // Sets the text displayed for this pass
		Main.worldSurface = Main.maxTilesY - 42; // Hides the underground layer just out of bounds
		Main.rockLayer = Main.maxTilesY; // Hides the cavern layer way out of bounds
		for (int x = 0; x < Main.maxTilesX; x++)
		{
			for (int y = 0; y < Main.maxTilesY; y++)
			{
				progress.Set((y + x * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY)); // Controls the progress bar, should only be set between 0f and 1f
				Tile tile = Main.tile[x, y];
				if (y <= FloorY)
				{
					continue; //We don't want any tiles below y = 500
				}

				tile.HasTile = true;
				tile.TileType = TileID.Stone;

				if (SpawnWalls)
				{
					tile.WallType = WallID.Stone;
				}
			}
		}
	}
}