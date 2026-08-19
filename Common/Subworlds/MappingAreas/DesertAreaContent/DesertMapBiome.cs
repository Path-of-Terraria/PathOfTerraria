using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.MappingAreas.DesertAreaContent;

internal class DesertMapBiome : ModSceneEffect
{
	public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

	public override int Music => MusicID.UndergroundDesert;

	public override bool IsSceneEffectActive(Player player)
	{
		return SubworldSystem.Current is DesertArea;
	}
}
