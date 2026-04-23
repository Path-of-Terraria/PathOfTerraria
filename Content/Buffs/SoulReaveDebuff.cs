using PathOfTerraria.Common.Buffs;
using PathOfTerraria.Content.Passives.Summon.Masteries;

namespace PathOfTerraria.Content.Buffs;

internal class SoulReaveDebuff : ModBuff
{
	public static void Apply(NPC npc, int time, int fromWho, int damage)
	{
		damage = Math.Max((int)(damage * SoulReaveMastery.ReaveDamageProportion / 100f), 1);
		DoTFunctionality.ApplyPlayerInteraction(npc, fromWho);
		npc.GetGlobalNPC<SoulReaveMastery.SoulReaveNPC>().Stacks.Add(new SoulReaveMastery.SoulReaveNPC.SoulReaveStack(time, fromWho, damage));
		npc.AddBuff(ModContent.BuffType<SoulReaveDebuff>(), time);
	}

	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		if (npc.GetGlobalNPC<SoulReaveMastery.SoulReaveNPC>().Stacks.Count == 0)
		{
			npc.DelBuff(buffIndex);
			buffIndex--;
		}
	}
}
