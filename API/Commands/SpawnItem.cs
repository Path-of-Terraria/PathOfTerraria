using PathOfTerraria.Content.GUI;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.Experience;
using PathOfTerraria.Core.Systems.ModPlayers;
using System.CodeDom;

namespace PathOfTerraria.API.Commands;

[Autoload]
public class SpawnItem : ModCommand {
	public override string Command => "spawnitem";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /spawnitem <relative X> <relative Y> <count> <ilevel> <quality increase> <geartype>]";

	public override string Description => "Spawns item(s) for testing items and loot generation, only x and y positions are neccecary.";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (args.Length < 2)
		{
			caller.Reply("Command expected 2 arguments", Color.Red);
			return;
		}

		string[] nArgs = new string[6];
		for (int i = 0; i < args.Length; i++)
		{
			nArgs[i] = args[i];
		}
		args = nArgs;

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

		uint count;
		if (!uint.TryParse(args[2], out count))
		{
			count = 1;
		}

		uint ilevel;
		if (!uint.TryParse(args[3], out ilevel))
		{
			ilevel = 0;
		}

		float qualityIncrease;
		if (!float.TryParse(args[4], out qualityIncrease))
		{
			qualityIncrease = 0;
		}

		string geartype = args[5];
		// need to impliment at some point.

		for (int i = 0; i < count; i++)
		{
			Gear.SpawnItem(caller.Player.Center + new Vector2(relX, relY), (int)ilevel, qualityIncrease);
		}
		caller.Reply($"Item(s) spawned!", Color.Green);
	}
}