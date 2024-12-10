using Terraria.DataStructures;

namespace PathOfTerraria.Common.NPCs;

/// <summary>
/// Spawns the given NPC at <see cref="TileSpawn"/> when Ravencrest is first generated. Otherwise, does nothing.
/// </summary>
internal interface ISpawnInRavencrestNPC : ILoadable
{
	public Point16 TileSpawn { get; }
	public int Type { get; }
}
