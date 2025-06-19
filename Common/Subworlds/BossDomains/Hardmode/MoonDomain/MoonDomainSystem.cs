using SubworldLibrary;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain;

internal class MoonDomainSystem : ModSystem
{
	public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
	{
		if (SubworldSystem.Current is not MoonLordDomain)
		{
			return;
		}

		int y = (int)(Main.LocalPlayer.Center.Y / 16f);

		if (y < MoonLordDomain.CloudTop)
		{
			tileColor = Color.Black;
			backgroundColor = Color.Black;
		}
		else if (y < MoonLordDomain.CloudBottom)
		{
			tileColor = Color.Lerp(Color.Black, tileColor, (y - MoonLordDomain.CloudTop) / (float)(MoonLordDomain.CloudBottom - MoonLordDomain.CloudTop));
			backgroundColor = tileColor;
		}
	}
}
