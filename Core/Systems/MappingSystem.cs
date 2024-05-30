
using PathOfTerraria.Content.Items.Consumables.Maps;
using PathOfTerraria.Core.Subworlds;
using SubworldLibrary;
using Terraria.DataStructures;

namespace PathOfTerraria.Core.Systems;
internal class MappingSystem : ModSystem
{
	public static Map Map = null;
	public static int TriesLeft = 10;
	public static bool InMap => Map != null;

	public static void EnterMap(Map map)
	{
		TriesLeft = 10;
		Map = map;
		SubworldSystem.Enter<TestSubworld>();
	}

	// apply map affixes here
}

internal class MappingPlayer : ModPlayer
{
	// apply map affixes here
	public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
	{
		if (!MappingSystem.InMap)
		{
			return;
		}

		MappingSystem.TriesLeft--;
		if (MappingSystem.TriesLeft == 0)
		{
			SubworldSystem.Exit();
			MappingSystem.Map = null;
		}
	}
}

internal class MappingGlobalNPC : GlobalNPC
{
	// apply map affixes here
	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		if (MappingSystem.InMap)
		{
			// maxSpawns = 0; if we onlt want custom spawn-ins...
		}
	}
}