namespace PathOfTerraria.Common.NPCs;

internal interface ITavernNPC : ILoadable
{
	public bool ForceSpawnInTavern()
	{
		return false;
	}

	public float SpawnChanceInTavern();
}
