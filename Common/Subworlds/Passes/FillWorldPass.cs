using PathOfTerraria.Common.Systems;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.Passes;

public class FillWorldPass() : GenPass("Terrain", 1)
{
	protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = "Filling " + MappingSystem.Map.GetNameAndTier(); // Sets the text displayed for this pass
		Main.worldSurface = Main.maxTilesY - 42; // Hides the underground layer just out of bounds
		Main.rockLayer = Main.maxTilesY; // Hides the cavern layer way out of bounds

		for (int x = 0; x < Main.maxTilesX; x++)
		{
			progress.Set(x/(float)Main.maxTilesX); // Controls the progress bar, should only be set between 0f and 1f
			for (int y = 0; y < Main.maxTilesY; y++)
			{
				Tile tile = Main.tile[x, y];
				tile.HasTile = true;
				tile.TileType = MappingSystem.Map.GetTileAt(x, y);
			}
		}
	}
}