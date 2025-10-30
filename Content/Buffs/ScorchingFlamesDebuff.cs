using PathOfTerraria.Common.Systems.ElementalDamage;

namespace PathOfTerraria.Content.Buffs;

public sealed class ScorchingFlamesDebuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		if (npc.TryGetGlobalNPC(out ElementalNPC element))
		{
			element.Container[ElementType.Fire].Resistance -= 0.3f;
		}
	}
}
