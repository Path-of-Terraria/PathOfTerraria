using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs.ElementalBuffs;

internal class IgnitedDebuff : ModBuff
{
	public static void ApplyTo(NPC npc, int hitDamage, int time = 4 * 60)
	{
		// TODO: Add time duration modifier(s)
		IgnitedNPC ignited = npc.GetGlobalNPC<IgnitedNPC>();
		ignited.Stacks.Add(new IgnitedNPC.IgnitedStack(time, hitDamage));
		ignited.Stacks = [.. ignited.Stacks.OrderBy(x => x.BaseDamage)];

		npc.AddBuff(ModContent.BuffType<IgnitedDebuff>(), time);
	}

	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		if (!npc.TryGetGlobalNPC(out IgnitedNPC ignited))
		{
			return;
		}

		if (ignited.Stacks.Count == 0)
		{
			npc.DelBuff(buffIndex);
			buffIndex--;
		}

		if (Main.rand.NextBool(Math.Max(200 - (int)MathF.Sqrt(ignited.Stacks[0].BaseDamage), 1)))
		{
			Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch);
		}
	}
}

internal class IgnitedNPC : GlobalNPC
{
	public record IgnitedStack(int Time, int BaseDamage);

	public override bool InstancePerEntity => true;

	public List<IgnitedStack> Stacks = [];
	public float elapsedDoT = 0;

	public override bool PreAI(NPC npc)
	{
		if (npc.HasBuff<IgnitedDebuff>())
		{

		}

		return true;
	}
}