using PathOfTerraria.Common.Subworlds.RavencrestContent;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Sends a message to the server so that the Morven tile can be spawned and synced properly.
/// </summary>
internal class SpawnMorvenStuckHandler : Handler
{
	public static void Send()
	{
		Networking.GetPacket<SpawnMorvenStuckHandler>().Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		RavencrestSystem.SpawnMorvenStuckInOverworld();
	}
}
