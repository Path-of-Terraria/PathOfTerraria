namespace PathOfTerraria.Common.NPCs;

internal class ModeUtils
{
	/// <summary>
	/// Allows for determining a stat, such as max life, per mode. Use this in <see cref="ModNPC.ApplyDifficultyAndPlayerScaling(int, float, float)"/>.<br/>
	/// All values are intended to be unscaled, as with the aforementioned hook in ModNPC.
	/// </summary>
	/// <param name="normal">Normal mode value.</param>
	/// <param name="expert">Expert mode value.</param>
	/// <param name="master">Master mode value.</param>
	/// <param name="legendary">
	/// Legendary mode value. For reference, "Legendary" is the mode available in a getfixedboi world if you choose master mode.<br/>
	/// Generally, this mode has no new content aside from stat bloat, so the default value -1 will make it equal the <paramref name="master"/> value.
	/// </param>
	/// <returns></returns>
	public static int ByMode(int normal, int expert, int master, int legendary = -1)
	{
		if (legendary == -1)
		{
			legendary = master;
		}

		if (Main.masterMode)
		{
			return Main.getGoodWorld ? legendary : master;
		}
		else if (Main.expertMode)
		{
			return expert;
		}

		return normal;
	}

	/// <inheritdoc cref="ByMode(int, int, int, int)"/>
	public static float ByMode(float normal, float expert, float master, float legendary = -1)
	{
		if (legendary == -1)
		{
			legendary = master;
		}

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

	/// <summary>
	/// Modifies damage to work consistently in harder modes. The damage is explicitly defined per mode, ignoring vanilla's annoying multipliers.<br/>
	/// By default, projectiles have their damage doubled (Normal), quadrupled (Expert), or sextupled (Master). This remedies that annoyance.
	/// </summary>
	/// <param name="normal">Normal mode value.</param>
	/// <param name="expert">Expert mode value.</param>
	/// <param name="master">Master mode value.</param>
	/// <param name="legendary">
	/// Legendary mode value. For reference, "Legendary" is the mode available in a getfixedboi world if you choose master mode.<br/>
	/// Generally, this mode has no new content aside from stat bloat, so the default value -1 will make it equal the <paramref name="master"/> value.
	/// </param>
	/// <returns>The adjusted damage value.</returns>
	public static int ProjectileDamage(int normal, int expert, int master, int legendary = -1)
	{
		int value = ByMode(normal, expert, master, legendary);

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
