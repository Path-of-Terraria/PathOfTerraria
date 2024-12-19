using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems;

internal class PersistentDataSystem : ModSystem
{
	public static string CurrentLocation => SubworldSystem.Current is not RavencrestSubworld ? "Overworld" : "Ravencrest";

	public HashSet<string> ObelisksByLocation = ["Ravencrest"];

	public override void ClearWorld()
	{
		ResetLocationData();
	}

	private void ResetLocationData()
	{
		ObelisksByLocation.Clear();
		ObelisksByLocation.Add("Ravencrest");
	}

	public override void SaveWorldData(TagCompound tag)
	{
		tag.Add("hasMainObelisk", ObelisksByLocation.ToArray());
	}

	public override void LoadWorldData(TagCompound tag)
	{
		ObelisksByLocation = new(tag.Get<string[]>("hasMainObelisk"));
	}

	internal void CopyDataToRavencrest()
	{
		SubworldSystem.CopyWorldData("hasMainObelisk", ObelisksByLocation.ToArray());
	}

	internal void ReadDataInRavencrest()
	{
		ObelisksByLocation = new(SubworldSystem.ReadCopiedWorldData<string[]>("hasMainObelisk"));
	}
}
