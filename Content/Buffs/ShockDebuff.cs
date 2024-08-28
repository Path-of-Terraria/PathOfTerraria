using Terraria.ID;

namespace PathOfTerraria.Content.Buffs;

public sealed class ShockDebuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
		BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		if (Main.rand.NextBool(40))
		{
			Dust.NewDust(npc.position, npc.width, npc.height, DustID.Electric);
		}
	}

	private sealed class ShockedNPC : GlobalNPC
	{
		public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
		{
			if (npc.HasBuff<ShockDebuff>())
			{
				modifiers.FinalDamage += 0.1f;
			}
		}
	}
}