using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.Passes.CaveSystemPasses;
internal class SpawnMobsPass() : GenPass("MobSpawning", 1)
{
	protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
	{
		for (int i = 2; i < CaveSystemWorld.Rooms.Count; i++)
		{
			CaveRoom room = CaveSystemWorld.Rooms[i];

			NPC n = NPC.NewNPCDirect(null, room.Position * 16f, NPCID.GreenSlime);
		}

		foreach (Tuple<Vector2, Vector2> line in CaveSystemWorld.Lines)
		{
			Vector2 diff = line.Item2 - line.Item1;
			Vector2 dir = Vector2.Normalize(diff);

			int npcsToSpawn = 10;

			for (int i = 0; i < npcsToSpawn; i++)
			{
				//MobMappingSystem.MakeNPC((line.Item2 + dir * Main.rand.NextFloat() * diff.Length()) * 16f, NPCID.GreenSlime);
			}
		}

		//MobMappingSystem.MakeNPC(CaveSystemWorld.BossRoom.Position * 16f, NPCID.KingSlime);

		// might want to save the npcs in the future for things, idk...
		// espetially the boss; as killing the boss means clearing the map

		// we save it in MobMappingSystem, so make it work with that, ig.
	}
}
