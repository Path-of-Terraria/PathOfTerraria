using PathOfTerraria.Common;
using PathOfTerraria.Common.Enums;

namespace PathOfTerraria.Core.Commands;

#if DEBUG
public sealed class SpawnUniqueItemCommand : ModCommand
{
	public override string Command => "spawnuitem";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /spawnuitem <relative X> <relative Y> <count> <ilevel> <quality increase> <geartype>]";

	public override string Description => "Spawns unique item(s) for testing items and loot generation, only x and y positions are necessary.";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		_ = new Color(50, 100, 200);
		_ = new Color(50, 100, 200, 155);
		_ = new Color(0.25f, 0.5f, 0.75f);
		_ = new Color(0.25f, 0.5f, 0.75f, 0.1f);
		
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
		
		// TODO: Implementation.

		for (int i = 0; i < count; i++)
		{
			ItemSpawner.SpawnRandomItem(caller.Player.Center + new Vector2(relX, relY), x => x.Rarity == ItemRarity.Unique, (int)ilevel, qualityIncrease);
		}

		caller.Reply("Item(s) spawned!", Color.Green);
	}
}
#endif