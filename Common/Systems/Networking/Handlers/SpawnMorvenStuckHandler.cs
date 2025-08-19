using PathOfTerraria.Common.Subworlds.RavencrestContent;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

/// <summary>
/// Sends a message to the server so that the Morven tile can be spawned and synced properly.
/// </summary>
internal class SpawnMorvenStuckHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.TellMorvenToSpawn;

	/// <inheritdoc cref="Networking.Message.TellMorvenToSpawn"/>
	public override void Send(params object[] parameters)
	{
		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		RavencrestSystem.SpawnMorvenStuckInOverworld();
	}
}
