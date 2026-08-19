using SubworldLibrary;

namespace PathOfTerraria.Common.Subworlds.RavencrestContent;

/// <summary>
/// Solely used to reduce spawns in Ravencrest.
/// </summary>
internal class RavencrestSpawningModifications : GlobalNPC
{
	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		if (SubworldSystem.Current is RavencrestSubworld)
		{
			spawnRate *= 2;
			maxSpawns = (int)(maxSpawns * 0.6f);
		}
	}
}
