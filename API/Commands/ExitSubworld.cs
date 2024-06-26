﻿using PathOfTerraria.Core.Systems;
using SubworldLibrary;

namespace PathOfTerraria.API.Commands;

[Autoload]
public class ExitSubworld : ModCommand {
	public override string Command => "exitworld";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /exitworld]";

	public override string Description => "Exits the subworld";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		SubworldSystem.Exit();
		MappingSystem.Map = null;
	}
}