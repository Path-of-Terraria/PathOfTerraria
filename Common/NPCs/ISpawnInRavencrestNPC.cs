using Terraria.DataStructures;

namespace PathOfTerraria.Common.NPCs;

internal interface ISpawnInRavencrestNPC : ILoadable
{
	public Point16 TileSpawn { get; }
	public int Type { get; }
}
