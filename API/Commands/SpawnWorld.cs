using PathOfTerraria.Core.Subworlds;
using SubworldLibrary;

namespace PathOfTerraria.API.Commands
{
	[Autoload]
	public class SpawnWorld : ModCommand {
		public override string Command => "newworld";

		public override CommandType Type => CommandType.Chat;

		public override string Usage => "[c/ff6a00:Usage: /newworld]";

		public override string Description => "Generates a new subworld";

		public override void Action(CommandCaller caller, string input, string[] args){
			SubworldSystem.Enter<TestSubworld>();
		}
	}
}