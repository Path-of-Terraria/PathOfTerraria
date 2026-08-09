using PathOfTerraria.Common.Systems.NPCCritFunctionality;

namespace PathOfTerraria.Common.Systems.Affixes.Maps;

public class MapMobCritChanceAffix : MapAffix
{
	public override void ModifyNewNPC(NPC npc)
	{
		if (npc.TryGetGlobalNPC(out CriticalStrikeNPC crit))
		{
			crit.CriticalStrikeChance += Value / 100f;
		}
	}
}
