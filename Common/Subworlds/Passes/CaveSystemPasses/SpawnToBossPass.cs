using System.Collections.Generic;
using System.Linq;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.Passes.CaveSystemPasses;
internal class SpawnToBossPass() : GenPass("BossAndSpawnRoomLinking", 1)
{
	const float dotMin = -0.4f;
	const float dotMax = 0.4f;
	protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
	{
		int currentRoom = 0; // Spawn

		float _dotMin = dotMin;
		float _dotMax = dotMax;

		while (currentRoom != 1) // Untill boss room
		{
			float startBossDist = Vector2.Distance(CaveSystemWorld.BossRoom.Position, CaveSystemWorld.Rooms[currentRoom].Position);

			List<Tuple<float, int>> allowed = [];

			for (int i = 0; i < CaveSystemWorld.Rooms.Count; i++)
			{
				CaveRoom possibleRoom = CaveSystemWorld.Rooms[i];

				Vector2 diff = CaveSystemWorld.Rooms[currentRoom].Position - possibleRoom.Position;
				float dot = Vector2.Dot(Vector2.Normalize(diff), new Vector2(0, 1));

				float bossDist = Vector2.Distance(CaveSystemWorld.BossRoom.Position, possibleRoom.Position);
				if (bossDist < startBossDist && i != currentRoom && dot > _dotMin && dot < _dotMax)
				{
					allowed.Add(new(Vector2.Distance(CaveSystemWorld.Rooms[currentRoom].Position, possibleRoom.Position), i));
				}
			}

			if (allowed.Any())
			{
				int nextRoom = allowed.OrderBy(r => r.Item1).First().Item2;

				CaveSystemWorld.AddLine(currentRoom, nextRoom);
				currentRoom = nextRoom;

				_dotMin = dotMin;
				_dotMax = dotMax;
			}
			else
			{
				if (_dotMax > 1f && _dotMin < -1f)
				{
					return; // something went wrong and we cant connect anything at all :(
				}

				_dotMin -= 0.1f;
				_dotMax += 0.1f;
			}
		}
	}
}
