namespace PathOfTerraria.Common.NPCs;

internal interface ITavernNPC : ILoadable
{
	public string FullName { get; }

	public bool ForceSpawnInTavern()
	{
		return false;
	}

	public float SpawnChanceInTavern();
}
