using PathOfTerraria.Common.Systems.ModPlayers;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Grants experience to the receiving client.
/// </summary>
internal class ExperienceHandler : Handler
{
	public static void Send(byte target, int xpValue)
	{
		ModPacket packet = Networking.GetPacket<ExperienceHandler>();
		packet.Write(xpValue);
		packet.Send(target);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		int xp = reader.ReadInt32();
		Main.LocalPlayer.GetModPlayer<ExpModPlayer>().Exp += xp;
	}
}
