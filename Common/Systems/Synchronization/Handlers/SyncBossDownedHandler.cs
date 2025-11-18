using PathOfTerraria.Common.Systems.BossTrackingSystems;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal class SyncBossDownedHandler : Handler
{
	/// <inheritdoc cref="Networking.Message.SyncBossDowned"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out int type);

		ModPacket packet = Networking.GetPacket(Id);
		packet.Write(type);
		packet.Send();
	}
	
	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		int type = reader.ReadInt32();

		ModPacket packet = Networking.GetPacket(Id);
		packet.Write(type);
		packet.Send();

		BossTracker.AddDowned(type, true);
#if DEBUG
		PoTMod.Instance.Logger.Debug("Got BOSS: " + type);
#endif
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		int type = reader.ReadInt32();
		BossTracker.AddDowned(type, true);
	}
}

