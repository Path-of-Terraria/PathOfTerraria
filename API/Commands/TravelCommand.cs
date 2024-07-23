using PathOfTerraria.Core.Systems.WorldNavigation;

namespace PathOfTerraria.API.Commands;

[Autoload]
public class TravelCommand : ModCommand {
	public override string Command => "travel";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /travel <worldname>]";

	public override string Description => "Travels to the world";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (args.Length == 0)
		{
			throw new UsageException("You must specify a world name.");
		}

		string worldName = string.Join(" ", args);
		foreach (Type type in PathOfTerraria.Instance.Code.GetTypes())
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(WorldNavigation)))
			{
				continue;
			}

			if (type.Name.ToLower() == worldName.ToLower())
			{
				if (Activator.CreateInstance(type) is not WorldNavigation world)
				{
					throw new UsageException($"World '{worldName}' not found.");
				}

				world.LoadWorld();
				caller.Player.Teleport(world.SpawnPosition);
				return;
			}
		}
		
		Main.NewText("World not found.");
	}
}