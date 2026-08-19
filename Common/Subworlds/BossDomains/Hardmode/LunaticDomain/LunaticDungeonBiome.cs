using SubworldLibrary;
using Terraria.Graphics.Capture;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.LunaticDomain;

internal class LunaticDungeonBiome : ModBiome
{
	public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Corrupt;
	public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

	public override int Music => MusicID.Dungeon;
	public override string BackgroundPath => "Terraria/Images/MapBG5";
	public override Color? BackgroundColor => base.BackgroundColor;
	public override string MapBackground => "Terraria/Images/MapBG5";

	public override bool IsBiomeActive(Player player)
	{
		return SubworldSystem.Current is CultistDomain;
	}
}