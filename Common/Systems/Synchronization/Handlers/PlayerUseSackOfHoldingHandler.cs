using PathOfTerraria.Common.Looting.VirtualBagUI;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <inheritdoc cref="Networking.Message.PlayerUseSackOfHolding"/>
internal class PlayerUseSackOfHoldingHandler : Handler
{
	/// <inheritdoc cref="Networking.Message.PlayerUseSackOfHolding"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte player, out bool enabled);

		ModPacket packet = Networking.GetPacket(Id);
		packet.Write(player);
		packet.Write(enabled);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		byte player = reader.ReadByte();
		bool enabled = reader.ReadBoolean();

		Main.player[player].GetModPlayer<VirtualBagStoragePlayer>().UsesVirtualBag = enabled;

		ModPacket packet = Networking.GetPacket(Id);
		packet.Write(player);
		packet.Write(enabled);
		packet.Send(-1, player);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		byte player = reader.ReadByte();
		bool enabled = reader.ReadBoolean();

		Main.player[player].GetModPlayer<VirtualBagStoragePlayer>().UsesVirtualBag = enabled;
	}
}
