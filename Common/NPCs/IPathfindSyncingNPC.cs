namespace PathOfTerraria.Common.NPCs;

internal interface IPathfindSyncingNPC
{
	public void EnablePathfinding(byte followPlayer);
	public void DisablePathfinding();
}
