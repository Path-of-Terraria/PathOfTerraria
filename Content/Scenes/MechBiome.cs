using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using SubworldLibrary;

namespace PathOfTerraria.Content.Scenes;

internal class MechBiome : ModBiome
{
	public static bool LocallyInBiome = false;

	public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

	// Needs sprite
	public override string MapBackground => "PathOfTerraria/Assets/Backgrounds/MechAreaMapBack";
	public override string BackgroundPath => MapBackground;
	public override Color? BackgroundColor => Color.White;
	public override string BestiaryIcon => "PathOfTerraria/Assets/Backgrounds/MechArea_Icon";

	public override bool IsBiomeActive(Player player)
	{
		return LocallyInBiome;
	}
}

public class MechSystem : ModSystem
{
	public override void PostUpdateEverything()
	{
		MechBiome.LocallyInBiome = false;
	}
}

public class MechPlayer : ModPlayer
{
	public override void PreUpdate()
	{
		if (SubworldSystem.Current is TwinsDomain domain)
		{
			int y = (int)(Player.Center.Y / 16f);

			if (y > TwinsDomain.DirtLayerEnd && y <= TwinsDomain.MetalLayerEnd)
			{
				MechBiome.LocallyInBiome = true;
			}
		}
	}
}