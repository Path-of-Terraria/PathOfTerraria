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

		if (y < 1600)
		{
			tileColor = Color.Black;
			backgroundColor = Color.Black;
		}
		else if (y < 1700)
		{
			tileColor = Color.Lerp(Color.Black, tileColor, (y - 1600) / 100f);
			backgroundColor = tileColor;
		}
	}
}
