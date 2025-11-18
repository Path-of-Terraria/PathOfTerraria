using PathOfTerraria.Common.Subworlds.RavencrestContent;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Sends a message to the server so that the Morven tile can be spawned and synced properly.
/// </summary>
internal class SpawnMorvenStuckHandler : Handler
{
	/// <inheritdoc cref="Networking.Message.TellMorvenToSpawn"/>
	public override void Send(params object[] parameters)
	{
		ModPacket packet = Networking.GetPacket(Id);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		RavencrestSystem.SpawnMorvenStuckInOverworld();
	}
}
