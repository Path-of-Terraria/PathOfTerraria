using PathOfTerraria.Common.Systems.Skills;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal class SkillPassiveValueHandler : Handler
{
	/// <inheritdoc cref="Networking.Message.SkillPassiveValue"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte player, out string treeName, out string nodeName, out byte level);

		ModPacket packet = Networking.GetPacket(Id);

		packet.Write(player);
		packet.Write(treeName);
		packet.Write(nodeName);
		packet.Write(level);
		packet.Send();

		if (TryGetOptionalValue(parameters, 4, out bool runLocally) && runLocally)
		{
			SetPlayerNodeStrength(player, treeName, nodeName, level);
		}
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

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		byte target = reader.ReadByte();
		string treeName = reader.ReadString();
		string nodeName = reader.ReadString();
		byte level = reader.ReadByte();

		SetPlayerNodeStrength(target, treeName, nodeName, level);
	}
}
