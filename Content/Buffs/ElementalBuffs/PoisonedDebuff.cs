using PathOfTerraria.Common.Buffs;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Buffs.ElementalBuffs;

internal class PoisonedDebuff : ModBuff
{
	public static void Apply(NPC npc, int damage, int time)
	{
		npc.GetGlobalNPC<PoisonNPC>().Stacks.Add(new PoisonNPC.PoisonStack(damage, time));
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
	public class PoisonStack(int damage, int time)
	{
		public int Damage = damage;
		public int Time = time;
	}

	public override bool InstancePerEntity => true;

	internal readonly List<PoisonStack> Stacks = [];
	internal float ElapsedDoT = 0;

	public override bool PreAI(NPC npc)
	{
		foreach (PoisonStack stack in Stacks)
		{
			stack.Time--;

			ElapsedDoT += stack.Damage / 60f;
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