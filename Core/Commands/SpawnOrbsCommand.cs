using PathOfTerraria.Common.Systems.Experience;

namespace PathOfTerraria.Core.Commands;

#if DEBUG
public sealed class SpawnOrbsCommand : ModCommand
{
	public override string Command => "orb";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /orb <xp>]";

	public override string Description => "Spawns experience orbs relative to the player";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (args.Length != 1)
		{
			caller.Reply("Command expected 1 arguments", Color.Red);
			return;
		}

		if (!uint.TryParse(args[0], out uint xp) || xp == 0)
		{
			caller.Reply("Argument 1 must be an unsigned integer which is greater than zero", Color.Red);
			return;
		}

		int[] indices = ExperienceTracker.SpawnExperience(
			(int)xp,
			caller.Player.Center + new Vector2(100, 100),
			Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * 6,
			caller.Player.whoAmI
		);
		caller.Reply($"Spawned {indices.Length} experience orbs!", Color.Green);
	}
}
#endif