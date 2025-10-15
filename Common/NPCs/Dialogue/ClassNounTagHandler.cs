using PathOfTerraria.Common.Classing;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.NPCs.Dialogue;

internal class ClassNounTagHandler : ITagHandler
{
	public TextSnippet Parse(string text, Color baseColor = default, string options = null)
	{
		Player player;
		
		if (int.TryParse(text, out int value) && value >= 0 && value < Main.maxPlayers && Main.player[value] is { active: true} p)
		{
			player = p;
		}
		else
		{
			player = Main.LocalPlayer;
		}

		bool lowercase = false;
		int index = 0;

		if (!string.IsNullOrEmpty(options))
		{
			string[] split = options.Split(',');

			foreach (string parameter in split)
			{
				if (parameter.Equals("l", StringComparison.CurrentCultureIgnoreCase))
				{
					lowercase = true;
				}
				else if (int.TryParse(parameter, out int newIndex))
				{
					index = newIndex;
				}
			}
		}

		string noun = ClassingPlayer.GetClassNoun(player, index);

		if (lowercase)
		{
			noun = noun.ToLowerInvariant();
		}

		return new TextSnippet(noun, baseColor, 1);
	}
}
