using PathOfTerraria.Content.Items.Gear.Weapons.Staff;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal class SyncStaffAltHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.SyncUseStaffAltUse;

	/// <summary>
	/// Sets a player to be "using" the Staff alt use functionality.
	/// </summary>
	/// <param name="whoAmI">Index of the player.</param>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte whoAmI);

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(whoAmI);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		byte index = reader.ReadByte();

		Player player = Main.player[index];
		player.GetModPlayer<StaffPlayer>().EmpoweredStaffTime = Staff.AltActiveTime;
		player.GetModPlayer<AltUsePlayer>().SetAltCooldown(Staff.AltCooldownTime, Staff.AltActiveTime);

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(index);
		packet.Send(-1, index);
	}

	internal override void ClientRecieve(BinaryReader reader)
	{
		byte index = reader.ReadByte();

		Player player = Main.player[index];
		player.GetModPlayer<StaffPlayer>().EmpoweredStaffTime = Staff.AltActiveTime;
		player.GetModPlayer<AltUsePlayer>().SetAltCooldown(Staff.AltCooldownTime, Staff.AltActiveTime);
	}
}
