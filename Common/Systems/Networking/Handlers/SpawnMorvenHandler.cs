using PathOfTerraria.Common.Subworlds.RavencrestContent;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal class SpawnMorvenHandler : Handler
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
