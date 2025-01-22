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

	/// <summary>
	/// Modifies damage to work consistently in harder modes.<br/>
	/// By default, projectiles have their damage doubled (Normal), quadrupled (Expert), or sextupled (Master). This remedies that annoyance.
	/// </summary>
	/// <param name="value">The base damage value.</param>
	/// <returns>The adjusted damage value.</returns>
	public static int ProjectileDamage(int value)
	{
		if (Main.masterMode)
		{
			return value / 6;
		}
		else if (Main.expertMode)
		{
			return value / 4;
		}

		return value / 2;
	}
}
