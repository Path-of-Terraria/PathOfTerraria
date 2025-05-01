using PathOfTerraria.Common.Systems.MiscUtilities;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Subworlds;

/// <summary>
/// Handles enemies spawned by arenas which require being blocked off before progressing.<br/>
/// Examples include the Queen Bee domain and the Wall of Flesh domain.
/// </summary>
internal class ArenaEnemyNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;
	protected override bool CloneNewInstances => true;

	public byte? SpawnerIndex;
	public bool Arena = false;
	public bool StillDropStuff = false;

	public override GlobalNPC Clone(NPC from, NPC to)
	{
		GlobalNPC clone = base.Clone(from, to);
		(clone as ArenaEnemyNPC).Arena = Arena;
		(clone as ArenaEnemyNPC).StillDropStuff = StillDropStuff;
		return clone;
	}

	public override bool CheckActive(NPC npc)
	{
		const int despawnRange = 16 * 100;

		if (Arena)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient) //Manually despawn arena NPCs
			{
				bool despawn = true;

				foreach (Player player in Main.ActivePlayers)
				{
					if (npc.Distance(player.Center) < despawnRange)
					{
						despawn = false;
						break;
					}
				}

				if (despawn)
				{
					npc.active = false;
					SpawnerSystem.ResetSpawner(npc.whoAmI);
				}
			}

			return false;
		}

		return true;
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
