using PathOfTerraria.Content.Buffs.ElementalBuffs;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Adds a stack of Bleed to an NPC. Should only be called from the client.
/// </summary>
internal class BleedStackHandler : Handler
{
	public static void Send(short npc, ushort time, ushort damage)
	{
		ModPacket packet = Networking.GetPacket<BleedStackHandler>(8);
		packet.Write(npc);
		packet.Write(time);
		packet.Write(damage);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		short npc = reader.ReadInt16();
		ushort time = reader.ReadUInt16();
		ushort damage = reader.ReadUInt16();

		BleedDebuff.Apply(Main.player[sender], Main.npc[npc], damage, time);
	}
}
