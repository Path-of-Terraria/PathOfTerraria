using PathOfTerraria.Common;
using PathOfTerraria.Content.SkillPassives.FireballPassives;
using PathOfTerraria.Content.SkillTrees;

namespace PathOfTerraria.Content.Buffs;

public class EverburningShadowflameDebuff : ModBuff
{
	public class EverburningShadowflameNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		internal int LastPlayerApplied = 0;

		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			if (npc.HasBuff<EverburningShadowflameDebuff>())
			{
				npc.lifeRegen = Math.Min(npc.lifeRegen, 0);
				npc.lifeRegen -= 30;

				if (Main.player[LastPlayerApplied].HasTreePassive<FireballTree, Pyremaniac>())
				{
					npc.lifeRegen -= (int)Pyremaniac.DoTBuff;
					damage = Math.Max(damage, 8);
				}
			}
		}
	}

	public override void SetStaticDefaults()
	{
		Main.buffNoSave[Type] = true;
		Main.debuff[Type] = false;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		npc.buffTime[buffIndex]++;
		npc.shadowFlame = true;
	}
}