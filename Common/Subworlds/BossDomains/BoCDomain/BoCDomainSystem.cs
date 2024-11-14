using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Subworlds.BossDomains.BoCDomain;

internal class BoCDomainSystem : ModSystem
{
	public bool HasLloyd = true;

	public override void SaveWorldData(TagCompound tag)
	{
		if (HasLloyd)
		{
			tag.Add("hasLloyd", HasLloyd);
		}
	}

	public override void LoadWorldData(TagCompound tag)
	{
		HasLloyd = tag.ContainsKey("hasLloyd");
	}
}
