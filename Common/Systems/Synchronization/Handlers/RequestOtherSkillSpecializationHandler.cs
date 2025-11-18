using PathOfTerraria.Common.Systems.Skills;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal class RequestOtherSkillSpecializationHandler : Handler
{
	/// <inheritdoc cref="Networking.Message.RequestOthersSkillSpecialization"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte player);

		ModPacket packet = Networking.GetPacket(Id);
		packet.Write(player);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		ModPacket packet = Networking.GetPacket(Id);
		byte target = reader.ReadByte();

		packet.Write(target);
		packet.Send(-1, target);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		byte target = reader.ReadByte();

		if (Main.myPlayer == target)
		{
			Main.LocalPlayer.GetModPlayer<SkillTreePlayer>().SyncAllSpecializations();
		}
	}
}
