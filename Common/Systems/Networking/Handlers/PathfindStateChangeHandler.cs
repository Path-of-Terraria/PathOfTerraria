using PathOfTerraria.Common.NPCs;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal class PathfindStateChangeHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.PathfindChangeState;

	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte player, out byte who, out bool enable);

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(player);
		packet.Write(who);
		packet.Write(enable);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
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
