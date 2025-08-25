using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal class SyncAltUseHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.SyncAltUse;

	/// <inheritdoc cref="Networking.Message.SyncAltUse"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte whoAmI, out short cooldown, out short activeTime);

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(whoAmI);
		packet.Write(cooldown);
		packet.Write(activeTime);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		byte who = reader.ReadByte();
		short cooldown = reader.ReadInt16();
		short activeTime = reader.ReadInt16();

		Player player = Main.player[who];
		player.GetModPlayer<AltUsePlayer>().SetAltCooldown(cooldown, activeTime);

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(who);
		packet.Write(cooldown);
		packet.Write(activeTime);
		packet.Send();
	}

	internal override void ClientRecieve(BinaryReader reader)
	{
		byte index = reader.ReadByte();
		short cooldown = reader.ReadInt16();
		short activeTime = reader.ReadInt16();

		Player player = Main.player[index];
		player.GetModPlayer<AltUsePlayer>().SetAltCooldown(cooldown, activeTime);
	}
}
