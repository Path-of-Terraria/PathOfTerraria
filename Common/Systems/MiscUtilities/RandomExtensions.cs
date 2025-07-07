using Terraria.Utilities;

namespace PathOfTerraria.Common.Systems.MiscUtilities;

internal static class RandomExtensions
{
	public static int NextDirection(this UnifiedRandom random)
	{
		return random.NextBool() ? 1 : -1;
	}
}
