#if DEBUG
using PathOfTerraria.Content.Conflux;
using SubworldLibrary;

namespace PathOfTerraria.Core.Commands;

public sealed class YryothRealmCommand : ModCommand
{
	public override string Command => "yryothrealm";
	public override CommandType Type => CommandType.Chat;
	public override string Usage => "[c/ff6a00:Usage: /yryothrealm]";
	public override string Description => "Travels directly to Yryoth's Glacial Realm";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (GlacialRealm.IsActive)
		{
			caller.Reply("You are already in Yryoth's Glacial Realm.", Color.Cyan);
			return;
		}

		caller.Reply("Entering Yryoth's Glacial Realm...", Color.Cyan);
		SubworldSystem.Enter<GlacialRealm>();
	}
}
#endif
