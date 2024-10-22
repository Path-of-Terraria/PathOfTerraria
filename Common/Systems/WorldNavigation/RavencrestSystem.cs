using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Subworlds.RavencrestContent;

public class RavencrestSystem : ModSystem
{
	public bool SpawnedScout = false;
	public Point16 EntrancePosition;

	List<ImprovableStructure> structures = [];

	public override void Load()
	{
		structures.Add(new ImprovableStructure()
		{
			StructurePath = "Assets/Structures/RavencrestBuildings/Forge_",
			StructureIndex = 0,
			Position = new Point()
		});
	}

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
