namespace PathOfTerraria.Common.Systems.FullMenuPausing;

public class MenuSafeNPC : GlobalNPC
{
	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
		if (MenuSafeSystem.MenuOpen) {
			spawnRate = int.MaxValue;
			maxSpawns = 0;
		}
	}
}