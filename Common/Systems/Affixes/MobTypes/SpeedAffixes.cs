﻿
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
}
