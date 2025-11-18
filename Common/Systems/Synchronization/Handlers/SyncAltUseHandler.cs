using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal class SyncAltUseHandler : Handler
{
	/// <inheritdoc cref="Networking.Message.SyncAltUse"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte whoAmI, out short cooldown, out short activeTime);

		ModPacket packet = Networking.GetPacket(Id);
		packet.Write(whoAmI);
		packet.Write(cooldown);
		packet.Write(activeTime);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		byte who = reader.ReadByte();
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
