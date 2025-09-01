using PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;
using SubworldLibrary;
using Terraria.GameContent.Events;

namespace PathOfTerraria.Common.Subworlds.MappingAreas.DesertAreaContent;

internal class DesertAreaSystem : ModSystem
{
	public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
	{
		if (SubworldSystem.Current is DesertArea)
		{
			float mod = SunDevourerSunEdit.Blackout;
			tileColor = new Color(255, 180, 120) * (mod * 0.85f + 0.15f);
			backgroundColor = new Color(255, 220, 215) * mod;
		}
		else
		{
			SunDevourerSunEdit.Blackout = 1;
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

			if (!Main.CurrentFrameFlags.AnyActiveBossNPC)
			{
				SunDevourerSunEdit.Blackout = MathHelper.Lerp(SunDevourerSunEdit.Blackout, 1, 0.02f);
			}
		}
	}
}
