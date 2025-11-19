using PathOfTerraria.Common.Looting.VirtualBagUI;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Enables or disables the player's <see cref="UI.VirtualBagUI.VirtualBagStoragePlayer.UsesVirtualBag"/> for all clients.<br/>Signature:<br/>
/// <c>byte player, bool enabled</c>
/// </summary>
internal class PlayerUseSackOfHoldingHandler : Handler
{
	public static void Send(bool enabled)
	{
		ModPacket packet = Networking.GetPacket<PlayerUseSackOfHoldingHandler>();
		packet.Write(enabled);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		bool enabled = reader.ReadBoolean();

		Main.player[sender].GetModPlayer<VirtualBagStoragePlayer>().UsesVirtualBag = enabled;

		ModPacket packet = Networking.GetPacket(Id);
		packet.Write(sender);
		packet.Write(enabled);
		packet.Send(-1, sender);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		byte player = reader.ReadByte();
		bool enabled = reader.ReadBoolean();

		Main.player[player].GetModPlayer<VirtualBagStoragePlayer>().UsesVirtualBag = enabled;
	}
}
