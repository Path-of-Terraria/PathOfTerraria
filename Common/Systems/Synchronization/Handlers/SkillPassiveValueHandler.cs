using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using System.IO;
using System.Reflection;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Syncs all of one player's passives with everyone else + the server.
/// </summary>
internal class SkillPassiveValueHandler : Handler
{
	public static void Send(string skillName, string nodeName, byte level)
	{
		ModPacket packet = Networking.GetPacket<SkillPassiveValueHandler>();
		packet.Write(skillName);
		packet.Write(nodeName);
		packet.Write(level);
		packet.Send();
	}

	/// <summary>
	/// Forcefully sets the player's strength for the given <paramref name="nodeName"/> on the given <paramref name="treeName"/>.
	/// </summary>
	private static void SetPlayerNodeStrength(byte player, string skillName, string nodeName, byte level)
	{
		Type skillType = typeof(PoTMod).Assembly.GetType(skillName);
		Type nodeType = typeof(PoTMod).Assembly.GetType(nodeName);
		MethodInfo method = typeof(ModContent).GetMethod(nameof(ModContent.GetInstance)).MakeGenericMethod(skillType);
		var skill = (Skill)method.Invoke(null, null);
		Main.player[player].GetModPlayer<SkillTreePlayer>().ModifyPassive(skill.Tree, nodeType, level, false);
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		ModPacket packet = Networking.GetPacket(Id);
		byte target = sender;
		string skillName = reader.ReadString();
		string nodeName = reader.ReadString();
		byte level = reader.ReadByte();

		packet.Write(target);
		packet.Write(skillName);
		packet.Write(nodeName);
		packet.Write(level);
		packet.Send(-1, target);

		SetPlayerNodeStrength(target, skillName, nodeName, level);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		byte who = reader.ReadByte();
		string skillName = reader.ReadString();
		string nodeName = reader.ReadString();
		byte level = reader.ReadByte();

		SetPlayerNodeStrength(who, skillName, nodeName, level);
	}
}
