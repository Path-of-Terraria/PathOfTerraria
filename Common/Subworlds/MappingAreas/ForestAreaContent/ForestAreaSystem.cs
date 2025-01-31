using SubworldLibrary;

namespace PathOfTerraria.Common.Subworlds.MappingAreas.ForestAreaContent;

internal class ForestAreaSystem : ModSystem
{
	public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
	{
		if (SubworldSystem.Current is ForestArea)
		{
			tileColor = new Color(60, 45, 80);
			backgroundColor = new Color(60, 60, 75);
		}
	}
}
