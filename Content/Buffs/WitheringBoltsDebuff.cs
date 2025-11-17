using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Buffs;

public sealed class WitheringBoltsDebuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		float speedMult;
		if (npc.boss)
		{
			speedMult = 0.1f * 1.5f;
		}
		else
		{
			speedMult = 0.1f * 4f;
		}
		npc.GetGlobalNPC<SlowDownNPC>().AddSlowDown(speedMult);
	}
}