#if DEBUG
using PathOfTerraria.Common.Mapping;
using PathOfTerraria.Content.Conflux;

namespace PathOfTerraria.Core.Commands;

public sealed class AddConfluxCommand : ModCommand
{
	public override string Command => "addconflux";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /addconflux]";

	public override string Description => "Adds 100 of each conflux resource";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		const int amount = 100;

		MapResources.ModifyValue<InfernalConflux>(amount, ResourceDiscovery.Always);
		MapResources.ModifyValue<GlacialConflux>(amount, ResourceDiscovery.Always);
		MapResources.ModifyValue<CelestialConflux>(amount, ResourceDiscovery.Always);

		caller.Reply($"Added {amount} of each conflux resource.", Color.LimeGreen);
	}
}
#endif
