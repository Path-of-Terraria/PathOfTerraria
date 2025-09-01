using Terraria.DataStructures;

namespace PathOfTerraria.Common.NPCs;

/// <summary>
/// Spawns the given NPC at <see cref="TileSpawn"/> in Ravencrest.
/// </summary>
internal interface ISpawnInRavencrestNPC : ILoadable
{
	public Point16 TileSpawn { get; }
	public int Type { get; }

	/// <summary>
	/// Whether this NPC can spawn. Useful for characters that can spawn in Ravencrest, but only under specific conditions.<br/>
	/// By default, only spawns NPCs during worldgen and respawning logic, as long as there is no duplicate NPC.
	/// </summary>
	public bool CanSpawn(NPCSpawnTimeframe timeframe, bool alreadyExists)
	{
		return timeframe is NPCSpawnTimeframe.WorldGen or NPCSpawnTimeframe.NaturalSpawn && !alreadyExists;
	}
}
