using SubworldLibrary;
using Terraria.Graphics.Capture;

namespace PathOfTerraria.Content.Swamp;

internal class SwampBiome : ModBiome
{
    public override ModWaterStyle WaterStyle => ModContent.GetInstance<SwampWaterStyle>();
	public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<SwampBackground>();
	public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Mushroom;
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

	public override int Music => -1;
    public override string BestiaryIcon => "PathOfTerraria/Assets/BiomeContent/SwampBiome_Icon";
    public override string BackgroundPath => MapBackground;
    public override Color? BackgroundColor => base.BackgroundColor;
    public override string MapBackground => "PathOfTerraria/Assets/Backgrounds/MechAreaMapBack";

    public override bool IsBiomeActive(Player player)
    {
		return SubworldSystem.Current is SwampArea;
    }
}