using Terraria.ID;
using PathOfTerraria.Common.Systems;

namespace PathOfTerraria.Content.Buffs;

public sealed class SlowburnDebuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;

		BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		npc.GetGlobalNPC<SlowDownNPC>().AddSlowDown(0.5f);
	}
}