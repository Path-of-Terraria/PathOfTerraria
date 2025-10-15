using PathOfTerraria.Content.Buffs.ElementalBuffs;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <inheritdoc cref="Networking.Message.BleedStack"/>
internal class BleedStackHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.BleedStack;

	/// <inheritdoc cref="Networking.Message.BleedStack"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte player, out short npc, out ushort time, out ushort damage);

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(player);
		packet.Write(npc);
		packet.Write(time);
		packet.Write(damage);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		byte player = reader.ReadByte();
		short npc = reader.ReadInt16();
		ushort time = reader.ReadUInt16();
		ushort damage = reader.ReadUInt16();

		BleedDebuff.Apply(Main.player[player], Main.npc[npc], time, damage);
	}
}
