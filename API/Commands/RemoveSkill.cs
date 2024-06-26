﻿using PathOfTerraria.Core.Systems.ModPlayers;

namespace PathOfTerraria.API.Commands;

[Autoload]
public class RemoveSkill : ModCommand {
	public override string Command => "removeskill";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /removeskill <slot>]";

	public override string Description => "Removes the given skill slot.";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (args.Length != 1)
		{
			caller.Reply("Command expected 1 argument (<int slot>)", Color.Red);
			return;
		}

		if (!int.TryParse(args[0], out int slot) || slot < 0 || slot > 2)
		{
			caller.Reply($"Argument 0 (slot) must be an int between 0 and 2, inclusive!", Color.Red);
			return;
		}

		Main.LocalPlayer.GetModPlayer<SkillPlayer>().Skills[slot] = null;
	}
}