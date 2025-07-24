using PathOfTerraria.Common.Systems.Skills;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal static class SyncSkillSpecializationHandler
{
	public static void Send(byte player, string skillName, string specName)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.SyncSkillSpecialization);

		packet.Write(player);
		packet.Write(skillName);
		packet.Write(specName);
		packet.Send();
	}

	internal static void ServerRecieve(BinaryReader reader)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.SyncSkillSpecialization);
		byte target = reader.ReadByte();
		string skillName = reader.ReadString();
		string specName = reader.ReadString();

		packet.Write(target);
		packet.Write(skillName);
		packet.Write(specName);
		packet.Send(-1, target);

		SetSkillOnPlayer(target, skillName, specName);
	}

	internal static void ClientRecieve(BinaryReader reader)
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
