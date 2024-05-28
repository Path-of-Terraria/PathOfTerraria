using PathOfTerraria.Content.GUI;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.Experience;
using PathOfTerraria.Core.Systems.ModPlayers;

namespace PathOfTerraria.API.Commands;

[Autoload]
public class SpawnItem : ModCommand {
	public override string Command => "spawnitem";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /spawnitem <relative X> <relative Y> <count>]";

	public override string Description => "Spawns item(s) for testing items and loot generatio.";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (args.Length != 3)
		{
			caller.Reply("Command expected 3 arguments", Color.Red);
			return;
		}

		if (!float.TryParse(args[0], out float relX))
		{
			caller.Reply("Argument 1 must be a floating-point value", Color.Red);
			return;
		}

		if (!float.TryParse(args[1], out float relY))
		{
			caller.Reply("Argument 2 must be a floating-point value", Color.Red);
			return;
		}

		if (!uint.TryParse(args[2], out uint count) || count == 0)
		{
			caller.Reply("Argument 3 must be an unsigned integer which is greater than zero", Color.Red);
			return;
		}

		for (int i = 0; i < count; i++)
		{
			Gear.SpawnItem(caller.Player.Center + new Vector2(relX, relY));
		}
		caller.Reply($"Item(s) spawned!", Color.Green);
	}
}