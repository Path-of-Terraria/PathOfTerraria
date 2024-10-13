using System.IO;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal static class SpawnNPCOnServerHandler
{
	public static void Send(short type, Vector2 position)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.SpawnNPCOnServer);

		//packet.Write(2);
		packet.Write(type);
		packet.WriteVector2(position);
		packet.Send();
	}

	internal static void ServerRecieve(BinaryReader reader)
	{
		//int paramCount = reader.ReadInt16();
		short type = reader.ReadInt16();
		Vector2 pos = reader.ReadVector2();

		NPC.NewNPC(new EntitySource_SpawnNPC(), (int)pos.X, (int)pos.Y, type);
	}
}
