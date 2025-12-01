using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Content.Buffs;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Adds a stack of elemental echoing damage to an NPC. Should only be called from the client.
/// </summary>
internal class ElementalReverberationHandler : Handler
{
	public static void Send(short npc, short delay, int damage, ElementType elementType)
	{
		ModPacket packet = Networking.GetPacket<ElementalReverberationHandler>(8);
		packet.Write(npc);
		packet.Write(delay);
		packet.Write(damage);
		packet.Write((byte)elementType);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		short npc = reader.ReadInt16();
		short delay = reader.ReadInt16();
		int damage = reader.ReadInt32();
		var type = (ElementType)reader.ReadByte();

		ElementalReverberationBuff.Apply(Main.player[sender], Main.npc[npc], delay, damage, type);
	}
}
