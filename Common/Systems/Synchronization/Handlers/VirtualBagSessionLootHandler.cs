using PathOfTerraria.Common.Looting.VirtualBagUI;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal sealed class VirtualBagSessionLootHandler : Handler
{
	public static void SendConfluxResource(byte target, int itemType, int amount)
	{
		ModPacket packet = Networking.GetPacket<VirtualBagSessionLootHandler>();
		packet.Write7BitEncodedInt(itemType);
		packet.Write7BitEncodedInt(amount);
		packet.Send(target);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		int itemType = reader.Read7BitEncodedInt();
		int amount = reader.Read7BitEncodedInt();

		Main.LocalPlayer.GetModPlayer<VirtualBagStoragePlayer>().RecordConfluxResource(itemType, amount);
	}
}
