namespace PathOfTerraria.Common.NPCs;

/// <summary>
/// Allows this NPC to spawn in the tavern randomly or guaranteed. Both hooks run on client, and NPCs are synced to the server automatically.
/// </summary>
internal interface ITavernNPC : ILoadable
{
	public string FullName { get; }

	/// <summary>
	/// Whether this NPC will be pseudo-guaranteed to spawn in the tavern. Use this if you want to have precedence over other NPCs spawning randomly.<br/>
	/// This is pseudo-guaranteed as the tavern only has 5 seats - if 5 NPCs already exist, this NPC will not spawn even if it's "forced" to.<br/>
	/// If you want a random chance, use <see cref="SpawnChanceInTavern"/>.
	/// </summary>
	/// <returns>If the NPC must spawn in the tavern.</returns>
	public bool ForceSpawnInTavern()
	{
		return false;
	}

	/// <summary>
	/// Returns the chance of this NPC spawning in the tavern. Runs after <see cref="ForceSpawnInTavern"/>.
	/// </summary>
	/// <returns>The chance of the NPC spawning in the tavern, 0 - 1.</returns>
	public float SpawnChanceInTavern();
}
