using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.BossDomains;
using SubworldLibrary;

namespace PathOfTerraria.Core.Commands;

#if DEBUG
public sealed class SpawnWorldCommand : ModCommand
{
	public override string Command => "newworld";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /newworld]";

	public override string Description => "Generates a new subworld";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (args.Length == 0) // Debug quickenter
		{
			SubworldSystem.Enter<BrainDomain>();
		}

		if (args.Length == 2)
		{
			throw new ArgumentException("Command takes only one parameter!", nameof(args));
		}

		string param = args[0].ToLower();

		if (BossDomainSubworld.NameToSubworld.TryGetValue(param, out BossDomainSubworld subworld))
		{
			SubworldSystem.Enter($"{PoTMod.ModName}/{subworld.Name}");
		}
	}
}
#endif