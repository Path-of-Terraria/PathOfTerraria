using SubworldLibrary;

namespace PathOfTerraria.Common.Subworlds;

internal class BossDomainSystem : ModSystem
{
	public override void PreUpdateGores()
	{
		if (SubworldSystem.Current is BossDomainSubworld domain)
		{
			if (domain.ForceTime.time != -1)
			{
				Main.time = domain.ForceTime.time;
				Main.dayTime = domain.ForceTime.isDay;
			}
		}
	}
}
