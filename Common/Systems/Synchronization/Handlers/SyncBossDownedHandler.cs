using PathOfTerraria.Common.Systems.BossTrackingSystems;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal class SyncBossDownedHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.SyncBossDowned;

	/// <inheritdoc cref="Networking.Message.SyncBossDowned"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out int type);

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(type);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		int type = reader.ReadInt32();

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(type);
		packet.Send();

		BossTracker.AddDowned(type, true);
#if DEBUG
		PoTMod.Instance.Logger.Debug("Got BOSS: " + type);
#endif
	}

	internal override void ClientRecieve(BinaryReader reader)
	{
		int type = reader.ReadInt32();
		BossTracker.AddDowned(type, true);
	}
}

