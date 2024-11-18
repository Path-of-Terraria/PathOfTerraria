using PathOfTerraria.Common.NPCs;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal static class PathfindStateChangeHandler
{
	public static void Send(byte player, byte who, bool enable)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.PathfindChangeState);

		packet.Write(player);
		packet.Write(who);
		packet.Write(enable);
		packet.Send();
	}

	internal static void ServerRecieve(BinaryReader reader)
	{
		byte player = reader.ReadByte();
		int who = reader.ReadByte();
		bool enable = reader.ReadBoolean();

		NPC npc = Main.npc[who];

		if (!npc.active || npc.ModNPC is not IPathfindSyncingNPC pathfinder)
		{
			return;
		}

		if (enable)
		{
			pathfinder.EnablePathfinding(player);
		}
		else
		{
			pathfinder.DisablePathfinding();
		}

		npc.netUpdate = true;
	}
}
