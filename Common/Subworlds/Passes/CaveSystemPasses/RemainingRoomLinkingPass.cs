using System.Collections.Generic;
using System.Linq;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.Passes.CaveSystemPasses;
internal class RemainingRoomLinkingPass() : GenPass("RoomLinking", 1)
{
	const float dotMin = -0.4f;
	const float dotMax = 0.4f;
	protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
	{
		float _dotMin = dotMin;
		float _dotMax = dotMax;

		while (true)
		{
			List<Tuple<float, int>> allowed = [];

			IEnumerable<CaveRoom> activeRooms = CaveSystemWorld.Rooms.Where(r => r.Connections != 0);

			if (activeRooms.Count() == CaveSystemWorld.Rooms.Count)
			{
				return; // all rooms connected to something
			}

			Tuple<float, CaveRoom, CaveRoom> connection = CaveSystemWorld.Rooms.Where(r => r.Connections < 3)
				.Select(r1 => activeRooms.Where(r2 =>
					{
						Vector2 diff = r1.Position - r2.Position;
						float dot = Vector2.Dot(Vector2.Normalize(diff), new Vector2(0, 1));

						return !r1.AllConnections.Contains(r2) && dot > _dotMin && dot < _dotMax && r2.Connections < 3;
					}).Select(r2 => new Tuple<float, CaveRoom, CaveRoom>(r1.Position.Distance(r2.Position), r1, r2))
					.OrderBy(r => r.Item1).FirstOrDefault(new Tuple<float, CaveRoom, CaveRoom>(-1, null, null)))
				.Where(r => r.Item1 > 0)
				.OrderBy(r => r.Item1).FirstOrDefault(new Tuple<float, CaveRoom, CaveRoom>(-1, null, null));

			if (connection.Item1 > 0)
			{
				CaveSystemWorld.AddLine(connection.Item2, connection.Item3);
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