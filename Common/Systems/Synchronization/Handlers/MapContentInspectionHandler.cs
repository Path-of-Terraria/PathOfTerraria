#if DEBUG
using System.IO;
using PathOfTerraria.Common.Debugging;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Requests a server-authoritative Map Content Inspector snapshot and returns it to the requesting client.
/// </summary>
internal sealed class MapContentInspectionHandler : Handler
{
	internal static void Request()
	{
		if (Main.netMode == NetmodeID.SinglePlayer)
		{
			MapContentInspection.LatestSnapshot = MapContentInspection.Capture(Main.myPlayer, "Single player");
			return;
		}

		Networking.GetPacket<MapContentInspectionHandler>().Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		if (sender >= Main.maxPlayers || !Main.player[sender].active || !MapContentInspection.IsInExplorationMap)
		{
			return;
		}

		MapInspectionSnapshot snapshot = MapContentInspection.Capture(sender, "Server");
		ModPacket packet = Networking.GetPacket(Id);
		snapshot.Write(packet);
		packet.Send(sender);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		MapContentInspection.LatestSnapshot = MapInspectionSnapshot.Read(reader);
	}
}
#endif
