using System.Linq;
using PathOfTerraria.Common.Systems;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.Passes.CaveSystemPasses;
internal class RoomPositionPass() : GenPass("RoomSpawn", 1)
{
	protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = "Creating rooms for " + MappingSystem.Map.GetNameAndTier();

		float totalStart = CaveSystemWorld.AvailablePositions.Count;
		while (CaveSystemWorld.AvailablePositions.Any())
		{
			progress.Set(1 - CaveSystemWorld.AvailablePositions.Count / totalStart);
			Vector2 pos = Main.rand.Next(CaveSystemWorld.AvailablePositions);

			CaveSystemWorld.AddRoom(pos, Main.rand.Next(CaveSystemWorld.Map.RoomSizeMin, CaveSystemWorld.Map.RoomSizeMax));
		}
	}
}
