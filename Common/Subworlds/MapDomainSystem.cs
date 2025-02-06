using SubworldLibrary;

namespace PathOfTerraria.Common.Subworlds;

internal class MapDomainSystem : ModSystem
{
	public override void PreUpdateGores()
	{
		if (SubworldSystem.Current is MappingWorld domain)
		{
			if (domain.ForceTime.time != -1)
			{
				Main.time = domain.ForceTime.time;
				Main.dayTime = domain.ForceTime.isDay;
			}
		}
	}
}
