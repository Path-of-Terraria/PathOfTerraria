using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.WorldNavigation;

public class RavencrestSystem : ModSystem
{
	public bool SpawnedScout = false;
	public Point16 EntrancePosition;

	public override void PostUpdateEverything()
	{
		if (SubworldSystem.Current is not RavencrestSubworld)
		{
			SpawnedScout = false;
		}
	}

	public override void LoadWorldData(TagCompound tag)
	{
		EntrancePosition = tag.Get<Point16>("entrance");
	}

	public override void SaveWorldData(TagCompound tag)
	{
		tag.Add("entrance", EntrancePosition);
	}
}
