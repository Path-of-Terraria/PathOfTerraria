using SubworldLibrary;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.DukeDomain;

internal class FishronPlayer : ModPlayer
{
	public override void ResetEffects()
	{
		if (SubworldSystem.Current is FishronDomain)
		{
			Player.ignoreWater = true;
		}
	}
}
