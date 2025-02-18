using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.QueenDomain;

internal class QueenSlimeScene : ModSceneEffect
{
	public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
	public override int Music => QueenSlimeDomain.ModDistance(Main.LocalPlayer.Center, GetCircleCenter()) < 696 * 16 ? MusicID.TheHallow : MusicID.UndergroundHallow;

	public static Vector2 GetCircleCenter()
	{
		if (SubworldSystem.Current is QueenSlimeDomain)
		{
			return QueenSlimeDomain.CircleCenter.ToWorldCoordinates();
		}

		return new Vector2(-5000);
	}

	public override bool IsSceneEffectActive(Player player)
	{
		return SubworldSystem.Current is QueenSlimeDomain;
	}
}
