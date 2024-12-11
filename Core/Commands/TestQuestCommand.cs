using PathOfTerraria.Common.Systems.Questing;

namespace PathOfTerraria.Core.Commands;

#if DEBUG
public sealed class TestQuestCommand : ModCommand
{
	public override string Command => "testquest";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /testquest]";

	public override string Description => "Starts the testquest";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (!caller.Player.TryGetModPlayer(out QuestModPlayer questPlayer))
		{
			return;
		}
		
		//questPlayer.RestartQuestTest();
	}
}
#endif