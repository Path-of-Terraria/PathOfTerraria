using PathOfTerraria.Common.NPCs;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal class PathfindStateChangeHandler : Handler
{
	public static void Send(byte who, bool enable)
	{
		ModPacket packet = Networking.GetPacket<PathfindStateChangeHandler>();
		packet.Write(who);
		packet.Write(enable);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		int who = reader.ReadByte();
		bool enable = reader.ReadBoolean();

		NPC npc = Main.npc[who];

		if (!npc.active || npc.ModNPC is not IPathfindSyncingNPC pathfinder)
		{
			return;
		}

		if (enable)
		{
			pathfinder.EnablePathfinding(sender);
		}
		else
		{
			pathfinder.DisablePathfinding();
		}

		npc.netUpdate = true;
	}
}
