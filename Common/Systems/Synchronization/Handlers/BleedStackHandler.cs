using PathOfTerraria.Content.Buffs.ElementalBuffs;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <inheritdoc cref="Networking.Message.BleedStack"/>
internal class BleedStackHandler : Handler
{
	/// <inheritdoc cref="Networking.Message.BleedStack"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte player, out short npc, out ushort time, out ushort damage);

		ModPacket packet = Networking.GetPacket(Id, 8);
		packet.Write(player);
		packet.Write(npc);
		packet.Write(time);
		packet.Write(damage);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		byte player = reader.ReadByte();
		short npc = reader.ReadInt16();
		ushort time = reader.ReadUInt16();
		ushort damage = reader.ReadUInt16();

		BleedDebuff.Apply(Main.player[player], Main.npc[npc], time, damage);
	}
}
