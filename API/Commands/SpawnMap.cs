using PathOfTerraria.Content.Items.Consumables.Maps;

namespace PathOfTerraria.API.Commands;

[Autoload]
public class SpawnMap : ModCommand{
	public override string Command => "spawnmap";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /spawnmap ]";

	public override string Description => "Spawns experience orbs relative to the player";

	public override void Action(CommandCaller caller, string input, string[] args){
		Map.SpawnItem(Main.LocalPlayer.Center);
	}
}