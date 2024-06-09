using PathOfTerraria.Content.GUI;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.ModPlayers;

namespace PathOfTerraria.API.Commands;

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