using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Passives;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs;

internal class GiveChilledNPCFunctionality : GlobalBuff
{
	public override void Update(int type, NPC npc, ref int buffIndex)
	{
		if (npc.HasBuff(BuffID.Chilled))
		{
			float multiplier = 1f;

			if (npc.lastInteraction != 255)
			{
				int chillPower = Main.player[npc.lastInteraction].GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(nameof(StrongerChillPassive));
				multiplier += chillPower * 0.1f;
			}

			npc.GetGlobalNPC<SlowDownNPC>().SlowDown += 0.1f * multiplier;
		}
	}
}
