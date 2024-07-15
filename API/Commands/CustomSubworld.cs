using PathOfTerraria.Content.Items.Consumables.Maps;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Systems.CustomSubworlds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathOfTerraria.API.Commands;

[Autoload]
public class CustomSubworld : ModCommand
{
	public override string Command => "enter";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /enter name w h]";

	public override string Description => "Enters a subworld -case sensitive- | if the world has not been entered before w and h define the initial size, defaults to 2000x1000";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (args.Length < 1)
		{
			caller.Reply("Command expected at least 1 arguments", Color.Red);
			return;
		}

		string[] nArgs = new string[3];
		for (int i = 0; i < Math.Min(args.Length, nArgs.Length); i++)
		{
			nArgs[i] = args[i];
		}
		args = nArgs;

		string name = args[0];

		if (!int.TryParse(args[1], out int width))
		{
			width = 2000;
		}

		if (!int.TryParse(args[2], out int height))
		{
			height = 1000;
		}

		CustomSubworldsSystem.EnterCustomSubworld(name, width, height);
	}
}