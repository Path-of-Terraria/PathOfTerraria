namespace PathOfTerraria.Common.NPCs;

internal class ModeUtils
{
	public static int ByMode(int normal, int expert, int master)
	{
		if (Main.masterMode)
		{
			return master;
		}
		else if (Main.expertMode)
		{
			return expert;
		}

		return normal;
	}

	public static float ByMode(float normal, float expert, float master)
	{
		if (Main.masterMode)
		{
			return master;
		}
		else if (Main.expertMode)
		{
			return expert;
		}

		return normal;
	}
}
