using PathOfTerraria.Common.Systems.Skills;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal class RequestOtherSkillSpecializationHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.RequestOthersSkillSpecialization;

	/// <inheritdoc cref="Networking.Message.RequestOthersSkillSpecialization"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte player);

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(player);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		ModPacket packet = Networking.GetPacket(MessageType);
		byte target = reader.ReadByte();

		packet.Write(target);
		packet.Send(-1, target);
	}

	internal override void ClientRecieve(BinaryReader reader)
	{
		byte target = reader.ReadByte();

		if (Main.myPlayer == target)
		{
			Main.LocalPlayer.GetModPlayer<SkillTreePlayer>().SyncAllSpecializations();
		}
	}
}
