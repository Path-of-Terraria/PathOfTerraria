using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;

namespace PathOfTerraria.Content.Scenes;

internal class RavencrestBiome : ModBiome
{
	public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

	// Needs sprite
	//public override string MapBackground => base.MapBackground;
	//public override string BackgroundPath => MapBackground;
	public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<RavencrestSurfaceBackground>();

	public override bool IsBiomeActive(Player player)
	{
		return SubworldSystem.Current is RavencrestSubworld;
	}
}
