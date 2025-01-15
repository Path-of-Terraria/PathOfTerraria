
namespace PathOfTerraria.Common.Systems.Affixes.MobTypes;

internal class SpeedAffixes
{
	public class FastAffix : MobAffix
	{
		public override bool PreAI(NPC npc)
		{
			npc.GetGlobalNPC<SpeedUpNPC>().ExtraAISpeed += 0.3f;
			return true;
		}
	}

	public class AggravatorAffix : MobAffix
	{
		public override bool PreAI(NPC npc)
		{
			if (npc.life < npc.lifeMax)
			{
				npc.GetGlobalNPC<SpeedUpNPC>().ExtraAISpeed += 0.3f;
				npc.defense = npc.defDefense + 10;
			}

			return true;
		}
	}
}
