using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;

namespace PathOfTerraria.Core.Commands;

#if DEBUG
public sealed class EnterRavencrestCommand : ModCommand
{
	public override string Command => "crest";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /crest]";

	public override string Description => "Enters Ravencrest";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		SubworldSystem.Enter<RavencrestSubworld>();
	}
}
#endif