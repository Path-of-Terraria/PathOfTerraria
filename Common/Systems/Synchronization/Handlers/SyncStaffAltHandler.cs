using PathOfTerraria.Content.Items.Gear.Weapons.Staff;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal class SyncStaffAltHandler : Handler
{
	public static void Send()
	{
		Networking.GetPacket<SyncStaffAltHandler>().Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		Player player = Main.player[sender];
		player.GetModPlayer<StaffPlayer>().EmpoweredStaffTime = Staff.AltActiveTime;
		player.GetModPlayer<AltUsePlayer>().SetAltCooldown(Staff.AltCooldownTime, Staff.AltActiveTime);

		ModPacket packet = Networking.GetPacket(Id);
		packet.Write(sender);
		packet.Send(-1, sender);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		byte index = reader.ReadByte();

		Player player = Main.player[index];
		player.GetModPlayer<StaffPlayer>().EmpoweredStaffTime = Staff.AltActiveTime;
		player.GetModPlayer<AltUsePlayer>().SetAltCooldown(Staff.AltCooldownTime, Staff.AltActiveTime);
	}
}
