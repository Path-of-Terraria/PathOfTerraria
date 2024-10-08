using PathOfTerraria.Common.Systems;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs;

internal class GiveChilledNPCFunctionality : GlobalBuff
{
	public override void Update(int type, NPC npc, ref int buffIndex)
	{
		if (npc.HasBuff(BuffID.Chilled))
		{
			npc.GetGlobalNPC<SlowDownNPC>().SlowDown += 0.1f;
		}
	}
}
