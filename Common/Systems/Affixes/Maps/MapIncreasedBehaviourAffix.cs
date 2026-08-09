using PathOfTerraria.Common.Systems;

namespace PathOfTerraria.Common.Systems.Affixes.Maps;

public class MapIncreasedBehaviourAffix : MapAffix
{
	public override void PreAI(NPC npc)
	{
		npc.GetGlobalNPC<SpeedUpNPC>().ExtraAISpeed += Value / 100f;
	}
}
