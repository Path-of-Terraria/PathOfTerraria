using PathOfTerraria.Common.Systems.BossTrackingSystems;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Marks a boss as downed. This is used for sending boss downs to the main server through <see cref="Networking.SendPacketToMainServer(ModPacket, string)"/>.
/// </summary>
internal class SyncBossDownedHandler : Handler
{
	public static void Send(int type, int toClient = -1, int ignoreClient = -1)
	{
		ModPacket packet = Networking.GetPacket<SyncBossDownedHandler>();
		packet.Write(type);
		packet.Send(toClient, ignoreClient);
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

