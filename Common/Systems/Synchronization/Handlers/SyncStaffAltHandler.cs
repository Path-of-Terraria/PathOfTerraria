using PathOfTerraria.Content.Items.Gear.Weapons.Staff;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal class SyncStaffAltHandler : Handler
{
	/// <inheritdoc cref="Networking.Message.SyncUseStaffAltUse"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte whoAmI);

		ModPacket packet = Networking.GetPacket(Id);
		packet.Write(whoAmI);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		byte index = reader.ReadByte();

		Player player = Main.player[index];
		player.GetModPlayer<StaffPlayer>().EmpoweredStaffTime = Staff.AltActiveTime;
		player.GetModPlayer<AltUsePlayer>().SetAltCooldown(Staff.AltCooldownTime, Staff.AltActiveTime);

		ModPacket packet = Networking.GetPacket(Id);
		packet.Write(index);
		packet.Send(-1, index);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		byte index = reader.ReadByte();

		Player player = Main.player[index];
		player.GetModPlayer<StaffPlayer>().EmpoweredStaffTime = Staff.AltActiveTime;
		player.GetModPlayer<AltUsePlayer>().SetAltCooldown(Staff.AltCooldownTime, Staff.AltActiveTime);
	}
}
