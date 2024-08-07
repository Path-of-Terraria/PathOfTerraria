﻿using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.ModPlayers;

namespace PathOfTerraria.Core.Commands;

public sealed class SwitchClassCommand : ModCommand
{
	public override string Command => "switchclass";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /switchclass]";

	public override string Description => "Prompts the class selection again";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (!caller.Player.TryGetModPlayer(out ClassModPlayer classPlayer))
		{
			return;
		}

		classPlayer.SelectedClass = PlayerClass.None;
	}
}