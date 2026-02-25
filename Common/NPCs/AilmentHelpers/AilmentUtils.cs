using Terraria;

namespace PathOfTerraria.Common.NPCs.AilmentHelpers;

internal static class AilmentUtils
{
	private const float DefaultAilmentCap = 0.5f;

	public static float GetNPCAilmentCap(NPC npc)
	{
		return DefaultAilmentCap; // Constant for now; will be variable in the future, and this will allow for easy impl then
	}

	public static float GetPlayerAilmentCap(Player player)
	{
		return DefaultAilmentCap; // Constant for now; will be variable in the future, and this will allow for easy impl then
	}

	private static float GetAilmentThreshold(int damageDealt, int maxHealth, float ailmentCap)
	{
		float healthCap = maxHealth * ailmentCap;
		return damageDealt / healthCap / 4f;
	}

	public static float GetNPCAilmentThreshold(NPC npc, int damageDealt)
	{
		return GetAilmentThreshold(damageDealt, npc.lifeMax, GetNPCAilmentCap(npc));
	}

	public static float GetPlayerAilmentThreshold(Player player, int damageDealt)
	{
		return GetAilmentThreshold(damageDealt, player.statLifeMax2, GetPlayerAilmentCap(player));
	}

	public static float GetEntityAilmentThreshold(Entity entity, int damageDealt)
	{
		return entity switch
		{
			NPC npc => GetNPCAilmentThreshold(npc, damageDealt),
			Player plr => GetPlayerAilmentThreshold(plr, damageDealt),
			_ => 0,
		};
	}
}
