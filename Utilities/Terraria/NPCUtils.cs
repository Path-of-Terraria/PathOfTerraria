using System.Linq;

#nullable enable

namespace PathOfTerraria.Utilities.Terraria;

internal enum NPCCachedDraw
{
	Default,
	BehindWalls,
	BehindNonSolidTiles,
	Projectiles,
	OverPlayers,
	Unknown,
}

file sealed class NPCCachedDraws : ModSystem
{
	public static NPCCachedDraw Context;
	
	public override void Load()
	{
		On_Main.DrawCachedNPCs += (orig, self, cache, behindTiles) =>
		{
			NPCCachedDraw ctx;

			if (cache == Main.instance.DrawCacheNPCsMoonMoon) { ctx = NPCCachedDraw.BehindWalls; }
			else if (cache == Main.instance.DrawCacheNPCsBehindNonSolidTiles) { ctx = NPCCachedDraw.BehindNonSolidTiles; }
			else if (cache == Main.instance.DrawCacheNPCProjectiles) { ctx = NPCCachedDraw.Projectiles; }
			else if (cache == Main.instance.DrawCacheNPCsOverPlayers) { ctx = NPCCachedDraw.OverPlayers; }
			else { ctx = NPCCachedDraw.Unknown; }
			
			using var _ = ValueOverride.Create(ref Context, ctx);
			orig(self, cache, behindTiles);
		};
	}
}

internal static class NPCUtil
{
	public static NPCCachedDraw GetCachedDrawContext()
	{
		return NPCCachedDraws.Context;
	}
	
	public static Entity? GetTargetEntity(this NPC npc)
	{
		if (npc.HasValidTarget)
		{
			if (npc.HasPlayerTarget) { return Main.player[npc.target]; }
			if (npc.HasNPCTarget) { return Main.npc[npc.TranslatedTargetIndex]; }
		}

		return null;
	}

	public static void KillAllWithType(ReadOnlySpan<int> types)
	{
		foreach (int type in types)
		{
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.type == type)
				{
					try { npc.StrikeInstantKill(); }
					catch { }
				}
			}
		}
	}
}