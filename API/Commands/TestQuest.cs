using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.Questing;
using PathOfTerraria.Core.Systems.Questing.QuestingEventHandlers;
using SubworldLibrary;

namespace PathOfTerraria.API.Commands;

[Autoload]
public class TestQuest : ModCommand {
	public override string Command => "testquest";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /testquest]";

	public override string Description => "Starts the testquest";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		Main.LocalPlayer.GetModPlayer<QuestHandler>().RestartQuestTest();
	}
}