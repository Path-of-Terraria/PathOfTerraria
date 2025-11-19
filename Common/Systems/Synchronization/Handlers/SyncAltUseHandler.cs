using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Syncs any use of <see cref="AltUsePlayer"/>'s <see cref="AltUsePlayer.SetAltCooldown(int, int)"/>. Should be sent from the client that's running the alt use.
/// </summary>
internal class SyncAltUseHandler : Handler
{
	public static void Send(short cooldown, short activeTime, int toClient = -1, int ignoreClient = -1)
	{
		ModPacket packet = Networking.GetPacket<SyncAltUseHandler>();
		packet.Write(cooldown);
		packet.Write(activeTime);
		packet.Send(toClient, ignoreClient);
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		byte who = sender;
		short cooldown = reader.ReadInt16();
		short activeTime = reader.ReadInt16();

		Player player = Main.player[who];
		player.GetModPlayer<AltUsePlayer>().SetAltCooldown(cooldown, activeTime);

		ModPacket packet = Networking.GetPacket(Id);
		packet.Write(who);
		packet.Write(cooldown);
		packet.Write(activeTime);
		packet.Send();
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		byte index = reader.ReadByte();
		short cooldown = reader.ReadInt16();
		short activeTime = reader.ReadInt16();

		Player player = Main.player[index];
		player.GetModPlayer<AltUsePlayer>().SetAltCooldown(cooldown, activeTime);
	}
}
