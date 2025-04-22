using SubworldLibrary;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Subworlds;

internal class DisableEnemySpawnSystem : GlobalNPC
{
	public override void OnSpawn(NPC npc, IEntitySource source)
	{
		// Stops friendly NPCs from spawning in any mapping domain
		if (npc.isLikeATownNPC && SubworldSystem.Current is MappingWorld and not RavencrestSubworld)
		{
			npc.active = false;
		}
	}
}
