using System.IO;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Subworlds.BossDomains.WoFDomain;

internal class ArenaEnemyNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;
	protected override bool CloneNewInstances => true;

	public bool Arena = false;

	public override GlobalNPC Clone(NPC from, NPC to)
	{
		GlobalNPC clone = base.Clone(from, to);
		(clone as ArenaEnemyNPC).Arena = Arena;
		return clone;
	}

	public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
	{
		bitWriter.WriteBit(Arena);
	}

	public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
	{
		Arena = bitReader.ReadBit();
	}
}
