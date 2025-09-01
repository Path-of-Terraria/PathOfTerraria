using PathOfTerraria.Common.ItemDropping;
using PathOfTerraria.Content.Items.Consumables.Maps;

namespace PathOfTerraria.Core.Commands;

#if DEBUG
public sealed class SpawnMapCommand : ModCommand
{
	public override string Command => "spawnmap";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /spawnmap <count> <tier>]";

	public override string Description => "Spawns a few maps for testing";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (args.Length == 0 || !int.TryParse(args[0], out int count))
		{
			count = 1;
		}

		if (args.Length < 2 || !int.TryParse(args[1], out int tier))
		{
			tier = 1;
		}

		for (int i = 0; i < count; i++)
		{
			ItemSpawner.SpawnMap(caller.Player.Center, tier);
		}
	}
}
#endif