using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.Experience;

namespace PathOfTerraria.API.Commands
{
	[Autoload]
	public class SpawnOrbs : ModCommand{
		public override string Command => "orb";

		public override CommandType Type => CommandType.Chat;

		public override string Usage => "[c/ff6a00:Usage: /orb <relative X> <relative Y> <xp>]";

		public override string Description => "Spawns experience orbs relative to the player";

		public override void Action(CommandCaller caller, string input, string[] args){
			if(args.Length != 3){
				caller.Reply("Command expected 3 arguments", Color.Red);
				return;
			}

			if(!float.TryParse(args[0], out float relX)){
				caller.Reply("Argument 1 must be a floating-point value", Color.Red);
				return;
			}

			if(!float.TryParse(args[1], out float relY)){
				caller.Reply("Argument 2 must be a floating-point value", Color.Red);
				return;
			}

			if(!uint.TryParse(args[2], out uint xp) || xp == 0){
				caller.Reply("Argument 3 must be an unsigned integer which is greater than zero", Color.Red);
				return;
			}

			int[] indices = ExperienceTracker.SpawnExperience((int)xp, caller.Player.Center + new Vector2(relX, relY), 6f, caller.Player.whoAmI);
			caller.Reply($"Spawned {indices.Length} experience orbs!", Color.Green);
		}
	}
}