using System.IO;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal class SpawnNPCOnServerHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.SpawnNPCOnServer;

	/// <inheritdoc cref="Networking.Message.SpawnNPCOnServer"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out short type, out Vector2 position);

		if (TryGetOptionalValue(parameters, 2, out Vector2 velocity))
		{
			ModPacket packet = Networking.GetPacket(MessageType);
			packet.Write((byte)3);
			packet.Write(type);
			packet.WriteVector2(position);
			packet.WriteVector2(velocity);
			packet.Send();
		}
		else 
		{ 
			ModPacket packet = Networking.GetPacket(MessageType);
			packet.Write((byte)2);
			packet.Write(type);
			packet.WriteVector2(position);
			packet.Send();
		}
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		int paramCount = reader.ReadByte();
		short type = reader.ReadInt16();
		Vector2 pos = reader.ReadVector2();
		Vector2 velocity = default;

		if (paramCount > 2)
		{
			velocity = reader.ReadVector2();
		}

		int who = NPC.NewNPC(new EntitySource_SpawnNPC(), (int)pos.X, (int)pos.Y, type, Start: 1);

		if (velocity != default)
		{
			Main.npc[who].velocity = velocity;
			NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, who);
		}
	}
}
