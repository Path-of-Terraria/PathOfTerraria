using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using PathOfTerraria.Common.Subworlds.MappingAreas;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;
using PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;
using PathOfTerraria.Content.Swamp;
using PathOfTerraria.Content.Swamp.NPCs.SwampBoss;
using SubworldLibrary;
using System.Collections.Generic;
using System.Reflection;

namespace PathOfTerraria.Common.ModCompatibility;

internal sealed class DiscordRichPresenceCompatibility : ModSystem
{
	private const string PathOfTerrariaDiscordClientKey = "pathofterraria";
	private const string PathOfTerrariaDiscordApplicationId = "1089695863217074227";

	private static readonly string[] SupportedModNames =
	[
		"DiscordRPCAPI",
		"DiscordRPAPI",
		"Discord-Rich-Presence-API-Mod",
		"DiscordRichPresenceAPI",
	];
	private string _lastPresenceWorldKey = string.Empty;

	public override void PostSetupContent()
	{
		if (Main.dedServ || !TryGetDiscordRpcMod(out Mod discordRpc))
		{
			return;
		}

		try
		{
			RegisterDiscordClient(discordRpc);
			RegisterSubworlds(discordRpc);
			RegisterBosses(discordRpc);
			RegisterPlayerStats(discordRpc);
		}
		catch (Exception ex)
		{
			Mod.Logger.Warn("Failed to register Discord Rich Presence compatibility.", ex);
		}
	}

	public override void PostUpdatePlayers()
	{
		if (Main.dedServ || Main.gameMenu || Main.LocalPlayer?.active != true || !TryGetDiscordRpcMod(out Mod discordRpc))
		{
			return;
		}

		UpdateDiscordClientOverride(discordRpc);

		string currentWorldKey = GetCurrentWorldKey();

		if (_lastPresenceWorldKey == currentWorldKey)
		{
			return;
		}

		_lastPresenceWorldKey = currentWorldKey;
		RefreshDiscordRichPresence(discordRpc);
	}

	private static bool TryGetDiscordRpcMod(out Mod mod)
	{
		foreach (string name in SupportedModNames)
		{
			if (ModLoader.TryGetMod(name, out mod))
			{
				return true;
			}
		}

		mod = null;
		return false;
	}

	private static string GetCurrentWorldKey()
	{
		return SubworldSystem.Current?.GetType().FullName ?? string.Empty;
	}

	private static void RegisterDiscordClient(Mod discordRpc)
	{
		discordRpc.GetType().GetMethod("AddDiscordAppID", BindingFlags.Instance | BindingFlags.Public)
			?.Invoke(discordRpc, [PathOfTerrariaDiscordClientKey, PathOfTerrariaDiscordApplicationId]);
	}

	private static void UpdateDiscordClientOverride(Mod discordRpc)
	{
		discordRpc.Call("SetClientOverride", PathOfTerrariaDiscordClientKey);
	}

	private void RefreshDiscordRichPresence(Mod discordRpc)
	{
		try
		{
			const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			Type type = discordRpc.GetType();

			type.GetMethod("UpdateWorldStaticInfo", Flags)?.Invoke(discordRpc, null);
			type.GetMethod("ClientUpdatePlayer", Flags)?.Invoke(discordRpc, null);
			type.GetMethod("UpdateLobbyInfo", Flags)?.Invoke(discordRpc, null);
			type.GetMethod("ClientForceUpdate", Flags)?.Invoke(discordRpc, null);
		}
		catch (Exception ex)
		{
			Mod.Logger.Warn("Failed to refresh Discord Rich Presence after subworld change.", ex);
		}
	}

	private void RegisterSubworlds(Mod discordRpc)
	{
		List<(Func<bool> active, string imageKey, string displayNameKey, float priority, string client)> subworlds =
		[
			(() => SubworldSystem.Current is RavencrestSubworld, "ravencrestdomain", "Mods.PathOfTerraria.Subworlds.RavencrestSubworld.DisplayName", 220f, PathOfTerrariaDiscordClientKey),
			(() => SubworldSystem.Current is ForestArea, "biome_forest", "Mods.PathOfTerraria.Subworlds.ForestArea.DisplayName", 221f, "default"),
			(() => SubworldSystem.Current is DesertArea, "biome_desert", "Mods.PathOfTerraria.Subworlds.DesertArea.DisplayName", 222f, "default"),
			(() => SubworldSystem.Current is SwampArea, "biome_jungle", "Mods.PathOfTerraria.Subworlds.SwampArea.DisplayName", 223f, "default"),
			(() => SubworldSystem.Current is KingSlimeDomain, "boss_kingslime", "Mods.PathOfTerraria.Subworlds.KingSlimeDomain.DisplayName", 230f, "default"),
			(() => SubworldSystem.Current is EyeDomain, "boss_eoc", "Mods.PathOfTerraria.Subworlds.EyeDomain.DisplayName", 231f, "default"),
			(() => SubworldSystem.Current is EaterDomain, "boss_eow", "Mods.PathOfTerraria.Subworlds.EaterDomain.DisplayName", 232f, "default"),
			(() => SubworldSystem.Current is BrainDomain, "boss_boc", "Mods.PathOfTerraria.Subworlds.BrainDomain.DisplayName", 233f, "default"),
			(() => SubworldSystem.Current is QueenBeeDomain, "boss_queenbee", "Mods.PathOfTerraria.Subworlds.QueenBeeDomain.DisplayName", 234f, "default"),
			(() => SubworldSystem.Current is SkeletronDomain, "boss_skeletron", "Mods.PathOfTerraria.Subworlds.SkeletronDomain.DisplayName", 235f, "default"),
			(() => SubworldSystem.Current is DeerclopsDomain, "boss_placeholder", "Mods.PathOfTerraria.Subworlds.DeerclopsDomain.DisplayName", 236f, "default"),
			(() => SubworldSystem.Current is WallOfFleshDomain, "boss_wof", "Mods.PathOfTerraria.Subworlds.WallOfFleshDomain.DisplayName", 237f, "default"),
			(() => SubworldSystem.Current is QueenSlimeDomain, "boss_queenslime", "Mods.PathOfTerraria.Subworlds.QueenSlimeDomain.DisplayName", 238f, "default"),
			(() => SubworldSystem.Current is TwinsDomain, "boss_twins", "Mods.PathOfTerraria.Subworlds.TwinsDomain.DisplayName", 239f, "default"),
			(() => SubworldSystem.Current is DestroyerDomain, "destroyerdomain", "Mods.PathOfTerraria.Subworlds.DestroyerDomain.DisplayName", 240f, PathOfTerrariaDiscordClientKey),
			(() => SubworldSystem.Current is PrimeDomain, "primedomain", "Mods.PathOfTerraria.Subworlds.PrimeDomain.DisplayName", 241f, PathOfTerrariaDiscordClientKey),
			(() => SubworldSystem.Current is PlanteraDomain, "planteradomain", "Mods.PathOfTerraria.Subworlds.PlanteraDomain.DisplayName", 242f, PathOfTerrariaDiscordClientKey),
			(() => SubworldSystem.Current is GolemDomain, "golemdomain", "Mods.PathOfTerraria.Subworlds.GolemDomain.DisplayName", 243f, PathOfTerrariaDiscordClientKey),
			(() => SubworldSystem.Current is FishronDomain, "fishrondomain", "Mods.PathOfTerraria.Subworlds.FishronDomain.DisplayName", 244f, PathOfTerrariaDiscordClientKey),
			(() => SubworldSystem.Current is EmpressDomain, "empressdomain", "Mods.PathOfTerraria.Subworlds.EmpressDomain.DisplayName", 245f, PathOfTerrariaDiscordClientKey),
			(() => SubworldSystem.Current is CultistDomain, "cultistdomaind", "Mods.PathOfTerraria.Subworlds.CultistDomain.DisplayName", 246f, PathOfTerrariaDiscordClientKey),
			(() => SubworldSystem.Current is MoonLordDomain, "moonlorddomain", "Mods.PathOfTerraria.Subworlds.MoonLordDomain.DisplayName", 247f, PathOfTerrariaDiscordClientKey),
		];

		foreach ((Func<bool> active, string imageKey, string displayNameKey, float priority, string client) in subworlds)
		{
			discordRpc.Call("AddBiome", active, imageKey, displayNameKey, priority, client);
		}

		discordRpc.Call("AddWorld", "ravencrestdomain", "Mods.PathOfTerraria.Subworlds.RavencrestSubworld.DisplayName", PathOfTerrariaDiscordClientKey);
	}

	private void RegisterBosses(Mod discordRpc)
	{
		List<(int type, string imageKey, float priority)> bosses =
		[
			(ModContent.NPCType<Grovetender>(), "boss_placeholder", 18f),
			(ModContent.NPCType<SunDevourerNPC>(), "boss_placeholder", 19f),
			(ModContent.NPCType<Mossmother>(), "boss_placeholder", 20f),
		];

		foreach ((int type, string imageKey, float priority) in bosses)
		{
			discordRpc.Call("AddBoss", new List<int> { type }, Lang.GetNPCNameValue(type), imageKey, priority);
		}
	}

	private static void RegisterPlayerStats(Mod discordRpc)
	{
		discordRpc.Call("AddPlayerStat", "Mods.PathOfTerraria.UI.StatUI.Level", (Func<string>)(() =>
		{
			Player player = Main.LocalPlayer;
			return player?.active == true ? player.GetModPlayer<ExpModPlayer>().EffectiveLevel.ToString() : "0";
		}));
	}
}
