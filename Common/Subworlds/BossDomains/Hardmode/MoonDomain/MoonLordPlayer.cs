using SubworldLibrary;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain;

internal class MoonLordPlayer : ModPlayer
{
	public override void ResetEffects()
	{
		if (SubworldSystem.Current is MoonLordDomain)
		{
			Main.shimmerAlpha = 1f;
		}
	}
}
