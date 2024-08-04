using PathOfTerraria.Common.Systems;
using SubworldLibrary;

namespace PathOfTerraria.Core.Commands;

public sealed class ExitSubworldCommand : ModCommand
{
	public override string Command => "exitworld";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /exitworld]";

	public override string Description => "Exits the subworld";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		SubworldSystem.Exit();
		
		MappingSystem.Map = null;
	}
}