namespace PathOfTerraria.Content.Buffs;

public class BloodclotDebuff() : SmartBuff(true)
{
	public override void Update(NPC npc, ref int buffIndex)
	{
		npc.position -= npc.velocity * 0.05f;
	}
}
