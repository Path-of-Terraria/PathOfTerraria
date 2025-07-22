using Terraria.DataStructures;

namespace PathOfTerraria.Common.NPCs;

/// <summary>
/// Spawns the given NPC at <see cref="TileSpawn"/> when Ravencrest is first generated. Otherwise, does nothing.
/// </summary>
internal interface ISpawnInRavencrestNPC : ILoadable
{
	public Point16 TileSpawn { get; }
	public int Type { get; }

	/// <summary>
	/// Whether this NPC can spawn. Useful for characters that can spawn in Ravencrest, but only under specific conditions.<br/>
	/// By default, only spawns NPCs during worldgen, and not a duplicate just in case.
	/// </summary>
	public bool CanSpawn(bool worldGen, bool alreadyExists)
	{
		return worldGen && !alreadyExists;
	}
}
