using PathOfTerraria.Content.Swamp;
using SubworldLibrary;
using Terraria.Graphics.Capture;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp;

internal class SwampBiome : ModBiome
{
    public override ModWaterStyle WaterStyle => ModContent.GetInstance<SwampWaterStyle>();
    //public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.Find<ModSurfaceBackgroundStyle>("Verdant/VerdantSurfaceBgStyle");
    public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Mushroom;
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;

	public override int Music => MusicID.JungleNight;
    public override string BestiaryIcon => "PathOfTerraria/Assets/BiomeContent/SwampBiome_Icon";
    public override string BackgroundPath => MapBackground;
    public override Color? BackgroundColor => base.BackgroundColor;
    public override string MapBackground => "PathOfTerraria/Assets/Backgrounds/MechAreaMapBack";

    public override bool IsBiomeActive(Player player)
    {
		return SubworldSystem.Current is SwampArea;
    }
}