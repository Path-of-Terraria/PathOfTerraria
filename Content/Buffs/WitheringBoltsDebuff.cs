namespace PathOfTerraria.Content.Buffs;

public sealed class WitheringBoltsDebuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		npc.velocity *= 0.6f;
	}
}