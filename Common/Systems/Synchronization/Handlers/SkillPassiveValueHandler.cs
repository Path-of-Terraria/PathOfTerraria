using PathOfTerraria.Common.Systems.Skills;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Syncs all of one player's passives with everyone else + the server.
/// </summary>
internal class SkillPassiveValueHandler : Handler
{
	public static void Send(string treeName, string nodeName, byte level)
	{
		ModPacket packet = Networking.GetPacket<SkillPassiveValueHandler>();
		packet.Write(treeName);
		packet.Write(nodeName);
		packet.Write(level);
		packet.Send();
	}

	/// <summary>
	/// Forcefully sets the player's strength for the given <paramref name="nodeName"/> on the given <paramref name="treeName"/>.
	/// </summary>
	private static void SetPlayerNodeStrength(byte player, string treeName, string nodeName, byte level)
	{
		Type treeType = typeof(PoTMod).Assembly.GetType(treeName);
		Type nodeType = typeof(PoTMod).Assembly.GetType(nodeName);
		var tree = Activator.CreateInstance(treeType) as SkillTree;
		Main.player[player].GetModPlayer<SkillTreePlayer>().ModifyPassive(SkillTree.TypeToSkillTree[tree.ParentSkill], nodeType, level, false, true);
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		ModPacket packet = Networking.GetPacket(Id);
		byte target = sender;
		string treeName = reader.ReadString();
		string nodeName = reader.ReadString();
		byte level = reader.ReadByte();

		packet.Write(target);
		packet.Write(treeName);
		packet.Write(nodeName);
		packet.Write(level);
		packet.Write(sender);
		packet.Send(-1, target);

		SetPlayerNodeStrength(target, treeName, nodeName, level);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		string treeName = reader.ReadString();
		string nodeName = reader.ReadString();
		byte level = reader.ReadByte();
		byte who = reader.ReadByte();

		SetPlayerNodeStrength(who, treeName, nodeName, level);
	}
}
