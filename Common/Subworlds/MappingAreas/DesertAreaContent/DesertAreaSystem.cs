using SubworldLibrary;
using Terraria.GameContent.Events;

namespace PathOfTerraria.Common.Subworlds.MappingAreas.DesertAreaContent;

internal class DesertAreaSystem : ModSystem
{
	public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
	{
		if (SubworldSystem.Current is DesertArea)
		{
			tileColor = new Color(255, 180, 120);
			backgroundColor = new Color(255, 220, 215);
		}
	}

	public override void PreUpdateEntities()
	{
		if (SubworldSystem.Current is DesertArea)
		{
			if (Sandstorm.Happening)
			{
				Sandstorm.UpdateTime();
				Sandstorm.EmitDust();
			}
		}
	}
}
