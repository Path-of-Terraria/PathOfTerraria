using PathOfTerraria.Content.Items.Consumables.Maps;

namespace PathOfTerraria.Core.Commands;

#if DEBUG
public sealed class SpawnMapCommand : ModCommand
{
	public override string Command => "spawnmap";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /spawnmap ]";

	public override string Description => "Spawns a few maps for testing";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		for (int i = 0; i < 4; i++)
		{
			Map.SpawnItem(caller.Player.Center);
		}
	}
}
#endif