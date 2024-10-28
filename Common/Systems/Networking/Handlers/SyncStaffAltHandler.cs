using PathOfTerraria.Content.Items.Gear.Weapons.Staff;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal static class SyncStaffAltHandler
{
	/// <summary>
	/// Sets a player to be "using" the Staff alt use functionality.
	/// </summary>
	/// <param name="whoAmI">Index of the player.</param>
	public static void Send(byte whoAmI)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.SyncUseStaffAltUse);
		packet.Write(whoAmI);
		packet.Send();
	}

	internal static void ServerRecieve(BinaryReader reader)
	{
		byte index = reader.ReadByte();

		Player player = Main.player[index];
		player.GetModPlayer<StaffPlayer>().EmpoweredStaffTime = Staff.AltActiveTime;
		player.GetModPlayer<AltUsePlayer>().SetAltCooldown(Staff.AltCooldownTime, Staff.AltActiveTime);

		ModPacket packet = Networking.GetPacket(Networking.Message.SyncUseStaffAltUse);
		packet.Write(index);
		packet.Send(-1, index);
	}

	internal static void ClientRecieve(BinaryReader reader)
	{
		byte index = reader.ReadByte();

		Player player = Main.player[index];
		player.GetModPlayer<StaffPlayer>().EmpoweredStaffTime = Staff.AltActiveTime;
		player.GetModPlayer<AltUsePlayer>().SetAltCooldown(Staff.AltCooldownTime, Staff.AltActiveTime);
	}
}
