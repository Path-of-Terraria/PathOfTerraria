using PathOfTerraria.Common.UI.VirtualBagUI;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <inheritdoc cref="Networking.Message.PlayerUseSackOfHolding"/>
internal class PlayerUseSackOfHoldingHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.PlayerUseSackOfHolding;

	/// <inheritdoc cref="Networking.Message.PlayerUseSackOfHolding"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte player, out bool enabled);

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(player);
		packet.Write(enabled);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		byte player = reader.ReadByte();
		bool enabled = reader.ReadBoolean();

		Main.player[player].GetModPlayer<VirtualBagStoragePlayer>().UsesVirtualBag = enabled;

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(player);
		packet.Write(enabled);
		packet.Send(-1, player);
	}

	internal override void ClientRecieve(BinaryReader reader)
	{
		byte player = reader.ReadByte();
		bool enabled = reader.ReadBoolean();

		Main.player[player].GetModPlayer<VirtualBagStoragePlayer>().UsesVirtualBag = enabled;
	}
}
