
namespace PathOfTerraria.Common.Systems.Affixes.MobTypes;

internal class AggravatorAffix : MobAffix
{
	public override bool PreAI(NPC npc)
	{
		if (npc.life < npc.lifeMax / 2)
		{
			npc.GetGlobalNPC<SpeedUpNPC>().ExtraAISpeed += 0.3f;
			npc.defense = npc.defDefense + 10;
		}

		return true;
	}
}