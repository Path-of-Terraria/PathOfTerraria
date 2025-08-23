namespace PathOfTerraria.Common.Systems.DiscordRPC;

public class DiscordRPCSystem : ModSystem
{
	public static bool DiscordRPCInitialized { get; set; }

	public override void Load()
	{
		DiscordRPCInitialized = false;
		
		if (!ModLoader.TryGetMod("DiscordRPCAPI", out Mod discordRPCMod))
		{
			return;
		}
		
		discordRPCMod.Call("AddClient", "1089695863217074227", "pathofterraria");
	}
}
