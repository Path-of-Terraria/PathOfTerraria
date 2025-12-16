namespace PathOfTerraria.Content.Buffs;

public class EverburningShadowflameDebuff : ModBuff
{
	public class EverburningShadowflameNPC : GlobalNPC
	{
		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			if (npc.HasBuff<EverburningShadowflameDebuff>())
			{
				npc.lifeRegen = Math.Min(npc.lifeRegen, 0);
				npc.lifeRegen -= 30;
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