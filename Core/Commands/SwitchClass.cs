using PathOfTerraria.Common;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Loaders.UILoading;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.UI;

namespace PathOfTerraria.Core.Commands;

[Autoload]
public class SwitchClass : ModCommand {
	public override string Command => "switchclass";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /switchclass]";

	public override string Description => "Prompts the class selection again";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		Main.LocalPlayer.GetModPlayer<ClassModPlayer>().SelectedClass = PlayerClass.None;
		UILoader.GetUIState<ClassSelection>().IsVisible = true;
	}
}