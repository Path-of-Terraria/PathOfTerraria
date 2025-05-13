using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal static class SyncAltUseHandler
{
	/// <summary>
	/// Sets a player to be "using" the Staff alt use functionality.
	/// </summary>
	/// <param name="whoAmI">Index of the player.</param>
	public static void Send(byte whoAmI, short cooldown, short activeTime)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.SyncAltUse);
		packet.Write(whoAmI);
		packet.Write(cooldown);
		packet.Write(activeTime);
		packet.Send();
	}

	internal static void ServerRecieve(BinaryReader reader)
	{
		byte who = reader.ReadByte();
		short cooldown = reader.ReadInt16();
		short activeTime = reader.ReadInt16();

		Player player = Main.player[who];
		player.GetModPlayer<AltUsePlayer>().SetAltCooldown(cooldown, activeTime);

		ModPacket packet = Networking.GetPacket(Networking.Message.SyncAltUse);
		packet.Write(who);
		packet.Write(cooldown);
		packet.Write(activeTime);
		packet.Send();
	}

	internal static void ClientRecieve(BinaryReader reader)
	{
		byte index = reader.ReadByte();
		short cooldown = reader.ReadInt16();
		short activeTime = reader.ReadInt16();

		Player player = Main.player[index];
		player.GetModPlayer<AltUsePlayer>().SetAltCooldown(cooldown, activeTime);
	}
}
