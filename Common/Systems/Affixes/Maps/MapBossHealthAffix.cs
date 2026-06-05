using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Affixes.Maps;

public class MapBossHealthAffix : MapAffix
{
	public override void ModifyNewNPC(NPC npc)
	{
		if (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type])
		{
			npc.lifeMax = (int)(npc.lifeMax * (1 + Value / 100f));
			npc.life = npc.lifeMax;
		}
	}
}
