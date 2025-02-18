using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.QueenDomain;

internal class NerfQueenSlimeAdds : GlobalNPC
{
	public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
	{
		return entity.type is NPCID.QueenSlimeMinionBlue or NPCID.QueenSlimeMinionPink or NPCID.QueenSlimeMinionPurple;
	}

	public override void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment)
	{
		npc.lifeMax /= 4;
	}
}
