using PathOfTerraria.Common.Systems.Skills;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Syncs all of one player's skill specializations with everyone else + the server.
/// </summary>
internal class SyncSkillSpecializationHandler : Handler
{
	public static void Send(string skillName, string specName, int toPlayer = -1, int ignorePlayer = -1)
	{
		ModPacket packet = Networking.GetPacket<SyncSkillSpecializationHandler>();
		packet.Write(skillName);
		packet.Write(specName);
		packet.Send(toPlayer, ignorePlayer);
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		ModPacket packet = Networking.GetPacket(Id);
		byte target = sender;
		string skillName = reader.ReadString();
		string specName = reader.ReadString();

		packet.Write(target);
		packet.Write(skillName);
		packet.Write(specName);
		packet.Send(-1, target);

		SetSkillOnPlayer(target, skillName, specName);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
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
