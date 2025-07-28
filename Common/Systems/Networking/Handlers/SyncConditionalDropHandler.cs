using PathOfTerraria.Common.NPCs.ConditionalDropping;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal static class SyncConditionalDropHandler
{
	public static void Send(int id, bool add)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.SyncConditionalDrop);
		packet.Write(id);
		packet.Write(add);
		packet.Send();
	}

	internal static void ServerRecieve(BinaryReader reader)
	{
		int id = reader.ReadInt32();
		bool add = reader.ReadBoolean();

		if (add)
		{
			ConditionalDropHandler.AddId(id);
		}
		else
		{
			ConditionalDropHandler.RemoveId(id);
		}
	}
}
