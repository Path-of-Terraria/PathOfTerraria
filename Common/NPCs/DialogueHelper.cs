using PathOfTerraria.Common.Classing;
using System.Text;

namespace PathOfTerraria.Common.NPCs;

internal static class DialogueHelper
{
	public static string PolishString(string input)
	{
		return input;
		StringBuilder builder = new(input);
		string noun = ClassingPlayer.GetClassNoun(Main.LocalPlayer);
		builder.Replace("{ClassNoun}", noun);
		builder.Replace("{ClassNoun^L}", noun.ToLowerInvariant());
		return builder.ToString();
	}
}
