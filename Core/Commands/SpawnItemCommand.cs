using PathOfTerraria.Common.ItemDropping;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;

namespace PathOfTerraria.Core.Commands;

#if DEBUG
public sealed class SpawnItemCommand : ModCommand
{
	public override string Command => "spawnitem";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /spawnitem <count> <ilevel> <quality increase> <geartype>]";

	public override string Description => "Spawns item(s) for testing items and loot generation.";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		string[] nArgs = new string[6];
		
		for (int i = 0; i < args.Length; i++)
		{
			nArgs[i] = args[i];
		}

		args = nArgs;

		if (!uint.TryParse(args[0], out uint count))
		{
			count = 1;
		}

		if (!uint.TryParse(args[1], out uint ilevel))
		{
			ilevel = 0;
		}

		if (!float.TryParse(args[2], out float qualityIncrease))
		{
			qualityIncrease = 0;
		}

		string geartype = args[5];
		List<ItemDatabase.ItemRecord> items = DropTable.RollManyMobDrops((int)count, (int)ilevel, qualityIncrease);

		for (int i = 0; i < count; i++)
		{
			ItemDatabase.ItemRecord record = items[i];
			ItemSpawner.SpawnItem(record.ItemId, caller.Player.Center, 0, record.Rarity);
		}

		caller.Reply("Item(s) spawned!", Color.Green);
	}
}
#endif