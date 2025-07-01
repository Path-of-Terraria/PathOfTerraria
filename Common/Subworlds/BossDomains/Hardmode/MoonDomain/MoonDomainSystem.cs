using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain.Generation;
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
		int top = MoonLordDomain.CloudTop + 30;

		if (y < top)
		{
			tileColor = Color.Black;
			backgroundColor = Color.Black;
		}
		else if (y < MoonLordDomain.CloudBottom)
		{
			tileColor = Color.Lerp(Color.Black, tileColor, (y - top) / (float)(MoonLordDomain.CloudBottom - top));
			backgroundColor = tileColor;
		}
		else if (y < MoonLordDomain.TerrariaHeight)
		{
			// Empty block because I'm too lazy to write logic
		}
		else if (y < MoonlordTerrainGen.StoneCutoff)
		{

		}
	}
}
