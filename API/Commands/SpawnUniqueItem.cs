﻿using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core;

namespace PathOfTerraria.API.Commands;

[Autoload]
public class SpawnUniqueItem : ModCommand {
	public override string Command => "spawnuitem";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /spawnuitem <relative X> <relative Y> <count> <ilevel> <quality increase> <geartype>]";

	public override string Description => "Spawns unique item(s) for testing items and loot generation, only x and y positions are necessary.";

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

		if (!uint.TryParse(args[2], out uint count))
		{
			count = 1;
		}

		if (!uint.TryParse(args[3], out uint ilevel))
		{
			ilevel = 0;
		}

		if (!float.TryParse(args[4], out float qualityIncrease))
		{
			qualityIncrease = 0;
		}

		string geartype = args[5];
		// need to impliment at some point.

		for (int i = 0; i < count; i++)
		{
			PoTItem.SpawnRandomItem(caller.Player.Center + new Vector2(relX, relY), x => x.Item2 == Rarity.Unique, (int)ilevel, qualityIncrease);
		}

		caller.Reply($"Item(s) spawned!", Color.Green);
	}
}