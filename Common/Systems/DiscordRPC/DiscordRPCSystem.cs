namespace PathOfTerraria.Common.Systems.DiscordRPC;

public class DiscordRPCSystem : ModSystem
{
	public static bool DiscordRPCInitialized { get; set; }

	public override void Load()
	{
		DiscordRPCInitialized = false;
	}
}
