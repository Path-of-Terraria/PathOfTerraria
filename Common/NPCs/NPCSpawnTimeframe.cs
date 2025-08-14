namespace PathOfTerraria.Common.NPCs;

public enum NPCSpawnTimeframe
{
	/// <summary>
	/// Used only when a world or subworld is first generated.
	/// </summary>
	WorldGen,
	/// <summary>
	/// Used when a world or subworld is entered, not necessarily for the first time.
	/// </summary>
	WorldLoad,
	/// <summary>
	/// Used when a subworld attempts to spawn its missing NPCs.
	/// </summary>
	NaturalSpawn,
}
