using PathOfTerraria.Common.Classing;

namespace PathOfTerraria.Common.NPCs;

internal static class DialogueHelper
{
	public static string PolishString(string input)
	{
		return input.Replace("{ClassNoun}", ClassingPlayer.GetRandomClassNoun(Main.LocalPlayer));
	}
}
