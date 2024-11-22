using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Content.Items.Consumables.Maps;
using SubworldLibrary;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems;
internal class MappingSystem : ModSystem
{
	public static Map Map = null;
	public static int TriesLeft = 10;
	public static bool InMap => Map != null;

	private static void EnterMap<T>(Map map) where T : Subworld
	{
		//MobMappingSystem.OpenSubworld();
		TriesLeft = 10; // get from map
		Map = map;
		SubworldSystem.Enter<T>();
	}

	public static void EnterMap(Map map)
	{
		EnterMap<TestSubworld>(map);
	}
	
	public static void EnterCaveMap(CaveMap map)
	{
		CaveSystemWorld.Map = map;
		EnterMap<CaveSystemWorld>(map);
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