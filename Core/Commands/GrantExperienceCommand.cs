using PathOfTerraria.Common.Systems.ModPlayers;

namespace PathOfTerraria.Core.Commands;

#if DEBUG
public sealed class GrantExperienceCommand : ModCommand
{
	public override string Command => "xp";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /xp <xp>]";

	public override string Description => "Grants experience to the player";

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

		ExpModPlayer.GrantExperience(caller.Player, (int)xp);
		caller.Reply($"Granted {xp} experience!", Color.Green);
	}
}
#endif
