using System.IO;
using PathOfTerraria.Common.NPCs;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Spawns an NPC on the server. Note that this spawns all NPCs with a Start value of 1, so worms spawn normally.
/// </summary>
internal class SpawnNPCOnServerHandler : Handler
{
	public static void Send(short type, Vector2 position, Vector2 velocity)
	{
		ModPacket packet = Networking.GetPacket<SpawnNPCOnServerHandler>();
		packet.Write((byte)3);
		packet.Write(type);
		packet.WriteVector2(position);
		packet.WriteVector2(velocity);
		packet.Send();
	}

	public static void Send(short type, Vector2 position)
	{
		ModPacket packet = Networking.GetPacket<SpawnNPCOnServerHandler>();
		packet.Write((byte)2);
		packet.Write(type);
		packet.WriteVector2(position);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		int paramCount = reader.ReadByte();
		short type = reader.ReadInt16();
		Vector2 pos = reader.ReadVector2();
		Vector2 velocity = default;

		if (paramCount > 2)
		{
			velocity = reader.ReadVector2();
		}

		if (ContentSamples.NpcsByNetId[type].ModNPC is ITavernNPC && NPC.AnyNPCs(type))
		{
			return;
		}

		int who = NPC.NewNPC(new EntitySource_SpawnNPC(), (int)pos.X, (int)pos.Y, type, Start: 1);

		if (velocity != default)
		{
			Main.npc[who].velocity = velocity;
			NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, who);
		}
	}
}
