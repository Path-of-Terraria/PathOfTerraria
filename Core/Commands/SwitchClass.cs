using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Loaders.UILoading;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.UI;

namespace PathOfTerraria.Core.Commands;

public sealed class SwitchClass : ModCommand
{
	public override string Command => "switchclass";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /switchclass]";

	public override string Description => "Prompts the class selection again";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (!caller.Player.TryGetModPlayer(out ClassModPlayer classPlayer))
		{
			return;
		}

		classPlayer.SelectedClass = PlayerClass.None;
		
		UILoader.GetUIState<ClassSelection>().IsVisible = true;
	}
}