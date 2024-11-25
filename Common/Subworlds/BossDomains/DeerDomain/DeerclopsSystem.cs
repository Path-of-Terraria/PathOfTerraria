using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Subworlds.BossDomains.DeerDomain;

internal class DeerclopsSystem : ModSystem
{
	public Point16 AntlerLocation = Point16.Zero;

	public override void SaveWorldData(TagCompound tag)
	{
		tag.Add("antler", AntlerLocation);
	}

	public override void LoadWorldData(TagCompound tag)
	{
		AntlerLocation = tag.Get<Point16>("antler");
	}
}
