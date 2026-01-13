using SubworldLibrary;

namespace PathOfTerraria.Common.Subworlds.MappingAreas.SwampAreaContent;

internal class SwampPlayer : ModPlayer
{
	public override void PostUpdateEquips()
	{
		if (SubworldSystem.Current is SwampArea)
		{
			Player.ignoreWater = true;
		}
	}
}
