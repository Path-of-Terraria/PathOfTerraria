using PathOfTerraria.Common.Systems.Skills;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal class SyncSkillSpecializationHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.SyncSkillSpecialization;

	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte player, out string skillName, out string specName);

		ModPacket packet = Networking.GetPacket(MessageType);

		packet.Write(player);
		packet.Write(skillName);
		packet.Write(specName);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		ModPacket packet = Networking.GetPacket(MessageType);
		byte target = reader.ReadByte();
		string skillName = reader.ReadString();
		string specName = reader.ReadString();

		packet.Write(target);
		packet.Write(skillName);
		packet.Write(specName);
		packet.Send(-1, target);

		SetSkillOnPlayer(target, skillName, specName);
	}

	internal override void ClientRecieve(BinaryReader reader)
	{
		byte target = reader.ReadByte();
		string skillName = reader.ReadString();
		string specName = reader.ReadString();

		SetSkillOnPlayer(target, skillName, specName);
	}

	private static void SetSkillOnPlayer(byte target, string skillName, string specName)
	{
		Player player = Main.player[target];
		player.GetModPlayer<SkillTreePlayer>().AddSkillSpecBasedOnTypeNames(specName, skillName, false);
	}
}
