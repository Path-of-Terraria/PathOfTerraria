using SubworldLibrary;

namespace PathOfTerraria.Common.Subworlds.MappingAreas.SwampAreaContent;

internal class SwampSystem : ModSystem
{
	public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
	{
		if (SubworldSystem.Current is SwampArea)
		{
			tileColor = new Color(140, 170, 180);
			backgroundColor = new Color(140, 170, 180);
		}
	}
}
