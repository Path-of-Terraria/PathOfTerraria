using PathOfTerraria.Common.Systems.BossTrackingSystems;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

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
	}

	internal override void ClientRecieve(BinaryReader reader)
	{
		BossTracker.AddDowned(reader.ReadInt32(), true);
	}
}

internal class SyncPlayerBossDownedHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.SyncPlayerDownedBoss;

	/// <inheritdoc cref="Networking.Message.SyncPlayerDownedBoss"/>
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
	}

	internal override void ClientRecieve(BinaryReader reader)
	{
		Main.LocalPlayer.GetModPlayer<BossTrackingPlayer>().CachedBossesDowned.Add(reader.ReadInt32());
	}
}
