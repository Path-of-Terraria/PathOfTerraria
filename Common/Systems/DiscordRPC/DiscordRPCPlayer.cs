using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.ModPlayers;
using SubworldLibrary;

namespace PathOfTerraria.Common.Systems.DiscordRPC;

public class DiscordRPCPlayer : ModPlayer
{
	public override void OnEnterWorld()
	{
		if (Player.whoAmI == Main.myPlayer)
		{
			TryInitializeDiscordRPC();
		}
	}

	private void TryInitializeDiscordRPC()
	{
		if (DiscordRPCSystem.DiscordRPCInitialized)
		{
			return;
		}
		
		if (!ModLoader.TryGetMod("DiscordRPCAPI", out Mod discordRPCMod))
		{
			return;
		}
		
		discordRPCMod.Call("AddPlayerStat", "Level", (Func<string>)GetLevel);

		AddAllSubworlds(discordRPCMod);

		DiscordRPCSystem.DiscordRPCInitialized = true;
	}

	private static bool AddAllSubworlds(Mod discordRPCMod)
	{
		Type subworldSystemType = typeof(SubworldSystem);
		
		FieldInfo subworldsField = subworldSystemType.GetField("subworlds", BindingFlags.NonPublic | BindingFlags.Static);
		
		if (subworldsField == null)
		{
			return false;
		}

		if (subworldsField.GetValue(null) is not List<Subworld> subworlds)
		{
			return false;
		}

		var mappingWorlds = subworlds.OfType<MappingWorld>().ToList();
		if (mappingWorlds.Count == 0)
		{
			return false;
		}
		foreach (MappingWorld mappingWorld in mappingWorlds)
		{
			// ideally we would add the texture to be used directly in each subworld we create, just using a static one for now
			discordRPCMod.Call("AddWorld", "https://i.imgur.com/ZWOW3ka.png", mappingWorld.DisplayName.Value.Split("Subworld")[0].Trim());
		}

		return true;
	}

	private string GetLevel()
	{
		return Player.GetModPlayer<ExpModPlayer>().Level.ToString() ?? "0";
	}
}