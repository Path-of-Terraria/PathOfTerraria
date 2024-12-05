using PathOfTerraria.Common.Subworlds.BossDomains;
using SubworldLibrary;

namespace PathOfTerraria.Core.Commands;

#if DEBUG
public sealed class TravelCommand : ModCommand
{
	public override string Command => "travel";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /travel <worldname>]";

	public override string Description => "Travels to the given world";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (args.Length == 0)
		{
			throw new UsageException("You must specify a subworld name.");
		}

		if (args[0].Equals("-list", StringComparison.CurrentCultureIgnoreCase))
		{
			string name = "";

			foreach (Subworld world in ModContent.GetContent<Subworld>())
			{
				name += world.FullName + "  |  ";
			}

			Main.NewText(name);
			return;
		}

		if (args[0].Equals("-t", StringComparison.CurrentCultureIgnoreCase))
		{
			SubworldSystem.Enter<DeerclopsDomain>();
			return;
		}

		if (args[0].Equals("-r", StringComparison.CurrentCultureIgnoreCase))
		{
			SubworldSystem.Exit();
			return;
		}

		string subworldName = string.Join(" ", args);

		foreach (Subworld world in ModContent.GetContent<Subworld>())
		{
			if (world.Name.Equals(subworldName, StringComparison.CurrentCultureIgnoreCase) || 
				world.Name.Replace("Domain", "").Equals(subworldName, StringComparison.CurrentCultureIgnoreCase) ||
				world.Name.Replace("Subworld", "").Equals(subworldName, StringComparison.CurrentCultureIgnoreCase))
			{
				SubworldSystem.Enter(world.FullName);
				return;
			}
		}

		Main.NewText("World not found.", Color.Red);
	}
}
#endif