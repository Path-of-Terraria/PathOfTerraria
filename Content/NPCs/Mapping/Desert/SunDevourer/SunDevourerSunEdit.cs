using PathOfTerraria.Common.Systems.SunModifications;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;

internal class SunDevourerSunEdit : SunEditInstance
{
	public static float Blackout = 1;

	public override void PreModifySunDrawing(ref Texture2D texture, ref Vector2 position, ref Color color)
	{
		color = Color.Lerp(color, Color.Black, 1 - Blackout);
	}
}
