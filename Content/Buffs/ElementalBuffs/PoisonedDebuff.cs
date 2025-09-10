using PathOfTerraria.Common.Buffs;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs.ElementalBuffs;

internal class PoisonedDebuff : ModBuff
{
	public override void Load()
	{
		On_NPC.AddBuff += ModifyPoisonAddition;
	}

	private void ModifyPoisonAddition(On_NPC.orig_AddBuff orig, NPC self, int type, int time, bool quiet)
	{
		if (type == BuffID.Poisoned)
		{
			Apply(self, time);
			return;
		}

		orig(self, type, time, quiet);
	}

	public static void Apply(NPC npc, int time)
	{
		npc.GetGlobalNPC<PoisonNPC>().Stacks.Add(time);
		npc.AddBuff(ModContent.BuffType<PoisonedDebuff>(), time);
	}

	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		if (npc.GetGlobalNPC<PoisonNPC>().Stacks.Count == 0)
		{
			npc.DelBuff(buffIndex);
			buffIndex--;
		}
	}
}

internal class PoisonNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	internal readonly List<int> Stacks = [];
	internal float ElapsedDoT = 0;

	public override bool PreAI(NPC npc)
	{
		for (int i = 0; i < Stacks.Count; i++)
		{
			Stacks[i]--;
			ElapsedDoT += 4 / 60f;
		}

		if (ElapsedDoT > 60)
		{
			DoTFunctionality.ApplyDoT(npc, 60, ref ElapsedDoT, Color.GreenYellow, Color.DarkGreen);
		}
		else if (ElapsedDoT > 0 && Stacks.Count == 0)
		{
			DoTFunctionality.ApplyDoT(npc, (int)ElapsedDoT, ref ElapsedDoT, Color.GreenYellow, Color.DarkGreen);
		}

		return true;
	}
}