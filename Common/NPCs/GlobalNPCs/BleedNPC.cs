using Terraria.ID;

namespace PathOfTerraria.Common.NPCs.GlobalNPCs;

/// <summary>
/// Adds in <see cref="BuffID.Bleeding"/> functionality for NPCs, which usually don't have it.
/// </summary>
internal class BleedNPC : GlobalNPC
{
	public override void UpdateLifeRegen(NPC npc, ref int damage)
	{
		if (npc.HasBuff(BuffID.Bleeding))
		{
			npc.lifeRegen = Math.Min(npc.lifeRegen, 0);
			npc.lifeRegen -= 5;
			damage = Math.Max(5, damage);
		}
	}

	public override bool PreAI(NPC npc)
	{
		if (npc.HasBuff(BuffID.Bleeding) && Main.rand.NextBool(20))
		{
			Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, 0, Main.rand.NextFloat(2));
		}

		return true;
	}
}
