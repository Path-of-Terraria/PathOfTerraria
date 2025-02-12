using System.IO;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.WoFDomain;

internal class ArenaEnemyNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;
	protected override bool CloneNewInstances => true;

	public bool Arena = false;
	public bool StillDropStuff = false;

	public override GlobalNPC Clone(NPC from, NPC to)
	{
		GlobalNPC clone = base.Clone(from, to);
		(clone as ArenaEnemyNPC).Arena = Arena;
		return clone;
	}

	public override bool CheckActive(NPC npc)
	{
		return !Arena;
	}

	public override void Load()
	{
		On_NPC.NPCLoot_DropItems += StopArenaNPCsFromDroppingLoot;
	}

	private void StopArenaNPCsFromDroppingLoot(On_NPC.orig_NPCLoot_DropItems orig, NPC self, Player closestPlayer)
	{
		ArenaEnemyNPC arena = self.GetGlobalNPC<ArenaEnemyNPC>();

		if (arena.Arena && !arena.StillDropStuff)
		{
			return;
		}

		orig(self, closestPlayer);
	}

	public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
	{
		bitWriter.WriteBit(Arena);
		bitWriter.WriteBit(StillDropStuff);
	}

	public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
	{
		Arena = bitReader.ReadBit();
		StillDropStuff = bitReader.ReadBit();
	}
}
