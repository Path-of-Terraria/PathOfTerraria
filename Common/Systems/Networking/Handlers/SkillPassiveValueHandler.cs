using PathOfTerraria.Common.Systems.Skills;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal static class SkillPassiveValueHandler
{
	public static void Send(byte player, string treeName, string nodeName, byte level, bool runLocally = false)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.SkillPassiveValue);

		packet.Write(player);
		packet.Write(treeName);
		packet.Write(nodeName);
		packet.Write(level);
		packet.Send();

		if (runLocally)
		{
			SetPlayerNodeStrength(player, treeName, nodeName, level);
		}
	}

	private static void SetPlayerNodeStrength(byte player, string treeName, string nodeName, byte level)
	{
		Type treeType = typeof(PoTMod).Assembly.GetType(treeName);
		Type nodeType = typeof(PoTMod).Assembly.GetType(nodeName);
		var tree = Activator.CreateInstance(treeType) as SkillTree;
		Main.player[player].GetModPlayer<SkillTreePlayer>().ModifyPassive(SkillTree.TypeToSkillTree[tree.ParentSkill], nodeType, level, false, true);
	}

	internal static void ServerRecieve(BinaryReader reader)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.SkillPassiveValue);
		byte target = reader.ReadByte();
		string treeName = reader.ReadString();
		string nodeName = reader.ReadString();
		byte level = reader.ReadByte();

		packet.Write(target);
		packet.Write(treeName);
		packet.Write(nodeName);
		packet.Write(level);
		packet.Send(-1, target);

		SetPlayerNodeStrength(target, treeName, nodeName, level);
	}

	internal static void ClientRecieve(BinaryReader reader)
	{
		byte target = reader.ReadByte();
		string treeName = reader.ReadString();
		string nodeName = reader.ReadString();
		byte level = reader.ReadByte();

		SetPlayerNodeStrength(target, treeName, nodeName, level);
	}
}
