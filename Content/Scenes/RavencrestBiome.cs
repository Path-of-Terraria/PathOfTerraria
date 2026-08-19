using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Content.Scenes;

internal class RavencrestBiome : ModBiome
{
	public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
	public override string MapBackground => "PathOfTerraria/Assets/Backgrounds/RavencrestMap";
	public override string BackgroundPath => MapBackground;
	public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<RavencrestSurfaceBackground>();
	public override int Music => Main.dayTime ? MusicID.AltOverworldDay : MusicID.TownNight;

	public override bool IsBiomeActive(Player player)
	{
		return SubworldSystem.Current is RavencrestSubworld;
	}
}
