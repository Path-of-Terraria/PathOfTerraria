using System.IO;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal static class SpawnNPCOnServerHandler
{
	public static void Send(short type, Vector2 position)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.SpawnNPCOnServer);

		packet.Write((byte)2);
		packet.Write(type);
		packet.WriteVector2(position);
		packet.Send();
	}

	public static void Send(short type, Vector2 position, Vector2 velocity)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.SpawnNPCOnServer);

		packet.Write((byte)3);
		packet.Write(type);
		packet.WriteVector2(position);
		packet.WriteVector2(velocity);
		packet.Send();
	}

	internal static void ServerRecieve(BinaryReader reader)
	{
		int paramCount = reader.ReadByte();
		short type = reader.ReadInt16();
		Vector2 pos = reader.ReadVector2();
		Vector2 velocity = default;

		if (paramCount > 2)
		{
			velocity = reader.ReadVector2();
		}

		int who = NPC.NewNPC(new EntitySource_SpawnNPC(), (int)pos.X, (int)pos.Y, type);

		if (velocity != default)
		{
			Main.npc[who].velocity = velocity;
			PoTMod.Instance.Logger.Debug("GOT: " + velocity);
			NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, who);
		}
	}
}
